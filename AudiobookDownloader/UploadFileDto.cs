using System;
using System.Collections.Generic;

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
		public string LocalDevicePathUpload { get; set; }
		public string Content { get; set; }
		public int Size { get; set; }
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
