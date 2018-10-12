using AudiobookDownloader.Service;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using AudiobookDownloader.Repository;
using System.Drawing;
using AudiobookDownloader.Logging;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	public partial class AudiobookDownloader : Form
	{
		private readonly ICustomLogger logger;
		private readonly IAudiobookService service;
		private readonly IAudiobookRepository db;

		private readonly OwnRadioClient client;
		private readonly Grabber grabber;

		private readonly string baseUrl = ConfigurationManager.AppSettings["AbookService"];

		public AudiobookDownloader()
		{
			InitializeComponent();

			logger = new CustomLogger(textLog);
			service = new AbooksService(logger, baseUrl);
			client = new OwnRadioClient(logger);
			grabber = new Grabber(service, logger, db, client);
			db = new SqLiteAudiobookRepository();
		}

		/// <summary>
		/// Метод скачивания файлов с сайта и последующей выгрузкой на сторону Rdev-a
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			try
			{
				// Количество скаченных со страницы аудиокниг
				int countDownloaded = 0;

				// Формируем объект категории "Новинки"
				var novelty = new Category { Name = "Новинки", Url = baseUrl };

				logger.Debug("Получаем общее количество страниц в категории Новинки.");

				// Получаем общее количество страниц на сайте в категории "Новинки"
				int countPage = await service.GetPagesCount(novelty);

				// Запускаем цикл обхода страниц с книгам начиная с конца (самые новые книги находятся на 1 странице)
				for (int page = countPage; page >= 1; page--)
				{
					logger.Debug($"Получаем количество аудиокниг со страницы {page}.");

					// Получаем  список аудиокниг со страницы
					var audiobooks = await service.GetAudiobooks(novelty, page);

					logger.Debug($"Количество аудиокниг на странице {page}: {audiobooks.Count}.");

					// Запускаем цикл на последовательное скачивание аудиокниг со страницы
					foreach (var audiobook in audiobooks)
					{
						logger.Log($"Загружаем аудиокнигу: {audiobook.Title}.");

						await grabber.Grab(audiobook);
						++countDownloaded;

						logger.Success($"Аудиокнига {audiobook.Title} загружена, " +
							$"оставшееся количество аудиокниг на странице {audiobooks.Count - countDownloaded}."
						);
					}

					countDownloaded = 0;
				}
			}
			catch (Exception ex)
			{
				logger.Error($"Скачивание остановлено по причине ошибки: {ex.Message}.");
			}
		}

		/// <summary>
		/// Метод запускает загрузку файлов на стороне Rdev-а (на момент 2018-10-04 не используется)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void RdevLoad_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = baseUrl };

				int countPage = await service.GetPagesCount(novelty);

				Debug($"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.");

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await service.GetAudiobooks(novelty, page);

					Debug($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						int audiobookId = await service.GetAudiobookId(audiobook);

						Debug($"Попытка загрузить аудиокнигу {audiobook.Title}.");

						var result = await client.StartDownload(
							audiobook.Title, 
							audiobook.Url, 
							$"{baseUrl}/download/{audiobookId}"
						);

						if(result != System.Net.HttpStatusCode.OK)
							Debug($"При попытке запустить скачивание {audiobook.Title} возникла ошибка.");

						Debug($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch (Exception ex)
			{
				Debug($"Необработанная ошибка: {ex.Message}");
			}
		}

		/// <summary>
		/// Метод отправки файлов из локальной директории на Rdev по http
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void DirUpload_Click(object sender, EventArgs e)
		{
			try
			{
				// Получаем путь к директории с аудиокнигами
				var path = ConfigurationManager.AppSettings["Audiobooks"];
				int counter = 0;

				// Заканчиваем работу метода если не удалось найти директорию с аудиокнигами
				if (!Directory.Exists(path))
				{
					Debug("Директория с аудиокнигами не существует!");
					return;
				}

				// Получаем список файлов из директории (аудиокниги в директории представлены в виде .zip архива)
				var files = Directory.GetFiles(path);

				Debug("Запущена загрузка аудиокниг из локальной директории");

				// Запускаем цикл на последовательный обход архива с аудиокнигами
				foreach (var file in files)
				{
					using (var zip = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.GetEncoding(866)))
					{
						//Получаем список файлов
						var zipContent = zip.Entries;

						// Ищем txt файл со ссылкой на аудиокнигу
						var txtName = zipContent.FirstOrDefault(x => Path.GetExtension(x.Name.Replace('<', ' ').Replace('>', ' ')) == ".txt");

						// Копируем содержимое файла в memory stream
						var memStream = new MemoryStream();
						await txtName.Open().CopyToAsync(memStream);

						// Получаем содержимое файла
						string txtContnent = Encoding.ASCII.GetString(memStream.ToArray());

						// Формируем объект аудиокниги
						var audiobook = new Audiobook
						{
							Title = Path.GetFileNameWithoutExtension(txtName.Name.Replace('<', ' ').Replace('>', ' ')),
							Url = txtContnent
						};

						// Сохраняем аудиокнигу в таблицу скаченных аудиокниг
						await db.SaveDownloadAudiobook(audiobook);
						Debug($"Попытка загрузки аудиокниги {audiobook.Title}, URL:{audiobook.Url}");

						// Получаем все mp3 файлы
						var mp3Content = zipContent.Where(x => Path.GetExtension(x.Name.Replace('<', ' ').Replace('>', ' ')) == ".mp3").ToList();
						int chapter = 0;

						Debug($"Количество аудиофайлов: {mp3Content.Count}");
						Debug($"Проверяем была ли аудиокнига: {audiobook.Title} загружена ранее");

						// Если книга полностью отдана на Rdev, идем к следующей
						if (db.IsUploadAudiobook(audiobook))
						{
							Debug($"Количество загруженных книг {++counter}");
							continue;
						}

						Debug($"Получаем ownerrecid для файла аудиокниги: {audiobook.Title}");

						// Получаем идентификатор книги, если книга уже выгружалась на рдев но не все файлы были переданы
						// получаем значение идентификатора из бд иначе если это первая выгрузка формируем новый идентификатор
						Guid ownerRecId = db.GetOwnerRecid(audiobook);

						Debug($"Запускаем upload аудиофайлов книги: {audiobook.Title}");
						foreach (var entry in mp3Content)
						{
							using (var fs = entry.Open())
							{
								Guid recId = Guid.NewGuid();

								// Формируем объект отправляемого файла
								var sendedFile = new Audiofile()
								{
									Name = entry.Name,
									Chapter = ++chapter,
									OwnerRecid = ownerRecId.ToString(),
									AudiobookName = audiobook.Title,
									AudiobookUrl = audiobook.Url
								};

								Debug($"Проверяем отправлялся ли файл {sendedFile.Name} ранее");

								// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
								// иначе отдаем файл
								if (db.IsUploadAudiofile(sendedFile))
								{
									Debug($"Файл {sendedFile.Name} уже отправлялся, переходим к следующему");
									continue;
								}

								Debug($"Отправляем аудиофайл: {sendedFile.Name}");

								// Отправляем файл на Rdev
								await client.Upload(sendedFile, fs, recId);

								Debug($"Сохраняем информацию о переданном файле {sendedFile.Name}");

								// В случае если файл был успешно передан на Rdev (вернулся статус код 200)
								// сохраняем информацию о файле в бд, иначе выставляем флаг ошибки, и продолжаем передавать следующие файлы
								// когда отправка всех файлов завершится перед сохранением аудиокниги в успешно отданные анализируем флаг, 
								// если возникла ошибка то не добавляем аудиокнигу в отданные чтобы можно было повторить upload тех файлов которые не были переданы
								await db.SaveUploadAudiofile(sendedFile);
							}
						}

						Debug($"Все файлы аудиокниги {audiobook.Title} были переданны, сохраняем аудиокнигу в историю загрузок.");

						// Добавляем запись о полностью отданной на Rdev книге, если не было ошибки при передаче файлов
						await db.SaveUploadAudiobook(audiobook);

						Debug($"Количество загруженных книг{++counter}");
					}
				}
			}
			catch(Exception ex)
			{
				Error(ex.Message);
			}
		}

		/// <summary>
		/// Метод скачивания аудиокниг в локальное хранилище (в директорию на диске)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void LocalDownload_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = baseUrl };
				int countPage = await service.GetPagesCount(novelty);

				Debug($"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.");

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await service.GetAudiobooks(novelty, page);

					Debug($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						await grabber.GrabLocal(audiobook);
						Debug($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch (HttpRequestException ex)
			{
				Debug($"Необработанная ошибка: {ex.InnerException.Message}");
			}
			catch (Exception ex)
			{
				Debug($"Необработанная ошибка: {ex.Message}");
			}
		}

		private async void AuthTest_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				await client.Authorize();
				await client.TestRequest();
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
			}
		}

		private void ClearLog_Click(object sender, EventArgs e)
		{
			logger.Clear();
		}

		/// <summary>
		/// Метод вывода текста в лог
		/// </summary>
		/// <param name="text"></param>
		private void Print(string text, Color color)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append($"[{DateTime.Now}]:");
			sb.Append(text);
			sb.AppendLine();

			textLog.SelectionColor = color;
			textLog.AppendText(sb.ToString());
		}

		private void Debug(string text)
		{
			Print($"[DEBUG]: {text}", Color.Black);
		}

		private void Success(string text)
		{
			Print($"[SUCCESS]: {text}", Color.Green);
		}

		private void Error(string text)
		{
			Print($"[ERROR]: {text}", Color.Red);
		}
	}
}
