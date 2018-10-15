namespace AudiobookDownloader
{
	partial class ProxySettingsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProxyHost = new System.Windows.Forms.TextBox();
			this.ProxyPort = new System.Windows.Forms.TextBox();
			this.hostLbl = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.psOk = new System.Windows.Forms.Button();
			this.psCancel = new System.Windows.Forms.Button();
			this.Error = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ProxyHost
			// 
			this.ProxyHost.Location = new System.Drawing.Point(76, 13);
			this.ProxyHost.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ProxyHost.Name = "ProxyHost";
			this.ProxyHost.Size = new System.Drawing.Size(174, 28);
			this.ProxyHost.TabIndex = 0;
			// 
			// ProxyPort
			// 
			this.ProxyPort.Location = new System.Drawing.Point(76, 48);
			this.ProxyPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ProxyPort.Name = "ProxyPort";
			this.ProxyPort.Size = new System.Drawing.Size(174, 28);
			this.ProxyPort.TabIndex = 1;
			// 
			// hostLbl
			// 
			this.hostLbl.AutoSize = true;
			this.hostLbl.Location = new System.Drawing.Point(23, 17);
			this.hostLbl.Name = "hostLbl";
			this.hostLbl.Size = new System.Drawing.Size(48, 20);
			this.hostLbl.TabIndex = 2;
			this.hostLbl.Text = "Хост:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(23, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(51, 20);
			this.label1.TabIndex = 3;
			this.label1.Text = "Порт:";
			// 
			// psOk
			// 
			this.psOk.Location = new System.Drawing.Point(27, 93);
			this.psOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.psOk.Name = "psOk";
			this.psOk.Size = new System.Drawing.Size(107, 34);
			this.psOk.TabIndex = 4;
			this.psOk.Text = "ОК";
			this.psOk.UseVisualStyleBackColor = true;
			this.psOk.Click += new System.EventHandler(this.PsOk_Click);
			// 
			// psCancel
			// 
			this.psCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.psCancel.Location = new System.Drawing.Point(141, 93);
			this.psCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.psCancel.Name = "psCancel";
			this.psCancel.Size = new System.Drawing.Size(107, 34);
			this.psCancel.TabIndex = 5;
			this.psCancel.Text = "Отмена";
			this.psCancel.UseVisualStyleBackColor = true;
			// 
			// Error
			// 
			this.Error.AutoSize = true;
			this.Error.ForeColor = System.Drawing.Color.DarkRed;
			this.Error.Location = new System.Drawing.Point(23, 135);
			this.Error.Name = "Error";
			this.Error.Size = new System.Drawing.Size(0, 20);
			this.Error.TabIndex = 6;
			// 
			// ProxySettingsForm
			// 
			this.AcceptButton = this.psOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.psCancel;
			this.ClientSize = new System.Drawing.Size(280, 162);
			this.Controls.Add(this.Error);
			this.Controls.Add(this.psCancel);
			this.Controls.Add(this.psOk);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.hostLbl);
			this.Controls.Add(this.ProxyPort);
			this.Controls.Add(this.ProxyHost);
			this.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "ProxySettingsForm";
			this.Text = "Настройки прокси";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ProxyHost;
		private System.Windows.Forms.TextBox ProxyPort;
		private System.Windows.Forms.Label hostLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button psOk;
		private System.Windows.Forms.Button psCancel;
		private System.Windows.Forms.Label Error;
	}
}