// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using IJLib;
using nicorank.FilterTester;
using NicoTools;

namespace nicorank
{
    public partial class FormMain : Form, NicoRankManagerMsgReceiver
    {
        public delegate void StringDelegate(string str);

        private int program_version_;
        private const string program_version_suffix_ = "";

        private FormChoicePic form_choice_pic_;
        private FormLayout form_layout_;
        private FormBrowser form_browser_;
        private NicoRankManager nicorank_mgr_;
        private FormTransOption form_trans_option_ = null;
        private FormFilterTester form_filter_tester_ = null;

        private string userdat_filename = "user.dat";

        private bool is_startup_success = false;

        private int mylist_new_counter = 3;
        private int adding_mylist_counter = 3;
        private int adding_mylist_desc_counter = 3;
        private int adding_tag_counter = 3;
        private int post_comment_counter = 3;

        private TransDetailOption trans_detail_option_ = new TransDetailOption();
        private CategoryManagerWithCListBox category_manager_;

        public FormMain()
        {
            
            InitializeComponent();
            category_manager_ = new CategoryManagerWithCListBox(checkedListBoxDlRankCategory);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            nicorank_mgr_ = new NicoRankManager(this);
            try
            {
                LoadConfig("config.txt");
            }
            catch (Exception)
            {
                MessageBox.Show(this, "設定ファイルの読み込みに失敗しました。OKボタンを押すと設定ファイルを読み込まずに起動します。", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (listBoxSortNew.SelectedIndex < 0)
            {
                listBoxSortNew.SelectedIndex = 0;
            }
            if (comboBoxTransFileKind.SelectedIndex < 0)
            {
                comboBoxTransFileKind.SelectedIndex = 0;
            }
            if (comboBoxRedundantSearchMethod.SelectedIndex < 0)
            {
                comboBoxRedundantSearchMethod.SelectedIndex = 0;
            }
            if (textBoxTagSearchInterval.Text == "")
            {
                textBoxTagSearchInterval.Text = "12";
            }
            if (textBoxGettingDetailInterval.Text == "")
            {
                textBoxGettingDetailInterval.Text = "0.3";
            }
            if (!SearchingTicketManager.TicketsExists())
            {
                this.buttonRestartSearch.Enabled = false;
            }
            nicorank_mgr_.GetPathMgr().SetBaseDir(Application.StartupPath);
            nicorank_mgr_.SetNoCache(checkBoxIsNoCache.Checked);

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyCopyrightAttribute copy_right_attr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));

            string version = Application.ProductVersion;
            version = version.Remove(version.Length - 4, 4); // 後ろの ".0.0" を削る
            string[] version_string_array = version.Split('.');
            program_version_ = int.Parse(version_string_array[0]) * 1000 + int.Parse(version_string_array[1]) * 10;

            labelProgramVersion.Text = "ニコニコランキングメーカー Ver " + version + program_version_suffix_;
            labelCopyRight.Text = copy_right_attr.Copyright;

            if (File.Exists(userdat_filename)) // ユーザとパスワードファイルが存在するときだけロード
            {
                string str = IJFile.Read(userdat_filename);
                string[] sArray = str.Split('\t');
            
                textBoxUser.Text = IJStringUtil.DecryptString(sArray[0], "dailyvocaran");
                textBoxPassword.Text = IJStringUtil.DecryptString(sArray[1], "dailyvocaran");
            }

            category_manager_.ParseCategoryFile2(this);

            SetPath();
            SetButtonDialog();
            is_startup_success = true;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (is_startup_success)
            {
                try
                {
                    SaveConfig("config.txt");
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "設定ファイルの書き込みに失敗しました。OKボタンを押すと終了します。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void buttonCheckLogin_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.CheckLogin, null, false);
        }

        private void buttonReloadCookie_Click(object sender, EventArgs e)
        {
            try
            {
                string profile_dir = "";
                NicoNetwork.CookieKind cookie_kind = NicoNetwork.CookieKind.None;
                if (radioButtonBrowserIE.Checked)
                {
                    cookie_kind = NicoNetwork.CookieKind.IE;
                }
                else if (radioButtonBrowserFirefox3.Checked)
                {
                    cookie_kind = NicoNetwork.CookieKind.Firefox3;
                    profile_dir = textBoxFirefoxProfileDir.Text;
                }
                else if (radioButtonBrowserOpera.Checked)
                {
                    cookie_kind = NicoNetwork.CookieKind.Opera;
                }
                else if (radioButtonBrowserChrome.Checked)
                {
                    cookie_kind = NicoNetwork.CookieKind.Chrome;
                }
                else
                {
                    return;
                }
                try
                {
                    nicorank_mgr_.ReloadCookie(cookie_kind, profile_dir);
                }
                catch (FileNotFoundException)
                {
                    textBoxInfo.AppendText("Firefox のプロファイルフォルダからデータを読み込めませんでした。" +
                        "Firefox のプロファイルフォルダの指定を確認してください。\r\n");
                    return;
                }
                if (nicorank_mgr_.GetUserSession() == "")
                {
                    throw new Exception("失敗");
                }
            }
            catch (Exception)
            {
                textBoxInfo.AppendText("読み込めませんでした。\r\n");
                return;
            }
            textBoxInfo.AppendText("読み込みました。\r\n");
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.LoginNiconico, null, false, textBoxUser.Text, textBoxPassword.Text);
        }

        private void buttonSaveAccount_Click(object sender, EventArgs e)
        {
            MakeUserFile();
            Write("ユーザー名とパスワードをファイルに保存しました。\r\n");
        }

        private void buttonGetUserSession_Click(object sender, EventArgs e)
        {
            textBoxUserSession.Text = nicorank_mgr_.GetUserSession();
        }

        private void buttonResetUserSession_Click(object sender, EventArgs e)
        {
            nicorank_mgr_.ResetUserSession(textBoxUserSession.Text);
            Write("user_session をセットしました。\r\n");
        }

        private void buttonReportVersion_Click(object sender, EventArgs e)
        {
            StringBuilder buff = new StringBuilder();
            buff.Append("機能の不具合を報告する場合は以下のテキストを送ってください。知られたくない情報は削除しても構いません。\r\n");
            buff.Append("-------------------------------------------------\r\n");
            buff.Append("バージョン: " + ((double)program_version_ / 1000.0).ToString("0.00") + program_version_suffix_ + "\r\n");
            buff.Append("OSバージョン: " + Environment.OSVersion.Platform.ToString() + "/");
            buff.Append(Environment.OSVersion.Version.ToString() + "\r\n");
            bool is_exists_ffmpeg = File.Exists(textBoxFFmpegPath.Text);
            buff.Append("FFmpeg: " + (is_exists_ffmpeg ? "有り" : "無し") + "\r\n");
            if (is_exists_ffmpeg)
            {
                FileInfo info = new FileInfo(textBoxFFmpegPath.Text);
                buff.Append("FFmpeg バージョン: " + info.LastWriteTime.ToString("yy/MM/dd") + "\r\n");
            }
            buff.Append("WaveFlt: " + (File.Exists(textBoxWavfltPath.Text) ? "有り" : "無し") + "\r\n");
            buff.Append("ファイル書き込みテスト: " + (CheckFileWrite() ? "OK" : "NG") + "\r\n");
            buff.Append("ブラウザ読込選択: ");
            if (radioButtonBrowserIE.Checked)
            {
                buff.Append("IE ");
            }
            else if (radioButtonBrowserFirefox3.Checked)
            {
                buff.Append("Firefox3 ");
            }
            else if (radioButtonBrowserOpera.Checked)
            {
                buff.Append("Opera ");
            }
            else if (radioButtonBrowserChrome.Checked)
            {
                buff.Append("Chrome ");
            }
            else
            {
                buff.Append("なし ");
            }
            buff.Append("\r\n");
            buff.Append("-------------------------------------------------\r\n");
            Write(buff.ToString());
        }

        private void buttonRankDl_Click(object sender, EventArgs e)
        {
            DownloadKind download_kind = new DownloadKind();
            download_kind.SetDuration(checkBoxDlRankDurationTotal.Checked,
                checkBoxDlRankDurationMonthly.Checked, 
                checkBoxDlRankDurationWeekly.Checked, 
                checkBoxDlRankDurationDaily.Checked, 
                checkBoxDlRankDurationHourly.Checked);
            download_kind.SetTarget(false, checkBoxDlRankView.Checked, checkBoxDlRankRes.Checked, checkBoxDlRankMylist.Checked); // 「総合」には未対応

            download_kind.CategoryList = category_manager_.GetDownloadCategoryItemList();

            if (radioButtonDlRankRss.Checked)
            {
                download_kind.IsRss = true;
            }
            //int hour = -1;
            if (checkBoxRankingDlSetHour.Checked)
            {
                // 一時的に無効
                //hour = int.Parse(textBoxRankingDlHour.Text);
            }
            StartThread(nicorank_mgr_.DownloadRanking, null, false, download_kind);
        }

        private void buttonNicoChartDl_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.DownloadNicoChart, null, false, dateTimePickerDownloadNicoChartStart.Value,
                dateTimePickerDownloadNicoChartEnd.Value);
        }

