using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AudiobookDownloader.Service
{
	interface IAudiobookService
	{
		Task<List<Category>> GetCategories();
		Task<List<Audiobook>> GetAudiobooks(Category category, int page);
		Task GetAudiobook(Audiobook audiobook, Stream stream);
		Task<int> GetPagesCount(Category category);
		Category GetNovelty();
	}
}
