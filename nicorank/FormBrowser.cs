// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IJLib;
using NicoTools;

namespace nicorank
{
    public partial class FormBrowser : Form
    {
        public FormBrowser()
        {
            InitializeComponent();
        }

        private string current_video_id_ = "";
        private double current_start_ = -1.0;
        private double current_end_ = -1.0;
        private bool is_already_write_ = true;
        private DateTime start_time_;
        private NicoRankManager nicorank_mgr_;
        private string user_session_ = "";

        private enum NavigateKind { FromGo, FromStart, FromEnd };

        private NavigateKind navigate_kind_ = NavigateKind.FromGo;

        public void SetInputRankFilePath(string rank_file_path)
        {
            textBoxRankFilePath.Text = rank_file_path;
        }

        public void SetNicoRankMgr(NicoRankManager mgr)
        {
            nicorank_mgr_ = mgr;
        }

        public void SetUserSession(string user_session)
        {
            user_session_ = user_session;
        }

        private void buttonNavigate_Click(object sender, EventArgs e)
        {
            if (!is_already_write_)
            {
                DialogResult result = MessageBox.Show("まだ保存されていませんが続けますか？",
                    "確認", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            string url = comboBoxNavigateUrl.Text;
            try
            {
                current_video_id_ = NicoUtil.CutNicoVideoId(url);
            }
            catch (FormatException)
            {
                MessageBox.Show("動画IDまたはURLを入力してください。");
                return;
            }
            current_start_ = -1.0;
            current_end_ = -1.0;
            textBoxVideoStart.Text = "";
            textBoxVideoEnd.Text = "";
            navigate_kind_ = NavigateKind.FromGo;
            is_already_write_ = false;
            timer1.Enabled = false;
            label4.Text = "0";
            labelInfo.Text = "";

            webBrowser1.Navigate("http://www.nicovideo.jp/watch/" + current_video_id_);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string url = webBrowser1.Url.ToString();
            int index = url.IndexOf('#') + 1;
            switch (navigate_kind_)
            {
                case NavigateKind.FromGo:
                    comboBoxNavigateUrl.Text = webBrowser1.Url.ToString();
                    break;
                case NavigateKind.FromStart:
                    if (index >= 0)
                    {
                        current_start_ = double.Parse(url.Substring(index));
                        textBoxVideoStart.Text = current_start_.ToString("0.000");
                    }
                    break;
                case NavigateKind.FromEnd:
                    if (index >= 0)
                    {
                        current_end_ = double.Parse(url.Substring(index));
                        textBoxVideoEnd.Text = current_end_.ToString("0.000");
                    }
                    break;
            }
        }

        private void buttonVideoStart_Click(object sender, EventArgs e)
        {
            if (sender == buttonVideoStart)
            {
                navigate_kind_ = NavigateKind.FromStart;
                timer1.Enabled = true;
                start_time_ = DateTime.Now;
            }
            else
            {
                navigate_kind_ = NavigateKind.FromEnd;
                timer1.Enabled = false;
            }
            string url = comboBoxNavigateUrl.Text;
            string script = "javascript:{var a=$('flvplayer').GetVariable('moved_time');var b = \"" +
                url + "#\" + a;location.href=b;} void 0";
            webBrowser1.Navigate(script);
        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            string video_id;
            try
            {
                video_id = NicoUtil.CutNicoVideoId(webBrowser1.Url.ToString());
            }
            catch (FormatException)
            {
                MessageBox.Show("動画IDまたはURLを入力してください。");
                return;
            }
            UpdateCutList(video_id, textBoxVideoStart.Text, textBoxVideoEnd.Text);
            is_already_write_ = true;
        }

        private void FormBrowser_Load(object sender, EventArgs e)
        {
            if (File.Exists(textBoxRankFilePath.Text))
            {
                LoadRankFile();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label4.Text = (DateTime.Now - start_time_).TotalSeconds.ToString("0");
        }

        private void buttonLoadRankFile_Click(object sender, EventArgs e)
        {
            LoadRankFile();
        }

        private void LoadRankFile()
        {
            if (!File.Exists(textBoxRankFilePath.Text))
            {
                MessageBox.Show("ランクファイルが存在しません。");
                return;
            }
            RankFile rank_file = new RankFile(textBoxRankFilePath.Text, new RankFileCustomFormat());
            comboBoxNavigateUrl.Items.Clear();
            for (int i = 0; i < rank_file.Count; ++i)
            {
                comboBoxNavigateUrl.Items.Add(rank_file[i] + "   " + rank_file.GetVideo(i).title);
            }
            labelInfo.Text = "読み込みました";
        }

        private void UpdateCutList(string video_id, string start_time_str, string end_time_str)
        {
            if (start_time_str == "")
            {
                MessageBox.Show("開始時間を設定してください。");
                return;
            }
            if (end_time_str == "")
            {
                MessageBox.Show("終了時間を設定してください。");
                return;
            }
            if (textBoxCutListPath.Text == "")
            {
                MessageBox.Show("カットリストのファイル名を設定してください。");
                return;
            }
            StringBuilder buff = new StringBuilder();
            if (File.Exists(textBoxCutListPath.Text))
            {
                string str = IJFile.Read(textBoxCutListPath.Text);
                int index = str.IndexOf(video_id);
                if (index >= 0)
                {
                    int end = str.IndexOf('\n', index) + 1;
                    buff.Append(str.Substring(0, index));
                    buff.Append(video_id + "\t" + start_time_str + "\t" + end_time_str + "\r\n");
                    buff.Append(str.Substring(end));
                }
                else
                {
                    buff.Append(str);
                    buff.Append(video_id + "\t" + start_time_str + "\t" + end_time_str + "\r\n");
                }
            }
            else
            {
                buff.Append(video_id + "\t" + start_time_str + "\t" + end_time_str + "\r\n");
            }
            IJFile.Write(textBoxCutListPath.Text, buff.ToString());
            labelInfo.Text = "書き込みました";
        }
    }
}
