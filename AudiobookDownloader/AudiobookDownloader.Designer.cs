﻿namespace AudiobookDownloader
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
			this.AbooksBtn = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.dirUpload = new System.Windows.Forms.Button();
			this.rdevLoad = new System.Windows.Forms.Button();
			this.textLog = new System.Windows.Forms.RichTextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.clearItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AuthTest = new System.Windows.Forms.ToolStripMenuItem();
			this.ClearLog = new System.Windows.Forms.ToolStripMenuItem();
			this.проксиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ProxyItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ProxySettingsItem = new System.Windows.Forms.ToolStripMenuItem();
			this.IsProxy = new System.Windows.Forms.CheckBox();
			this.proxy = new System.Windows.Forms.Label();
			this.tmpLabel = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// AbooksBtn
			// 
			this.AbooksBtn.BackColor = System.Drawing.Color.Lavender;
			this.AbooksBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AbooksBtn.Location = new System.Drawing.Point(6, 21);
			this.AbooksBtn.Name = "AbooksBtn";
			this.AbooksBtn.Size = new System.Drawing.Size(195, 45);
			this.AbooksBtn.TabIndex = 1;
			this.AbooksBtn.Text = "Загрузить";
			this.AbooksBtn.UseVisualStyleBackColor = false;
			this.AbooksBtn.Click += new System.EventHandler(this.AbooksBtn_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.dirUpload);
			this.groupBox1.Controls.Add(this.rdevLoad);
			this.groupBox1.Controls.Add(this.AbooksBtn);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.groupBox1.Location = new System.Drawing.Point(788, 40);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(207, 232);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Abooks";
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Lavender;
			this.button1.Enabled = false;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(6, 173);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(196, 46);
			this.button1.TabIndex = 4;
			this.button1.Text = "Загрузить локально";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.LocalDownload_ClickAsync);
			// 
			// dirUpload
			// 
			this.dirUpload.BackColor = System.Drawing.Color.Lavender;
			this.dirUpload.Enabled = false;
			this.dirUpload.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.dirUpload.Location = new System.Drawing.Point(6, 122);
			this.dirUpload.Name = "dirUpload";
			this.dirUpload.Size = new System.Drawing.Size(195, 45);
			this.dirUpload.TabIndex = 3;
			this.dirUpload.Text = "Загрузить из директории";
			this.dirUpload.UseVisualStyleBackColor = false;
			this.dirUpload.Click += new System.EventHandler(this.DirUpload_Click);
			// 
			// rdevLoad
			// 
			this.rdevLoad.BackColor = System.Drawing.Color.Lavender;
			this.rdevLoad.Enabled = false;
			this.rdevLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rdevLoad.Location = new System.Drawing.Point(6, 72);
			this.rdevLoad.Name = "rdevLoad";
			this.rdevLoad.Size = new System.Drawing.Size(195, 44);
			this.rdevLoad.TabIndex = 2;
			this.rdevLoad.Text = "Загрузить через Rdev";
			this.rdevLoad.UseVisualStyleBackColor = false;
			this.rdevLoad.Click += new System.EventHandler(this.RdevLoad_ClickAsync);
			// 
			// textLog
			// 
			this.textLog.BackColor = System.Drawing.SystemColors.ControlLight;
			this.textLog.Location = new System.Drawing.Point(0, 31);
			this.textLog.Name = "textLog";
			this.textLog.ReadOnly = true;
			this.textLog.Size = new System.Drawing.Size(779, 497);
			this.textLog.TabIndex = 5;
			this.textLog.Text = "";
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1001, 28);
			this.menuStrip1.TabIndex = 6;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// clearItem
			// 
			this.clearItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AuthTest,
            this.ClearLog,
            this.проксиToolStripMenuItem});
			this.clearItem.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.clearItem.Name = "clearItem";
			this.clearItem.Size = new System.Drawing.Size(138, 24);
			this.clearItem.Text = "Дополнительно";
			// 
			// AuthTest
			// 
			this.AuthTest.Name = "AuthTest";
			this.AuthTest.Size = new System.Drawing.Size(216, 26);
			this.AuthTest.Text = "Тест авторизации";
			this.AuthTest.Click += new System.EventHandler(this.AuthTest_ClickAsync);
			// 
			// ClearLog
			// 
			this.ClearLog.Name = "ClearLog";
			this.ClearLog.Size = new System.Drawing.Size(216, 26);
			this.ClearLog.Text = "Очистить лог";
			this.ClearLog.Click += new System.EventHandler(this.ClearLog_Click);
			// 
			// проксиToolStripMenuItem
			// 
			this.проксиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProxyItem,
            this.ProxySettingsItem});
			this.проксиToolStripMenuItem.Name = "проксиToolStripMenuItem";
			this.проксиToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
			this.проксиToolStripMenuItem.Text = "Прокси";
			// 
			// ProxyItem
			// 
			this.ProxyItem.Name = "ProxyItem";
			this.ProxyItem.Size = new System.Drawing.Size(216, 26);
			this.ProxyItem.Text = "Вкл/Откл";
			this.ProxyItem.Click += new System.EventHandler(this.ProxyItem_Click);
			// 
			// ProxySettingsItem
			// 
			this.ProxySettingsItem.Name = "ProxySettingsItem";
			this.ProxySettingsItem.Size = new System.Drawing.Size(216, 26);
			this.ProxySettingsItem.Text = "Настройки";
			this.ProxySettingsItem.Click += new System.EventHandler(this.ProxySettingsItem_Click);
			// 
			// IsProxy
			// 
			this.IsProxy.AutoSize = true;
			this.IsProxy.Enabled = false;
			this.IsProxy.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.IsProxy.Location = new System.Drawing.Point(788, 471);
			this.IsProxy.Name = "IsProxy";
			this.IsProxy.Size = new System.Drawing.Size(87, 24);
			this.IsProxy.TabIndex = 7;
			this.IsProxy.Text = "Прокси";
			this.IsProxy.UseVisualStyleBackColor = true;
			// 
			// proxy
			// 
			this.proxy.AutoSize = true;
			this.proxy.Location = new System.Drawing.Point(785, 498);
			this.proxy.Name = "proxy";
			this.proxy.Size = new System.Drawing.Size(0, 17);
			this.proxy.TabIndex = 8;
			// 
			// tmpLabel
			// 
			this.tmpLabel.AutoSize = true;
			this.tmpLabel.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.tmpLabel.ForeColor = System.Drawing.Color.DarkRed;
			this.tmpLabel.Location = new System.Drawing.Point(0, 535);
			this.tmpLabel.Name = "tmpLabel";
			this.tmpLabel.Size = new System.Drawing.Size(321, 20);
			this.tmpLabel.TabIndex = 9;
			this.tmpLabel.Text = "Три режима загрузки временно отключены";
			// 
			// AudiobookDownloader
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.ClientSize = new System.Drawing.Size(1001, 564);
			this.Controls.Add(this.tmpLabel);
			this.Controls.Add(this.proxy);
			this.Controls.Add(this.IsProxy);
			this.Controls.Add(this.textLog);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "AudiobookDownloader";
			this.Text = "Скачивалка аудиокниг";
			this.groupBox1.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button AbooksBtn;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button rdevLoad;
		private System.Windows.Forms.Button dirUpload;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.RichTextBox textLog;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem clearItem;
		private System.Windows.Forms.ToolStripMenuItem AuthTest;
		private System.Windows.Forms.ToolStripMenuItem ClearLog;
		private System.Windows.Forms.CheckBox IsProxy;
		private System.Windows.Forms.Label proxy;
		private System.Windows.Forms.Label tmpLabel;
		private System.Windows.Forms.ToolStripMenuItem проксиToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ProxyItem;
		private System.Windows.Forms.ToolStripMenuItem ProxySettingsItem;
	}
}

