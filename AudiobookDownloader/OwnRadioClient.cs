using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class OwnRadioClient
	{
		private readonly HttpClient _client;

		public OwnRadioClient()
		{
			_client = new HttpClient();
		}

		public async Task<bool> Upload(AudioBook audiobook)
		{
			foreach (var file in audiobook.Files)
			{
				Debug.Print($"Отправили файл {file.Name} с id {file.Id} размером {file.Content.Length} байт");
			}
			
			return false;
		}
	}
}
