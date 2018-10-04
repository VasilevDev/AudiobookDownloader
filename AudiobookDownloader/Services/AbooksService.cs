using System.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using System.Net;
using System.Configuration;

namespace AudiobookDownloader.Service
{
	struct ProxySettings
	{
		public string Ip { get; set; }
		public int Port { get; set; }
	}

	internal class AbooksService : IAudiobookService
	{
		private ProxySettings proxy = new ProxySettings {
			Ip = ConfigurationManager.AppSettings["ProxyIp"],
			Port = Int32.Parse(ConfigurationManager.AppSettings["ProxyPort"])
		};

		private readonly string baseUrl = ConfigurationManager.AppSettings["AbookService"];
		private readonly bool isUseProxy = Boolean.Parse(ConfigurationManager.AppSettings["IsUseProxy"]);
		private readonly HtmlParser _parser = new HtmlParser();

		public async Task<List<Category>> GetCategories()
		{
			// Получаем html разметку сайта Abooks
			string html = await GetHtml(baseUrl);

			// Получаем объектно-ориентированное представление html документа
			var parseResult = _parser.Parse(html);

			// Получаем коллекцию с информацией о категориях на сайте
			var categories = parseResult.GetElementsByClassName("mfn-megamenu-title");

			var list = new List<Category>();

			// Формируем собственную коллекцию из объектно-ориентированного предтавления категории
			foreach (var category in categories)
			{
				list.Add(new Category
				{
					Name = category.TextContent,
					Url = category.GetAttribute("href")
				});
			}

			return list;
		}

		public async Task<List<Audiobook>> GetAudiobooks(Category category, int page)
		{
			// Получаем html разметку сайта Abooks, с определенной страницы указанной категории
			string html = await GetHtml($"{category.Url}/page/{page}");

			// Получаем объектно-ориентированное представление html документа
			var parseResult = _parser.Parse(html);

			// Получаем коллекцию книг со страницы
			var audiobooks = parseResult.GetElementsByClassName("card__title");

			var list = new List<Audiobook>();

			// Формируем собственную коллекцию из объектно-ориентированного предтавления аудиокниг
			foreach (var audiobook in audiobooks)
			{
				list.Add(new Audiobook
				{
					Title = audiobook.FirstElementChild.TextContent,
					Url = audiobook.FirstElementChild.GetAttribute("href")
				});
			}

			return list;
		}

		public async Task GetAudiobook(Audiobook audiobook, Stream stream)
		{
			// Получаем идентификатор книги, по которому будет произведено обращение на скачивание
			int id = await GetAudiobookId(audiobook);

			// Формируем запрос на скачивание, если необходимо используем скачивание через proxy сервер
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create($"{baseUrl}/download/{id}");
			if (isUseProxy) request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream responseStream = response.GetResponseStream();

			// Скачиваем аудиокнигу
			await responseStream.CopyToAsync(stream);
		}

		public async Task<int> GetPagesCount(Category category)
		{
			// Получаем html разметку сайта Abooks для указанной категории
			string html = await GetHtml(category.Url);

			// Получаем объектно - ориентированное представление html документа
			var parseResult = _parser.Parse(html);

			// Получаем из разметки информацию о номерах страниц
			var pages = parseResult.GetElementsByClassName("page");

			int lastPage = 0;

			// Получаем наибольшее значение страницы, которое соответствует последней странице
			foreach (var page in pages)
			{
				if (Int32.TryParse(page.TextContent, out int pageNumber))
				{
					if (pageNumber > lastPage)
					{
						lastPage = pageNumber;
					}
				}
			}

			return lastPage;
		}

		public async Task<int> GetAudiobookId(Audiobook audiobook)
		{
			// Получаем html разметку сайта Abooks c определенной аудиокнигой
			string html = await GetHtml(audiobook.Url);

			// Получаем объектно - ориентированное представление html документа
			var parseResult = _parser.Parse(html);

			// Получаем значение идентификатора указанной книги
			string uri = parseResult.GetElementsByClassName("button button_js button_orange")[0].GetAttribute("href");
			Int32.TryParse(HttpUtility.ParseQueryString(new Uri(uri).Query).Get("book_id"), out int id);

			return id;
		}

		private async Task<string> GetHtml(string url)
		{
			// Формируем запрос на обращение к сервису Abooks
			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

			// Если необходимо, обращение выполняем через прокси сервер
			if(isUseProxy) request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

			// Выполняем запрос
			HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
			var stream = response.GetResponseStream();

			// Получаем ответ (текстовое содержимое html разметки)
			StreamReader reader = new StreamReader(stream);
			string html = await reader.ReadToEndAsync();

			return html;
		}
	}
}
