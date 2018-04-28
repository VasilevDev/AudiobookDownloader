using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
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

		public async Task Grab(Audiobook audiobook)
		{
			await DownloadAudiobook(audiobook);
			await UploadAudiobook(audiobook);
		}

		private async Task DownloadAudiobook(Audiobook audiobook)
		{
			List<DownloadAudiobook> downloadList = null;

			if (_db.DownloadAudiobook.Count() != 0)
			{
				downloadList = _db.DownloadAudiobook.Where(m => m.Audiobook.Title == audiobook.Title && m.Audiobook.Url == audiobook.Url).ToList();
			}

			if (downloadList == null || downloadList.Count == 0)
			{
				using (var fs = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite))
				{
					await _service.GetAudiobook(audiobook, fs);
					_db.DownloadAudiobook.Add(new DownloadAudiobook { Audiobook = audiobook });
					await _db.SaveChangesAsync();
				}
			}
		}

		private async Task UploadAudiobook(Audiobook audiobook)
		{
			List<UploadAudiobook> uploadList = null;

			if (_db.UploadAudiobook.Count() != 0)
			{
				uploadList = _db.UploadAudiobook.Where(m => m.Audiobook.Title == audiobook.Title && m.Audiobook.Url == audiobook.Url).ToList();
			}

			if (uploadList == null || uploadList.Count == 0)
			{
				Guid ownerRecId = Guid.NewGuid();

				_db.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
				await _db.SaveChangesAsync();

				
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
								await _client.Upload(fs, ++chapter, entry.Name, ownerRecId, recId);

								_db.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
								_db.SaveChanges();
							}
						}
					}
				}
			}
		}
	}
}
