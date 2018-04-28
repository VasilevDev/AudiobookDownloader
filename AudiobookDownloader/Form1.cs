using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
			Category novelty = _service.GetNovelty();

			DownloadedPage downloadedPage = new DownloadedPage();
			downloadedPage.PageNumber = await _service.GetPagesCount(novelty);

			if (_db.DownloadedPage.Count() == 0)
			{
				_db.DownloadedPage.Add(downloadedPage);
				_db.SaveChanges();
			}
			else
			{
				var numbers = _db.DownloadedPage.Select(m => new { m.PageNumber }).ToList();

				if (numbers.Count != 1)
					return;

				downloadedPage.PageNumber = numbers[0].PageNumber;
			}

			for (int page = downloadedPage.PageNumber; page >= 1; page--)
			{
				_audiobooks = await _service.GetAudiobooks(novelty, page);

				if(downloadedPage.PageNumber != page)
				{
					downloadedPage.PageNumber = page;

					_db.Entry(downloadedPage).State = EntityState.Modified;
					_db.SaveChanges();
				}

				ListOfCategories.Items.Clear();

				foreach (var audiobook in _audiobooks)
				{
					await _grabber.Grab(audiobook);
					ListOfCategories.Items.Add($"Загружена книга {audiobook.Title} со страницы {page}");
				}
			}
		}
	}
}
