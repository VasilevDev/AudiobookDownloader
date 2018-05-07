using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
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
		private readonly string _connection = ConfigurationManager.ConnectionStrings["RdevServer"].ConnectionString;

		public OwnRadioClient()
		{
			_client = new HttpClient();
		}

		public async Task<HttpStatusCode> Upload(Audiofile file, Stream stream, Guid recid)
		{
			JObject request = new JObject();
			byte[] bytes = null;

			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}

			if (file.Chapter == 1)
				recid = Guid.Parse(file.OwnerRecid);

			request.Add("tablename", "tracks");
			request.Add("method", "upload");
			request.Add("params", new JObject() {
					{"recid", recid.ToString()},
					{"mediatype", "audiobook"},
					{"chapter", file.Chapter.ToString()},
					{"recname", $"{recid.ToString()}.mp3"},
					{"ownerrecid", file.OwnerRecid},
					{"content", Convert.ToBase64String(bytes)}
				}
			);

			var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

			using (var response = await _client.PostAsync(_connection, content).ConfigureAwait(false))
			{
				return response.StatusCode;
			}
		}
	}
}
