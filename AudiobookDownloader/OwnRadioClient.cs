using AudiobookDownloader.Auth;
using AudiobookDownloader.Entity;
using AudiobookDownloader.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class OwnRadioClient
	{
		private readonly HttpClient client;
		private readonly ICustomLogger logger;

		private readonly string rdevUrl = ConfigurationManager.ConnectionStrings["RdevServer"].ConnectionString;
		private readonly string authLogin = ConfigurationManager.ConnectionStrings["AuthLogin"].ConnectionString;

		private bool isAuthCompleted = false;

		public OwnRadioClient(ICustomLogger logger)
		{
			client = new HttpClient
			{
				Timeout = TimeSpan.FromMinutes(30)
			};
			this.logger = logger;
		}

		/// <summary>
		/// Метод авторизации на сервере Rdev
		/// </summary>
		/// <returns></returns>
		public async Task Authorize()
		{
			try
			{
				logger.Log("Проверяем нужно ли получать токен авторизации.");

				// Если уже авторизованы, выходим
				if (isAuthCompleted)
				{
					logger.Log("Авторизация была выполнена ранее, выходим из метода.");
					return;
				}

				logger.Log("Получаем значение логина и пароля для авторизации.");

				// Получаем значение логина из настроек
				var login = ConfigurationManager.AppSettings["UserLogin"];

				// В App.config должно быть указано значение логина
				if (string.IsNullOrEmpty(login))
					throw new Exception("В настройках отсутствует значение UserLogin.");

				// Получаем значение пароля, может быть пустым
				var password = ConfigurationManager.AppSettings["UserPassword"];

				logger.Log($"Login:{login}, Password:{password}.");

				// Формируем объект с данными для авторизации
				var content = JsonConvert.SerializeObject(new { Login = login, Password = password });
				var authContent = new StringContent(content, Encoding.UTF8, "application/json");

				logger.Log($"Отправляем запрос на авторизацию, по адресу {authLogin}, указав полученные учетные данные.");

				// Отправляем данные для авторизации, ожидаем получить токен
				using (var response = await client.PostAsync(authLogin, authContent).ConfigureAwait(false))
				{
					if (response.StatusCode != HttpStatusCode.OK)
						throw new Exception($"Сервер вернул статус код с ошибкой: {response.StatusCode}.");

					var user = await response.Content.ReadAsStringAsync();

					if (string.IsNullOrEmpty(user))
						throw new Exception("Не удалось получить информацию о пользователе.");

					var userObj = JsonConvert.DeserializeObject<User>(user);

					if(string.IsNullOrEmpty(userObj.Token))
						throw new Exception("Не удалось получить token авторизованного пользователя.");

					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userObj.Token);

					logger.Success("Авторизация успешно выполнена.");

					isAuthCompleted = true;
				}
			}
			catch(Exception ex)
			{
				throw new Exception($"Необработанная ошибка при попытке авторизоваться на сервере Rdev: {ex.Message}.");
			}
		}

		/// <summary>
		/// Метод отдачи файлов на Rdev
		/// </summary>
		/// <param name="file">Отдаваемый аудиофайл</param>
		/// <param name="stream">Поток с содержимым файла</param>
		/// <param name="recid">recid файла, для 1 главы значение recid совпадает с ownerrecid</param>
		/// <returns></returns>
		public async Task Upload(Audiofile file, Stream stream, Guid recid)
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
					LocalDevicePathUpload = Path.GetFileName(file.AudiobookUrl),
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
			using (var response = await client.PostAsync(rdevUrl, content).ConfigureAwait(false))
			{
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(await response.Content.ReadAsStringAsync());
			}
		}

		/// <summary>
		/// Метод запускает скачивание аудиокниги на Rdev
		/// </summary>
		/// <param name="name"></param>
		/// <param name="url"></param>
		/// <param name="downloadUrl"></param>
		/// <returns></returns>
		public async Task StartDownload(string name, string url, string downloadUrl)
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

			// Проведем авторизацию если это необходимо
			await Authorize();

			var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

			using (var response = await client.PostAsync(rdevUrl, content).ConfigureAwait(false))
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw new Exception(
						$"Ошибка при попытке запустить скачивание аудиокниги через Rdev. Сервер вернул статус код: {response.StatusCode}."
					);
				}
			}
		}

		public async Task TestRequest()
		{
			logger.Log("Выполняем тестовый запрос на Rdev.");

			// Выполняем запрос на Rdev
			using (var response = await client.PostAsync(rdevUrl, new StringContent("{\"test\": \"testValue \"}", Encoding.UTF8, "application/json")).ConfigureAwait(false))
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					logger.Warning("Тестовый запрос вернул ошибку, как и ожидалось.");
					throw new Exception($"Вернулся статус код: {response.StatusCode}.");
				}
			}
		}
	}
}
