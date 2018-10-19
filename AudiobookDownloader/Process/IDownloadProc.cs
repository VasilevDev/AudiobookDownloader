using System.Threading.Tasks;

namespace AudiobookDownloader.Process
{
	interface IDownloadProc
	{
		Task DownloadAsync();
	}
}
