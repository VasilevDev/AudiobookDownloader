using AudiobookDownloader.Logging;
using AudiobookDownloader.Service;
using System.Threading.Tasks;

namespace AudiobookDownloader.Process
{
	public class DownloadAndUploadProc : IDownloadProc
	{
		private readonly IAudiobookService service;
		private readonly ICustomLogger logger;
		private readonly Grabber grabber;
		private readonly string baseUrl;

		public DownloadAndUploadProc(IAudiobookService service, ICustomLogger logger, Grabber grabber, string baseUrl)
		{
			this.service = service;
			this.logger = logger;
			this.grabber = grabber;
			this.baseUrl = baseUrl;
		}

		public async Task DownloadAsync()
		{
			// Количество скаченных со страницы аудиокниг
			int countDownloaded = 0;

			// Формируем объект категории "Новинки"
			var novelty = new Category { Name = "Новинки", Url = baseUrl };

			logger.Debug("Получаем общее количество страниц в категории Новинки.");

			// Получаем общее количество страниц на сайте в категории "Новинки"
			int countPage = await service.GetPagesCount(novelty);

			// Запускаем цикл обхода страниц с книгам начиная с конца (самые новые книги находятся на 1 странице)
			for (int page = countPage; page >= 1; page--)
			{
				logger.Debug($"Получаем количество аудиокниг со страницы {page}.");

				// Получаем  список аудиокниг со страницы
				var audiobooks = await service.GetAudiobooks(novelty, page);

				logger.Debug($"Количество аудиокниг на странице {page}: {audiobooks.Count}.");

				// Запускаем цикл на последовательное скачивание аудиокниг со страницы
				foreach (var audiobook in audiobooks)
				{
					logger.Log($"Загружаем аудиокнигу: {audiobook.Name}.");
					//Получим id чтобы убедиться что книга не удалена
					var id = await service.GetAudiobookId(audiobook);

					if (id == -1)
					{
						logger.Warning($"Аудиокнига {audiobook.Name} была удалена с сайта, пытаемся получить следующую.");
						continue;
					}

					await grabber.Grab(audiobook);
					++countDownloaded;

					logger.Success($"Аудиокнига {audiobook.Name} загружена, " +
						$"оставшееся количество аудиокниг на странице {audiobooks.Count - countDownloaded}."
					);
				}

				countDownloaded = 0;
			}
		}
	}
}
