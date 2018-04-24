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
		private readonly OwnRadioClient _radio;

		public AbookUploadHandler()
		{
			_repos = new AudioBookRepository();
			_downloader = new Downloader();
			_radio = new OwnRadioClient();
		}

		public async void Upload(AudioBook audiobook)
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

			audiobook.Files = UnpackArchive("Archives.zip", $"Audiobooks/{audiobook.Title}");

			UploadAudioBook(audiobook);
		}

		private List<AudioFile> UnpackArchive(string src, string dest)
		{
			List<AudioFile> audioFiles = new List<AudioFile>();

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

			DirectoryInfo di = new DirectoryInfo(dest);
			var files = di.GetFiles("*.mp3");
			int count = 0;

			foreach (var file in files)
			{
				audioFiles.Add(new AudioFile
				{
					Id = ++count,
					Name = file.Name,
					Content = File.ReadAllBytes($"{dest}/{file.Name}")
				});
			}

			return audioFiles;
		}

		private async void UploadAudioBook(AudioBook audiobook)
		{
			await _radio.Upload(audiobook);
		}
	}
}
