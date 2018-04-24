using AudiobookDownloader.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.Handlers
{
	class AbookUploadHandler
	{
		private readonly AudioBookRepository _repos;
		private readonly Downloader _downloader;

		public AbookUploadHandler()
		{
			_repos = new AudioBookRepository();
			_downloader = new Downloader();
		}

		public async void Upload(Audiobook audiobook)
		{
			var downloadAudiobook = await _downloader.DownloadFile($"https://1abooks.zone/download/{audiobook.Id}");

			if(downloadAudiobook == null)
			{
				return;
			}

			using (var fs = new FileStream("Archives.zip", FileMode.Create, FileAccess.ReadWrite))
			{
				downloadAudiobook.CopyTo(fs);
			}

			UnpackArchive("Archives.zip", $"Audiobooks/{audiobook.Title}");
			UploadFiles($"Audiobooks/{audiobook.Title}");
		}

		private void UnpackArchive(string src, string dest)
		{
			if (!Directory.Exists(dest))
			{
				Directory.CreateDirectory(dest);
			}
			
			using (var zip = ZipFile.OpenRead(src))
			{
				var zipContent = zip.Entries;

				foreach (var item in zipContent)
				{
					item.ExtractToFile($"{dest}/{item.Name}", true);
				}
			}
		}

		private void UploadFiles(string src)
		{
			var files = Directory.GetFiles(src);

			foreach (var file in files)
			{
				//TODO: проверить отправлялся ли данный файл

			}
		}
	}
}
