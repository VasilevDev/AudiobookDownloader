﻿using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;

namespace AudiobookDownloader.Core
{
	/// <summary>
	/// Класс для парсинга HTML разметки с сайта https://1abooks.zone
	/// </summary>
	class AbooksParser
	{
		private readonly HtmlParser parser;

		public AbooksParser()
		{
			parser = new HtmlParser();
		}

		#region Получение ссылки на скачивание трека
		/// <summary>
		/// Метод нахождения в разметке ссылки на трек с определенным id
		/// </summary>
		/// <param name="htmlDocument">Разметка</param>
		/// <returns>Ссылка на трек</returns>
		public IElement AudiobookIdParse(string htmlDocument)
		{
			return Parse(htmlDocument, "button button_js button_green")[0];
		}
		#endregion

		#region Получение списка категорий
		/// <summary>
		/// Метод нахождения категорий в HTML разметке
		/// </summary>
		/// <param name="htmlDocument">Разметка</param>
		/// <returns>Список категорий</returns>
		public Dictionary<string, Category> CategoriesParse(string htmlDocument)
		{
			Dictionary<string, Category> _categories = new Dictionary<string, Category>();
			var listOfCategory = Parse(htmlDocument, "mfn-megamenu-title");

			foreach (var item in listOfCategory)
			{
				var category = new Category
				{
					Name = item.TextContent,
					Url = item.GetAttribute("href")
				};

				if(!_categories.ContainsKey(category.Name))
					_categories.Add(category.Name, category);
			}

			return _categories;
		}
		#endregion

		#region Получение списка книг
		/// <summary>
		/// Метод нахождения аудиокниг в HTML разметке
		/// </summary>
		/// <param name="htmlDocument">Разметка</param>
		/// <returns>Список аудиокниг</returns>
		public List<AudioBook> AudiobooksParse(string htmlDocument)
		{
			List<AudioBook> _books = new List<AudioBook>();
			var listOfBooks = Parse(htmlDocument, "post-desc");

			foreach (var item in listOfBooks)
			{
				var book = new AudioBook
				{
					Title = item.GetElementsByClassName("entry-title")[0].TextContent,
					Url = item.GetElementsByClassName("post-more")[0].GetAttribute("href")
				};

				_books.Add(book);
			}

			return _books;
		} 
		#endregion

		public int GetLastPageNumber(string htmlDocument)
		{
			var pages = Parse(htmlDocument, "page");
			int lastPage = 1;

			foreach (var page in pages)
			{
				if (page.TextContent == "...")
					continue;

				int pageNumber = Convert.ToInt32(page.TextContent);
				if (pageNumber > lastPage)
				{
					lastPage = pageNumber;
				}
			}

			return lastPage;
		}

		private IHtmlCollection<IElement> Parse(string htmlDocument, string selector)
		{
			var result = parser.Parse(htmlDocument);
			return result.GetElementsByClassName(selector);
		}
	}
}
