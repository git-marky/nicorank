namespace nicorank
{
    partial class FormTransOption
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
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.checkBoxIsChangeSize = new System.Windows.Forms.CheckBox();
            this.checkBoxTransIsFixAspect = new System.Windows.Forms.CheckBox();
            this.textBoxTransChangeWidth = new System.Windows.Forms.TextBox();
            this.textBoxTransChangeHeight = new System.Windows.Forms.TextBox();
            this.labelChangeSize = new System.Windows.Forms.Label();
            this.groupBoxTransColorSpace = new System.Windows.Forms.GroupBox();
            this.radioButtonTransHuffyuv = new System.Windows.Forms.RadioButton();
            this.radioButtonTransBgr24Flip = new System.Windows.Forms.RadioButton();
            this.radioButtonTransNormal = new System.Windows.Forms.RadioButton();
            this.radioButtonTransBgr24 = new System.Windows.Forms.RadioButton();
            this.radioButtonTransYuv420p = new System.Windows.Forms.RadioButton();
            this.textBoxTransDetailOption = new System.Windows.Forms.TextBox();
            this.checkBoxIsOnlyNm = new System.Windows.Forms.CheckBox();
            this.checkBoxIsOnlySm = new System.Windows.Forms.CheckBox();
            this.checkBoxWindowShow = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxFrameRate = new System.Windows.Forms.TextBox();
            this.radioButtonFrameRateChange = new System.Windows.Forms.RadioButton();
            this.radioButtonFrameRateNoChange = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox22.SuspendLayout();
            this.groupBoxTransColorSpace.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.checkBoxIsChangeSize);
            this.groupBox22.Controls.Add(this.checkBoxTransIsFixAspect);
            this.groupBox22.Controls.Add(this.textBoxTransChangeWidth);
            this.groupBox22.Controls.Add(this.textBoxTransChangeHeight);
            this.groupBox22.Controls.Add(this.labelChangeSize);
            this.groupBox22.Location = new System.Drawing.Point(12, 141);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(225, 84);
            this.groupBox22.TabIndex = 72;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "動画・画像サイズ変更";
            // 
            // checkBoxIsChangeSize
            // 
            this.checkBoxIsChangeSize.AutoSize = true;
            this.checkBoxIsChangeSize.Location = new System.Drawing.Point(8, 19);
            this.checkBoxIsChangeSize.Name = "checkBoxIsChangeSize";
            this.checkBoxIsChangeSize.Size = new System.Drawing.Size(129, 16);
            this.checkBoxIsChangeSize.TabIndex = 60;
            this.checkBoxIsChangeSize.Text = "動画サイズを変更する";
            this.checkBoxIsChangeSize.UseVisualStyleBackColor = true;
            this.checkBoxIsChangeSize.CheckedChanged += new System.EventHandler(this.checkBoxIsChangeSize_CheckedChanged);
            // 
            // checkBoxTransIsFixAspect
            // 
            this.checkBoxTransIsFixAspect.AutoSize = true;
            this.checkBoxTransIsFixAspect.Enabled = false;
            this.checkBoxTransIsFixAspect.Location = new System.Drawing.Point(8, 62);
            this.checkBoxTransIsFixAspect.Name = "checkBoxTransIsFixAspect";
            this.checkBoxTransIsFixAspect.Size = new System.Drawing.Size(204, 16);
            this.checkBoxTransIsFixAspect.TabIndex = 59;
            this.checkBoxTransIsFixAspect.Text = "アスペクト比保持のため黒ベタを入れる";
            this.checkBoxTransIsFixAspect.UseVisualStyleBackColor = true;
            // 
            // textBoxTransChangeWidth
            // 
            this.textBoxTransChangeWidth.Enabled = false;
            this.textBoxTransChangeWidth.Location = new System.Drawing.Point(8, 38);
            this.textBoxTransChangeWidth.Name = "textBoxTransChangeWidth";
            this.textBoxTransChangeWidth.Size = new System.Drawing.Size(40, 19);
            this.textBoxTransChangeWidth.TabIndex = 4;
            // 
            // textBoxTransChangeHeight
            // 
            this.textBoxTransChangeHeight.Enabled = false;
            this.textBoxTransChangeHeight.Location = new System.Drawing.Point(70, 38);
            this.textBoxTransChangeHeight.Name = "textBoxTransChangeHeight";
            this.textBoxTransChangeHeight.Size = new System.Drawing.Size(40, 19);
            this.textBoxTransChangeHeight.TabIndex = 5;
            // 
            // labelChangeSize
            // 
            this.labelChangeSize.AutoSize = true;
            this.labelChangeSize.Enabled = false;
            this.labelChangeSize.Location = new System.Drawing.Point(52, 42);
            this.labelChangeSize.Name = "labelChangeSize";
            this.labelChangeSize.Size = new System.Drawing.Size(17, 12);
            this.labelChangeSize.TabIndex = 57;
            this.labelChangeSize.Text = "×";
            // 
            // groupBoxTransColorSpace
            // 
            this.groupBoxTransColorSpace.Controls.Add(this.radioButtonTransHuffyuv);
            this.groupBoxTransColorSpace.Controls.Add(this.radioButtonTransBgr24Flip);
            this.groupBoxTransColorSpace.Controls.Add(this.radioButtonTransNormal);
            this.groupBoxTransColorSpace.Controls.Add(this.radioButtonTransBgr24);
            this.groupBoxTransColorSpace.Controls.Add(this.radioButtonTransYuv420p);
            this.groupBoxTransColorSpace.Location = new System.Drawing.Point(12, 11);
            this.groupBoxTransColorSpace.Name = "groupBoxTransColorSpace";
            this.groupBoxTransColorSpace.Size = new System.Drawing.Size(225, 124);
            this.groupBoxTransColorSpace.TabIndex = 73;
            this.groupBoxTransColorSpace.TabStop = false;
            this.groupBoxTransColorSpace.Text = "AVI形式";
            // 
            // radioButtonTransHuffyuv
            // 
            this.radioButtonTransHuffyuv.AutoSize = true;
            this.radioButtonTransHuffyuv.Location = new System.Drawing.Point(8, 98);
            this.radioButtonTransHuffyuv.Name = "radioButtonTransHuffyuv";
            this.radioButtonTransHuffyuv.Size = new System.Drawing.Size(61, 16);
            this.radioButtonTransHuffyuv.TabIndex = 4;
            this.radioButtonTransHuffyuv.Text = "huffyuv";
            this.toolTip1.SetToolTip(this.radioButtonTransHuffyuv, "huffyuv(AVI)に変換します");
            this.radioButtonTransHuffyuv.UseVisualStyleBackColor = true;
            // 
            // radioButtonTransBgr24Flip
            // 
            this.radioButtonTransBgr24Flip.AutoSize = true;
            this.radioButtonTransBgr24Flip.Checked = true;
            this.radioButtonTransBgr24Flip.Location = new System.Drawing.Point(8, 38);
            this.radioButtonTransBgr24Flip.Name = "radioButtonTransBgr24Flip";
            this.radioButtonTransBgr24Flip.Size = new System.Drawing.Size(200, 16);
            this.radioButtonTransBgr24Flip.TabIndex = 3;
            this.radioButtonTransBgr24Flip.TabStop = true;
            this.radioButtonTransBgr24Flip.Text = "非圧縮AVI、bgr24 + 上下反転修正";
            this.toolTip1.SetToolTip(this.radioButtonTransBgr24Flip, "非圧縮AVI、bgr24に変換します。FFmpegの反転バグを修正するため時間がかかります");
            this.radioButtonTransBgr24Flip.UseVisualStyleBackColor = true;
            // 
            // radioButtonTransNormal
            // 
            this.radioButtonTransNormal.AutoSize = true;
            this.radioButtonTransNormal.Location = new System.Drawing.Point(8, 18);
            this.radioButtonTransNormal.Name = "radioButtonTransNormal";
            this.radioButtonTransNormal.Size = new System.Drawing.Size(171, 16);
            this.radioButtonTransNormal.TabIndex = 2;
            this.radioButtonTransNormal.Text = "非圧縮AVI (色空間変換無し)";
            this.toolTip1.SetToolTip(this.radioButtonTransNormal, "非圧縮AVIに変換します。色空間は元動画から変えません");
            this.radioButtonTransNormal.UseVisualStyleBackColor = true;
            // 
            // radioButtonTransBgr24
            // 
            this.radioButtonTransBgr24.AutoSize = true;
            this.radioButtonTransBgr24.Location = new System.Drawing.Point(8, 58);
            this.radioButtonTransBgr24.Name = "radioButtonTransBgr24";
            this.radioButtonTransBgr24.Size = new System.Drawing.Size(114, 16);
            this.radioButtonTransBgr24.TabIndex = 1;
            this.radioButtonTransBgr24.Text = "非圧縮AVI、bgr24";
            this.toolTip1.SetToolTip(this.radioButtonTransBgr24, "非圧縮AVI、bgr24に変換しますが、上下が反転する場合があります。");
            this.radioButtonTransBgr24.UseVisualStyleBackColor = true;
            // 
            // radioButtonTransYuv420p
            // 
            this.radioButtonTransYuv420p.AutoSize = true;
            this.radioButtonTransYuv420p.Location = new System.Drawing.Point(8, 78);
            this.radioButtonTransYuv420p.Name = "radioButtonTransYuv420p";
            this.radioButtonTransYuv420p.Size = new System.Drawing.Size(128, 16);
            this.radioButtonTransYuv420p.TabIndex = 0;
            this.radioButtonTransYuv420p.Text = "非圧縮AVI、yuv420p";
            this.toolTip1.SetToolTip(this.radioButtonTransYuv420p, "非圧縮AVI、yuv420pに変換します");
            this.radioButtonTransYuv420p.UseVisualStyleBackColor = true;
            // 
            // textBoxTransDetailOption
            // 
            this.textBoxTransDetailOption.Enabled = false;
            this.textBoxTransDetailOption.Location = new System.Drawing.Point(136, 320);
            this.textBoxTransDetailOption.Name = "textBoxTransDetailOption";
            this.textBoxTransDetailOption.Size = new System.Drawing.Size(50, 19);
            this.textBoxTransDetailOption.TabIndex = 76;
            this.textBoxTransDetailOption.Visible = false;
            // 
            // checkBoxIsOnlyNm
            // 
            this.checkBoxIsOnlyNm.AutoSize = true;
            this.checkBoxIsOnlyNm.Location = new System.Drawing.Point(137, 280);
            this.checkBoxIsOnlyNm.Name = "checkBoxIsOnlyNm";
            this.checkBoxIsOnlyNm.Size = new System.Drawing.Size(99, 16);
            this.checkBoxIsOnlyNm.TabIndex = 79;
            this.checkBoxIsOnlyNm.Text = "nm IDのみ処理";
            this.checkBoxIsOnlyNm.UseVisualStyleBackColor = true;
            // 
            // checkBoxIsOnlySm
            // 
            this.checkBoxIsOnlySm.AutoSize = true;
            this.checkBoxIsOnlySm.Location = new System.Drawing.Point(137, 260);
            this.checkBoxIsOnlySm.Name = "checkBoxIsOnlySm";
            this.checkBoxIsOnlySm.Size = new System.Drawing.Size(99, 16);
            this.checkBoxIsOnlySm.TabIndex = 78;
            this.checkBoxIsOnlySm.Text = "sm IDのみ処理";
            this.checkBoxIsOnlySm.UseVisualStyleBackColor = true;
            // 
            // checkBoxWindowShow
            // 
            this.checkBoxWindowShow.AutoSize = true;
            this.checkBoxWindowShow.Location = new System.Drawing.Point(137, 240);
            this.checkBoxWindowShow.Name = "checkBoxWindowShow";
            this.checkBoxWindowShow.Size = new System.Drawing.Size(100, 16);
            this.checkBoxWindowShow.TabIndex = 77;
            this.checkBoxWindowShow.Text = "ウィンドウを表示";
            this.checkBoxWindowShow.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxFrameRate);
            this.groupBox1.Controls.Add(this.radioButtonFrameRateChange);
            this.groupBox1.Controls.Add(this.radioButtonFrameRateNoChange);
            this.groupBox1.Location = new System.Drawing.Point(12, 231);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(118, 120);
            this.groupBox1.TabIndex = 81;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "フレームレート";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 12);
            this.label2.TabIndex = 82;
            this.label2.Text = "レート数";
            // 
            // textBoxFrameRate
            // 
            this.textBoxFrameRate.Location = new System.Drawing.Point(58, 57);
            this.textBoxFrameRate.Name = "textBoxFrameRate";
            this.textBoxFrameRate.Size = new System.Drawing.Size(50, 19);
            this.textBoxFrameRate.TabIndex = 82;
            // 
            // radioButtonFrameRateChange
            // 
            this.radioButtonFrameRateChange.AutoSize = true;
            this.radioButtonFrameRateChange.Location = new System.Drawing.Point(6, 18);
            this.radioButtonFrameRateChange.Name = "radioButtonFrameRateChange";
            this.radioButtonFrameRateChange.Size = new System.Drawing.Size(66, 16);
            this.radioButtonFrameRateChange.TabIndex = 2;
            this.radioButtonFrameRateChange.Text = "変換する";
            this.radioButtonFrameRateChange.UseVisualStyleBackColor = true;
            // 
            // radioButtonFrameRateNoChange
            // 
            this.radioButtonFrameRateNoChange.AutoSize = true;
            this.radioButtonFrameRateNoChange.Checked = true;
            this.radioButtonFrameRateNoChange.Location = new System.Drawing.Point(6, 37);
            this.radioButtonFrameRateNoChange.Name = "radioButtonFrameRateNoChange";
            this.radioButtonFrameRateNoChange.Size = new System.Drawing.Size(76, 16);
            this.radioButtonFrameRateNoChange.TabIndex = 0;
            this.radioButtonFrameRateNoChange.TabStop = true;
            this.radioButtonFrameRateNoChange.Text = "変換しない";
            this.radioButtonFrameRateNoChange.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 36);
            this.label1.TabIndex = 83;
            this.label1.Text = "フレームレートを変換\r\nすると動画の長さが\r\n変わります";
            // 
            // FormTransOption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 364);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxIsOnlyNm);
            this.Controls.Add(this.checkBoxIsOnlySm);
            this.Controls.Add(this.checkBoxWindowShow);
            this.Controls.Add(this.textBoxTransDetailOption);
            this.Controls.Add(this.groupBoxTransColorSpace);
            this.Controls.Add(this.groupBox22);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormTransOption";
            this.Text = "詳細オプション";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormTransOption_FormClosed);
            this.groupBox22.ResumeLayout(false);
            this.groupBox22.PerformLayout();
            this.groupBoxTransColorSpace.ResumeLayout(false);
            this.groupBoxTransColorSpace.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox22;
        private System.Windows.Forms.CheckBox checkBoxTransIsFixAspect;
        private System.Windows.Forms.TextBox textBoxTransChangeWidth;
        private System.Windows.Forms.TextBox textBoxTransChangeHeight;
        private System.Windows.Forms.Label labelChangeSize;
        private System.Windows.Forms.GroupBox groupBoxTransColorSpace;
        private System.Windows.Forms.RadioButton radioButtonTransBgr24;
        private System.Windows.Forms.RadioButton radioButtonTransYuv420p;
        private System.Windows.Forms.TextBox textBoxTransDetailOption;
        private System.Windows.Forms.CheckBox checkBoxIsOnlyNm;
        private System.Windows.Forms.CheckBox checkBoxIsOnlySm;
        private System.Windows.Forms.CheckBox checkBoxWindowShow;
        private System.Windows.Forms.CheckBox checkBoxIsChangeSize;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxFrameRate;
        private System.Windows.Forms.RadioButton radioButtonFrameRateChange;
        private System.Windows.Forms.RadioButton radioButtonFrameRateNoChange;
        private System.Windows.Forms.RadioButton radioButtonTransHuffyuv;
        private System.Windows.Forms.RadioButton radioButtonTransBgr24Flip;
        private System.Windows.Forms.RadioButton radioButtonTransNormal;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
    }
}