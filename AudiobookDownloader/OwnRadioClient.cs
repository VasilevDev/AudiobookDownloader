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

		public async Task<HttpStatusCode> Upload(int bookId, AudioFile audioFile)
		{
			var form = new MultipartFormDataContent {
				{ new StringContent(bookId.ToString()), "bookid" },
				{ new StringContent(audioFile.Name), "recname" },
				{ new StringContent(audioFile.Id.ToString()), "fileid" }, //TODO
				{ new ByteArrayContent(audioFile.Content, 0, audioFile.Content.Length), "audiobookFile", audioFile.Name }
			};

			/*
					"tablename":"tracks",
					"method": "upload",
					"params": {
						"bookid": bookId.ToString(),
						"recname": audioFile.Name,
						"fileId": audioFile.Id.ToString(),
						"content": file
					}
			*/

			using (var response = await _client.PostAsync($"http://localhost:55607/api/execute/js", form).ConfigureAwait(false)) //TODO
			{
				return response.StatusCode;
			}
		}
	}
}
