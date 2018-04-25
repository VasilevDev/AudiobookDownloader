using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.Service
{
	interface IAudioBookService
	{
		Task<List<string>> GetCategories();
		Task<bool> DownloadAudioBooks(string categoryName);
	}
}
