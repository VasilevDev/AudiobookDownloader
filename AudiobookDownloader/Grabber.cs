using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
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

		private readonly Context _db;

		public Grabber(IAudiobookService service)
		{
			_service = service;
			_db = new Context();
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

		/// <summary>
		/// Метод загружает аудиокнигу с сайта, в случае успешного скачивания книги,
		/// сохраняет запись в таблицу DownloadAudiobook
		/// </summary>
		/// <param name="audiobook">Аудиокнига, которую необходимо загрузить</param>
		/// <returns></returns>
		private async Task Download(Audiobook audiobook)
		{
			Audiobook dbAudiobook = null;

			if (_db.DownloadAudiobook.Count() > 0)
			{
				dbAudiobook = _db.DownloadAudiobook.Select(m => m.Audiobook).Where(m => 
					m.Title == audiobook.Title && 
					m.Url == audiobook.Url
				).FirstOrDefault();
			}

			if (dbAudiobook == null)
			{
				using (var fs = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite))
				{
					await _service.GetAudiobook(audiobook, fs);

					_db.DownloadAudiobook.Add(new DownloadAudiobook { Audiobook = audiobook });
					await _db.SaveChangesAsync();
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
					return;
			}

			Audiofile uploadFile = null;
			Guid ownerRecId;

			// Если в таблице, отданных на Rdev файлов, имеется хотя бы одна запись о файлах из данной книги, то Ownerrecid получаем из этой записи,
			// иначе генерируем новый
			if (_db.UploadAudiofile.Count() > 0)
				uploadFile = _db.UploadAudiofile.Select(m => m.File).Where(m => m.AudiobookUrl == audiobook.Url).FirstOrDefault();

			ownerRecId = (uploadFile != null) ? Guid.Parse(uploadFile.OwnerRecid) : Guid.NewGuid();

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
							var dbFile = _db.UploadAudiofile
								.Select(m => m.File)
								.Where(m => 
									m.Name == entry.Name && 
									m.Chapter == chapter &&
									m.OwnerRecid == ownerRecId.ToString()
								).FirstOrDefault();

							if (dbFile != null)
								continue;

							await _client.Upload(file, fs, recId);

							// Добавляем файл в таблицу отданных файлов
							_db.UploadAudiofile.Add(new UploadAudiofile { File = file });
							await _db.SaveChangesAsync();
						}
					}
				}

				// Добавляем запись о полностью отданной на Rdev книге
				_db.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
				await _db.SaveChangesAsync();
			}
		}
	}
}
