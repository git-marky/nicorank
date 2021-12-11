namespace nicorank
{
    partial class FormChoicePic
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
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.comboBoxChoiceVideo = new System.Windows.Forms.ComboBox();
            this.textBoxChoiceVideo = new System.Windows.Forms.TextBox();
            this.buttonShowPic = new System.Windows.Forms.Button();
            this.pictureBoxCurrent = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonChoice9 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice8 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice7 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice6 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice5 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice4 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice3 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice2 = new System.Windows.Forms.RadioButton();
            this.radioButtonChoice1 = new System.Windows.Forms.RadioButton();
            this.pictureBoxChoice9 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice8 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice7 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice6 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice5 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice4 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoice1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxChoiceHand = new System.Windows.Forms.PictureBox();
            this.buttonChange = new System.Windows.Forms.Button();
            this.textBoxHandNo = new System.Windows.Forms.TextBox();
            this.checkBoxChoiceHand = new System.Windows.Forms.CheckBox();
            this.buttonHandShow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCurrent)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoiceHand)).BeginInit();
            this.SuspendLayout();
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(12, 15);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(124, 16);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "ランクファイルから選択";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(12, 38);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(115, 16);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "直接動画IDを入力";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // comboBoxChoiceVideo
            // 
            this.comboBoxChoiceVideo.FormattingEnabled = true;
            this.comboBoxChoiceVideo.Location = new System.Drawing.Point(136, 12);
            this.comboBoxChoiceVideo.Name = "comboBoxChoiceVideo";
            this.comboBoxChoiceVideo.Size = new System.Drawing.Size(471, 20);
            this.comboBoxChoiceVideo.TabIndex = 2;
            // 
            // textBoxChoiceVideo
            // 
            this.textBoxChoiceVideo.Enabled = false;
            this.textBoxChoiceVideo.Location = new System.Drawing.Point(136, 37);
            this.textBoxChoiceVideo.Name = "textBoxChoiceVideo";
            this.textBoxChoiceVideo.Size = new System.Drawing.Size(100, 19);
            this.textBoxChoiceVideo.TabIndex = 3;
            // 
            // buttonShowPic
            // 
            this.buttonShowPic.Location = new System.Drawing.Point(252, 34);
            this.buttonShowPic.Name = "buttonShowPic";
            this.buttonShowPic.Size = new System.Drawing.Size(92, 23);
            this.buttonShowPic.TabIndex = 4;
            this.buttonShowPic.Text = "表示(再取得)";
            this.buttonShowPic.UseVisualStyleBackColor = true;
            this.buttonShowPic.Click += new System.EventHandler(this.buttonShowPic_Click);
            // 
            // pictureBoxCurrent
            // 
            this.pictureBoxCurrent.Location = new System.Drawing.Point(6, 111);
            this.pictureBoxCurrent.Name = "pictureBoxCurrent";
            this.pictureBoxCurrent.Size = new System.Drawing.Size(160, 120);
            this.pictureBoxCurrent.TabIndex = 5;
            this.pictureBoxCurrent.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(51, 239);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "現在の画像";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonChoice9);
            this.groupBox1.Controls.Add(this.radioButtonChoice8);
            this.groupBox1.Controls.Add(this.radioButtonChoice7);
            this.groupBox1.Controls.Add(this.radioButtonChoice6);
            this.groupBox1.Controls.Add(this.radioButtonChoice5);
            this.groupBox1.Controls.Add(this.radioButtonChoice4);
            this.groupBox1.Controls.Add(this.radioButtonChoice3);
            this.groupBox1.Controls.Add(this.radioButtonChoice2);
            this.groupBox1.Controls.Add(this.radioButtonChoice1);
            this.groupBox1.Controls.Add(this.pictureBoxChoice9);
            this.groupBox1.Controls.Add(this.pictureBoxChoice8);
            this.groupBox1.Controls.Add(this.pictureBoxChoice7);
            this.groupBox1.Controls.Add(this.pictureBoxChoice6);
            this.groupBox1.Controls.Add(this.pictureBoxChoice5);
            this.groupBox1.Controls.Add(this.pictureBoxChoice4);
            this.groupBox1.Controls.Add(this.pictureBoxChoice3);
            this.groupBox1.Controls.Add(this.pictureBoxChoice2);
            this.groupBox1.Controls.Add(this.pictureBoxChoice1);
            this.groupBox1.Location = new System.Drawing.Point(170, 63);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(426, 378);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            // 
            // radioButtonChoice9
            // 
            this.radioButtonChoice9.AutoSize = true;
            this.radioButtonChoice9.Enabled = false;
            this.radioButtonChoice9.Location = new System.Drawing.Point(323, 355);
            this.radioButtonChoice9.Name = "radioButtonChoice9";
            this.radioButtonChoice9.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice9.TabIndex = 17;
            this.radioButtonChoice9.TabStop = true;
            this.radioButtonChoice9.Text = "選択";
            this.radioButtonChoice9.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice8
            // 
            this.radioButtonChoice8.AutoSize = true;
            this.radioButtonChoice8.Enabled = false;
            this.radioButtonChoice8.Location = new System.Drawing.Point(192, 355);
            this.radioButtonChoice8.Name = "radioButtonChoice8";
            this.radioButtonChoice8.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice8.TabIndex = 16;
            this.radioButtonChoice8.TabStop = true;
            this.radioButtonChoice8.Text = "選択";
            this.radioButtonChoice8.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice7
            // 
            this.radioButtonChoice7.AutoSize = true;
            this.radioButtonChoice7.Enabled = false;
            this.radioButtonChoice7.Location = new System.Drawing.Point(57, 355);
            this.radioButtonChoice7.Name = "radioButtonChoice7";
            this.radioButtonChoice7.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice7.TabIndex = 15;
            this.radioButtonChoice7.TabStop = true;
            this.radioButtonChoice7.Text = "選択";
            this.radioButtonChoice7.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice6
            // 
            this.radioButtonChoice6.AutoSize = true;
            this.radioButtonChoice6.Enabled = false;
            this.radioButtonChoice6.Location = new System.Drawing.Point(323, 236);
            this.radioButtonChoice6.Name = "radioButtonChoice6";
            this.radioButtonChoice6.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice6.TabIndex = 14;
            this.radioButtonChoice6.TabStop = true;
            this.radioButtonChoice6.Text = "選択";
            this.radioButtonChoice6.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice5
            // 
            this.radioButtonChoice5.AutoSize = true;
            this.radioButtonChoice5.Enabled = false;
            this.radioButtonChoice5.Location = new System.Drawing.Point(192, 236);
            this.radioButtonChoice5.Name = "radioButtonChoice5";
            this.radioButtonChoice5.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice5.TabIndex = 13;
            this.radioButtonChoice5.TabStop = true;
            this.radioButtonChoice5.Text = "選択";
            this.radioButtonChoice5.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice4
            // 
            this.radioButtonChoice4.AutoSize = true;
            this.radioButtonChoice4.Enabled = false;
            this.radioButtonChoice4.Location = new System.Drawing.Point(57, 236);
            this.radioButtonChoice4.Name = "radioButtonChoice4";
            this.radioButtonChoice4.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice4.TabIndex = 12;
            this.radioButtonChoice4.TabStop = true;
            this.radioButtonChoice4.Text = "選択";
            this.radioButtonChoice4.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice3
            // 
            this.radioButtonChoice3.AutoSize = true;
            this.radioButtonChoice3.Enabled = false;
            this.radioButtonChoice3.Location = new System.Drawing.Point(323, 118);
            this.radioButtonChoice3.Name = "radioButtonChoice3";
            this.radioButtonChoice3.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice3.TabIndex = 11;
            this.radioButtonChoice3.TabStop = true;
            this.radioButtonChoice3.Text = "選択";
            this.radioButtonChoice3.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice2
            // 
            this.radioButtonChoice2.AutoSize = true;
            this.radioButtonChoice2.Enabled = false;
            this.radioButtonChoice2.Location = new System.Drawing.Point(192, 118);
            this.radioButtonChoice2.Name = "radioButtonChoice2";
            this.radioButtonChoice2.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice2.TabIndex = 10;
            this.radioButtonChoice2.TabStop = true;
            this.radioButtonChoice2.Text = "選択";
            this.radioButtonChoice2.UseVisualStyleBackColor = true;
            // 
            // radioButtonChoice1
            // 
            this.radioButtonChoice1.AutoSize = true;
            this.radioButtonChoice1.Checked = true;
            this.radioButtonChoice1.Enabled = false;
            this.radioButtonChoice1.Location = new System.Drawing.Point(57, 118);
            this.radioButtonChoice1.Name = "radioButtonChoice1";
            this.radioButtonChoice1.Size = new System.Drawing.Size(47, 16);
            this.radioButtonChoice1.TabIndex = 9;
            this.radioButtonChoice1.TabStop = true;
            this.radioButtonChoice1.Text = "選択";
            this.radioButtonChoice1.UseVisualStyleBackColor = true;
            // 
            // pictureBoxChoice9
            // 
            this.pictureBoxChoice9.Enabled = false;
            this.pictureBoxChoice9.Location = new System.Drawing.Point(283, 256);
            this.pictureBoxChoice9.Name = "pictureBoxChoice9";
            this.pictureBoxChoice9.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice9.TabIndex = 8;
            this.pictureBoxChoice9.TabStop = false;
            // 
            // pictureBoxChoice8
            // 
            this.pictureBoxChoice8.Enabled = false;
            this.pictureBoxChoice8.Location = new System.Drawing.Point(149, 256);
            this.pictureBoxChoice8.Name = "pictureBoxChoice8";
            this.pictureBoxChoice8.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice8.TabIndex = 7;
            this.pictureBoxChoice8.TabStop = false;
            // 
            // pictureBoxChoice7
            // 
            this.pictureBoxChoice7.Enabled = false;
            this.pictureBoxChoice7.Location = new System.Drawing.Point(15, 256);
            this.pictureBoxChoice7.Name = "pictureBoxChoice7";
            this.pictureBoxChoice7.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice7.TabIndex = 6;
            this.pictureBoxChoice7.TabStop = false;
            // 
            // pictureBoxChoice6
            // 
            this.pictureBoxChoice6.Enabled = false;
            this.pictureBoxChoice6.Location = new System.Drawing.Point(283, 137);
            this.pictureBoxChoice6.Name = "pictureBoxChoice6";
            this.pictureBoxChoice6.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice6.TabIndex = 5;
            this.pictureBoxChoice6.TabStop = false;
            // 
            // pictureBoxChoice5
            // 
            this.pictureBoxChoice5.Enabled = false;
            this.pictureBoxChoice5.Location = new System.Drawing.Point(149, 137);
            this.pictureBoxChoice5.Name = "pictureBoxChoice5";
            this.pictureBoxChoice5.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice5.TabIndex = 4;
            this.pictureBoxChoice5.TabStop = false;
            // 
            // pictureBoxChoice4
            // 
            this.pictureBoxChoice4.Enabled = false;
            this.pictureBoxChoice4.Location = new System.Drawing.Point(15, 137);
            this.pictureBoxChoice4.Name = "pictureBoxChoice4";
            this.pictureBoxChoice4.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice4.TabIndex = 3;
            this.pictureBoxChoice4.TabStop = false;
            // 
            // pictureBoxChoice3
            // 
            this.pictureBoxChoice3.Enabled = false;
            this.pictureBoxChoice3.Location = new System.Drawing.Point(283, 19);
            this.pictureBoxChoice3.Name = "pictureBoxChoice3";
            this.pictureBoxChoice3.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice3.TabIndex = 2;
            this.pictureBoxChoice3.TabStop = false;
            // 
            // pictureBoxChoice2
            // 
            this.pictureBoxChoice2.Enabled = false;
            this.pictureBoxChoice2.Location = new System.Drawing.Point(149, 19);
            this.pictureBoxChoice2.Name = "pictureBoxChoice2";
            this.pictureBoxChoice2.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice2.TabIndex = 1;
            this.pictureBoxChoice2.TabStop = false;
            // 
            // pictureBoxChoice1
            // 
            this.pictureBoxChoice1.Enabled = false;
            this.pictureBoxChoice1.Location = new System.Drawing.Point(15, 19);
            this.pictureBoxChoice1.Name = "pictureBoxChoice1";
            this.pictureBoxChoice1.Size = new System.Drawing.Size(128, 96);
            this.pictureBoxChoice1.TabIndex = 0;
            this.pictureBoxChoice1.TabStop = false;
            // 
            // pictureBoxChoiceHand
            // 
            this.pictureBoxChoiceHand.Location = new System.Drawing.Point(4, 282);
            this.pictureBoxChoiceHand.Name = "pictureBoxChoiceHand";
            this.pictureBoxChoiceHand.Size = new System.Drawing.Size(160, 120);
            this.pictureBoxChoiceHand.TabIndex = 17;
            this.pictureBoxChoiceHand.TabStop = false;
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(44, 254);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(75, 23);
            this.buttonChange.TabIndex = 18;
            this.buttonChange.Text = "入れ替え";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // textBoxHandNo
            // 
            this.textBoxHandNo.Enabled = false;
            this.textBoxHandNo.Location = new System.Drawing.Point(134, 406);
            this.textBoxHandNo.Name = "textBoxHandNo";
            this.textBoxHandNo.Size = new System.Drawing.Size(33, 19);
            this.textBoxHandNo.TabIndex = 19;
            // 
            // checkBoxChoiceHand
            // 
            this.checkBoxChoiceHand.AutoSize = true;
            this.checkBoxChoiceHand.Location = new System.Drawing.Point(6, 408);
            this.checkBoxChoiceHand.Name = "checkBoxChoiceHand";
            this.checkBoxChoiceHand.Size = new System.Drawing.Size(126, 16);
            this.checkBoxChoiceHand.TabIndex = 20;
            this.checkBoxChoiceHand.Text = "手動で入れ替え　No.";
            this.checkBoxChoiceHand.UseVisualStyleBackColor = true;
            this.checkBoxChoiceHand.CheckedChanged += new System.EventHandler(this.checkBoxChoiceHand_CheckedChanged);
            // 
            // buttonHandShow
            // 
            this.buttonHandShow.Enabled = false;
            this.buttonHandShow.Location = new System.Drawing.Point(81, 422);
            this.buttonHandShow.Name = "buttonHandShow";
            this.buttonHandShow.Size = new System.Drawing.Size(44, 19);
            this.buttonHandShow.TabIndex = 21;
            this.buttonHandShow.Text = "表示";
            this.buttonHandShow.UseVisualStyleBackColor = true;
            this.buttonHandShow.Click += new System.EventHandler(this.buttonHandShow_Click);
            // 
            // FormChoicePic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 446);
            this.Controls.Add(this.buttonHandShow);
            this.Controls.Add(this.checkBoxChoiceHand);
            this.Controls.Add(this.textBoxHandNo);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.pictureBoxChoiceHand);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxCurrent);
            this.Controls.Add(this.buttonShowPic);
            this.Controls.Add(this.textBoxChoiceVideo);
            this.Controls.Add(this.comboBoxChoiceVideo);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Name = "FormChoicePic";
            this.Text = "画像選択 - ニコニコランキングメーカー";
            this.Load += new System.EventHandler(this.FormChoicePic_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCurrent)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoice1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChoiceHand)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.ComboBox comboBoxChoiceVideo;
        private System.Windows.Forms.TextBox textBoxChoiceVideo;
        private System.Windows.Forms.Button buttonShowPic;
        private System.Windows.Forms.PictureBox pictureBoxCurrent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonChoice9;
        private System.Windows.Forms.RadioButton radioButtonChoice8;
        private System.Windows.Forms.RadioButton radioButtonChoice7;
        private System.Windows.Forms.RadioButton radioButtonChoice6;
        private System.Windows.Forms.RadioButton radioButtonChoice5;
        private System.Windows.Forms.RadioButton radioButtonChoice4;
        private System.Windows.Forms.RadioButton radioButtonChoice3;
        private System.Windows.Forms.RadioButton radioButtonChoice2;
        private System.Windows.Forms.RadioButton radioButtonChoice1;
        private System.Windows.Forms.PictureBox pictureBoxChoice9;
        private System.Windows.Forms.PictureBox pictureBoxChoice8;
        private System.Windows.Forms.PictureBox pictureBoxChoice7;
        private System.Windows.Forms.PictureBox pictureBoxChoice6;
        private System.Windows.Forms.PictureBox pictureBoxChoice5;
        private System.Windows.Forms.PictureBox pictureBoxChoice4;
        private System.Windows.Forms.PictureBox pictureBoxChoice3;
        private System.Windows.Forms.PictureBox pictureBoxChoice2;
        private System.Windows.Forms.PictureBox pictureBoxChoice1;
        private System.Windows.Forms.PictureBox pictureBoxChoiceHand;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.TextBox textBoxHandNo;
        private System.Windows.Forms.CheckBox checkBoxChoiceHand;
        private System.Windows.Forms.Button buttonHandShow;
    }
}