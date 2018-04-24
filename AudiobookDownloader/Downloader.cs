using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	class Downloader
	{
		HttpClient _client;

		public Downloader()
		{
			_client = new HttpClient();
		}

		public async Task<string> DownloadHtml(string url)
		{
			var response = await _client.GetAsync(url).ConfigureAwait(false);
			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}

		public async Task<Stream> DownloadFile(string url)
		{
			var response = await _client.GetAsync(url).ConfigureAwait(false);
			return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		}
	}
}
