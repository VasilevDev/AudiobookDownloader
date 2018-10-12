using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AudiobookDownloader.Logging
{
	public class CustomLogger : ICustomLogger
	{
		private readonly RichTextBox log;

		public CustomLogger(RichTextBox log)
		{
			this.log = log;
		}

		public void Clear()
		{
			log.Clear();
		}

		public void Debug(string text)
		{
			Print($"[DEBUG]:{text}", Color.DarkBlue);
		}

		public void Error(string text)
		{
			Print($"[ERROR]:{text}", Color.Red);
		}

		public void Log(string text)
		{
			Print($"[LOGGING]:{text}", Color.Black);
		}

		public void Success(string text)
		{
			Print($"[SUCCESS]:{text}", Color.Green);
		}

		public void Warning(string text)
		{
			Print($"[WARN]:{text}", Color.Orange);
		}

		private void Print(string text, Color color)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append($"[{DateTime.Now}]:");
			sb.Append(text);
			sb.AppendLine();

			log.BeginInvoke(new Action(() => {
				log.SelectionColor = color;
				log.AppendText(sb.ToString());
			}));
		}
	}
}
