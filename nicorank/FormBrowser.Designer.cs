namespace nicorank
{
    partial class FormBrowser
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboBoxNavigateUrl = new System.Windows.Forms.ComboBox();
            this.buttonNavigate = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.buttonVideoStart = new System.Windows.Forms.Button();
            this.buttonVideoEnd = new System.Windows.Forms.Button();
            this.buttonWrite = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxVideoStart = new System.Windows.Forms.TextBox();
            this.textBoxVideoEnd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBoxRankFilePath = new System.Windows.Forms.TextBox();
            this.textBoxCutListPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonLoadRankFile = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxNavigateUrl
            // 
            this.comboBoxNavigateUrl.FormattingEnabled = true;
            this.comboBoxNavigateUrl.Location = new System.Drawing.Point(99, 36);
            this.comboBoxNavigateUrl.MaxDropDownItems = 30;
            this.comboBoxNavigateUrl.Name = "comboBoxNavigateUrl";
            this.comboBoxNavigateUrl.Size = new System.Drawing.Size(407, 20);
            this.comboBoxNavigateUrl.TabIndex = 0;
            // 
            // buttonNavigate
            // 
            this.buttonNavigate.Location = new System.Drawing.Point(512, 30);
            this.buttonNavigate.Name = "buttonNavigate";
            this.buttonNavigate.Size = new System.Drawing.Size(44, 33);
            this.buttonNavigate.TabIndex = 1;
            this.buttonNavigate.Text = "go";
            this.buttonNavigate.UseVisualStyleBackColor = true;
            this.buttonNavigate.Click += new System.EventHandler(this.buttonNavigate_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(0, 114);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(686, 532);
            this.webBrowser1.TabIndex = 2;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // buttonVideoStart
            // 
            this.buttonVideoStart.Location = new System.Drawing.Point(106, 65);
            this.buttonVideoStart.Name = "buttonVideoStart";
            this.buttonVideoStart.Size = new System.Drawing.Size(39, 19);
            this.buttonVideoStart.TabIndex = 3;
            this.buttonVideoStart.Text = "設定";
            this.buttonVideoStart.UseVisualStyleBackColor = true;
            this.buttonVideoStart.Click += new System.EventHandler(this.buttonVideoStart_Click);
            // 
            // buttonVideoEnd
            // 
            this.buttonVideoEnd.Location = new System.Drawing.Point(106, 87);
            this.buttonVideoEnd.Name = "buttonVideoEnd";
            this.buttonVideoEnd.Size = new System.Drawing.Size(39, 19);
            this.buttonVideoEnd.TabIndex = 4;
            this.buttonVideoEnd.Text = "設定";
            this.buttonVideoEnd.UseVisualStyleBackColor = true;
            this.buttonVideoEnd.Click += new System.EventHandler(this.buttonVideoStart_Click);
            // 
            // buttonWrite
            // 
            this.buttonWrite.Location = new System.Drawing.Point(151, 85);
            this.buttonWrite.Name = "buttonWrite";
            this.buttonWrite.Size = new System.Drawing.Size(43, 23);
            this.buttonWrite.TabIndex = 7;
            this.buttonWrite.Text = "書込";
            this.buttonWrite.UseVisualStyleBackColor = true;
            this.buttonWrite.Click += new System.EventHandler(this.buttonWrite_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "開始";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "URL or 動画ID";
            // 
            // textBoxVideoStart
            // 
            this.textBoxVideoStart.Location = new System.Drawing.Point(48, 65);
            this.textBoxVideoStart.Name = "textBoxVideoStart";
            this.textBoxVideoStart.Size = new System.Drawing.Size(52, 19);
            this.textBoxVideoStart.TabIndex = 10;
            // 
            // textBoxVideoEnd
            // 
            this.textBoxVideoEnd.Location = new System.Drawing.Point(48, 87);
            this.textBoxVideoEnd.Name = "textBoxVideoEnd";
            this.textBoxVideoEnd.Size = new System.Drawing.Size(52, 19);
            this.textBoxVideoEnd.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "終了";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(156, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "0.0";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBoxRankFilePath
            // 
            this.textBoxRankFilePath.Location = new System.Drawing.Point(99, 11);
            this.textBoxRankFilePath.Name = "textBoxRankFilePath";
            this.textBoxRankFilePath.Size = new System.Drawing.Size(334, 19);
            this.textBoxRankFilePath.TabIndex = 14;
            // 
            // textBoxCutListPath
            // 
            this.textBoxCutListPath.Location = new System.Drawing.Point(210, 87);
            this.textBoxCutListPath.Name = "textBoxCutListPath";
            this.textBoxCutListPath.Size = new System.Drawing.Size(313, 19);
            this.textBoxCutListPath.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(208, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "書込するカットリスト名";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "ランクファイル";
            // 
            // buttonLoadRankFile
            // 
            this.buttonLoadRankFile.Location = new System.Drawing.Point(439, 11);
            this.buttonLoadRankFile.Name = "buttonLoadRankFile";
            this.buttonLoadRankFile.Size = new System.Drawing.Size(47, 19);
            this.buttonLoadRankFile.TabIndex = 18;
            this.buttonLoadRankFile.Text = "読込";
            this.buttonLoadRankFile.UseVisualStyleBackColor = true;
            this.buttonLoadRankFile.Click += new System.EventHandler(this.buttonLoadRankFile_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(538, 90);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(0, 12);
            this.labelInfo.TabIndex = 19;
            // 
            // FormBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 646);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonLoadRankFile);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxCutListPath);
            this.Controls.Add(this.textBoxRankFilePath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxVideoEnd);
            this.Controls.Add(this.textBoxVideoStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonWrite);
            this.Controls.Add(this.buttonVideoEnd);
            this.Controls.Add(this.buttonVideoStart);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.buttonNavigate);
            this.Controls.Add(this.comboBoxNavigateUrl);
            this.Name = "FormBrowser";
            this.Text = "シーンカットブラウザ";
            this.Load += new System.EventHandler(this.FormBrowser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxNavigateUrl;
        private System.Windows.Forms.Button buttonNavigate;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button buttonVideoStart;
        private System.Windows.Forms.Button buttonVideoEnd;
        private System.Windows.Forms.Button buttonWrite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxVideoStart;
        private System.Windows.Forms.TextBox textBoxVideoEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBoxRankFilePath;
        private System.Windows.Forms.TextBox textBoxCutListPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonLoadRankFile;
        private System.Windows.Forms.Label labelInfo;
    }
}