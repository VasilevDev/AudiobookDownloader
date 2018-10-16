using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AudiobookDownloader.Entity
{
	[Table("audiobooks")]
	public class Audiobook : BaseEntity
	{
		/// <summary>
		/// Адрес на котором находится аудиокнига
		/// </summary>
		[Column("url")]
		public string Url { get; set; }

		/// <summary>
		/// Оригинальное название аудиокниги
		/// </summary>
		[Column("original_name")]
		public string OriginalName { get; set; }

		/// <summary>
		/// Адрес на скачивание аудиокниги
		/// </summary>
		[Column("download_url")]
		public string DownloadUrl { get; set; }

		/// <summary>
		/// Признак того что аудиокнига полностью скачана
		/// </summary>
		[Column("is_full_downloaded")]
		public bool IsFullDownloaded { get; set; }

		/// <summary>
		/// Количество аудиофайлов в аудиокниге
		/// </summary>
		[Column("files_count")]
		public int FilesCount { get; set; }

		/// <summary>
		/// Путь к аудиокниге в локальном хранилище
		/// </summary>
		[Column("local_path")]
		public string LocalPath { get; set; }

		/// <summary>
		/// Коллекция аудиофайлов входящих в состав аудиокниги 
		/// Свойство необходимое для создания связи один ко многим
		/// </summary>
		public virtual List<Audiofile> Audiofiles { get; set; }
	}
}
