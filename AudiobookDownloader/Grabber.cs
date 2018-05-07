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
		
		public async Task Grab(Audiobook audiobook)
		{
			await Download(audiobook);
			await Upload(audiobook);
		}

		private async Task Download(Audiobook audiobook)
		{
			List<Audiobook> downloadList = null;

			if (_db.DownloadAudiobook.Count() != 0)
			{
				downloadList = _db.DownloadAudiobook.Select(m => m.Audiobook).Where(m => 
					m.Title == audiobook.Title && 
					m.Url == audiobook.Url
				).ToList();
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

		private async Task Upload(Audiobook audiobook)
		{
			if (_db.UploadAudiobook.Count() != 0)
			{
				var dbAudiobooks = _db.UploadAudiobook.Select(m => m.Audiobook).Where(m => m.Title == audiobook.Title && m.Url == audiobook.Url).ToList();

				if (dbAudiobooks.Count > 0)
					return;
			}

			Guid ownerRecId;

			if (_db.UploadAudiofile.Count() > 0)
			{
				var files = _db.UploadAudiofile.Select(m => m.File).Where(m => m.AudiobookUrl == audiobook.Url).ToList();
				ownerRecId = (files.Count > 0) ? Guid.Parse(files[0].OwnerRecid) : Guid.NewGuid();
			}
			else
			{
				ownerRecId = Guid.NewGuid();
			}

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

							var files = _db.UploadAudiofile.Select(m => m.File).Where(m => 
								m.Name == entry.Name && 
								m.Chapter == chapter &&
								m.OwnerRecid == ownerRecId.ToString()
							).ToList();

							if (files.Count > 0)
							{
								continue;
							}

							await _client.Upload(file, fs, recId);

							_db.UploadAudiofile.Add(new UploadAudiofile { File = file });
							await _db.SaveChangesAsync();
						}
					}
				}

				_db.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
				await _db.SaveChangesAsync();
			}
		}
	}
}
