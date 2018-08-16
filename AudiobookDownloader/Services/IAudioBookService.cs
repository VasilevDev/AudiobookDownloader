using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AudiobookDownloader.Service
{
	interface IAudiobookService
	{
		/// <summary>
		/// Метод получает из html разметки список категорий
		/// </summary>
		/// <returns>Список объектов Category, представляющих собой название категории и ссылку</returns>
		Task<List<Category>> GetCategories();

		/// <summary>
		/// Получает из html разметки список аудиокниг
		/// </summary>
		/// <param name="category">Категория, для которой получаем список аудиокниг</param>
		/// <param name="page">Страница, с которой получаем список аудиокниг</param>
		/// <returns>Список объектов Audiobook</returns>
		Task<List<Audiobook>> GetAudiobooks(Category category, int page);

		/// <summary>
		/// Метод загружает аудиокнигу с сайта
		/// </summary>
		/// <param name="audiobook">Аудиокнига, которую необходимо загрузить</param>
		/// <param name="stream">Поток, в который копируем поток загрузки аудиокниги</param>
		/// <returns></returns>
		Task GetAudiobook(Audiobook audiobook, Stream stream);

		/// <summary>
		/// Метод получения количества страниц, для определенной категории
		/// </summary>
		/// <param name="category">Категория, для которой необходимо получить список страниц</param>
		/// <returns></returns>
		Task<int> GetPagesCount(Category category);

		/// <summary>
		/// Метод получения идентификатора аудиокниги
		/// </summary>
		/// <param name="audiobook"></param>
		/// <returns></returns>
		Task<int> GetAudiobookId(Audiobook audiobook);
	}
}
