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
			_client.Timeout = TimeSpan.FromMinutes(10);
		}

		/// <summary>
		/// Метод отдачи файлов на Rdev
		/// </summary>
		/// <param name="file">Отдаваемый аудиофайл</param>
		/// <param name="stream">Поток с содержимым файла</param>
		/// <param name="recid">recid файла, для 1 главы значение recid совпадает с ownerrecid</param>
		/// <returns></returns>
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

			// Формируем json содержимое для запроса с передачей аудиофайла на Rdev
			request.Add("method", "uploadaudiofile");
			request.Add("fields", new JObject() {
					{"recid", recid.ToString()},
					{"mediatype", "audiobook"},
					{"chapter", file.Chapter.ToString()},
					{"recname", $"{recid.ToString()}.mp3"},
					{"ownerrecid", file.OwnerRecid},
					{"url", file.AudiobookUrl},
					{"content", Convert.ToBase64String(bytes)}
				}
			);

			var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

			using (var response = await _client.PostAsync(_connection, content).ConfigureAwait(false))
			{
				return response.StatusCode;
			}
		}

		/// <summary>
		/// Метод запускает скачивание аудиокниги на Rdev
		/// </summary>
		/// <param name="name"></param>
		/// <param name="url"></param>
		/// <param name="downloadUrl"></param>
		/// <returns></returns>
		public async Task<HttpStatusCode> StartDownload(string name, string url, string downloadUrl)
		{
			JObject request = new JObject();

			request.Add("method", "downloadaudiobook");
			request.Add("fields", new JObject() {
					{"url", url},
					{"name", name},
					{"downloadurl", downloadUrl}
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
