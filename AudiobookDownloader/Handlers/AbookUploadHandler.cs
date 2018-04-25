using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace AudiobookDownloader.Handlers
{
	class AbookUploadHandler
	{
		private readonly Downloader _downloader;
		private readonly OwnRadioClient _radio;

		public AbookUploadHandler()
		{
			_downloader = new Downloader();
			_radio = new OwnRadioClient();
		}

		public async Task<bool> Upload(AudioBook audiobook)
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

			return await UploadAudioBook(audiobook);
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

		private async Task<bool> UploadAudioBook(AudioBook audiobook)
		{
			foreach (var file in audiobook.Files)
			{
				var res = await _radio.Upload(audiobook.Id, file);
				if(res == System.Net.HttpStatusCode.Created)
				{
					//TODO
				}
			}

			return false; //TODO
		}
	}
}
