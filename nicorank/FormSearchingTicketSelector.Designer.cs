namespace nicorank
{
    partial class FormSearchingTicketSelector
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
            this.components = new System.ComponentModel.Container();
            this.buttonMakeList = new System.Windows.Forms.Button();
            this.listViewTicket = new System.Windows.Forms.ListView();
            this.columnHeaderTicketID = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderWord = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderTagOrKeyword = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSort = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderPage = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDate = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDetail = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderMylist = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderRedundantSearchingMethod = new System.Windows.Forms.ColumnHeader();
            this.buttonClearTicket = new System.Windows.Forms.Button();
            this.buttonDeleteTicket = new System.Windows.Forms.Button();
            this.contextMenuStripRenameTicket = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripTextBoxNewID = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItemRenameTicket = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOpenSaveDirectory = new System.Windows.Forms.Button();
            this.contextMenuStripRenameTicket.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonMakeList
            // 
            this.buttonMakeList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMakeList.Enabled = false;
            this.buttonMakeList.Location = new System.Drawing.Point(483, 216);
            this.buttonMakeList.Name = "buttonMakeList";
            this.buttonMakeList.Size = new System.Drawing.Size(75, 23);
            this.buttonMakeList.TabIndex = 2;
            this.buttonMakeList.Text = "検索再開";
            this.buttonMakeList.UseVisualStyleBackColor = true;
            this.buttonMakeList.Click += new System.EventHandler(this.buttonMakeList_Click);
            // 
            // listViewTicket
            // 
            this.listViewTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTicket.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderTicketID,
            this.columnHeaderWord,
            this.columnHeaderTagOrKeyword,
            this.columnHeaderSort,
            this.columnHeaderPage,
            this.columnHeaderDate,
            this.columnHeaderDetail,
            this.columnHeaderMylist,
            this.columnHeaderRedundantSearchingMethod});
            this.listViewTicket.FullRowSelect = true;
            this.listViewTicket.GridLines = true;
            this.listViewTicket.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTicket.HideSelection = false;
            this.listViewTicket.Location = new System.Drawing.Point(12, 12);
            this.listViewTicket.MultiSelect = false;
            this.listViewTicket.Name = "listViewTicket";
            this.listViewTicket.ShowItemToolTips = true;
            this.listViewTicket.Size = new System.Drawing.Size(546, 198);
            this.listViewTicket.TabIndex = 1;
            this.listViewTicket.UseCompatibleStateImageBehavior = false;
            this.listViewTicket.View = System.Windows.Forms.View.Details;
            this.listViewTicket.SelectedIndexChanged += new System.EventHandler(this.listViewTicket_SelectedIndexChanged);
            this.listViewTicket.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewTicket_MouseUp);
            // 
            // columnHeaderTicketID
            // 
            this.columnHeaderTicketID.Text = "検索ID";
            this.columnHeaderTicketID.Width = 145;
            // 
            // columnHeaderWord
            // 
            this.columnHeaderWord.Text = "検索ワード";
            this.columnHeaderWord.Width = 200;
            // 
            // columnHeaderTagOrKeyword
            // 
            this.columnHeaderTagOrKeyword.Text = "タグ／キーワード";
            this.columnHeaderTagOrKeyword.Width = 100;
            // 
            // columnHeaderSort
            // 
            this.columnHeaderSort.Text = "検索順";
            this.columnHeaderSort.Width = 120;
            // 
            // columnHeaderPage
            // 
            this.columnHeaderPage.Text = "ページ指定";
            this.columnHeaderPage.Width = 100;
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Text = "投稿日時";
            this.columnHeaderDate.Width = 290;
            // 
            // columnHeaderDetail
            // 
            this.columnHeaderDetail.Text = "詳細";
            this.columnHeaderDetail.Width = 100;
            // 
            // columnHeaderMylist
            // 
            this.columnHeaderMylist.Text = "マイリスト下限";
            this.columnHeaderMylist.Width = 100;
            // 
            // columnHeaderRedundantSearchingMethod
            // 
            this.columnHeaderRedundantSearchingMethod.Text = "複数回検索";
            this.columnHeaderRedundantSearchingMethod.Width = 110;
            // 
            // buttonClearTicket
            // 
            this.buttonClearTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClearTicket.Enabled = false;
            this.buttonClearTicket.Location = new System.Drawing.Point(219, 216);
            this.buttonClearTicket.Name = "buttonClearTicket";
            this.buttonClearTicket.Size = new System.Drawing.Size(75, 23);
            this.buttonClearTicket.TabIndex = 4;
            this.buttonClearTicket.Text = "リセット";
            this.buttonClearTicket.UseVisualStyleBackColor = true;
            this.buttonClearTicket.Click += new System.EventHandler(this.buttonClearTicket_Click);
            // 
            // buttonDeleteTicket
            // 
            this.buttonDeleteTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDeleteTicket.Enabled = false;
            this.buttonDeleteTicket.Location = new System.Drawing.Point(138, 216);
            this.buttonDeleteTicket.Name = "buttonDeleteTicket";
            this.buttonDeleteTicket.Size = new System.Drawing.Size(75, 23);
            this.buttonDeleteTicket.TabIndex = 3;
            this.buttonDeleteTicket.Text = "削除";
            this.buttonDeleteTicket.UseVisualStyleBackColor = true;
            this.buttonDeleteTicket.Click += new System.EventHandler(this.buttonDeleteTicket_Click);
            // 
            // contextMenuStripRenameTicket
            // 
            this.contextMenuStripRenameTicket.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxNewID,
            this.toolStripMenuItemRenameTicket});
            this.contextMenuStripRenameTicket.Name = "contextMenuStripRenameTicket";
            this.contextMenuStripRenameTicket.Size = new System.Drawing.Size(211, 47);
            // 
            // toolStripTextBoxNewID
            // 
            this.toolStripTextBoxNewID.MaxLength = 256;
            this.toolStripTextBoxNewID.Name = "toolStripTextBoxNewID";
            this.toolStripTextBoxNewID.Size = new System.Drawing.Size(150, 19);
            // 
            // toolStripMenuItemRenameTicket
            // 
            this.toolStripMenuItemRenameTicket.Name = "toolStripMenuItemRenameTicket";
            this.toolStripMenuItemRenameTicket.Size = new System.Drawing.Size(210, 22);
            this.toolStripMenuItemRenameTicket.Text = "チケットID変更";
            this.toolStripMenuItemRenameTicket.Click += new System.EventHandler(this.toolStripMenuItemRenameTicket_Click);
            // 
            // buttonOpenSaveDirectory
            // 
            this.buttonOpenSaveDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenSaveDirectory.Enabled = false;
            this.buttonOpenSaveDirectory.Location = new System.Drawing.Point(12, 216);
            this.buttonOpenSaveDirectory.Name = "buttonOpenSaveDirectory";
            this.buttonOpenSaveDirectory.Size = new System.Drawing.Size(120, 23);
            this.buttonOpenSaveDirectory.TabIndex = 5;
            this.buttonOpenSaveDirectory.Text = "保存ディレクトリを開く";
            this.buttonOpenSaveDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenSaveDirectory.Click += new System.EventHandler(this.buttonOpenSaveDirectory_Click);
            // 
            // FormSearchingTicketSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 251);
            this.Controls.Add(this.buttonOpenSaveDirectory);
            this.Controls.Add(this.buttonDeleteTicket);
            this.Controls.Add(this.buttonClearTicket);
            this.Controls.Add(this.listViewTicket);
            this.Controls.Add(this.buttonMakeList);
            this.Name = "FormSearchingTicketSelector";
            this.Text = "再開選択";
            this.Load += new System.EventHandler(this.FormSearchingTicketSelector_Load);
            this.contextMenuStripRenameTicket.ResumeLayout(false);
            this.contextMenuStripRenameTicket.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonMakeList;
        private System.Windows.Forms.ListView listViewTicket;
        private System.Windows.Forms.Button buttonClearTicket;
        private System.Windows.Forms.Button buttonDeleteTicket;
        private System.Windows.Forms.ColumnHeader columnHeaderTicketID;
        private System.Windows.Forms.ColumnHeader columnHeaderWord;
        private System.Windows.Forms.ColumnHeader columnHeaderTagOrKeyword;
        private System.Windows.Forms.ColumnHeader columnHeaderSort;
        private System.Windows.Forms.ColumnHeader columnHeaderPage;
        private System.Windows.Forms.ColumnHeader columnHeaderDate;
        private System.Windows.Forms.ColumnHeader columnHeaderDetail;
        private System.Windows.Forms.ColumnHeader columnHeaderMylist;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRenameTicket;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRenameTicket;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxNewID;
        private System.Windows.Forms.ColumnHeader columnHeaderRedundantSearchingMethod;
        private System.Windows.Forms.Button buttonOpenSaveDirectory;

    }
}