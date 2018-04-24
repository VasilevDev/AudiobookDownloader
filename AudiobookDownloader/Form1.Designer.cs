namespace AudiobookDownloader
{
	partial class Form1
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
			this.ListOfCategories = new System.Windows.Forms.ListBox();
			this.AbooksBtn = new System.Windows.Forms.Button();
			this.label = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ListOfCategories
			// 
			this.ListOfCategories.FormattingEnabled = true;
			this.ListOfCategories.ItemHeight = 16;
			this.ListOfCategories.Location = new System.Drawing.Point(12, 44);
			this.ListOfCategories.Name = "ListOfCategories";
			this.ListOfCategories.ScrollAlwaysVisible = true;
			this.ListOfCategories.Size = new System.Drawing.Size(718, 452);
			this.ListOfCategories.TabIndex = 0;
			this.ListOfCategories.DoubleClick += new System.EventHandler(this.ListOfCategories_DoubleClick);
			// 
			// AbooksBtn
			// 
			this.AbooksBtn.Location = new System.Drawing.Point(736, 44);
			this.AbooksBtn.Name = "AbooksBtn";
			this.AbooksBtn.Size = new System.Drawing.Size(135, 29);
			this.AbooksBtn.TabIndex = 1;
			this.AbooksBtn.Text = "Abooks";
			this.AbooksBtn.UseVisualStyleBackColor = true;
			this.AbooksBtn.Click += new System.EventHandler(this.AbooksBtn_Click);
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Location = new System.Drawing.Point(12, 24);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(0, 17);
			this.label.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(881, 511);
			this.Controls.Add(this.label);
			this.Controls.Add(this.AbooksBtn);
			this.Controls.Add(this.ListOfCategories);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox ListOfCategories;
		private System.Windows.Forms.Button AbooksBtn;
		private System.Windows.Forms.Label label;
	}
}

