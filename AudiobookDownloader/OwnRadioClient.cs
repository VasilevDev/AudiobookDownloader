using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
			_client = new HttpClient
			{
				Timeout = TimeSpan.FromMinutes(30)
			};
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
			byte[] bytes = null;

			// Выгружаем файл в память
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}

			// Если файл представляет собой первую главу, то значению recid устанавливаем значение ownerrecid
			if (file.Chapter == 1)
				recid = Guid.Parse(file.OwnerRecid);

			// Устанавливаем js метод, который будет запущен на стороне Rdev
			var request = new UploadFileDto() {
				Method = "uploadaudiofile"
			};

			// Задаем описание для передаваемых файлов, в данной реализации передаем по одному файлу за раз
			// Содержимое файла передается в виде строки Base64 т.к файлы > 3 Мб на передачу уходит немалое время + 
			// нужно обратить внимание, чтобы после кодирования в строку регистр символов не изменялся до этапа декодирования
			var files = new List<FileDescription>()
			{
				new FileDescription()
				{
					RecId = recid,
					DeviceId = Guid.Parse("7fce47ab-4fa2-4b81-aa06-d49223442d07"),
					Recdescription = file.AudiobookName,
					Mediatype = "audiobook",
					Chapter = file.Chapter,
					Ownerrecid = Guid.Parse(file.OwnerRecid),
					LocalDevicePathUpload = file.AudiobookUrl,
					Name = $"{recid.ToString()}.mp3",		// Описывает непосредственно файл
					Content = Convert.ToBase64String(bytes),// Описывает непосредственно файл
					Size = bytes.Length / 1024
				}
			};

			// Устанавливаем описание файлов в json объект fields
			request.Fields = new FilesItemDto() { Files = files };

			string json = JsonConvert.SerializeObject(request);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			// Выполняем запрос на Rdev
			using (var response = await _client.PostAsync(_connection, content).ConfigureAwait(false))
			{
				var res = response.Content.ReadAsStringAsync();
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
			JObject request = new JObject
			{
				{ "method", "downloadaudiobook" },
				{
					"fields",
					new JObject() {
					{"url", url},
					{"recname", name},
					{"deviceid", "7fce47ab-4fa2-4b81-aa06-d49223442d07"},
					{"downloadurl", downloadUrl}
				}
				}
			};

			var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

			using (var response = await _client.PostAsync(_connection, content).ConfigureAwait(false))
			{
				return response.StatusCode;
			}
		}
	}
}
