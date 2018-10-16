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
using AudiobookDownloader.Logging;
using AudiobookDownloader.Entity;

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
		private readonly string baseDirectory = ConfigurationManager.AppSettings["Audiobooks"];

		public AudiobookDownloader()
		{
			InitializeComponent();

			logger = new CustomLogger(textLog);
			db = new SqLiteAudiobookRepository();
			service = new AbooksService(logger, baseUrl);
			client = new OwnRadioClient(logger);
			grabber = new Grabber(service, logger, db, client, baseDirectory);

			if (Boolean.Parse(ConfigurationManager.AppSettings["IsUseProxy"]))
			{
				IsProxy.Checked = true;
			}

			proxy.Text = ConfigurationManager.AppSettings["ProxyIp"];
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
						logger.Log($"Загружаем аудиокнигу: {audiobook.Name}.");

						await grabber.Grab(audiobook);
						++countDownloaded;

						logger.Success($"Аудиокнига {audiobook.Name} загружена, " +
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

				logger.Debug($"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.");

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await service.GetAudiobooks(novelty, page);

					logger.Debug($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						int audiobookId = await service.GetAudiobookId(audiobook);
						audiobook.DownloadUrl = $"{baseUrl}/download/{audiobookId}";

						logger.Debug($"Попытка загрузить аудиокнигу {audiobook.Name}.");

						await client.StartDownload(
							audiobook.Name, 
							audiobook.Url, 
							$"{baseUrl}/download/{audiobookId}"
						);

						logger.Success($"Книга {audiobook.Name} загружена.");
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error($"Необработанная ошибка: {ex.Message}");
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
				// Заканчиваем работу метода если не удалось найти директорию с аудиокнигами
				if (!Directory.Exists(baseDirectory))
				{
					logger.Warning("Директория с аудиокнигами не существует!");
					return;
				}

				// Получаем список файлов из директории (аудиокниги в директории представлены в виде .zip архива)
				var files = Directory.GetFiles(baseDirectory);

				logger.Debug("Запущена загрузка аудиокниг из локальной директории");
				logger.Log($"Количество файлов: {files.Count()}.");

				// Количество загруженных аудиокниг
				int counter = 0;

				// Запускаем цикл на последовательный обход архива с аудиокнигами
				foreach (var file in files)
				{
					if (Path.GetExtension(file) != ".zip")
						continue;

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
							Name = Path.GetFileNameWithoutExtension(txtName.Name.Replace('<', ' ').Replace('>', ' ')),
							Url = txtContnent,
							OriginalName = Path.GetFileName(txtContnent)
						};

						// Сохраняем аудиокнигу в таблицу скаченных аудиокниг
						await db.SaveDownloadAudiobook(audiobook);

						logger.Debug($"Попытка загрузки аудиокниги {audiobook.Name}, URL:{audiobook.Url}");

						// Получаем все mp3 файлы
						var mp3Content = zipContent.Where(x => Path.GetExtension(x.Name.Replace('<', ' ').Replace('>', ' ')) == ".mp3").ToList();
						int chapter = 0;

						audiobook.FilesCount = mp3Content.Count;

						logger.Log($"Аудиокнига {audiobook.Name} из {mp3Content.Count} аудиофайлов.");
						logger.Debug($"Проверяем была ли аудиокнига: {audiobook.Name} загружена ранее.");

						// Если книга полностью отдана на Rdev, идем к следующей
						if (await db.IsUploadAudiobook(audiobook))
						{
							logger.Warning($"Аудиокнига {audiobook.Name} была ранее передана на Rdev.");
							logger.Log($"Количество загруженных книг {++counter}.");
							continue;
						}

						logger.Debug($"Получаем ownerrecid для файла аудиокниги: {audiobook.Name}.");

						// Получаем идентификатор книги, если книга уже выгружалась на рдев но не все файлы были переданы
						// получаем значение идентификатора из бд иначе если это первая выгрузка формируем новый идентификатор
						Guid ownerRecId = await db.GetOwnerRecid(audiobook);

						logger.Debug($"Запускаем upload аудиофайлов книги: {audiobook.Name}.");
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
									AudiobookName = audiobook.Name,
									AudiobookUrl = audiobook.Url,
									AudiobookOriginalName = audiobook.OriginalName,
									Size = entry.Length
								};

								logger.Debug($"Проверяем отправлялся ли файл {sendedFile.Name} ранее.");

								// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
								// иначе отдаем файл
								if (await db.IsUploadAudiofile(sendedFile))
								{
									logger.Warning($"Файл {sendedFile.Name} уже отправлялся, переходим к следующему.");
									continue;
								}

								logger.Debug($"Отправляем аудиофайл: {sendedFile.Name}");

								// Отправляем файл на Rdev
								await client.Upload(sendedFile, fs, recId);

								logger.Debug($"Сохраняем информацию о переданном файле {sendedFile.Name}");

								// В случае если файл был успешно передан на Rdev (вернулся статус код 200)
								await db.SaveUploadAudiofile(sendedFile);

								logger.Success($"Информация о файле {sendedFile.Name} была успешно сохранена.");
							}
						}

						logger.Debug($"Все файлы аудиокниги {audiobook.Name} были переданны, сохраняем аудиокнигу в историю загрузок.");

						// Добавляем запись о полностью отданной на Rdev книге, если не было ошибки при передаче файлов
						await db.SaveUploadAudiobook(audiobook);

						logger.Success($"Информация об аудиокниге {audiobook.Name} была успешно сохранена.");
						logger.Debug($"Количество загруженных книг{++counter}");
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message);
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

				logger.Debug($"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.");

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await service.GetAudiobooks(novelty, page);

					logger.Log($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{

						logger.Debug($"Загружаем аудиокнигу {audiobook.Name}.");

						await grabber.GrabLocal(audiobook);

						logger.Success($"Аудиокнига {audiobook.Name} загружена.");
					}
				}
			}
			catch (HttpRequestException ex)
			{
				logger.Error($"Необработанная ошибка: {ex.InnerException.Message}.");
			}
			catch (Exception ex)
			{
				logger.Error($"Необработанная ошибка: {ex.Message}.");
			}
		}

		/// <summary>
		/// Тестовый метод для проверки авторизации на сервере Rdev
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void AuthTest_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				logger.Debug("Запущен тест проверки авторизации на Rdev.");

				await client.Authorize();

				logger.Success("Тест выполнен успешно.");
			}
			catch (Exception ex)
			{
				logger.Error("Ошибка при выполнении теста: " + ex.Message);
			}
		}

		/// <summary>
		/// Метод очистки лога
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClearLog_Click(object sender, EventArgs e)
		{
			logger.Clear();
		}

		/// <summary>
		/// Метод активирует/деактивирует использование прокси
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ProxyItem_Click(object sender, EventArgs e)
		{
			try
			{
				Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

				var isProxy = Boolean.Parse(config.AppSettings.Settings["IsUseProxy"].Value);
				isProxy = !isProxy;
				config.AppSettings.Settings["IsUseProxy"].Value = isProxy.ToString();
				config.Save();
				ConfigurationManager.RefreshSection("appSettings");

				IsProxy.Checked = isProxy;

				logger.Success($"Использование прокси {(isProxy ? "включено" : "отключено")}.");
			}
			catch(Exception ex)
			{
				logger.Error("Ошибка при активации/деактивации прокси запросов: " + ex.Message);
			}
		}

		/// <summary>
		/// Метод вызова формы с настройками прокси
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ProxySettingsItem_Click(object sender, EventArgs e)
		{
			using (var proxySettings = new ProxySettingsForm())
			{
				if(proxySettings.ShowDialog() == DialogResult.OK)
				{
					proxy.Text = ConfigurationManager.AppSettings.Get("ProxyIp");
				}
			}
		}
	}
}
