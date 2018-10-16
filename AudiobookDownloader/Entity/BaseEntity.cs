using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AudiobookDownloader.Entity
{
	public class BaseEntity
	{
		/// <summary>
		/// Уникальный идентификатор записи в таблице
		/// </summary>
		[Column("id")]
		public int Id { get; set; }

		/// <summary>
		/// Оригинальное название
		/// </summary>
		[Column("name")]
		public string Name { get; set; }

		/// <summary>
		/// Текст ошибки возникшей при обработке аудиокниги
		/// </summary>
		[Column("error")]
		public string Error { get; set; }

		/// <summary>
		/// Дата создания записи в БД
		/// </summary>
		[Column("created")]
		public DateTime Created { get; set; }

		/// <summary>
		/// Дата обновления записи в БД
		/// </summary>
		[Column("updated")]
		public DateTime Updated { get; set; }

		/// <summary>
		/// Признак того что аудиокнига/аудиофайл был(а) полностью передан(а) на Rdev
		/// </summary>
		[Column("is_full_uploaded")]
		public bool IsFullUploaded { get; set; }

		/// <summary>
		/// Размер в байтах
		/// </summary>
		[Column("size")]
		public long Size { get; set; }
	}
}
