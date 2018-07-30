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

		private const string _baseUrl = @"https://aobooks.zone/audiobooks";
		private readonly HtmlParser _parser = new HtmlParser();

		public async Task<List<Category>> GetCategories()
		{
			string html = await GetHtml(_baseUrl);
			var result = _parser.Parse(html);
			var categories = result.GetElementsByClassName("mfn-megamenu-title");

			var list = new List<Category>();

			foreach (var item in categories)
			{
				list.Add(new Category
				{
					Name = item.TextContent,
					Url = item.GetAttribute("href")
				});
			}

			return list;
			
		}

		public async Task<List<Audiobook>> GetAudiobooks(Category category, int page)
		{
			string html = await GetHtml($"{category.Url}/page/{page}");
			var result = _parser.Parse(html);
			var audiobooks = result.GetElementsByClassName("post-desc");

			var list = new List<Audiobook>();

			foreach (var audiobook in audiobooks)
			{
				list.Add(new Audiobook
				{
					Title = audiobook.GetElementsByClassName("entry-title")[0].TextContent,
					Url = audiobook.GetElementsByClassName("post-more")[0].GetAttribute("href")
				});
			}

			return list;
		}

		public async Task GetAudiobook(Audiobook audiobook, Stream stream)
		{
			int id = await GetAudiobookId(audiobook);

			HttpWebRequest request = (HttpWebRequest) WebRequest.Create($"https://aobooks.zone/download/{id}");
			request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream responseStream = response.GetResponseStream();

			await responseStream.CopyToAsync(stream);
		}

		public async Task<int> GetPagesCount(Category category)
		{
			string html = await GetHtml(category.Url);
			var result = _parser.Parse(html);

			var pages = result.GetElementsByClassName("page");

			int lastPage = 0;

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
			string html = await GetHtml(audiobook.Url);
			var result = _parser.Parse(html);
			string uri = result.GetElementsByClassName("button button_js button_green")[0].GetAttribute("href");
			Int32.TryParse(HttpUtility.ParseQueryString(new Uri(uri).Query).Get("book_id"), out int id);

			return id;
		}

		private async Task<string> GetHtml(string url)
		{
			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
			request.Proxy = new WebProxy(proxy.Ip, proxy.Port);

			HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
			var stream = response.GetResponseStream();

			StreamReader reader = new StreamReader(stream);
			string html = await reader.ReadToEndAsync();

			return html;
		}
	}
}
