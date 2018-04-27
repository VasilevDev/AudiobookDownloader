using AudiobookDownloader.Service;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Windows.Forms;

namespace AudiobookDownloader
{
	public partial class Form1 : Form
	{
		private readonly IAudiobookService _service = new AbooksService();
		private readonly Grabber _grabber;
		private List<Category> _categories = new List<Category>();
		private List<Audiobook> _audiobooks = new List<Audiobook>();

		public Form1()
		{
			InitializeComponent();
			_grabber = new Grabber(_service);
		}

		private async void AbooksBtn_Click(object sender, EventArgs e)
		{
			_categories = await _service.GetCategories();

			label.Text = "Категории";
			ListOfCategories.Items.Clear();

			foreach (var item in _categories)
				ListOfCategories.Items.Add(item.Name);
		}

		private async void ListOfCategories_DoubleClick(object sender, EventArgs e)
		{
			var category = _categories.Find(c => c.Name == ListOfCategories.SelectedItem.ToString());
			_audiobooks = await _service.GetAudiobooks(category, 1);
			ListOfCategories.Items.Clear();

			foreach (var audiobook in _audiobooks)
			{
				await _grabber.Grab(audiobook);
			}
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			OwnRadioClient client = new OwnRadioClient();

			Guid ownerRecId = Guid.NewGuid();

			using (var zip = ZipFile.OpenRead("tmp.zip"))
			{
				int chapter = 0;

				foreach (var entry in zip.Entries)
				{
					if (entry.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
					{
						using (var fs = entry.Open())
						{
							Guid recId = Guid.NewGuid();
							await client.Upload(fs, ++chapter, entry.Name, ownerRecId, recId);
						}
					}
				}
			}
		}
	}
}
