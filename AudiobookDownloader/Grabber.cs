using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Repository;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class Grabber
	{
		private readonly IAudiobookService _service;
		private readonly OwnRadioClient _client = new OwnRadioClient();
		private const string _filename = "tmp.zip";
		private readonly string dirPath = ConfigurationManager.AppSettings["Audiobooks"];

		private readonly IAudiobookRepository _db;

		public Grabber(IAudiobookService service)
		{
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);

			_service = service;
			_db = new SqLiteAudiobookRepository();
		}

		/// <summary>
		/// Метод скачивает архив с аудиокнигой и передает по файлам на сервер Rdev
		/// </summary>
		/// <param name="audiobook">Аудиокнига которую необходимо загрузить</param>
		/// <returns></returns>
		public async Task Grab(Audiobook audiobook)
		{
			// Авторизуемся на сервере Rdev
			await _client.Authorize();

			// Скачиваем аудиокнигу в локальную директорию
			await Download(audiobook);

			// Выгружаем аудиокнигу на Rdev из локальной директроии
			await Upload(audiobook);
		}

		public async Task GrabLocal(Audiobook audiobook)
		{
			await Download(audiobook, $"{audiobook.Title}.zip");
		}

		/// <summary>
		/// Метод загружает аудиокнигу с сайта, в случае успешного скачивания книги,
		/// сохраняет запись в таблицу DownloadAudiobook
		/// </summary>
		/// <param name="audiobook">Аудиокнига, которую необходимо загрузить</param>
		/// <returns></returns>
		private async Task Download(Audiobook audiobook, string filename = _filename)
		{
			try
			{
				// Проверяем не была ли загружена аудиокнига
				bool isDownload = _db.IsDownloadAudiobook(audiobook);

				// Если нет, скачиваем
				if (!isDownload)
				{
					using (var fs = new FileStream($"{dirPath}/{filename}", FileMode.Create, FileAccess.ReadWrite))
					{
						await _service.GetAudiobook(audiobook, fs);

						// В случае успешного скачивания сохраним информацию об аудиокниге в таблицке загрузок
						await _db.SaveDownloadAudiobook(audiobook);
					}
				}
			}
			catch(Exception ex)
			{
				throw new Exception($"Необработанная ошибка при загрузке аудиокниги: {audiobook.Title}: {ex.Message}.");
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
				// Если книга полностью отдана на Rdev, выходим из метода
				if (_db.IsUploadAudiobook(audiobook))
					return;

				// Получаем идентификатор аудиокниги
				Guid ownerRecId = _db.GetOwnerRecid(audiobook);

				// Запускаем пофайловую передачу файлов из архива на Rdev
				using (var zip = ZipFile.OpenRead(_filename))
				{
					int chapter = 0;

					foreach (var entry in zip.Entries)
					{
						// Берем только mp3 файлы
						if (entry.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
						{
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
									AudiobookName = audiobook.Title,
									AudiobookUrl = audiobook.Url
								};

								// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
								// иначе отдаем файл
								if (_db.IsUploadAudiofile(file))
									continue;

								// Отправляем файл на Rdev
								await _client.Upload(file, fs, recId);

								// Добавляем файл в таблицу отданных файлов
								await _db.SaveUploadAudiofile(file);
							}
						}
					}

					// Добавляем запись о полностью отданной на Rdev книге
					await _db.SaveUploadAudiobook(audiobook);
				}
			}
			catch(Exception ex)
			{
				throw new Exception($"Необработанная ошибка при попытке выгрузить аудиокнигу: {audiobook.Title} на Rdev: {ex.Message}.");
			}
		}
	}
}
