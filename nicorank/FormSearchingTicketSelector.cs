using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NicoTools;
using System.IO;
using System.Diagnostics;

namespace nicorank
{
    public partial class FormSearchingTicketSelector : Form
    {
        private const int COLUMN_INDEX_TICKET_ID = 0;
        private const int COLUMN_INDEX_WORD = 1;
        private const int COLUMN_INDEX_TAG_OR_KEYWORD = 2;
        private const int COLUMN_INDEX_DETAIL = 3;
        private const int COLUMN_INDEX_MYLIST = 4;
        private const int COLUMN_INDEX_PAGE = 5;
        private const int COLUMN_INDEX_DATE = 6;
        private const int COLUMN_INDEX_SORT = 7;

        private Dictionary<NicoNetwork.SearchSortMethod, string[]> search_sort_method_text_map;

        private string selected_ticket_id_;

        public FormSearchingTicketSelector()
        {
            InitializeComponent();

            this.search_sort_method_text_map = new Dictionary<NicoNetwork.SearchSortMethod, string[]>();
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.SubmitDate, new string[] { 
                "投稿日時が古い順", 
                "投稿日時が新しい順" });
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.View, new string[] { 
                "再生が少ない順", 
                "再生が多い順" });
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.ResNew, new string[] { 
                "コメントが古い順", 
                "コメントが新しい順" });
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.Res, new string[] { 
                "コメントが少ない順", 
                "コメントが多い順" });
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.Mylist, new string[] { 
                "マイリスト登録が少ない順", 
                "マイリスト登録が多い" });
            search_sort_method_text_map.Add(NicoNetwork.SearchSortMethod.Time, new string[] { 
                "再生時間が短い順", 
                "再生時間が長い順" });

            selected_ticket_id_ = null;
        }

        private void FormSearchingTicketSelector_Load(object sender, EventArgs e)
        {
            ResetTickets();
        }

        private void ResetTickets()
        {
            List<string> id_list = new List<string>(SearchingTicketManager.GetTicketIDs());
            id_list.Sort(CompareStringForDescendingSort);
            foreach (string ticket_id in id_list)
            {
                AddTicket(ticket_id);
            }
        }

        private int CompareStringForDescendingSort(string s1, string s2) {
            return -(string.Compare(s1, s2));
        }

        private void AddTicket(string ticket_id)
        {
            try
            {
                SearchingTagOption option = SearchingTicketManager.GetOption(ticket_id);
                
                ListViewItem item = new ListViewItem(ticket_id);
                
                string word = string.Join("\r\n", option.searching_tag_list.ToArray());
                item.SubItems.Add(word);

                string tag_or_word = option.is_searching_kind_tag ? "タグ" : "キーワード";
                item.SubItems.Add(tag_or_word);

                string sort;
                string[] text;
                if (this.search_sort_method_text_map.TryGetValue(option.GetSortMethod(), out text))
                {
                    if (option.GetSearchOrder() == NicoNetwork.SearchOrder.Asc)
                    {
                        sort = text[0];
                    }
                    else if (option.GetSearchOrder() == NicoNetwork.SearchOrder.Desc)
                    {
                        sort = text[1];
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                item.SubItems.Add(sort);

                string page;
                if (option.is_page_all)
                {
                    page = "指定無し";
                }
                else
                {
                    page = string.Format("{0} - {1}", option.page_start, option.page_end);
                }
                item.SubItems.Add(page);

                string date;
                if (option.is_using_condition)
                {
                    string date_format = "yyyy年MM月dd日 HH:mm:ss";
                    date = string.Format("{0} - {1}", option.date_from.ToString(date_format), option.date_to.ToString(date_format));
                }
                else
                {
                    date = "指定無し";
                }
                item.SubItems.Add(date);

                string detail = option.is_detail_getting ? "有" : "無";
                item.SubItems.Add(detail);

                string mylist;
                if (option.is_detail_getting)
                {
                    mylist = option.detail_info_lower.ToString();
                }
                else
                {
                    mylist = string.Empty;
                }
                item.SubItems.Add(mylist);

                string redundant_searching_method;
                switch (option.redundant_seatching_method)
                {
                    case RedundantSearchingMethod.Once:
                        redundant_searching_method = "無";
                        break;
                    case RedundantSearchingMethod.TwiceTakeFirst:
                        redundant_searching_method = "2回(1回目を優先)";
                        break;
                    case RedundantSearchingMethod.TwiceTakeLast:
                        redundant_searching_method = "2回(2回目を優先)";
                        break;
                    case RedundantSearchingMethod.TwiceMergeResult:
                        redundant_searching_method = "2回(マージ)";
                        break;
                    case RedundantSearchingMethod.AtMostThreeTimes:
                        redundant_searching_method = "最大3回";
                        break;
                    default:
                        redundant_searching_method = "無し";
                        break;
                }
                item.SubItems.Add(redundant_searching_method);


                this.listViewTicket.Items.Add(item);
            }
            catch (IOException)
            {
            }
            catch (FormatException)
            {
            }
        }

        public string SelectedTicketID
        {
            get { return selected_ticket_id_; }
        }

        private void listViewTicket_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listViewTicket.SelectedIndices.Count >= 1)
            {
                this.buttonOpenSaveDirectory.Enabled = true;
                this.buttonDeleteTicket.Enabled = true;
                this.buttonClearTicket.Enabled = true;
                this.buttonMakeList.Enabled = true;
            }
            else
            {
                this.buttonOpenSaveDirectory.Enabled = false;
                this.buttonDeleteTicket.Enabled = false;
                this.buttonClearTicket.Enabled = false;
                this.buttonMakeList.Enabled = false;
            }
        }

        private void buttonDeleteTicket_Click(object sender, EventArgs e)
        {
                if (this.listViewTicket.SelectedIndices.Count >= 1)
                {
                    ListViewItem item = this.listViewTicket.SelectedItems[0];
                    string ticket_id = item.SubItems[COLUMN_INDEX_TICKET_ID].Text;
                    DialogResult result = MessageBox.Show(
                        this,
                        string.Format("保存されている検索を削除します。\r\n「{0}」", ticket_id),
                        "検索削除",
                        MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            SearchingTicketManager.DeleteTicket(ticket_id);
                            this.listViewTicket.Items.Remove(item);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(string.Format("保存されている検索の削除に失敗しました。\r\nこの検索の再開は失敗する可能性があります\r\n「{0}」", ticket_id));
                        }
                    }

            }
        }

        private void buttonClearTicket_Click(object sender, EventArgs e)
        {
                if (this.listViewTicket.SelectedIndices.Count >= 1)
                {
                    string ticket_id = this.listViewTicket.SelectedItems[0].SubItems[COLUMN_INDEX_TICKET_ID].Text;
                    DialogResult result = MessageBox.Show(
                        this,
                        string.Format("検索条件だけを残して、保存されている検索をリセットします。\r\n「{0}」", ticket_id),
                        "検索リセット",
                        MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        try
                        {
                            SearchingTicketManager.ClearTicket(ticket_id);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(string.Format("保存されている検索のリセットに失敗しました。\r\n「{0}」", ticket_id));
                        }
                    }
                }

        }

        private void buttonMakeList_Click(object sender, EventArgs e)
        {
            if (this.listViewTicket.SelectedIndices.Count >= 1)
            {
                string ticket_id = this.listViewTicket.SelectedItems[0].SubItems[COLUMN_INDEX_TICKET_ID].Text;
                this.selected_ticket_id_ = ticket_id;
                this.Close();
            }
        }

        private void listViewTicket_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right &&
                this.listViewTicket.SelectedIndices.Count >= 1)
            {
                string ticket_id = this.listViewTicket.SelectedItems[0].SubItems[COLUMN_INDEX_TICKET_ID].Text;
                this.toolStripTextBoxNewID.Text = ticket_id;
                this.toolStripMenuItemRenameTicket.Tag = ticket_id;
                this.contextMenuStripRenameTicket.Show(Cursor.Position);
            }
        }

        private void toolStripMenuItemRenameTicket_Click(object sender, EventArgs e)
        {
            ListViewItem item = this.listViewTicket.SelectedItems[0];
            string src_ticket_id = item.SubItems[COLUMN_INDEX_TICKET_ID].Text;
            string dst_ticket_id = this.toolStripTextBoxNewID.Text;

            try
            {
                SearchingTicketManager.RenameTicket(src_ticket_id, dst_ticket_id);
                item.SubItems[COLUMN_INDEX_TICKET_ID].Text = dst_ticket_id;
            }
            catch (IOException)
            {
                MessageBox.Show(string.Format("検索ID変更に失敗しました。\r\n「{0}」→「{1}」", src_ticket_id, dst_ticket_id));
            }
            catch (ArgumentException)
            {
                MessageBox.Show(string.Format("検索ID変更に失敗しました。\r\n「{0}」→「{1}」", src_ticket_id, dst_ticket_id));
            }
        }

        private void buttonOpenSaveDirectory_Click(object sender, EventArgs e)
        {
            if (this.listViewTicket.SelectedIndices.Count >= 1)
            {
                string ticket_id = this.listViewTicket.SelectedItems[0].SubItems[COLUMN_INDEX_TICKET_ID].Text;
                string directory = SearchingTicketManager.GetTicketDirectory(ticket_id);
                Process.Start(directory);
            }
        }
    }
}
