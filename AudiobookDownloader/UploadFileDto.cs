using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudiobookDownloader
{
	public struct FileDescription
	{
		public Guid RecId { get; set; }
		public Guid DeviceId { get; set; }
		public string Recdescription { get; set; }
		public string Mediatype { get; set; }
		public int Chapter { get; set; }
		public string Name { get; set; }
		public Guid Ownerrecid { get; set; }
		public string Urn { get; set; }
		public string Content { get; set; }
	}

	public struct FilesItemDto
	{
		public ICollection<FileDescription> Files { get; set; }
	}

	public class UploadFileDto
	{
		public string Method { get; set; }
		public FilesItemDto Fields { get; set; }
	}
}
