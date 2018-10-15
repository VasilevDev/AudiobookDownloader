using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AudiobookDownloader
{
	public partial class ProxySettingsForm : Form
	{
		public ProxySettingsForm()
		{
			InitializeComponent();

			ProxyHost.Text = ConfigurationManager.AppSettings.Get("ProxyIp");
			ProxyPort.Text = ConfigurationManager.AppSettings.Get("ProxyPort");
		}

		private void PsOk_Click(object sender, EventArgs e)
		{
			Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
			MatchCollection result = ip.Matches(ProxyHost.Text);

			if(result.Count <= 0)
			{
				Error.Text = $"Хост должен иметь формат ip адреса.";
				return;
			}

			if(!Int32.TryParse(ProxyPort.Text, out int port))
			{
				Error.Text = $"Порт не является числом.";
				return;
			}

			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			config.AppSettings.Settings["ProxyIp"].Value = ProxyHost.Text;
			config.AppSettings.Settings["ProxyPort"].Value = ProxyPort.Text;

			config.Save();

			ConfigurationManager.RefreshSection("appSettings");

			Close();
		}

		private void PsCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
