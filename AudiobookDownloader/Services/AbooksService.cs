using AudiobookDownloader.Core;
using System.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AudiobookDownloader.Handlers;

namespace AudiobookDownloader.Service
{
	/// <summary>
	/// Класс сервиса для скачивания аудиокниг с сайта https://1abooks.zone и их последующей загрузке на сервер Rdev
	/// </summary>
	internal class AbooksService : IAudioBookService
	{
		private readonly Downloader _downloader;
		private readonly AbooksParser _parser;
		private readonly AbookUploadHandler _handler;

		private Dictionary<string, Category> _categories;

		private readonly string _baseUrl;

		public AbooksService()
		{
			_downloader = new Downloader();
			_parser = new AbooksParser();
			_handler = new AbookUploadHandler();

			_baseUrl = "https://1abooks.zone/audiobooks";
		}

		/// <summary>
		/// Метод получения списка категорий
		/// </summary>
		/// <returns>Список названий категорий</returns>
		public async Task<List<string>> GetCategories()
		{
			string homePage = await _downloader.DownloadHtml(_baseUrl);
			_categories = _parser.CategoriesParse(homePage);

			return new List<string>(_categories.Keys);
		}

		/// <summary>
		/// Метод загрузки аудиокниг
		/// </summary>
		/// <param name="categoryName">Название категории из которой загружаются аудиокниги</param>
		public async Task<bool> DownloadAudioBooks(string categoryName)
		{
			Category category = _categories[categoryName];

			if (category == null)
				return false;

			string pageOfCategory = await _downloader.DownloadHtml(category.Url);
			int countPages = _parser.GetLastPageNumber(pageOfCategory);

			for (int i = 1; i <= countPages; i++)
			{
				string page = await _downloader.DownloadHtml($"{category.Url}/page/{i}");
				var books = _parser.AudiobooksParse(page);

				UploadAudioBooks(books);
			}

			return true;
		}

		private async void UploadAudioBooks(List<AudioBook> audioBooks)
		{
			foreach (var audioBook in audioBooks)
			{
				string pageOfAudiobook = await _downloader.DownloadHtml(audioBook.Url);
				var linkByDownload = _parser.AudiobookIdParse(pageOfAudiobook);

				string id = HttpUtility.ParseQueryString(new Uri(linkByDownload.GetAttribute("href")).Query).Get("book_id");

				audioBook.Id = Convert.ToInt32(id);
				_handler.Upload(audioBook);
			}
		}
	}
}
