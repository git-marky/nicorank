namespace nicorank
{
    partial class FormShowingCand
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
            this.listViewCand = new System.Windows.Forms.ListView();
            this.buttonClose = new System.Windows.Forms.Button();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.labelMessage = new System.Windows.Forms.Label();
            this.checkBoxOnlyFileName = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listViewCand
            // 
            this.listViewCand.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewCand.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewCand.GridLines = true;
            this.listViewCand.Location = new System.Drawing.Point(12, 12);
            this.listViewCand.Name = "listViewCand";
            this.listViewCand.Size = new System.Drawing.Size(594, 453);
            this.listViewCand.TabIndex = 0;
            this.listViewCand.UseCompatibleStateImageBehavior = false;
            this.listViewCand.View = System.Windows.Forms.View.Details;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClose.Location = new System.Drawing.Point(269, 471);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "閉じる";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "変換前ファイル名";
            this.columnHeader1.Width = 286;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "変換後ファイル名";
            this.columnHeader2.Width = 304;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(12, 476);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(77, 12);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "検索中…";
            // 
            // checkBoxOnlyFileName
            // 
            this.checkBoxOnlyFileName.AutoSize = true;
            this.checkBoxOnlyFileName.Location = new System.Drawing.Point(128, 475);
            this.checkBoxOnlyFileName.Name = "checkBoxOnlyFileName";
            this.checkBoxOnlyFileName.Size = new System.Drawing.Size(91, 16);
            this.checkBoxOnlyFileName.TabIndex = 3;
            this.checkBoxOnlyFileName.Text = "ファイル名のみ";
            this.checkBoxOnlyFileName.UseVisualStyleBackColor = true;
            this.checkBoxOnlyFileName.CheckedChanged += new System.EventHandler(this.checkBoxOnlyFileName_CheckedChanged);
            // 
            // FormShowingCand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 500);
            this.Controls.Add(this.checkBoxOnlyFileName);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.listViewCand);
            this.Name = "FormShowingCand";
            this.Text = "変換ファイル名を確認";
            this.Load += new System.EventHandler(this.FormShowingCand_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewCand;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.CheckBox checkBoxOnlyFileName;

    }
}