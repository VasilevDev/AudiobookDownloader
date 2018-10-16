using System.ComponentModel.DataAnnotations.Schema;

namespace AudiobookDownloader.Entity
{
	[Table("audiofiles")]
	public class Audiofile : BaseEntity
	{
		/// <summary>
		/// Идентификатор принадлежности аудиофайла к определенной аудиокниге
		/// </summary>
		[Column("ownerrecid")]
		public string OwnerRecid { get; set; }

		/// <summary>
		/// Номер главы
		/// </summary>
		[Column("chapter")]
		public int Chapter { get; set; }

		/// <summary>
		/// Адрес по которому расположена аудиокнига
		/// </summary>
		[Column("audiobook_url")]
		public string AudiobookUrl { get; set; }

		/// <summary>
		/// Название аудиокниги
		/// </summary>
		[Column("audiobook_name")]
		public string AudiobookName { get; set; }

		/// <summary>
		/// Оригинальное название аудиокниги
		/// </summary>
		[Column("audiobook_original_name")]
		public string AudiobookOriginalName { get; set; }

		/// <summary>
		/// Свойство для реализации связи один ко многим
		/// </summary>
		public virtual Audiobook Audiobook { get; set; }
	}
}
