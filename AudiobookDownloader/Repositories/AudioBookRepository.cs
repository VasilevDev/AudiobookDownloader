using Npgsql;
using System.Configuration;

namespace AudiobookDownloader.Repositories
{
	class AudioBookRepository
	{
		private readonly string _connectionString;

		public AudioBookRepository()
		{
			_connectionString = ConfigurationManager.ConnectionStrings["OwnRadio"].ConnectionString;
		}

		public void Add(AudioBook book)
		{
			using(var connection = new NpgsqlConnection(_connectionString))
			{
				string textCmd = $"INSERT INTO OwnRadioRdev (audiobookid, title, type,path, filecount) = ({book.Id},'{book.Title}','{"audiobook"}',{book.Url},{5})";

				using(var cmd = new NpgsqlCommand(textCmd))
				{
						//TODO:
				}
			}
		}

		public AudioBook Get(int id)
		{
			return null;
		}
	}
}
