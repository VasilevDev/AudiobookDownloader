using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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

		public async Task<HttpStatusCode> Upload(Stream stream, int chapter, string fileName, Guid ownerRecId, Guid recId)
		{
			JObject request = new JObject();
			byte[] bytes = null;

			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}

			if (chapter == 1)
				recId = ownerRecId;

			request.Add("tablename", "tracks");
			request.Add("method", "upload");
			request.Add("params", new JObject() {
					{"recid", recId.ToString()},
					{"mediatype", "audiobook"},
					{"chapter", chapter.ToString()},
					{"recname", fileName},
					{"ownerrecid", ownerRecId.ToString()},
					{"content", Convert.ToBase64String(bytes)}
				}
			);

			var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

			using (var response = await _client.PostAsync($"http://localhost:5001/api/executejs", content).ConfigureAwait(false))
			{
				return response.StatusCode;
			}
		}
	}
}
