namespace nicorank
{
    partial class FormLayout
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.textBoxRankData = new System.Windows.Forms.TextBox();
            this.textBoxLayout = new System.Windows.Forms.TextBox();
            this.buttonLayoutSave = new System.Windows.Forms.Button();
            this.buttonPicSave = new System.Windows.Forms.Button();
            this.saveFileDialogPic = new System.Windows.Forms.SaveFileDialog();
            this.buttonLayoutLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCoordinate = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.radioButtonFromText = new System.Windows.Forms.RadioButton();
            this.radioButtonFromFile = new System.Windows.Forms.RadioButton();
            this.selectFileBoxLayout = new nicorank.SelectFileBox();
            this.selectFileBoxRankFile = new nicorank.SelectFileBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 384);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoad.Location = new System.Drawing.Point(426, 390);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonLoad.TabIndex = 1;
            this.buttonLoad.Text = "描画";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // textBoxRankData
            // 
            this.textBoxRankData.Location = new System.Drawing.Point(12, 58);
            this.textBoxRankData.Multiline = true;
            this.textBoxRankData.Name = "textBoxRankData";
            this.textBoxRankData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxRankData.Size = new System.Drawing.Size(476, 65);
            this.textBoxRankData.TabIndex = 2;
            this.textBoxRankData.TabStop = false;
            // 
            // textBoxLayout
            // 
            this.textBoxLayout.AcceptsTab = true;
            this.textBoxLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxLayout.Location = new System.Drawing.Point(5, 418);
            this.textBoxLayout.Multiline = true;
            this.textBoxLayout.Name = "textBoxLayout";
            this.textBoxLayout.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLayout.Size = new System.Drawing.Size(501, 140);
            this.textBoxLayout.TabIndex = 0;
            this.textBoxLayout.WordWrap = false;
            this.textBoxLayout.TextChanged += new System.EventHandler(this.textBoxLayout_TextChanged);
            // 
            // buttonLayoutSave
            // 
            this.buttonLayoutSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLayoutSave.Location = new System.Drawing.Point(305, 576);
            this.buttonLayoutSave.Name = "buttonLayoutSave";
            this.buttonLayoutSave.Size = new System.Drawing.Size(87, 23);
            this.buttonLayoutSave.TabIndex = 4;
            this.buttonLayoutSave.Text = "レイアウト保存";
            this.buttonLayoutSave.UseVisualStyleBackColor = true;
            this.buttonLayoutSave.Click += new System.EventHandler(this.buttonLayoutSave_Click);
            // 
            // buttonPicSave
            // 
            this.buttonPicSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPicSave.Location = new System.Drawing.Point(345, 390);
            this.buttonPicSave.Name = "buttonPicSave";
            this.buttonPicSave.Size = new System.Drawing.Size(75, 23);
            this.buttonPicSave.TabIndex = 5;
            this.buttonPicSave.Text = "画像保存";
            this.buttonPicSave.UseVisualStyleBackColor = true;
            this.buttonPicSave.Click += new System.EventHandler(this.buttonPicSave_Click);
            // 
            // saveFileDialogPic
            // 
            this.saveFileDialogPic.Filter = "pngファイル(*.png)|*.png";
            // 
            // buttonLayoutLoad
            // 
            this.buttonLayoutLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLayoutLoad.Location = new System.Drawing.Point(398, 576);
            this.buttonLayoutLoad.Name = "buttonLayoutLoad";
            this.buttonLayoutLoad.Size = new System.Drawing.Size(107, 23);
            this.buttonLayoutLoad.TabIndex = 6;
            this.buttonLayoutLoad.Text = "レイアウト読み込み";
            this.buttonLayoutLoad.UseVisualStyleBackColor = true;
            this.buttonLayoutLoad.Click += new System.EventHandler(this.buttonLayoutLoad_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 563);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "レイアウトファイル指定";
            // 
            // labelCoordinate
            // 
            this.labelCoordinate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCoordinate.AutoSize = true;
            this.labelCoordinate.Location = new System.Drawing.Point(15, 395);
            this.labelCoordinate.Name = "labelCoordinate";
            this.labelCoordinate.Size = new System.Drawing.Size(0, 12);
            this.labelCoordinate.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.selectFileBoxRankFile);
            this.groupBox1.Controls.Add(this.radioButtonFromText);
            this.groupBox1.Controls.Add(this.radioButtonFromFile);
            this.groupBox1.Controls.Add(this.textBoxRankData);
            this.groupBox1.Location = new System.Drawing.Point(6, 605);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(495, 131);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ランクファイル";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(325, 37);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(48, 19);
            this.numericUpDown1.TabIndex = 15;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(379, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "行目";
            // 
            // radioButtonFromText
            // 
            this.radioButtonFromText.AutoSize = true;
            this.radioButtonFromText.Checked = true;
            this.radioButtonFromText.Location = new System.Drawing.Point(12, 39);
            this.radioButtonFromText.Name = "radioButtonFromText";
            this.radioButtonFromText.Size = new System.Drawing.Size(111, 16);
            this.radioButtonFromText.TabIndex = 12;
            this.radioButtonFromText.TabStop = true;
            this.radioButtonFromText.Text = "テキストボックスから";
            this.radioButtonFromText.UseVisualStyleBackColor = true;
            this.radioButtonFromText.CheckedChanged += new System.EventHandler(this.radioButtonFromText_CheckedChanged);
            // 
            // radioButtonFromFile
            // 
            this.radioButtonFromFile.AutoSize = true;
            this.radioButtonFromFile.Location = new System.Drawing.Point(12, 18);
            this.radioButtonFromFile.Name = "radioButtonFromFile";
            this.radioButtonFromFile.Size = new System.Drawing.Size(75, 16);
            this.radioButtonFromFile.TabIndex = 12;
            this.radioButtonFromFile.Text = "ファイルから";
            this.radioButtonFromFile.UseVisualStyleBackColor = true;
            // 
            // selectFileBoxLayout
            // 
            this.selectFileBoxLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectFileBoxLayout.FileDialog = this.openFileDialog1;
            this.selectFileBoxLayout.FileName = "";
            this.selectFileBoxLayout.Location = new System.Drawing.Point(6, 577);
            this.selectFileBoxLayout.Name = "selectFileBoxLayout";
            this.selectFileBoxLayout.Size = new System.Drawing.Size(290, 23);
            this.selectFileBoxLayout.TabIndex = 14;
            // 
            // selectFileBoxRankFile
            // 
            this.selectFileBoxRankFile.Enabled = false;
            this.selectFileBoxRankFile.FileDialog = this.openFileDialog1;
            this.selectFileBoxRankFile.FileName = "";
            this.selectFileBoxRankFile.Location = new System.Drawing.Point(92, 15);
            this.selectFileBoxRankFile.Name = "selectFileBoxRankFile";
            this.selectFileBoxRankFile.Size = new System.Drawing.Size(398, 23);
            this.selectFileBoxRankFile.TabIndex = 13;
            // 
            // FormLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 742);
            this.Controls.Add(this.selectFileBoxLayout);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelCoordinate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonLayoutLoad);
            this.Controls.Add(this.buttonPicSave);
            this.Controls.Add(this.buttonLayoutSave);
            this.Controls.Add(this.textBoxLayout);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.pictureBox1);
            this.Name = "FormLayout";
            this.Text = "レイアウトビュアー - ニコニコランキングメーカー";
            this.Load += new System.EventHandler(this.FormLayout_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLayout_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.TextBox textBoxRankData;
        private System.Windows.Forms.TextBox textBoxLayout;
        private System.Windows.Forms.Button buttonLayoutSave;
        private System.Windows.Forms.Button buttonPicSave;
        private System.Windows.Forms.SaveFileDialog saveFileDialogPic;
        private System.Windows.Forms.Button buttonLayoutLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCoordinate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonFromFile;
        private System.Windows.Forms.RadioButton radioButtonFromText;
        private SelectFileBox selectFileBoxRankFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private SelectFileBox selectFileBoxLayout;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
    }
}