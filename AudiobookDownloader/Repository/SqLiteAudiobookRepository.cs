using AudiobookDownloader.DatabaseContext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	class SqLiteAudiobookRepository : IAudiobookRepository
	{
		public bool CheckDownloadAudiobook(Audiobook audiobook)
		{
			using (var context = new Context())
			{
				if (context.DownloadAudiobook.Count() > 0)
				{
					var dbItem = context.DownloadAudiobook.Select(m => m.Audiobook).Where(m =>
						m.Title == audiobook.Title &&
						m.Url == audiobook.Url
					).FirstOrDefault();

					if (dbItem != null)
						return true;
				}

				return false;
			}
		}

		public bool CheckUploadAudiobook(Audiobook audiobook)
		{
			using (var context = new Context())
			{
				if (context.UploadAudiobook.Count() > 0)
				{
					var dbItem = context.UploadAudiobook.Select(m => m.Audiobook)
						.Where(m =>
							m.Title == audiobook.Title &&
							m.Url == audiobook.Url
						).FirstOrDefault();

					if (dbItem != null)
						return true;
				}

				return false;
			}
		}

		public bool CheckUploadAudiofile(Audiofile audiofile)
		{
			using (var context = new Context())
			{
				if (context.UploadAudiofile.Count() > 0)
				{
					var dbItem = context.UploadAudiofile
					.Select(m => m.File)
					.Where(m =>
						m.Name == audiofile.Name &&
						m.Chapter == audiofile.Chapter &&
						m.OwnerRecid == audiofile.OwnerRecid
					).FirstOrDefault();

					if (dbItem != null)
						return true;
				}

				return false;
			}
		}

		public Guid GetOwnerRecid(Audiobook audiobook)
		{
			using(var context = new Context())
			{
				if (context.UploadAudiofile.Count() > 0)
				{
					var dbItem = context.UploadAudiofile.Select(m => m.File).Where(m => m.AudiobookUrl == audiobook.Url).FirstOrDefault();

					if (dbItem != null)
						return Guid.Parse(dbItem.OwnerRecid);
				}

				return Guid.NewGuid();
			}
		}

		public async Task SaveDownloadAudiobook(Audiobook audiobook)
		{
			var context = new Context();

			if (!CheckDownloadAudiobook(audiobook))
			{
				context.DownloadAudiobook.Add(new DownloadAudiobook { Audiobook = audiobook });
				await context.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiobook(Audiobook audiobook)
		{
			var context = new Context();

			if (!CheckUploadAudiobook(audiobook))
			{
				context.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
				await context.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiofile(Audiofile audiofile)
		{
			var context = new Context();

			if (!CheckUploadAudiofile(audiofile))
			{
				context.UploadAudiofile.Add(new UploadAudiofile { File = audiofile });
				await context.SaveChangesAsync();
			}
		}
	}
}
