﻿namespace ProcessHacker
{
    partial class VirusTotalUploaderWindow
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
            this.LogoImg = new System.Windows.Forms.PictureBox();
            this.labelFile = new System.Windows.Forms.Label();
            this.uploadedLabel = new System.Windows.Forms.Label();
            this.UploadWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.speedLabel = new System.Windows.Forms.Label();
            this.totalSizeLabel = new System.Windows.Forms.Label();
            this.progressBar = new ProcessHacker.Components.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.LogoImg)).BeginInit();
            this.SuspendLayout();
            // 
            // LogoImg
            // 
            this.LogoImg.Image = global::ProcessHacker.Properties.Resources.VirusTotal_logo;
            this.LogoImg.Location = new System.Drawing.Point(319, 7);
            this.LogoImg.Name = "LogoImg";
            this.LogoImg.Size = new System.Drawing.Size(103, 36);
            this.LogoImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LogoImg.TabIndex = 0;
            this.LogoImg.TabStop = false;
            // 
            // labelFile
            // 
            this.labelFile.AutoSize = true;
            this.labelFile.Location = new System.Drawing.Point(9, 7);
            this.labelFile.Name = "labelFile";
            this.labelFile.Size = new System.Drawing.Size(154, 13);
            this.labelFile.TabIndex = 2;
            this.labelFile.Text = "Uploading: ProcessHacker.exe";
            // 
            // uploadedLabel
            // 
            this.uploadedLabel.AutoSize = true;
            this.uploadedLabel.Location = new System.Drawing.Point(9, 45);
            this.uploadedLabel.Name = "uploadedLabel";
            this.uploadedLabel.Size = new System.Drawing.Size(86, 13);
            this.uploadedLabel.TabIndex = 3;
            this.uploadedLabel.Text = "Uploaded: 00.00";
            // 
            // UploadWorker
            // 
            this.UploadWorker.WorkerReportsProgress = true;
            this.UploadWorker.WorkerSupportsCancellation = true;
            this.UploadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UploadWorker_DoWork);
            this.UploadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.UploadWorker_RunWorkerCompleted);
            this.UploadWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.UploadWorker_ProgressChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(347, 64);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(142, 45);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(71, 13);
            this.speedLabel.TabIndex = 6;
            this.speedLabel.Text = "Speed: 00.00";
            // 
            // totalSizeLabel
            // 
            this.totalSizeLabel.AutoSize = true;
            this.totalSizeLabel.Location = new System.Drawing.Point(9, 26);
            this.totalSizeLabel.Name = "totalSizeLabel";
            this.totalSizeLabel.Size = new System.Drawing.Size(103, 13);
            this.totalSizeLabel.TabIndex = 7;
            this.totalSizeLabel.Text = "Total Size: 20.00MB";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 64);
            this.progressBar.Name = "progressBar";
            this.progressBar.ProgressBarColor = System.Drawing.Color.Blue;
            this.progressBar.Size = new System.Drawing.Size(325, 23);
            this.progressBar.TabIndex = 4;
            // 
            // VirusTotalUploaderWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 99);
            this.Controls.Add(this.totalSizeLabel);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.uploadedLabel);
            this.Controls.Add(this.labelFile);
            this.Controls.Add(this.LogoImg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VirusTotalUploaderWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "VirusTotal Uploader";
            this.Load += new System.EventHandler(this.VirusTotalUploaderWindow_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VirusTotalUploaderWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.LogoImg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox LogoImg;
        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.Label uploadedLabel;
        private System.ComponentModel.BackgroundWorker UploadWorker;
        private ProcessHacker.Components.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.Label totalSizeLabel;
    }
}