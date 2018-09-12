using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	interface IAudiobookRepository
	{
		//DownloadedPage
		bool IsDownloadAudiobook(Audiobook audiobook);
		bool IsUploadAudiobook(Audiobook audiobook);
		bool IsUploadAudiofile(Audiofile audiofile);

		Task SaveDownloadAudiobook(Audiobook audiobook);
		Task SaveUploadAudiobook(Audiobook audiobook);
		Task SaveUploadAudiofile(Audiofile audiofile);

		Guid GetOwnerRecid(Audiobook audiobook);
	}
}
