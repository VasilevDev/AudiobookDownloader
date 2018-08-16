using System.Data.Entity;
using SQLite.CodeFirst;

namespace AudiobookDownloader.DatabaseContext
{
	class Context : DbContext
	{
		public Context() : base(@"AudiobookDb")
		{

		}

		public DbSet<UploadAudiofile> UploadAudiofile { get; set; }

		public DbSet<UploadAudiobook> UploadAudiobook { get; set; }
		public DbSet<DownloadAudiobook> DownloadAudiobook { get; set; }

		public DbSet<DownloadedPage> DownloadedPage { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<Context>(modelBuilder);
			Database.SetInitializer(sqliteConnectionInitializer);
		}
	}
}
