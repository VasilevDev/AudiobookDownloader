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
			await Download(audiobook);
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
			bool isDownload = _db.CheckDownloadAudiobook(audiobook);

			if (!isDownload)
			{
				using (var fs = new FileStream($"{dirPath}/{filename}", FileMode.Create, FileAccess.ReadWrite))
				{
					await _service.GetAudiobook(audiobook, fs);
					await _db.SaveDownloadAudiobook(audiobook);
				}
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
			bool isUpload = _db.CheckUploadAudiobook(audiobook);

			// Если книга полностью отдана на Rdev, выходим из метода
			if (isUpload)
				return;

			Guid ownerRecId = _db.GetOwnerRecid(audiobook);

			using (var zip = ZipFile.OpenRead(_filename))
			{
				int chapter = 0;

				foreach (var entry in zip.Entries)
				{
					if (entry.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
					{
						using (var fs = entry.Open())
						{
							Guid recId = Guid.NewGuid();

							var file = new Audiofile()
							{
								Name = entry.Name,
								Chapter = ++chapter,
								OwnerRecid = ownerRecId.ToString(),
								AudiobookUrl = audiobook.Url
							};

							// Проверям был ли отдан файл с таким названием и главой на Rdev, если да, переходим к следующей итерации цикла,
							// иначе отдаем файл
							bool isUploadFile = _db.CheckUploadAudiofile(file);

							if (isUploadFile)
								continue;

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
	}
}
