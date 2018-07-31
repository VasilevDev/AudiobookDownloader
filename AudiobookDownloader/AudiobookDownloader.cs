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

namespace AudiobookDownloader
{
	public partial class AudiobookDownloader : Form
	{
		private readonly IAudiobookService _service;
		private readonly Grabber _grabber;
		private readonly Context _db;

		public AudiobookDownloader()
		{
			InitializeComponent();

			_service = new AbooksService();
			_grabber = new Grabber(_service);
			_db = new Context();
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = ConfigurationManager.AppSettings["BaseServer"] };
				int countPage = await _service.GetPagesCount(novelty);

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

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

		private async void rdevLoad_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = ConfigurationManager.AppSettings["BaseServer"] };

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
							$"{ConfigurationManager.AppSettings["BaseServer"]}/download/{audiobookId}"
						);

						if(result == System.Net.HttpStatusCode.BadRequest || result == System.Net.HttpStatusCode.NotFound)
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

		private async void dirUpload_Click(object sender, EventArgs e)
		{
			OwnRadioClient client = new OwnRadioClient();
			var path = ConfigurationManager.AppSettings["Audiobooks"];
			int counter = 0;

			if (!Directory.Exists(path))
			{
				log.Items.Add("Директория с аудиокнигами не существует!");
				return;
			}

			var files = Directory.GetFiles(path);

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

					byte[] bytes = new byte[(int)memStream.Length];

					memStream.Position = 0;
					await memStream.ReadAsync(bytes, 0, (int)memStream.Length);

					// Получаем содержимое файла
					string txtContnent = Encoding.ASCII.GetString(bytes);

					var audiobook = new Audiobook
					{
						Title = Path.GetFileNameWithoutExtension(txtName.Name.Replace('<', ' ').Replace('>', ' ')),
						Url = txtContnent
					};

					log.Items.Add($"Попытка загрузки аудиокниги {audiobook.Title}, URL:{audiobook.Url}");

					// Получаем все mp3 файлы
					var mp3Content = zipContent.Where(x => Path.GetExtension(x.Name.Replace('<', ' ').Replace('>', ' ')) == ".mp3").ToList();
					int chapter = 0;

					log.Items.Add($"Количество аудиофайлов: {mp3Content.Count}");
					log.Items.Add($"Проверяем была ли аудиокнига: {audiobook.Title} загружена ранее");

					// Если книга полностью отдана на Rdev, выходим из метода
					if (_db.UploadAudiobook.Count() > 0)
					{
						var dbAudiobook = _db.UploadAudiobook
							.Select(m => m.Audiobook)
							.Where(m =>
								m.Title == audiobook.Title &&
								m.Url == audiobook.Url
							).FirstOrDefault();

						if (dbAudiobook != null)
						{
							label.Text = $"Количество загруженных книг{++counter}";
							continue;
						}
					}

					Audiofile uploadFile = null;
					Guid ownerRecId;

					log.Items.Add($"Получаем ownerrecid для файла аудиокниги: {audiobook.Title}");

					// Если в таблице, отданных на Rdev файлов, имеется хотя бы одна запись о файлах из данной книги, то Ownerrecid получаем из этой записи,
					// иначе генерируем новый
					if (_db.UploadAudiofile.Count() > 0)
						uploadFile = _db.UploadAudiofile.Select(m => m.File).Where(m => m.AudiobookUrl == audiobook.Url).FirstOrDefault();

					ownerRecId = (uploadFile != null) ? Guid.Parse(uploadFile.OwnerRecid) : Guid.NewGuid();

					log.Items.Add($"Запускаем upload аудиофайлов книги: {audiobook.Title}");

					foreach (var entry in mp3Content)
					{
						using (var fs = entry.Open())
						{
							Guid recId = Guid.NewGuid();

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
							var dbFile = _db.UploadAudiofile
								.Select(m => m.File)
								.Where(m =>
									m.Name == entry.Name &&
									m.Chapter == chapter &&
									m.OwnerRecid == ownerRecId.ToString()
								).FirstOrDefault();

							if (dbFile != null)
							{
								log.Items.Add($"Файл {sendedFile.Name} уже отправлялся, переходим к следующему");
								continue;
							}

							log.Items.Add($"Отправляем аудиофайл: {sendedFile.Name}");

							await client.Upload(sendedFile, fs, recId);

							log.Items.Add($"Сохраняем информацию о переданном файле {sendedFile.Name}");

							// Добавляем файл в таблицу отданных файлов
							_db.UploadAudiofile.Add(new UploadAudiofile { File = sendedFile });
							await _db.SaveChangesAsync();
						}
					}

					log.Items.Add($"Все файлы аудиокниги {audiobook.Title} были переданны, сохраняем аудиокнигу в историю загрузок.");

					// Добавляем запись о полностью отданной на Rdev книге
					_db.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
					await _db.SaveChangesAsync();

					label.Text = $"Количество загруженных книг{++counter}";
				}
			}
		}

		private async void button1_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = ConfigurationManager.AppSettings["BaseServer"]};
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
	}
}
