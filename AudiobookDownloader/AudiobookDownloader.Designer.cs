namespace AudiobookDownloader
{
	partial class AudiobookDownloader
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
			this.log = new System.Windows.Forms.ListBox();
			this.AbooksBtn = new System.Windows.Forms.Button();
			this.label = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rdevLoad = new System.Windows.Forms.Button();
			this.dirUpload = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// log
			// 
			this.log.FormattingEnabled = true;
			this.log.ItemHeight = 16;
			this.log.Location = new System.Drawing.Point(12, 44);
			this.log.Name = "log";
			this.log.ScrollAlwaysVisible = true;
			this.log.Size = new System.Drawing.Size(718, 452);
			this.log.TabIndex = 0;
			// 
			// AbooksBtn
			// 
			this.AbooksBtn.Location = new System.Drawing.Point(6, 21);
			this.AbooksBtn.Name = "AbooksBtn";
			this.AbooksBtn.Size = new System.Drawing.Size(195, 45);
			this.AbooksBtn.TabIndex = 1;
			this.AbooksBtn.Text = "Загрузить";
			this.AbooksBtn.UseVisualStyleBackColor = true;
			this.AbooksBtn.Click += new System.EventHandler(this.AbooksBtn_Click);
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Location = new System.Drawing.Point(25, 24);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(0, 17);
			this.label.TabIndex = 2;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.dirUpload);
			this.groupBox1.Controls.Add(this.rdevLoad);
			this.groupBox1.Controls.Add(this.AbooksBtn);
			this.groupBox1.Location = new System.Drawing.Point(736, 44);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(207, 180);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Abooks";
			// 
			// rdevLoad
			// 
			this.rdevLoad.Location = new System.Drawing.Point(6, 72);
			this.rdevLoad.Name = "rdevLoad";
			this.rdevLoad.Size = new System.Drawing.Size(195, 44);
			this.rdevLoad.TabIndex = 2;
			this.rdevLoad.Text = "Загрузить через Rdev";
			this.rdevLoad.UseVisualStyleBackColor = true;
			this.rdevLoad.Click += new System.EventHandler(this.rdevLoad_ClickAsync);
			// 
			// dirUpload
			// 
			this.dirUpload.Location = new System.Drawing.Point(6, 122);
			this.dirUpload.Name = "dirUpload";
			this.dirUpload.Size = new System.Drawing.Size(195, 45);
			this.dirUpload.TabIndex = 3;
			this.dirUpload.Text = "Загрузить из директории";
			this.dirUpload.UseVisualStyleBackColor = true;
			this.dirUpload.Click += new System.EventHandler(this.dirUpload_Click);
			// 
			// AudiobookDownloader
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(950, 511);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label);
			this.Controls.Add(this.log);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AudiobookDownloader";
			this.Text = "Audiobook downloader";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button AbooksBtn;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button rdevLoad;
		private System.Windows.Forms.Button dirUpload;
	}
}

