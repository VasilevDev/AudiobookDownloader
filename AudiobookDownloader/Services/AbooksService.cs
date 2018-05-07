using System.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;

namespace AudiobookDownloader.Service
{
	internal class AbooksService : IAudiobookService
	{
		private const string _baseUrl = @"https://1abooks.zone/audiobooks";
		private readonly HtmlParser _parser = new HtmlParser();

		public async Task<List<Category>> GetCategories()
		{
			using (var http = new HttpClient())
			using (var response = await http.GetAsync(_baseUrl).ConfigureAwait(false))
			{
				var html = await response.Content.ReadAsStringAsync();
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
		}

		public async Task<List<Audiobook>> GetAudiobooks(Category category, int page)
		{
			using (var http = new HttpClient())
			using (var response = await http.GetAsync($"{category.Url}/page/{page}").ConfigureAwait(false))
			{
				var html = await response.Content.ReadAsStringAsync();
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
		}

		public async Task GetAudiobook(Audiobook audiobook, Stream stream)
		{
			using (var http = new HttpClient())
			{
				http.Timeout = TimeSpan.FromMinutes(60);

				using (var response = await http.GetAsync($"{audiobook.Url}").ConfigureAwait(false))
				{
					var html = await response.Content.ReadAsStringAsync();
					var result = _parser.Parse(html);
					string uri = result.GetElementsByClassName("button button_js button_green")[0].GetAttribute("href");
					string id = HttpUtility.ParseQueryString(new Uri(uri).Query).Get("book_id");

					using (var book = await http.GetAsync($"https://1abooks.zone/download/{id}").ConfigureAwait(false))
					using (var bs = await book.Content.ReadAsStreamAsync())
					{
						bs.CopyTo(stream);
					}
				}
			}
		}

		public async Task<int> GetPagesCount(Category category)
		{
			int lastPage = 1;

			using (var http = new HttpClient())
			using (var response = await http.GetAsync(category.Url).ConfigureAwait(false))
			{
				var html = await response.Content.ReadAsStringAsync();
				var result = _parser.Parse(html);
				var pages = result.GetElementsByClassName("page");

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
			}

			return lastPage;
		}

		public Category GetNovelty()
		{
			return new Category
			{
				Name = "Новинки",
				Url = "https://1abooks.zone"
			};
		}
	}
}
