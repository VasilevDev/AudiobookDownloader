using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace AudiobookDownloader
{
	public partial class Form1 : Form
	{
		private readonly IAudiobookService _service = new AbooksService();
		private readonly Grabber _grabber;
		private readonly Context _db;

		private List<Category> _categories = new List<Category>();
		private List<Audiobook> _audiobooks = new List<Audiobook>();

		public Form1()
		{
			InitializeComponent();
			_grabber = new Grabber(_service);
			_db = new Context();
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			label.Text = "Загрузка аудиокниг запущена";

			Category novelty = _service.GetNovelty();
			int currentPage = 0;

			DownloadedPage downloadedPage = new DownloadedPage() { Count = 0 };
			int countPagesByPortal = await _service.GetPagesCount(novelty);

			if (_db.DownloadedPage.Count() == 0)
			{
				currentPage = countPagesByPortal;
				_db.DownloadedPage.Add(downloadedPage);
				await _db.SaveChangesAsync();
			}
			else
			{
				int countDownloadedPages = _db.DownloadedPage.Select(m => new { m.Count }).ToList()[0].Count;
				currentPage = countPagesByPortal - countDownloadedPages;
			}

			int counter = 0;

			for (int page = currentPage; page >= 1; page--)
			{
				_audiobooks = await _service.GetAudiobooks(novelty, page);

				foreach (var audiobook in _audiobooks)
				{
					await _grabber.Grab(audiobook);
					ListOfCategories.Items.Add($"Загружена книга {audiobook.Title} со страницы {page}");
				}

				int countDownloadedPages = _db.DownloadedPage.Select(m => new { m.Count }).ToList()[0].Count;
				downloadedPage.Count = countDownloadedPages + ++counter;

				_db.Entry(downloadedPage).State = EntityState.Modified;
				await _db.SaveChangesAsync();
			}
		}
	}
}
