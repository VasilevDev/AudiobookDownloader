using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Entity;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	class SqLiteAudiobookRepository : IAudiobookRepository
	{
		public async Task<bool> IsDownloadAudiobook(Audiobook audiobook)
		{
			using (var db = new Context())
			{
				if (await db.Audiobooks.CountAsync() <= 0)
					return false;

				var dbItem = await db.Audiobooks
					.FirstOrDefaultAsync(x => x.OriginalName == audiobook.OriginalName && x.IsFullDownloaded);
				return dbItem != null;
			}
		}

		public async Task<bool> IsUploadAudiobook(Audiobook audiobook)
		{
			using (var db = new Context())
			{
				if (await db.Audiobooks.CountAsync() <= 0)
					return false;

				var dbItem = await db.Audiobooks
					.FirstOrDefaultAsync(x => x.OriginalName == audiobook.OriginalName && x.IsFullUploaded);
				return dbItem != null;
			}
		}

		public async Task<bool> IsUploadAudiofile(Audiofile audiofile)
		{
			using (var db = new Context())
			{
				if (await db.Audiofiles.CountAsync() <= 0)
					return false;

				var dbItem = await db.Audiofiles.FirstOrDefaultAsync(x => 
					x.Name == audiofile.Name && 
					x.Chapter == audiofile.Chapter && 
					x.OwnerRecid == audiofile.OwnerRecid &&
					x.IsFullUploaded
				);

				return dbItem != null;
			}
		}

		public async Task<Guid> GetOwnerRecid(Audiobook audiobook)
		{
			using(var db = new Context())
			{
				if (await db.Audiofiles.CountAsync() <= 0)
					return Guid.NewGuid();

				var dbItem = await db.Audiofiles.FirstOrDefaultAsync(x => x.AudiobookOriginalName == audiobook.OriginalName);
				return dbItem != null ? Guid.Parse(dbItem.OwnerRecid) : Guid.NewGuid();
			}
		}

		public async Task SaveDownloadAudiobook(Audiobook audiobook)
		{
			using(var db = new Context())
			{
				if(await db.Audiobooks.CountAsync() <= 0)
				{
					audiobook.Created = DateTime.Now;
					audiobook.IsFullDownloaded = true;

					db.Audiobooks.Add(audiobook);
				}
				else
				{
					var dbItem = await db.Audiobooks
						.FirstOrDefaultAsync(x => x.OriginalName == audiobook.OriginalName);

					if (dbItem == null)
					{
						audiobook.Created = DateTime.Now;
						audiobook.IsFullDownloaded = true;

						db.Audiobooks.Add(audiobook);
					}
					else
					{
						dbItem.Updated = DateTime.Now;
						dbItem.IsFullDownloaded = true;
					}
				}

				await db.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiobook(Audiobook audiobook)
		{
			using(var db = new Context())
			{
				if(await db.Audiobooks.CountAsync() <= 0)
				{
					audiobook.Updated = DateTime.Now;
					audiobook.IsFullUploaded = true;

					db.Audiobooks.Add(audiobook);
				}
				else
				{
					var dbItem = await db.Audiobooks.FirstOrDefaultAsync(x => x.OriginalName == audiobook.OriginalName);

					if (dbItem == null)
					{
						audiobook.Updated = DateTime.Now;
						audiobook.IsFullUploaded = true;

						db.Audiobooks.Add(audiobook);
					}
					else
					{
						dbItem.Updated = DateTime.Now;
						dbItem.IsFullUploaded = true;
					}
				}

				await db.SaveChangesAsync();
			}
		}

		public async Task SaveUploadAudiofile(Audiofile audiofile)
		{
			using(var db = new Context())
			{
				if(await db.Audiofiles.CountAsync() <= 0)
				{
					audiofile.Created = DateTime.Now;
					audiofile.Updated = DateTime.Now;
					audiofile.IsFullUploaded = true;

					db.Audiofiles.Add(audiofile);
				}
				else
				{
					var dbItem = await db.Audiofiles.FirstOrDefaultAsync(x =>
						x.Name == audiofile.Name &&
						x.Chapter == audiofile.Chapter &&
						x.OwnerRecid == audiofile.OwnerRecid
					);

					if(dbItem == null)
					{
						audiofile.Created = DateTime.Now;
						audiofile.Updated = DateTime.Now;
						audiofile.IsFullUploaded = true;

						db.Audiofiles.Add(audiofile);
					}
					else
					{
						dbItem.IsFullUploaded = true;
						dbItem.Updated = DateTime.Now;
					}
				}

				await db.SaveChangesAsync();
			}
		}

		public async Task UpdateAudiobook(string originName, Action<Audiobook> action)
		{
			if (string.IsNullOrEmpty(originName))
				throw new ArgumentNullException("Отсутствует значение ключа при обновлении записи об аудиокниге.");

			if (action == null)
				throw new ArgumentNullException("Отсутствует ссылка на действие для обновления записи об аудиокниге.");

			using(var db = new Context())
			{
				var dbItem = await db.Audiobooks.FirstOrDefaultAsync(x => x.OriginalName == originName);

				if (dbItem == null)
					throw new Exception("Не удалось найти запись об аудиокниге для обновления.");

				action(dbItem);
				dbItem.Updated = DateTime.Now;

				await db.SaveChangesAsync();
			}
		}
	}
}
