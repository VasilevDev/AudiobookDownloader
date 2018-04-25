using System.Data.Entity;
using SQLite.CodeFirst;

namespace AudiobookDownloader.DatabaseContext
{
	class SQLiteContext : DbContext
	{
		public SQLiteContext()
			: base(@"AudioFileDb")
		{

		}

		public DbSet<AudioBook> Audiobooks { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SQLiteContext>(modelBuilder);
			Database.SetInitializer(sqliteConnectionInitializer);
		}
	}
}
