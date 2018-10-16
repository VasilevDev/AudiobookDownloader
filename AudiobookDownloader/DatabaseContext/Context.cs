using System.Data.Entity;
using AudiobookDownloader.Entity;
using SQLite.CodeFirst;

namespace AudiobookDownloader.DatabaseContext
{
	class Context : DbContext
	{
		public Context()
			: base(@"AudiobookDb")
		{

		}

		public DbSet<Audiobook> Audiobooks { get; set; }
		public DbSet<Audiofile> Audiofiles { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<Context>(modelBuilder);
			Database.SetInitializer(sqliteConnectionInitializer);
		}
	}
}
