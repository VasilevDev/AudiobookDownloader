using AudiobookDownloader.Entity;
using AudiobookDownloader.Logging;
using AudiobookDownloader.Repository;
using AudiobookDownloader.Service;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	public class Grabber
	{
		private readonly ICustomLogger logger;
		private readonly IAudiobookService service;
		private readonly IAudiobookRepository db;

		private readonly OwnRadioClient client;
		private const string fileName = "tmp.zip";
		private string fileFullName;
		private readonly string dirPath;

		public Grabber(IAudiobookService service, ICustomLogger logger, IAudiobookRepository db, OwnRadioClient client, string dirPath)
		{
			this.logger = logger;
			this.service = service;
			this.db = db;
			this.client = client;
			this.dirPath = dirPath;

			if (!Directory.Exists(this.dirPath))
				Directory.CreateDirectory(this.dirPath);

			// Запишем в переменную полный путь к файлу, чтобы иметь к нему доступ в методе upload
			fileFullName = $"{dirPath}/{fileName}";
		}

		/// <summary>
		/// Метод скачивает архив с аудиокнигой и передает по файлам на сервер Rdev
		/// </summary>
		/// <param name="audiobook">Аудиокнига которую необходимо загрузить</param>
		/// <returns></returns>
		public async Task Grab(Audiobook audiobook)
		{
			logger.Debug("Запускаем авторизацию на сервере Rdev.");

			// Авторизуемся на сервере Rdev
			await client.Authorize();

			logger.Debug("Запускаем скачивание с удаленного сервера.");

			// Скачиваем аудиокнигу в локальную директорию
			await Download(audiobook);

			logger.Debug("Запускаем выгрузку файлов аудиокниги на сервер Rdev.");

			// Выгружаем аудиокнигу на Rdev из локальной директроии
			await Upload(audiobook);
		}

		public async Task GrabLocal(Audiobook audiobook)
		{
			await Download(audiobook, $"{audiobook.Name}.zip");
		}

		/// <summary>
		/// Метод загружает аудиокнигу с сайта, в случае успешного скачивания книги,
		/// сохраняет запись в таблицу DownloadAudiobook
		/// </summary>
		/// <param name="audiobook">Аудиокнига, которую необходимо загрузить</param>
		/// <returns></returns>
		private async Task Download(Audiobook audiobook, string filename = fileName)
		{
			try
			{
				logger.Log($"Проверяем, нет ли в БД в таблице скаченных аудиокниг информации об аудиокниге: {audiobook.Name}.");

				// Запишем в переменную полный путь к файлу, чтобы иметь к нему доступ в методе upload
				fileFullName = $"{dirPath}/{filename}";

				// Проверяем не была ли загружена аудиокнига
				bool isDownload = await db.IsDownloadAudiobook(audiobook);

				// Если нет, скачиваем
				if (!isDownload)
				{
					logger.Log($"Аудиокнига {audiobook.Name} не скачивалась ранее, запускаем загрузку.");
					logger.Log($"Абсолютный путь к файлу на диске: {$"{dirPath}/{filename}"}.");

					using (var fs = new FileStream(fileFullName, FileMode.Create, FileAccess.ReadWrite))
					{
						logger.Debug($"Запускаем скачивание аудиокниги {audiobook.Name}.");

						await service.GetAudiobook(audiobook, fs);

						logger.Success($"Аудиокнига {audiobook.Name} успешно скачена. " +
							$"Сохраняем информацию в БД, в таблицу загруженных аудиокниг."
						);

						audiobook.LocalPath = $"{dirPath}/{filename}";
						audiobook.Size = fs.Length;

						// В случае успешного скачивания сохраним информацию об аудиокниге в таблицке загрузок
						await db.SaveDownloadAudiobook(audiobook);

						logger.Success("Сохранение выполнено успешно.");
					}
				}
				else
				{
					logger.Log($"Аудиокнига {audiobook.Name} существует в таблице скаченных аудиокниг, поэтому пропускаем скачивание.");
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		/// <summary>
		/// Метод распаковывает архив с аудиокнигой и запускает пофайловую передачу на Rdev,
		/// после успешной передачи файла на сервер Rdev, сохраняет запись о файле в таблицу UploadAudiofile, 
		/// после успешной передачи всей аудиокниге сохраняет в таблицу UploadAudiobook запись о книге
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		private async Task Upload(Audiobook audiobook)
		{
			try
			{
				logger.Log($"Проверяем была ли аудиокнига {audiobook.Name} полностью на Rdev.");

				// Если книга полностью отдана на Rdev, выходим из метода
				if (await db.IsUploadAudiobook(audiobook))
				{
					logger.Log($"Аудиокнига {audiobook.Name} была полностью отдана на Rdev, выходим из метода.");
					return;
				}

				logger.Log($"Получаем значение ownerrecid для аудиокниги {audiobook.Name}.");

				// Получаем идентификатор аудиокниги
				Guid ownerRecId = await db.GetOwnerRecid(audiobook);

				logger.Log($"Запускаем чтение архива с аудиокнигой по адресу {fileName}.");

				// Запускаем пофайловую передачу файлов из архива на Rdev
				using (var zip = ZipFile.OpenRead(fileFullName))
				{
					// Запишем количество аудиофайлов
					audiobook.FilesCount = zip.Entries
						.Select(x => x.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
						.Count();

					await db.UpdateAudiobook(audiobook.OriginalName, (item) =>
					{
						item.FilesCount = audiobook.FilesCount;
					});

					int chapter = 0;

					foreach (var entry in zip.Entries)
					{
						// Берем только mp3 файлы
						if (entry.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
						{
							logger.Log($"Запускаем выгрузку аудиофайла {entry.Name}.");

							// Получаем файл на отправку
							using (var fs = entry.Open())
							{
								Guid recId = Guid.NewGuid();

								// Формируем объект прдеставляющий файл для отправки на Rdev
								var file = new Audiofile()
								{
									Name = entry.Name,
									Chapter = ++chapter,
									OwnerRecid = ownerRecId.ToString(),
									AudiobookName = audiobook.Name,
									AudiobookUrl = audiobook.Url,
									AudiobookOriginalName = audiobook.OriginalName,
									Size = entry.Length
								};

								logger.Log($"Проверяем отправлялся ли аудиофайл {entry.Name} ранее на Rdev.");

								// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
								// иначе отдаем файл
								if (await db.IsUploadAudiofile(file))
								{
									logger.Log($"Аудиофайл {entry.Name} ранее был отправлен на Rdev, переходим к следующему.");
									continue;
								}

								logger.Log(
									$"Отправляем аудиофайл {entry.Name} на Rdev. Данный аудиофайл является {file.Chapter} главой аудиокниги."
								);

								// Отправляем файл на Rdev
								await client.Upload(file, fs, recId);

								logger.Success($"Аудиофайл {entry.Name} был успешно отправлен на Rdev.");
								logger.Log("Сохраняем информацию по аудиофайлу в таблицу отданных аудиофайлов.");

								// Добавляем файл в таблицу отданных файлов
								await db.SaveUploadAudiofile(file);

								logger.Success("Сохранение прошло успешно.");
							}
						}
					}

					logger.Success($"Аудиокнига {audiobook.Name} была полностью передана на Rdev.");
					logger.Log($"Сохраняем информацию об аудиокниге {audiobook.Name} в таблицу отданных на Rdev аудиокниг.");

					// Добавляем запись о полностью отданной на Rdev книге
					await db.SaveUploadAudiobook(audiobook);

					logger.Success("Сохранение прошло успешно.");
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
	}
}
