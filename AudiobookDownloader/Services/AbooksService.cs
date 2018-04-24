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
	/// Класс сервиса для скачивания аудиокниг с сайта https://1abooks.zone
	/// </summary>
	internal class AbooksService : IAudioBookService
	{
		private readonly Downloader _downloader;
		private readonly AbooksParser _parser;
		private readonly AbookUploadHandler _handler;

		private List<Category> _categories;
		private List<AudioBook> _books;

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
		/// <returns></returns>
		public async Task<List<Category>> GetCategories()
		{
			string homePage = await _downloader.DownloadHtml(_baseUrl);
			_categories = _parser.CategoriesParse(homePage);

			return _categories;
		}

		/// <summary>
		/// Метод получения списка аудиокниг
		/// </summary>
		/// <param name="categoryName">Название категории из которой получаем аудиокниги</param>
		/// <returns>Список аудиокниг</returns>
		public async Task<List<AudioBook>> GetAudiobooks(string categoryName)
		{
			Category category = null;

			foreach (var item in _categories)
			{
				if (item.Name == categoryName)
				{
					category = item;
					break;
				}
			}

			if (category == null)
				return null;

			string pageOfCategory = await _downloader.DownloadHtml(category.Url);
			_books = _parser.AudiobooksParse(pageOfCategory);

			return _books;
		}

		// Метод скачивания аудиокниги по ее названию
		public async void DownloadAudioBook(string title)
		{
			AudioBook audiobook = null;

			foreach (var item in _books)
			{
				if(item.Title == title)
				{
					audiobook = item;
					break;
				}
			}

			if (audiobook == null)
				return;

			string pageOfAudiobook = await _downloader.DownloadHtml(audiobook.Url);
			var linkByDownload = _parser.AudiobookIdParse(pageOfAudiobook);

			Uri uri = new Uri(linkByDownload.GetAttribute("href"));
			string id = HttpUtility.ParseQueryString(uri.Query).Get("book_id");

			audiobook.Id = Convert.ToInt32(id);

			_handler.Upload(audiobook);
		}
	}
}
