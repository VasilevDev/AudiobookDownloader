using System.Data.Entity;
using SQLite.CodeFirst;

namespace AudiobookDownloader.DatabaseContext
{
	class Context : DbContext
	{
		public Context()
			: base(@"AudioFileDb")
		{

		}

		public DbSet<Audiobook> Audiobook { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<Context>(modelBuilder);
			Database.SetInitializer(sqliteConnectionInitializer);
		}
	}
}
