using AudiobookDownloader.Entity;
using System;
using System.Threading.Tasks;

namespace AudiobookDownloader.Repository
{
	interface IAudiobookRepository
	{
		/// <summary>
		/// Метод проверяет была ли загружена аудиокнига
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task<bool> IsDownloadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод проверяет была ли выгружена аудиокнига
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task<bool> IsUploadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод проверяется был ли выгружен аудиофайл
		/// </summary>
		/// <param name="audiofile"></param>
		/// <returns></returns>
		Task<bool> IsUploadAudiofile(Audiofile audiofile);

		/// <summary>
		/// Метод добавляет/обновляет аудиокнигу, устанавливая флаг IsFullDownloaded = true
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task SaveDownloadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод добавляет/обновляет аудиокнигу, устанавливая флаг IsFullUploaded = true
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task SaveUploadAudiobook(Audiobook audiobook);

		/// <summary>
		/// Метод добавляет/обновляет аудиофайл, устанавливая флаг IsFullUploaded = true
		/// </summary>
		/// <param name="audiofile"></param>
		/// <returns></returns>
		Task SaveUploadAudiofile(Audiofile audiofile);

		/// <summary>
		/// Метод возвращает значение OwnerRecId
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task<Guid> GetOwnerRecid(Audiobook audiobook);

		/// <summary>
		/// Метод обновления записи об аудиокниге
		/// </summary>
		/// <param name="originName"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		Task UpdateAudiobook(string originName, Action<Audiobook> action);
	}
}
