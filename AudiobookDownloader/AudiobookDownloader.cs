using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity;
using AudiobookDownloader.Repository;

namespace AudiobookDownloader
{
	public partial class AudiobookDownloader : Form
	{
		private readonly IAudiobookService _service;
		private readonly Grabber _grabber;
		private readonly IAudiobookRepository _db;
		private readonly string url = ConfigurationManager.AppSettings["AbookService"];

		public AudiobookDownloader()
		{
			InitializeComponent();

			_service = new AbooksService();
			_grabber = new Grabber(_service);
			_db = new SqLiteAudiobookRepository();
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
				// Формируем объект категории "Новинки"
				var novelty = new Category { Name = "Новинки", Url = url };

				// Получаем общее количество страниц на сайте в категории "Новинки"
				int countPage = await _service.GetPagesCount(novelty);

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				// Запускаем цикл обхода страниц с книгам начиная с конца (самые новые книги находятся на 1 странице)
				for (int page = countPage; page >= 1; page--)
				{
					// Получаем  список аудиокниг со страницы
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					// Запускаем цикл на последовательное скачивание аудиокниг со страницы
					foreach (var audiobook in audiobooks)
					{
						await _grabber.Grab(audiobook);
						log.Items.Add($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch(HttpRequestException ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.InnerException.Message}");
			}
			catch (Exception ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.Message}");
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
				var novelty = new Category { Name = "Новинки", Url = url };

				int countPage = await _service.GetPagesCount(novelty);
				var client = new OwnRadioClient();

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						int audiobookId = await _service.GetAudiobookId(audiobook);

						log.Items.Add($"Попытка загрузить аудиокнигу {audiobook.Title}.");

						var result = await client.StartDownload(
							audiobook.Title, 
							audiobook.Url, 
							$"{url}/download/{audiobookId}"
						);

						if(result != System.Net.HttpStatusCode.OK)
							log.Items.Add($"При попытке запустить скачивание {audiobook.Title} возникла ошибка.");

						log.Items.Add($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch (Exception ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.Message}");
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
					log.Items.Add("Директория с аудиокнигами не существует!");
					return;
				}

				// Получаем список файлов из директории (аудиокниги в директории представлены в виде .zip архива)
				var files = Directory.GetFiles(path);

				label.Text = "Запущена загрузка аудиокниг из локальной директории";

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
						await _db.SaveDownloadAudiobook(audiobook);
						log.Items.Add($"Попытка загрузки аудиокниги {audiobook.Title}, URL:{audiobook.Url}");

						// Получаем все mp3 файлы
						var mp3Content = zipContent.Where(x => Path.GetExtension(x.Name.Replace('<', ' ').Replace('>', ' ')) == ".mp3").ToList();
						int chapter = 0;

						log.Items.Add($"Количество аудиофайлов: {mp3Content.Count}");
						log.Items.Add($"Проверяем была ли аудиокнига: {audiobook.Title} загружена ранее");

						// Если книга полностью отдана на Rdev, идем к следующей
						if (_db.IsUploadAudiobook(audiobook))
						{
							label.Text = $"Количество загруженных книг {++counter}";
							continue;
						}

						log.Items.Add($"Получаем ownerrecid для файла аудиокниги: {audiobook.Title}");

						// Получаем идентификатор книги, если книга уже выгружалась на рдев но не все файлы были переданы
						// получаем значение идентификатора из бд иначе если это первая выгрузка формируем новый идентификатор
						Guid ownerRecId = _db.GetOwnerRecid(audiobook);

						log.Items.Add($"Запускаем upload аудиофайлов книги: {audiobook.Title}");
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

								log.Items.Add($"Проверяем отправлялся ли файл {sendedFile.Name} ранее");

								// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
								// иначе отдаем файл
								if (_db.IsUploadAudiofile(sendedFile))
								{
									log.Items.Add($"Файл {sendedFile.Name} уже отправлялся, переходим к следующему");
									continue;
								}

								log.Items.Add($"Отправляем аудиофайл: {sendedFile.Name}");

								// Создаем клиента для отправки файлов на Rdev
								var client = new OwnRadioClient();

								// Отправляем файл на Rdev
								await client.Upload(sendedFile, fs, recId);

								log.Items.Add($"Сохраняем информацию о переданном файле {sendedFile.Name}");

								// В случае если файл был успешно передан на Rdev (вернулся статус код 200)
								// сохраняем информацию о файле в бд, иначе выставляем флаг ошибки, и продолжаем передавать следующие файлы
								// когда отправка всех файлов завершится перед сохранением аудиокниги в успешно отданные анализируем флаг, 
								// если возникла ошибка то не добавляем аудиокнигу в отданные чтобы можно было повторить upload тех файлов которые не были переданы
								await _db.SaveUploadAudiofile(sendedFile);
							}
						}

						log.Items.Add($"Все файлы аудиокниги {audiobook.Title} были переданны, сохраняем аудиокнигу в историю загрузок.");

						// Добавляем запись о полностью отданной на Rdev книге, если не было ошибки при передаче файлов
						await _db.SaveUploadAudiobook(audiobook);

						label.Text = $"Количество загруженных книг{++counter}";
					}
				}
			}
			catch(Exception ex)
			{
				label.Text = $"[ОШИБКА]: {ex.Message}";
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
				var novelty = new Category { Name = "Новинки", Url = url };
				int countPage = await _service.GetPagesCount(novelty);

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						await _grabber.GrabLocal(audiobook);
						log.Items.Add($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch (HttpRequestException ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.InnerException.Message}");
			}
			catch (Exception ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.Message}");
			}
		}

		/// <summary>
		/// Метод для теста авторизации на сервере Rdev-a
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void TestAuth_Click(object sender, EventArgs e)
		{
			try
			{
				var client = new OwnRadioClient();

				await client.Authorize();
				await client.TestRequest();
			}
			catch(Exception ex)
			{
				log.Items.Add(ex.Message);
			}
		}
	}
}