        private void buttonFlvDl_Click(object sender, EventArgs e)
        {
            if (textBoxDlInterval.Text == "")
            {
                textBoxDlInterval.Text = "30";
            }
            double interval_min = 30.0;
            double interval_max = 30.0;
            try
            {
                IJStringUtil.ParseDlInterval(textBoxDlInterval.Text, ref interval_min, ref interval_max);
            }
            catch (FormatException)
            {
                textBoxInfo.AppendText("FLVのDL間隔の指定が正しくありません。\r\n");
                return;
            }
            if (interval_min < 30.0)
            {
                textBoxInfo.AppendText("FLVのDL間隔は30秒未満にはできません。\r\n");
                return;
            }

            StartThread(nicorank_mgr_.DownloadFlv, null, false, GetInputOutputOption(), textBoxDlInterval.Text, checkBoxIsFixFlvDlExtension.Checked);
        }

        private void buttonThumbnailDl_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.DownloadThumbnail, null, false, GetInputOutputOption());
        }

        private void buttonWatchFilter_Click(object sender, EventArgs e)
        {
            string filter_path = nicorank_mgr_.GetPathMgr().GetFullPath(textBoxFilterPath.Text);
            if (File.Exists(filter_path))
            {
                System.Diagnostics.Process.Start(filter_path);
            }
            else
            {
                textBoxInfo.AppendText("ファイルが見つかりません。\r\n");
            }
        }

        private void buttonTagSearchNew_Click(object sender, EventArgs e)
        {
            if (this.buttonTagSearchNew.Text == "中止")
            {
                nicorank_mgr_.Cancel();
                return;
            }

            if (!CheckTagSearchInterval() || !CheckGettingDetailInterval())
            {
                return;
            }

            int page_start;
            int page_end;

            if (textBoxTagSearchPageStart.Text != "")
            {
                if (!int.TryParse(textBoxTagSearchPageStart.Text, out page_start) || page_start <= 0)
                {
                    Write("ページ数には1以上の数字を指定する必要があります。\r\n");
                    return;
                }
            }
            else
            {
                page_start = 1;
            }
            if (textBoxTagSearchPageEnd.Text != "")
            {
                if (!int.TryParse(textBoxTagSearchPageEnd.Text, out page_end) || page_end <= 0)
                {
                    Write("ページ数には1以上の数字を指定する必要があります。\r\n");
                    return;
                }
            }
            else
            {
                page_end = int.MaxValue;
            }
            if (textBoxTagSearchPageStart.Text != "" && textBoxTagSearchPageEnd.Text != "")
            {
                if (page_start > page_end)
                {
                    int temp = page_start;
                    page_start = page_end;
                    page_end = temp;
                    textBoxTagSearchPageStart.Text = page_start.ToString();
                    textBoxTagSearchPageEnd.Text = page_end.ToString();
                }
            }

            SearchingTagOption searching_tag_option = new SearchingTagOption();
            searching_tag_option.is_searching_get_kind_api = radioButtonSearchGetKindAPI.Checked;
            searching_tag_option.SetTagList(textBoxTagNew.Text);
            searching_tag_option.is_searching_kind_tag = radioButtonSearchKindTag.Checked;
            searching_tag_option.is_detail_getting = checkBoxIsGettingDetailNew.Checked;
            searching_tag_option.detail_info_lower = (int)numericUpDownConditionMylistNew.Value;
            searching_tag_option.sort_kind_num = listBoxSortNew.SelectedIndex;
            searching_tag_option.is_page_all = radioButtonTagSearchPageAll.Checked;
            searching_tag_option.page_start = page_start;
            searching_tag_option.page_end = page_end;
            searching_tag_option.is_using_condition = checkBoxTagSearchIsUsingCondition.Checked;
            searching_tag_option.condition_lower = IJStringUtil.ToNumberWithDef(textBoxTagSearchLower.Text, 0);
            searching_tag_option.condition_upper = IJStringUtil.ToNumberWithDef(textBoxTagSearchUpper.Text, int.MaxValue);
            searching_tag_option.date_from = dateTimePickerTagSearchFrom.Value;
            searching_tag_option.date_to = dateTimePickerTagSearchTo.Value;
            searching_tag_option.searching_interval = textBoxTagSearchInterval.Text;
            searching_tag_option.getting_detail_interval = textBoxGettingDetailInterval.Text;
            searching_tag_option.is_create_ticket = checkBoxSaveSearch.Checked;
            if (searching_tag_option.is_searching_get_kind_api) // API検索のときは複数回検索はしない
            {
                searching_tag_option.SetRedundantSearchMethod(0);
            }
            else
            {
                searching_tag_option.SetRedundantSearchMethod(comboBoxRedundantSearchMethod.SelectedIndex);
            }
            searching_tag_option.is_sending_user_session = checkBoxIsSendingUserSession.Checked;

            // 処理の最初にボタンのテキストを「中止」にする
            NicoRankManager.ThreadStarterDelegate ts_delegate = delegate {
                MethodInvoker invoker = delegate {
                    this.buttonTagSearchNew.Text = "中止";
                };
                this.Invoke(invoker);
            };
            if (searching_tag_option.is_create_ticket == true)
            {
                searching_tag_option.ticket_id = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                ts_delegate += delegate
                {
                    SearchingTicketManager.CreateNewTicket(searching_tag_option.ticket_id, searching_tag_option);
                    MethodInvoker invoker = delegate
                    {
                        this.buttonRestartSearch.Enabled = true;
                    };
                    this.Invoke(invoker);
                };
            }
            ts_delegate += nicorank_mgr_.MakeListAndWriteBySearchTag;

            StartThread(ts_delegate, ChangeSearchButtonText, true, GetInputOutputOption(), searching_tag_option, GetRankingMethod());
        }

        private void buttonSelectSearchingTicket_Click(object sender, EventArgs e)
        {
            using (FormSearchingTicketSelector form = new FormSearchingTicketSelector())
            {
                form.ShowDialog(this);

                // 全ての検索チケットが削除されていたら、「再開」ボタンを無効にする
                if (!SearchingTicketManager.TicketsExists())
                {
                    this.buttonRestartSearch.Enabled = false;
                }

                // 検索の場合はnull以外が設定されている
                string searching_ticket_id = form.SelectedTicketID;
                if (searching_ticket_id != null)
                {
                    if (!CheckTagSearchInterval() || !CheckGettingDetailInterval())
                    {
                        return;
                    }

                    SearchingTagOption searching_tag_option = SearchingTicketManager.GetOption(searching_ticket_id);
                    searching_tag_option.is_create_ticket = false;
                    NicoRankManager.ThreadStarterDelegate ts_delegate = delegate
                    {
                        MethodInvoker invoker = delegate
                        {
                            this.buttonTagSearchNew.Text = "中止";
                        };
                        this.Invoke(invoker);
                    };
                    ts_delegate += nicorank_mgr_.MakeListAndWriteBySearchTag;
                    StartThread(ts_delegate, ChangeSearchButtonText, true, GetInputOutputOption(), searching_tag_option, GetRankingMethod());
                }
            }
        }

        private void buttonAnalyzeRanking_Click(object sender, EventArgs e)
        {
            NicoListManager.ParseRankingKind kind = NicoListManager.ParseRankingKind.TermPoint;
            if (radioButtonTotalPoint.Checked)
            {
                kind = NicoListManager.ParseRankingKind.TotalPoint;
            }
            StartThread(nicorank_mgr_.AnalyzeRanking, null, true, GetInputOutputOption(), GetRankingMethod(), kind);
        }

        private void buttonMylistSearch_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MylistSearch, null, true, textBoxMylistUrl.Text, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonUpdateRankFileByMylist_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.UpdateRankFileByMylist, null, true, textBoxMylistUrl.Text, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonNewArrival_Click(object sender, EventArgs e)
        {
            if (textBoxNewArrivalStart.Text == "")
            {
                textBoxNewArrivalStart.Text = "1";
            }
            if (textBoxNewArrivalEnd.Text == "")
            {
                textBoxNewArrivalEnd.Text = "10";
            }
            int start, end;
            try
            {
                start = int.Parse(textBoxNewArrivalStart.Text);
                end = int.Parse(textBoxNewArrivalEnd.Text);
            }
            catch (FormatException)
            {
                textBoxInfo.AppendText("ページ数には数字を指定する必要があります。\r\n");
                return;
            }
            StartThread(nicorank_mgr_.GetNewArrival, null, true, start, end, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonAnalyzeNicoChart_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.AnalyzeRankingNicoChart, null, true, GetInputOutputOption(), GetRankingMethod(),
                dateTimePickerAnalyzeNicoChartStart.Value,
                dateTimePickerAnalyzeNicoChartEnd.Value);
        }

        private void buttonSort_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.SortRankFile, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonUpdateDetail_Click(object sender, EventArgs e)
        {
            if (!CheckGettingDetailInterval())
            {
                return;
            }
            UpdateRankKind update_rank_kind = radioButtonDetailAddingTag.Checked ? UpdateRankKind.AddingTag :
                (radioButtonDetailTag.Checked ? UpdateRankKind.Tag :
                (radioButtonDetailExceptPoint.Checked ? UpdateRankKind.ExceptPoint : UpdateRankKind.All));
            StartThread(nicorank_mgr_.UpdateDetailInfo, null, true, GetInputOutputOption(), update_rank_kind, GetRankingMethod(), textBoxGettingDetailInterval.Text);
        }

        private void buttonGetDetail_Click(object sender, EventArgs e)
        {
            if (!CheckGettingDetailInterval())
            {
                return;
            }

            StartThread(nicorank_mgr_.GetDetailInfo, null, true, NicoUtil.GetNicoIdList(IJStringUtil.SplitWithCRLF(textBoxAddingIdList.Text)),
                GetInputOutputOption(), false, GetRankingMethod(), textBoxGettingDetailInterval.Text);
        }

        private void buttonAddToRankFile_Click(object sender, EventArgs e)
        {
            if (!CheckGettingDetailInterval())
            {
                return;
            }

            StartThread(nicorank_mgr_.GetDetailInfo, null, true, NicoUtil.GetNicoIdList(IJStringUtil.SplitWithCRLF(textBoxAddingIdList.Text)),
                GetInputOutputOption(), true, GetRankingMethod(), textBoxGettingDetailInterval.Text);
        }

        private void buttonExchangeCheckRankFile_Click(object sender, EventArgs e)
        {
            string temp = textBoxDiff1Path.Text;
            textBoxDiff1Path.Text = textBoxDiff2Path.Text;
            textBoxDiff2Path.Text = temp;
        }

        private void buttonMakeDiff_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeDiff, null, true, GetInputOutputOption(), GetRankingMethod(),
                (checkBoxIsExclusionDiff.Checked ? dateTimePickerDiffExclusionDate.Value : new DateTime(1999, 1, 1)));
        }

        private void buttonMakeDiffB_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeDiffB, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonMakeDup_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeDup, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonMergeRankFileA_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MergeRankFileA, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonMergeRankFileB_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MergeRankFileB, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonUpdatePoint_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.UpdatePoint, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonCalculateSum_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.CalculateSum, null, true, GetInputOutputOption(), GetRankingMethod());
        }

        private void buttonOpenTransDetailOption_Click(object sender, EventArgs e)
        {
            if (form_trans_option_ == null)
            {
                form_trans_option_ = new FormTransOption();
                form_trans_option_.SetAppOption(trans_detail_option_);
                form_trans_option_.SetFormMain(this);
                form_trans_option_.Show(this);
            }
        }

        private void buttonTransShowingCand_Click(object sender, EventArgs e)
        {
            FormShowingCand form_cand = new FormShowingCand();
            form_cand.SetTransOption(GetFFmpegOption());
            form_cand.Show(this);
        }

        private void buttonTrans_Click(object sender, EventArgs e)
        {
            if (CheckFFmpeg())
            {
                StartThread(nicorank_mgr_.TranslateVideo, null, false, GetFFmpegOption());
            }
        }

        private void buttonMakeRank_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.DrawRankPic, null, false, GetInputOutputOption());
        }

        private void buttonChoicePic_Click(object sender, EventArgs e)
        {
            SetPath();
            form_choice_pic_ = new FormChoicePic();
            form_choice_pic_.SetPath(GetInputOutputOption(), nicorank_mgr_.GetPathMgr().GetTransAfterDir());
            form_choice_pic_.Show();
        }

        private void buttonFont_Click(object sender, EventArgs e)
        {
            System.Drawing.Text.InstalledFontCollection ifc =
                new System.Drawing.Text.InstalledFontCollection();
            FontFamily[] ffs = ifc.Families;

            foreach (FontFamily ff in ffs)
            {
                if (ff.IsStyleAvailable(FontStyle.Regular))
                {
                    Font fnt = new Font(ff, 8);
                    textBoxFont.AppendText(fnt.Name + "\r\n");
                    fnt.Dispose();
                }
            }
        }

        private void buttonLayout_Click(object sender, EventArgs e)
        {
            SetPath();
            form_layout_ = new FormLayout();
            form_layout_.SetLayoutFile(nicorank_mgr_.GetPathMgr().GetLayoutPath());
            form_layout_.SetRankFile(nicorank_mgr_.GetPathMgr().GetInputRankFilePath());
            form_layout_.Show(this);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxInfo.Clear();
        }

        private void buttonMakingScript_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeScript, null, false, GetInputOutputOption());
        }

        private void buttonMylistNew_Click(object sender, EventArgs e)
        {
            if (radioButtonMylistUpdate.Checked && textBoxMylistUpdateId.Text == "")
            {
                MessageBox.Show("マイリストIDを入力してください。", "確認");
            }
            else if (textBoxMylistName.Text == "")
            {
                MessageBox.Show("マイリスト名を入力してください。", "確認");
            }
            else
            {
                if (mylist_new_counter > 0)
                {
                    --mylist_new_counter;

                    if (comboBoxMylistNewOrder.SelectedIndex < 0)
                    {
                        comboBoxMylistNewOrder.SelectedIndex = 1;
                    }
                    if (comboBoxMylistNewColor.SelectedIndex < 0)
                    {
                        comboBoxMylistNewColor.SelectedIndex = 0;
                    }

                    if (radioButtonMylistUpdate.Checked)
                    {
                        StartThread(nicorank_mgr_.UpdateMylistGroup, null, false, textBoxMylistUpdateId.Text, radioButtonMylistPublic.Checked,
                            textBoxMylistName.Text, textBoxMylistUpdateDescription.Text,
                            comboBoxMylistNewOrder.SelectedIndex, comboBoxMylistNewColor.SelectedIndex);
                    }
                    else
                    {
                        StartThread(nicorank_mgr_.MakeNewMylistGroup, null, false, radioButtonMylistPublic.Checked,
                            textBoxMylistName.Text, textBoxMylistUpdateDescription.Text,
                            comboBoxMylistNewOrder.SelectedIndex, comboBoxMylistNewColor.SelectedIndex);
                    }
                }
                if (mylist_new_counter <= 0)
                {
                    buttonMylistNew.Enabled = false;
                }
            }
        }

        private void buttonAddingMylist_Click(object sender, EventArgs e)
        {
            if (adding_mylist_counter > 0)
            {
                --adding_mylist_counter;
                string mylist_id = GetMylistIdFromUrl(textBoxMylistId.Text);
                if (mylist_id == "")
                {
                    MessageBox.Show("マイリストURLが正しくありません。", "確認");
                    return;
                }
                StartThread(nicorank_mgr_.AddMylist, null, false, GetInputOutputOption(), mylist_id);
            }
            if (adding_mylist_counter <= 0)
            {
                buttonAddingMylist.Enabled = false;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            nicorank_mgr_.Cancel();
        }

        private void buttonUpdateMylistDescription_Click(object sender, EventArgs e)
        {
            if (adding_mylist_desc_counter > 0)
            {
                string mylist_id = GetMylistIdFromUrl(textBoxMylistId.Text);
                if (mylist_id == "")
                {
                    MessageBox.Show("マイリストURLが正しくありません。", "確認");
                    return;
                }
                --adding_mylist_desc_counter;
                StartThread(nicorank_mgr_.UpdateMylistDescription, null, false, mylist_id, textBoxMylistDescription.Text, GetInputOutputOption());
            }
            if (adding_mylist_desc_counter <= 0)
            {
                buttonUpdateMylistDescription.Enabled = false;
            }
        }

        private void checkBoxTimer_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == checkBoxTimer1)
            {
                if (!checkBoxTimer1.Checked)
                {
                    labelTimer1.Text = "--";
                }
            }
            if (sender == checkBoxTimer2)
            {
                if (!checkBoxTimer2.Checked)
                {
                    labelTimer2.Text = "--";
                }
            }
            timer1.Enabled = checkBoxTimer1.Checked || checkBoxTimer2.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBoxTimer1.Checked)
            {
                DateTime dt = dateTimePickerTimer1.Value;
                if (DateTime.Now >= dt)
                {
                    checkBoxTimer1.Checked = false;
                    labelTimer1.Text = "--";
                    DoTimerCommand(comboBoxTimer1.SelectedIndex);
                }
                else
                {
                    labelTimer1.Text = GetExecTimeString(dt - DateTime.Now);
                }
            }
            if (checkBoxTimer2.Checked)
            {
                DateTime dt = dateTimePickerTimer2.Value;
                if (DateTime.Now >= dt)
                {
                    checkBoxTimer2.Checked = false;
                    labelTimer2.Text = "--";
                    DoTimerCommand(comboBoxTimer2.SelectedIndex);
                }
                else
                {
                    labelTimer2.Text = GetExecTimeString(dt - DateTime.Now);
                }
            }
        }

        private void buttonBrowserStart_Click(object sender, EventArgs e)
        {
            SetPath();
            form_browser_ = new FormBrowser();
            form_browser_.SetNicoRankMgr(nicorank_mgr_);
            form_browser_.SetInputRankFilePath(nicorank_mgr_.GetPathMgr().GetFullPath(textBoxInputRankFilePath.Text));
            string user_session = nicorank_mgr_.GetUserSession();
            if (user_session != "")
            {
                form_browser_.SetUserSession(user_session);
            }
            form_browser_.Show();
        }

        private void buttonFFmpegExec_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.FFmpegExec, null, false, textBoxFFmpegArgument.Text);
        }

        private void textBoxFlvDlDir_TextChanged(object sender, EventArgs e)
        {
            labelTransBefore.Text = textBoxFlvDlDir.Text;
        }

        private void buttonAddTag_Click(object sender, EventArgs e)
        {
            if (textBoxTagVideoId.Text == "")
            {
                MessageBox.Show("動画IDが設定されていません。", "確認");
                return;
            }
            if (adding_tag_counter > 0)
            {
                --adding_tag_counter;
                List<string> tag_list = new List<string>();
                List<bool> is_lock_list = new List<bool>();

                MakeCommentList(out tag_list, out is_lock_list);
                StartThread(nicorank_mgr_.AddTags, null, false, tag_list, is_lock_list, textBoxTagVideoId.Text);
            }
            if (adding_tag_counter <= 0)
            {
                buttonAddTag.Enabled = false;
            }
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            string filename = GetDropFilename(e);
            if (filename != "")
            {
                ((TextBox)sender).Text = filename;
            }
        }

        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private string GetDropFilename(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    return files[0];
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        private void buttonOpenDialog_Click(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)((Button)sender).Tag;
            string old_path = Directory.GetCurrentDirectory();
            string initial_dir = textBox.Text;

            if (initial_dir != "")
            {
                if (!(initial_dir.Length >= 2 && initial_dir[1] == ':')) // 絶対パスでないなら
                {
                    initial_dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), initial_dir);
                }
                if (!Directory.Exists(initial_dir))
                {
                    try
                    {
                        initial_dir = Path.GetDirectoryName(initial_dir);
                    }
                    catch (ArgumentException) { }
                    catch (PathTooLongException) { }
                } 
            }
            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = initial_dir;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox.Text = openFileDialog1.FileName;
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void buttonSaveDialog_Click(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)((Button)sender).Tag;
            string old_path = Directory.GetCurrentDirectory();
            string initial_dir = textBox.Text;

            if (initial_dir != "")
            {
                if (!(initial_dir.Length >= 2 && initial_dir[1] == ':')) // 絶対パスでないなら
                {
                    initial_dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), initial_dir);
                }
                if (!Directory.Exists(initial_dir))
                {
                    try
                    {
                        initial_dir = Path.GetDirectoryName(initial_dir);
                    }
                    catch (ArgumentException) { }
                    catch (PathTooLongException) { }
                }
            }
            saveFileDialog1.FileName = "";
            saveFileDialog1.InitialDirectory = initial_dir;
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox.Text = saveFileDialog1.FileName;
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void buttonFolderDialog_Click(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)((Button)sender).Tag;
            string old_path = Directory.GetCurrentDirectory();
            string initial_dir = textBox.Text;

            if (initial_dir != "")
            {
                if (!(initial_dir.Length >= 2 && initial_dir[1] == ':')) // 絶対パスでないなら
                {
                    initial_dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), initial_dir);
                }
                if (!Directory.Exists(initial_dir))
                {
                    try
                    {
                        initial_dir = Path.GetDirectoryName(initial_dir);
                    }
                    catch (ArgumentException) { }
                    catch (PathTooLongException) { }
                }
            }
            folderBrowserDialog1.SelectedPath = initial_dir;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox.Text = folderBrowserDialog1.SelectedPath;
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void buttonConfirmSharp_Click(object sender, EventArgs e)
        {
            textBoxInfo.AppendText(NicoUtil.GetReplacedString(textBoxConfirmSharp.Text, new string[] { textBoxConfirmSharpValue.Text }) + "\r\n");
        }

        private void buttonMylistDescriptionPreview_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.PreviewMylistDescription, null, false, textBoxMylistDescription.Text);
        }

        private void buttonComment_Click(object sender, EventArgs e)
        {
            if (textBoxTagVideoId.Text == "")
            {
                MessageBox.Show("動画IDが設定されていません。", "確認");
                return;
            }
            if (textBoxComment.Text == "")
            {
                MessageBox.Show("コメントが設定されていません。", "確認");
                return;
            }
            if (textBoxCommentTime.Text == "")
            {
                MessageBox.Show("コメント時間が設定されていません。", "確認");
                return;
            }
            if (post_comment_counter > 0)
            {
                --post_comment_counter;
                StartThread(nicorank_mgr_.PostComment, null, false, textBoxTagVideoId.Text, textBoxComment.Text, IJStringUtil.ToDoubleWithDef(textBoxCommentTime.Text, 0.0));
            }
            if (post_comment_counter <= 0)
            {
                buttonComment.Enabled = false;
            }
        }

        private void checkBoxTransIsCut_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTransCutStart.Enabled = checkBoxTransIsCut.Checked;
            textBoxTransCutEnd.Enabled = checkBoxTransIsCut.Checked;
            labelTransCut1.Enabled = checkBoxTransIsCut.Checked;
            labelTransCut2.Enabled = checkBoxTransIsCut.Checked;
            if (checkBoxTransIsCut.Checked)
            {
                checkBoxIsUsingCutList.Checked = false;
            }
        }

        private void checkBoxTransIsFadeIn_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTransFadeIn.Enabled = checkBoxTransIsFadeIn.Checked;
            labelTransFadeIn.Enabled = checkBoxTransIsFadeIn.Checked;
        }

        private void checkBoxTransIsFadeOut_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTransFadeOut.Enabled = checkBoxTransIsFadeOut.Checked;
            labelTransFadeOut.Enabled = checkBoxTransIsFadeOut.Checked;
        }

        private void checkBoxIsFlvToAvi_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxTransIncludeWav.Enabled = checkBoxIsFlvToAvi.Checked;
        }

        private void checkBoxIsExclusionDiff_CheckedChanged(object sender, EventArgs e)
        {
            labelIsExclusionDiff.Enabled = checkBoxIsExclusionDiff.Checked;
            dateTimePickerDiffExclusionDate.Enabled = checkBoxIsExclusionDiff.Checked;
        }

        private void checkBoxFilter_CheckedChanged(object sender, EventArgs e)
        {
            textBoxFilterPath.Enabled = checkBoxFilter.Checked;
            buttonWatchFilter.Enabled = checkBoxFilter.Checked;
            buttonSelectFilterPath.Enabled = checkBoxFilter.Checked;
            checkBoxIsOutputFilteredVideo.Enabled = checkBoxFilter.Checked;
        }

        private void checkBoxIsSameToInput_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxIsSameToInput.Checked)
            {
                radioButtonOutputToRankFile.Enabled = false;
                radioButtonOutputToTextBox.Enabled = false;
                textBoxOutputRankFilePath.Enabled = false;
                buttonSelectOutputRankFilePath.Enabled = false;
                textBoxOutputRank.Enabled = false;
                radioButtonInputFromRankFile.Checked = true;
                radioButtonInputFromTextBox.Enabled = false;
            }
            else
            {
                radioButtonOutputToRankFile.Enabled = true;
                radioButtonOutputToTextBox.Enabled = true;
                textBoxOutputRankFilePath.Enabled = radioButtonOutputToRankFile.Checked;
                buttonSelectOutputRankFilePath.Enabled = radioButtonOutputToRankFile.Checked;
                textBoxOutputRank.Enabled = !radioButtonOutputToRankFile.Checked;
                radioButtonInputFromTextBox.Enabled = true;
            }
        }

        private void radioButtonInputFromRankFile_CheckedChanged(object sender, EventArgs e)
        {
            textBoxInputRankFilePath.Enabled = radioButtonInputFromRankFile.Checked;
            buttonSelectInputRankFilePath.Enabled = radioButtonInputFromRankFile.Checked;
            textBoxInputRank.Enabled = !radioButtonInputFromRankFile.Checked;
        }

        private void radioButtonOutputToRankFile_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxIsSameToInput.Checked) // このガードが無いと起動時に矛盾が生じる
            {
                textBoxOutputRankFilePath.Enabled = radioButtonOutputToRankFile.Checked;
                buttonSelectOutputRankFilePath.Enabled = radioButtonOutputToRankFile.Checked;
                textBoxOutputRank.Enabled = !radioButtonOutputToRankFile.Checked;
            }
        }

        private void checkBoxIsUsingCutList_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxIsUsingCutList.Checked)
            {
                checkBoxTransIsCut.Checked = false;
            }
            else
            {
                textBoxTransCutStart.Enabled = checkBoxTransIsCut.Checked;
                textBoxTransCutEnd.Enabled = checkBoxTransIsCut.Checked;
            }
            //checkBoxTransIsCut.Enabled = !checkBoxIsUsingCutList.Checked;
            textBoxCutListPath.Enabled = checkBoxIsUsingCutList.Checked;
            buttonSelectCutListPath.Enabled = checkBoxIsUsingCutList.Checked;
        }

        private void checkBoxIsRankFileCustomize_CheckedChanged(object sender, EventArgs e)
        {
            textBoxInputRankFileFormat.Enabled = checkBoxIsRankFileCustomize.Checked;
            textBoxOutputRankFileFormat.Enabled = checkBoxIsRankFileCustomize.Checked;
            labelInputRankFileFormat.Enabled = checkBoxIsRankFileCustomize.Checked;
            labelOutputRankFileFormat.Enabled = checkBoxIsRankFileCustomize.Checked;
        }

        private void radioButtonBrowser_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (((RadioButton)sender).Checked)
                {
                    NicoNetwork.CookieKind cookie_kind = NicoNetwork.CookieKind.None;
                    if (radioButtonBrowserIE.Checked)
                    {
                        cookie_kind = NicoNetwork.CookieKind.IE;
                    }
                    else if (radioButtonBrowserFirefox3.Checked)
                    {
                        cookie_kind = NicoNetwork.CookieKind.Firefox3;
                    }
                    else if (radioButtonBrowserOpera.Checked)
                    {
                        cookie_kind = NicoNetwork.CookieKind.Opera;
                    }
                    nicorank_mgr_.SetCookieKind(cookie_kind);
                }
            }
            catch (Exception) { }
        }

        private void buttonMakeScriptAndAvi_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeScriptAndAvi, null, false);
        }

        private void buttonMakeAviFromScript_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.MakeAviFromScript, null, false);
        }

        private void listBoxSortNew_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] condition_label_text = { "", "再生数が", "", "コメント数が", "マイリスト数が" };

            switch (listBoxSortNew.SelectedIndex)
            {
                case 0: case 1:
                    checkBoxTagSearchIsUsingCondition.Enabled = true;
                    ChangeTagSearchConditionEnabled(checkBoxTagSearchIsUsingCondition.Checked);
                    labelTagSearchFrom.Text = "から";
                    labelTagSearchTo.Text = "まで";
                    labelTagSearchCondition.Text = "投稿日時が";
                    labelTagSearchFrom.Left = 468;
                    labelTagSearchTo.Left = 468;
                    dateTimePickerTagSearchFrom.Show();
                    dateTimePickerTagSearchTo.Show();
                    textBoxTagSearchLower.Hide();
                    textBoxTagSearchUpper.Hide();
                    labelTagSearchUnlimited.Hide();
                    break;
                case 2: case 3: case 6: case 7: case 8: case 9:
                    checkBoxTagSearchIsUsingCondition.Enabled = true;
                    ChangeTagSearchConditionEnabled(checkBoxTagSearchIsUsingCondition.Checked);
                    labelTagSearchFrom.Text = "以上";
                    labelTagSearchTo.Text = "以下";
                    labelTagSearchCondition.Text = condition_label_text[listBoxSortNew.SelectedIndex / 2];
                    labelTagSearchFrom.Left = 388;
                    labelTagSearchTo.Left = 388;
                    dateTimePickerTagSearchFrom.Hide();
                    dateTimePickerTagSearchTo.Hide();
                    textBoxTagSearchLower.Show();
                    textBoxTagSearchUpper.Show();
                    labelTagSearchUnlimited.Show();
                    break;
                case 4: case 5: case 10: case 11:
                    checkBoxTagSearchIsUsingCondition.Enabled = false;
                    ChangeTagSearchConditionEnabled(false);
                    break;
            }
        }

        private void ChangeTagSearchConditionEnabled(bool enabled)
        {
            labelTagSearchFrom.Enabled = enabled;
            labelTagSearchTo.Enabled = enabled;
            labelTagSearchCondition.Enabled = enabled;
            dateTimePickerTagSearchFrom.Enabled = enabled;
            dateTimePickerTagSearchTo.Enabled = enabled;
            textBoxTagSearchLower.Enabled = enabled;
            textBoxTagSearchUpper.Enabled = enabled;
            labelTagSearchUnlimited.Enabled = enabled;
        }

        private void radioButtonTagSearchPage_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTagSearchPageStart.Enabled = radioButtonTagSearchPagePart.Checked;
            textBoxTagSearchPageEnd.Enabled = radioButtonTagSearchPagePart.Checked;
            labelTagSearchPage1.Enabled = radioButtonTagSearchPagePart.Checked;
            labelTagSearchPage2.Enabled = radioButtonTagSearchPagePart.Checked;
        }

        private void checkBoxTagSearchIsUsingCondition_CheckedChanged(object sender, EventArgs e)
        {
            ChangeTagSearchConditionEnabled(checkBoxTagSearchIsUsingCondition.Checked);
        }

        private void checkBoxIsGettingDetailNew_CheckedChanged(object sender, EventArgs e)
        {
            labelGettingDetail1.Enabled = checkBoxIsGettingDetailNew.Checked;
            labelGettingDetail2.Enabled = checkBoxIsGettingDetailNew.Checked;
            numericUpDownConditionMylistNew.Enabled = checkBoxIsGettingDetailNew.Checked;
        }

        private void textBoxTransChangeSize_Validating(object sender, CancelEventArgs e)
        {
            textBoxForNumber0_Validating(sender, e);
            if (((TextBox)sender).Text != "" && !e.Cancel)
            {
                int value = int.Parse(((TextBox)sender).Text); // 数字であることが保証されている
                if (value % 2 != 0)
                {
                    errorProvider1.SetError((TextBox)sender, "サイズは偶数である必要があります。");
                    e.Cancel = true;
                }
            }
        }

        private void textBoxForNumber1_Validating(object sender, CancelEventArgs e)
        {
            Regex regex = new Regex("^[1-9][0-9]*$");
            if (((TextBox)sender).Text != "" && !regex.IsMatch(((TextBox)sender).Text))
            {
                errorProvider1.SetError((TextBox)sender, "1以上の数字を入力する必要があります。");
                e.Cancel = true;
            }
        }

        private void textBoxForNumber0_Validating(object sender, CancelEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            if (((TextBox)sender).Text != "" && !regex.IsMatch(((TextBox)sender).Text))
            {
                errorProvider1.SetError((TextBox)sender, "0以上の数字を入力する必要があります。");
                e.Cancel = true;
            }
        }

        private void textBoxForDouble_Validating(object sender, CancelEventArgs e)
        {
            double value;
            if (((TextBox)sender).Text != "" && !double.TryParse(((TextBox)sender).Text, out value))
            {
                errorProvider1.SetError((TextBox)sender, "0以上の整数または小数を入力する必要があります。");
                e.Cancel = true;
            }
        }

        private void textBox_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError((TextBox)sender, null);
        }

        private void buttonChangeExtension_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.RenameFlvInDirectory, null, false);
        }

        private void comboBoxTransFileKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTransFileKind.SelectedIndex <= 1)
            {
                labelTransBefore2.Text = "変換元フォルダ";
                labelTransAfter3.Text = "変換先フォルダ";
                labelTransAfter2.Visible = false;
                buttonSelectTransBeforeFileOrDir.Click -= buttonOpenDialog_Click;
                buttonSelectTransBeforeFileOrDir.Click -= buttonFolderDialog_Click;
                buttonSelectTransBeforeFileOrDir.Click += buttonFolderDialog_Click;
                buttonSelectTransAfterDir.Click -= buttonSaveDialog_Click;
                buttonSelectTransAfterDir.Click -= buttonFolderDialog_Click;
                buttonSelectTransAfterDir.Click += buttonFolderDialog_Click;
            }
            else
            {
                labelTransBefore2.Text = "変換元ファイル";
                labelTransAfter3.Text = "変換先ファイル";
                labelTransAfter2.Visible = true;
                buttonSelectTransBeforeFileOrDir.Click -= buttonFolderDialog_Click;
                buttonSelectTransBeforeFileOrDir.Click += buttonOpenDialog_Click;
                buttonSelectTransAfterDir.Click -= buttonFolderDialog_Click;
                buttonSelectTransAfterDir.Click += buttonSaveDialog_Click;
            }
        }

        private void buttonSaveConfig_Click(object sender, EventArgs e)
        {
            string old_path = Directory.GetCurrentDirectory();
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    SaveConfig(saveFileDialog1.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "保存に失敗しました。");
                    return;
                }
                MessageBox.Show(this, "保存しました。");
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void buttonLoadConfig_Click(object sender, EventArgs e)
        {
            string old_path = Directory.GetCurrentDirectory();
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    LoadConfig(openFileDialog1.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "読み込みに失敗しました。");
                    return;
                }
                MessageBox.Show(this, "読み込みました。");
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void buttonShowFilterTester_Click(object sender, EventArgs e)
        {
            if (form_filter_tester_ == null || form_filter_tester_.IsDisposed)
            {
                form_filter_tester_ = new FormFilterTester();
                form_filter_tester_.Show();
            }
        }

        private void checkBoxIsNoCache_CheckedChanged(object sender, EventArgs e)
        {
            nicorank_mgr_.SetNoCache(checkBoxIsNoCache.Checked);
        }

        private void buttonCustomRankFileDefault_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "デフォルト設定に置き換えます。よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                string custom_format_text = "separator=\"\\t\"\r\n" +
                    "<video_id/>\r\n" +
                    "<line_number/>\r\n" +
                    "<view/>\r\n" +
                    "<res/>\r\n" +
                    "<mylist/>\r\n" +
                    "<expression val=\"(view+mylist)/(view+res+mylist)\" seido=\"2\"/>\r\n" +
                    "<expression val=\"mylist*100/view\" seido=\"2\"/>\r\n" +
                    "<expression val=\"view+";
                if (radioButtonHoseiVocaran.Checked)
                {
                    custom_format_text += "int(res*int((view+mylist)*100/(view+res+mylist))/100)";
                }
                else
                {
                    custom_format_text += "res";
                }
                custom_format_text += "+mylist*" + numericUpDownMylistRate.Value.ToString() + "\"/>\r\n" +
                    "<title/>\r\n" +
                    "<date format=\"yyyy年MM月dd日 HH：mm：ss\"/>\r\n" +
                    "<video_id/>.png\r\n" +
                    "<extract_tag match=\"(.*P$)|(.*Ｐ$)\">\r\n" +
                    "<tag separator=\"\\n\"/>\r\n";
                if ((string)((Control)sender).Tag == "Input")
                {
                    textBoxInputRankFileFormat.Text = custom_format_text;
                }
                else if ((string)((Control)sender).Tag == "Output")
                {
                    textBoxOutputRankFileFormat.Text = custom_format_text;
                }
            }
        }

        private void buttonDlRankCheckOrUncheck_Click(object sender, EventArgs e)
        {
            bool checking = ((Button)sender == buttonDlRankCheckAll);

            for (int i = 0; i < checkedListBoxDlRankCategory.Items.Count; ++i)
            {
                checkedListBoxDlRankCategory.SetItemChecked(i, checking);
            }
        }

        private void radioButtonMylistNew_CheckedChanged(object sender, EventArgs e)
        {
            labelMylistUpdate.Enabled = radioButtonMylistUpdate.Checked;
            textBoxMylistUpdateId.Enabled = radioButtonMylistUpdate.Checked;
            buttonMylistNew.Text = (radioButtonMylistUpdate.Checked ? "更新" : "新規作成");
        }

        private void buttonOpenVideoCutter_Click(object sender, EventArgs e)
        {
            string videocut_path = textBoxVideocutPath.Text;

            if (File.Exists(videocut_path))
            {
                IJProcess.RunProcess(videocut_path, "", true);
            }
            else
            {
                textBoxInfo.AppendText("動画カッターが存在しません。パスを正しく指定してください。\r\n");
            }
        }

        private void buttonMakeUserId_Click(object sender, EventArgs e)
        {
            List<string> video_id_list = new List<string>();
            List<string> plist = new List<string>();
            int column = (int)numericUpDownMakeUserIdColumn.Value;

            List<string> file_list = new List<string>();

            if (radioButtonMakeUserIdFromDir.Checked)
            {
                file_list.AddRange(Directory.GetFiles(selectFileBoxMakeUserIdFromDir.FileName));
            }
            else
            {
                if (File.Exists(selectFileBoxMakeUserIdFromFile.FileName))
                {
                    file_list.Add(selectFileBoxMakeUserIdFromFile.FileName);
                }
            }

            foreach (string file in file_list)
            {
                string[] lines = IJStringUtil.SplitWithCRLF(IJFile.Read(file));
                for (int i = 0; i < lines.Length; ++i)
                {
                    string[] ar = lines[i].Split('\t');
                    string video_id = NicoUtil.CutNicoVideoId(ar[0]);
                    if (video_id != "" && video_id_list.IndexOf(video_id) < 0)
                    {
                        video_id_list.Add(video_id);
                        plist.Add((column - 1 < ar.Length) ? ar[column - 1] : "");
                    }
                }
            }
            if (video_id_list.Count > 0)
            {
                StartThread(nicorank_mgr_.UpdateUserId, null, false, video_id_list, plist, textBoxGettingDetailInterval.Text);
            }
        }

        private void linkLabelMakuUserId_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.daily-vocaran.info/nicorank/onegai.html");
        }

        private void buttonGetMyMylistList_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.GetMyMylistList, null, false);
        }

        private void buttonAddingMultipleMylist_Click(object sender, EventArgs e)
        {
            FormMultipleMylist form = new FormMultipleMylist(this);
            form.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartThread(nicorank_mgr_.GetDataFromNicoApi, null, false);
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButtonSearchGetKind_CheckedChanged(object sender, EventArgs e)
        {
            labelRedundantSearch.Enabled = radioButtonSearchGetKindHTML.Checked;
            comboBoxRedundantSearchMethod.Enabled = radioButtonSearchGetKindHTML.Checked;
        }
    }
}
