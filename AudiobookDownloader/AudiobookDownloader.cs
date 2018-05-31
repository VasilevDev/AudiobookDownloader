using AudiobookDownloader.DatabaseContext;
using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudiobookDownloader
{
	public partial class AudiobookDownloader : Form
	{
		private readonly IAudiobookService _service;
		private readonly Grabber _grabber;
		private readonly Context _db;

		public AudiobookDownloader()
		{
			InitializeComponent();

			_service = new AbooksService();
			_grabber = new Grabber(_service);
			_db = new Context();
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = "https://aubooks.zone" };
				int countPage = await _service.GetPagesCount(novelty);

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						await _grabber.Grab(audiobook);
						log.Items.Add($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch(Exception ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.Message}");
			}
		}

		private async void rdevLoad_ClickAsync(object sender, EventArgs e)
		{
			try
			{
				var novelty = new Category { Name = "Новинки", Url = "https://aubooks.zone" };

				int countPage = await _service.GetPagesCount(novelty);
				var client = new OwnRadioClient();

				label.Text = $"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.";

				for (int page = countPage; page >= 1; page--)
				{
					var audiobooks = await _service.GetAudiobooks(novelty, page);

					log.Items.Add($"Страница {page}, количество книг на странице {audiobooks.Count}.");

					foreach (var audiobook in audiobooks)
					{
						int audiobookId = await _service.GetAudiobookId(audiobook);

						log.Items.Add($"Попытка загрузить аудиокнигу {audiobook.Title}.");

						var result = await client.StartDownload(
							audiobook.Title, 
							audiobook.Url, 
							$"https://aubooks.zone/download/{audiobookId}"
						);

						if(result == System.Net.HttpStatusCode.BadRequest || result == System.Net.HttpStatusCode.NotFound)
							log.Items.Add($"При попытке запустить скачивание {audiobook.Title} возникла ошибка.");

						log.Items.Add($"Книга {audiobook.Title} загружена.");
					}
				}
			}
			catch (Exception ex)
			{
				log.Items.Add($"Необработанная ошибка: {ex.Message}");
			}
		}
	}
}
