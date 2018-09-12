using AudiobookDownloader.DatabaseContext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	class SqLiteAudiobookRepository : IAudiobookRepository
	{
		public bool IsDownloadAudiobook(Audiobook audiobook)
		{
			using (var context = new Context())
			{
				if (context.DownloadAudiobook.Count() > 0)
				{
					var dbItem = context.DownloadAudiobook.Select(m => m.Audiobook).Where(m =>
						m.Title == audiobook.Title &&
						m.Url == audiobook.Url
					).FirstOrDefault();

					return (dbItem != null);
				}

				return false;
			}
		}

		public bool IsUploadAudiobook(Audiobook audiobook)
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

					return (dbItem != null);
				}

				return false;
			}
		}

		public bool IsUploadAudiofile(Audiofile audiofile)
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

					return (dbItem != null);
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

			if (!IsDownloadAudiobook(audiobook))
			{
				context.DownloadAudiobook.Add(new DownloadAudiobook { Audiobook = audiobook });
				await context.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiobook(Audiobook audiobook)
		{
			var context = new Context();

			if (!IsUploadAudiobook(audiobook))
			{
				context.UploadAudiobook.Add(new UploadAudiobook { Audiobook = audiobook });
				await context.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiofile(Audiofile audiofile)
		{
			var context = new Context();

			if (!IsUploadAudiofile(audiofile))
			{
				context.UploadAudiofile.Add(new UploadAudiofile { File = audiofile });
				await context.SaveChangesAsync();
			}
		}
	}
}
