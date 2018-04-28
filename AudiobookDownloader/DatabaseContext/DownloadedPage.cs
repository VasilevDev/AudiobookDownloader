using System.ComponentModel.DataAnnotations;

namespace AudiobookDownloader.DatabaseContext
{
	public class DownloadedPage
	{
		public int Id { get; set; } = 1;
		public int PageNumber { get; set; }
	}
}