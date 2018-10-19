using AudiobookDownloader.Logging;
using AudiobookDownloader.Repository;
using AudiobookDownloader.Service;
using System.Threading.Tasks;

namespace AudiobookDownloader.Process
{
	public class RdevDownloadProc : IDownloadProc
	{
		private readonly IAudiobookService service;
		private readonly IAudiobookRepository db;
		private readonly ICustomLogger logger;
		private readonly OwnRadioClient client;
		private readonly string baseUrl;

		public RdevDownloadProc(IAudiobookService service, IAudiobookRepository db
			, ICustomLogger logger
			, OwnRadioClient client
			, string baseUrl
		)
		{
			this.service = service;
			this.db = db;
			this.logger = logger;
			this.client = client;
			this.baseUrl = baseUrl;
		}

		public async Task DownloadAsync()
		{
			var novelty = new Category { Name = "Новинки", Url = baseUrl };

			int countPage = await service.GetPagesCount(novelty);

			logger.Debug($"Запущена загрузка аудиокнги с сайта {novelty.Url}, количество страниц {countPage}.");

			for (int page = countPage; page >= 1; page--)
			{
				var audiobooks = await service.GetAudiobooks(novelty, page);

				logger.Debug($"Страница {page}, количество книг на странице {audiobooks.Count}.");

				foreach (var audiobook in audiobooks)
				{
					int audiobookId = await service.GetAudiobookId(audiobook);

					// Если книга была удалена с сайта
					if (audiobookId == -1)
					{
						logger.Warning($"Аудиокнига {audiobook.Name} была удалена с сайта, пытаемся получить следующую.");
						continue;
					}

					audiobook.DownloadUrl = $"{baseUrl}/download/{audiobookId}";

					logger.Debug($"Попытка загрузить аудиокнигу {audiobook.Name}.");

					await client.RunRdevDownload($"{baseUrl}/download/{audiobookId}");

					logger.Success($"Книга {audiobook.Name} загружена.");
				}
			}
		}
	}
}
