using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.Service
{
	interface IAudioBookService
	{
		Task<List<Category>> GetCategories();
		Task<List<Audiobook>> GetAudiobooks(string categoryName);

		void DownloadAudioBook(string title);
	}
}
