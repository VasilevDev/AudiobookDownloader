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
		private readonly AbooksService _aService;

		public Form1()
		{
			InitializeComponent();
			_aService = new AbooksService();
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			var _categories = await _aService.GetCategories();

			if(_categories == null)
			{
				label.Text = "Ошибка подключения к Abooks порталу";
				return;
			}

			label.Text = "Категории";
			ListOfCategories.Items.Clear();

			foreach (var item in _categories)
			{
				ListOfCategories.Items.Add(item.Name);
			}
		}

		private async void ListOfCategories_DoubleClick(object sender, EventArgs e)
		{
			if (label.Text == "Книги")
			{
				_aService.DownloadAudioBook(ListOfCategories.SelectedItem.ToString());
			}
			else
			{
				var _books = await _aService.GetAudiobooks(ListOfCategories.SelectedItem.ToString());

				if (_books == null)
				{
					label.Text = "Ошибка получения книг";
					return;
				}

				label.Text = "Книги";
				ListOfCategories.Items.Clear();

				foreach (var item in _books)
				{
					ListOfCategories.Items.Add(item.Title);
				}
			}
		}
	}
}
