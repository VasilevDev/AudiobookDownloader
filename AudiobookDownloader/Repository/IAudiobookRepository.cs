using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	interface IAudiobookRepository
	{
		/// <summary>
		/// Метод проверки была ли скачана аудиокнига
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		bool CheckDownloadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод проверки была ли аудиокнига выгружена на Rdev
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		bool CheckUploadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод проверки был ли отдан на Rdev текущий аудиофайл
		/// </summary>
		/// <param name="audiofile"></param>
		/// <returns></returns>
		bool CheckUploadAudiofile(Audiofile audiofile);

		/// <summary>
		/// Метод сохраняет информацию о скаченной аудиокниге
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task SaveDownloadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод сохраняет информацию об аудиокниге отданной на Rdev
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task SaveUploadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод сохраняет информацию об аудиофайле отданном на Rdev
		/// </summary>
		/// <param name="audiofile"></param>
		/// <returns></returns>
		Task SaveUploadAudiofile(Audiofile audiofile);

		/// <summary>
		/// Метод получения Ownerrecid для текущей аудиокниги
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Guid GetOwnerRecid(Audiobook audiobook);
	}
}
