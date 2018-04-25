using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudiobookDownloader
{
	public partial class Form1 : Form
	{
		private readonly AbooksService _abookService;

		public Form1()
		{
			InitializeComponent();
			_abookService = new AbooksService();
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			var _categories = await _abookService.GetCategories();

			if(_categories == null)
			{
				label.Text = "Ошибка подключения к Abooks порталу";
				return;
			}

			label.Text = "Категории";
			ListOfCategories.Items.Clear();

			foreach (var item in _categories)
			{
				ListOfCategories.Items.Add(item);
			}
		}

		private async void ListOfCategories_DoubleClick(object sender, EventArgs e)
		{
			bool res = await _abookService.DownloadAudioBooks(ListOfCategories.SelectedItem.ToString());

			/*var _books = await _abookService.GetAudiobooks(ListOfCategories.SelectedItem.ToString());
			string _books = null;

			if (_books == null)
			{
				label.Text = "Ошибка получения книг";
				return;
			}

			label.Text = $"Книги: {_books}";

			ListOfCategories.Items.Clear();

			foreach (var item in _books)
			{
				ListOfCategories.Items.Add(item);
			}*/
		}
	}
}
