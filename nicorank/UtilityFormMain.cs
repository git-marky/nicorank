// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using IJLib;
using NicoTools;

namespace nicorank
{
    partial class FormMain
    {
        ///////////// public method

        public void Write(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(Write), new object[] { str });
            }
            else
            {
                textBoxInfo.AppendText(str);
            }
        }

        public void WriteLine(string str)
        {
            Write(str + "\r\n");
        }

        public void UpdateSavedRankDir(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(UpdateSavedRankDir), new object[] { str });
            }
            else
            {
                textBoxSavedRankDir.Text = str;
            }
        }

        public void UpdateSavedRankNicoChartDir(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(UpdateSavedRankNicoChartDir), new object[] { str });
            }
            else
            {
                textBoxSavedRankNicoChartDir.Text = str;
            }
        }

        private void UpdateSavedRankNicoChartDirInner(string str)
        {
            textBoxSavedRankNicoChartDir.Text = str;
        }

        public void UpdateMylistId(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(UpdateMylistId), new object[] { str });
            }
            else
            {
                textBoxMylistId.Text = str;
            }
        }

        public void SetDownloadInfo(string info_text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(SetDownloadInfo), new object[] { info_text });
            }
            else
            {
                textBoxDownloadInfo.Text = info_text;
            }
        }

        public void SetTextBoxOutputRank(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(SetTextBoxOutputRank), new object[] { str });
            }
            else
            {
                textBoxOutputRank.Text = str;
            }
        }

        public void InformClosedFormTransOption(TransDetailOption option)
        {
            trans_detail_option_ = option;
            form_trans_option_ = null;
        }

        public void SetMakeUserIdOutput(string str)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(SetMakeUserIdOutput), new object[] { str });
            }
            else
            {
                textBoxMakeUserIdOutput.AppendText(str);
            }
        }

        public void AddMultipleMylist(List<string> mylist_id_list, List<int> mylist_count_list)
        {
            StartThread(nicorank_mgr_.AddMultipleMylist, null, false, GetInputOutputOption(), mylist_id_list, mylist_count_list);
        }

        ///////////// private method

        private void SetButtonDialog()
        {
            buttonSelectInputRankFilePath.Tag = textBoxInputRankFilePath;
            buttonSelectOutputRankFilePath.Tag = textBoxOutputRankFilePath;
            buttonSelectRankDlDir.Tag = textBoxRankDlDir;
            buttonSelectFlvDlDir.Tag = textBoxFlvDlDir;
            buttonSelectThumbnailDir.Tag = textBoxThumbnailDir;
            buttonSelectSavedRankDir.Tag = textBoxSavedRankDir;
            buttonSelectSavedRankNicoChartDir.Tag = textBoxSavedRankNicoChartDir;
            buttonSelectFilterPath.Tag = textBoxFilterPath;
            buttonSelectDiff1Path.Tag = textBoxDiff1Path;
            buttonSelectDiff2Path.Tag = textBoxDiff2Path;
            buttonSelectTransBeforeFileOrDir.Tag = textBoxTransBeforeFileOrDir;
            buttonSelectTransAfterDir.Tag = textBoxTransAfterFileOrDir;
            buttonSelectLayoutPath.Tag = textBoxLayoutPath;
            buttonSelectRankPicDir.Tag = textBoxRankPicDir;
            buttonSelectFFmpegPath.Tag = textBoxFFmpegPath;
            buttonSelectWavflvPath.Tag = textBoxWavfltPath;
            buttonSelectMencPath.Tag = textBoxMencPath;
            buttonSelectScriptInputPath.Tag = textBoxScriptInputPath;
            buttonSelectAviSynthScriptPath.Tag = textBoxAviSynthScriptPath;
            buttonSelectAviFromScriptPath.Tag = textBoxAviFromScriptPath;
            buttonSelectCutListPath.Tag = textBoxCutListPath;
            buttonSelectVideocutPath.Tag = textBoxVideocutPath;
            buttonSelectFirefoxProfileDir.Tag = textBoxFirefoxProfileDir;
            // これらは SelectFileBox に置き換える必要がある
        }

        private void SaveConfig(string filename)
        {
            StringBuilder buff = new StringBuilder();
            List<Control> control_list = new List<Control>();
            GetAllControl(this, ref control_list);

            buff.Append("version\t" + program_version_.ToString() + "\r\n");
            for (int i = 0; i < control_list.Count; ++i)
            {
                if (control_list[i] is TextBox)
                {
                    TextBox c = (TextBox)control_list[i];
                    if (c.Name == "textBoxUploadText" || c.Name == "textBoxUser" ||
                        c.Name == "textBoxPassword" || c.Name == "textBoxInfo" || c.Name == "textBoxEditRankFile" ||
                        c.Name == "textBoxEditExclusionList" || c.Name == "textBoxMencWatching"
                        )
                    {
                        continue;
                    }
                    if (c.Name == "")
                    {
                        continue;
                    }
                    buff.Append("text");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(IJStringUtil.EscapeForConfig(c.Text));
                    buff.Append("\r\n");
                }
                else if (control_list[i] is RadioButton)
                {
                    RadioButton c = (RadioButton)control_list[i];
                    buff.Append("radio");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(c.Checked.ToString());
                    buff.Append("\r\n");
                }
                else if (control_list[i] is CheckBox)
                {
                    CheckBox c = (CheckBox)control_list[i];
                    if (c.Name == "checkBoxTimer1" || c.Name == "checkBoxTimer2" || c.Name == "checkBoxDailyTimer" || c.Name == "checkBoxTimerNews")
                    {
                        continue;
                    }
                    buff.Append("checkBox");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(c.Checked.ToString());
                    buff.Append("\r\n");
                }
                else if (control_list[i] is ListBox && !(control_list[i] is CheckedListBox))
                {
                    ListBox c = (ListBox)control_list[i];
                    buff.Append("listBox");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(c.SelectedIndex.ToString());
                    buff.Append("\r\n");
                }
                else if (control_list[i] is DateTimePicker)
                {
                    DateTimePicker c = (DateTimePicker)control_list[i];
                    buff.Append("dateTimePicker");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(NicoUtil.DateToString(c.Value));
                    buff.Append("\r\n");
                }
                else if (control_list[i] is ComboBox)
                {
                    ComboBox c = (ComboBox)control_list[i];
                    buff.Append("comboBox");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(c.SelectedIndex.ToString());
                    buff.Append("\r\n");
                }
                else if (control_list[i] is NumericUpDown)
                {
                    NumericUpDown c = (NumericUpDown)control_list[i];
                    buff.Append("numericUpDown");
                    buff.Append('\t');
                    buff.Append(c.Name);
                    buff.Append('\t');
                    buff.Append(c.Value.ToString());
                    buff.Append("\r\n");
                }
            }
            if (form_trans_option_ != null)
            {
                trans_detail_option_ = form_trans_option_.GetAppOption();
            }
            buff.Append(trans_detail_option_.SaveData());

            buff.Append("dlrank_category\t").Append(category_manager_.GetSaveString()).Append("\r\n");

            IJFile.Write(filename, buff.ToString());
        }

        private void LoadConfig(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                bool is_old_rankfile = false;
                string is_old_rankfile_path = "";
                string str = IJFile.Read(filename);
                int version = 0;
                string[] sArray = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (!sArray[0].StartsWith("version"))
                {
                    throw new FormatException();
                }
                foreach (string s in sArray)
                {
                    if (s.StartsWith("TransDetailOption."))
                    {
                        trans_detail_option_.SetDataFromLine(s);
                        continue;
                    }
                    string[] sa = s.Split('\t');
                    if (sa[0] == "version")
                    {
                        version = int.Parse(sa[1]);
                        if (version <= 1560)
                        {
                            MessageBox.Show("動画変換のオプションの一部が変更になりました。\r\n" +
                                "動画変換を行う場合は再設定してください。", "お知らせ");
                        }
                    }
                    if (sa[0] == "dlrank_category")
                    {
                        category_manager_.SetString(sa[1]);
                    }
                    if (sa.Length < 3 || sa[1] == "")
                    {
                        continue;
                    }

                    if (sa[1] == "textBoxBaseDir")
                    {
                        if (sa[2] != Application.StartupPath && sa[2] != Application.StartupPath + "\\" && sa[2] != "")
                        {
                            MessageBox.Show("「相対指定のときの基準ディレクトリ」は廃止になりました。\r\n" +
                                "基準ディレクトリはプログラムの存在するディレクトリになります。", "お知らせ");
                        }
                    }
                    if (sa[1] == "textBoxRankFilePath" && sa[2] != "")
                    {
                        is_old_rankfile = true;
                        is_old_rankfile_path = sa[2];
                    }
                    if (sa[1] == "textBoxMylistRate" && sa[2] != "")
                    {
                        try
                        {
                            numericUpDownMylistRate.Value = decimal.Parse(sa[2]);
                        }
                        catch (FormatException) { }
                    }
                    if (sa[1] == "textBoxConditionMylistNew" && sa[2] != "")
                    {
                        try
                        {
                            numericUpDownConditionMylistNew.Value = decimal.Parse(sa[2]);
                        }
                        catch (FormatException) { }
                    }
                    Control[] c = this.Controls.Find(sa[1], true);
                    if (c.Length > 0)
                    {
                        switch (sa[0])
                        {
                            case "text":
                                if (version <= 1640) // 過去の互換性のため
                                {
                                    if (((TextBox)c[0]).Multiline)
                                    {
                                        sa[2] = sa[2].Replace("\\n", "\r\n").Replace("(doll-n-nicorank)", "\\n");
                                    }
                                    c[0].Text = sa[2];
                                }
                                else
                                {
                                    c[0].Text = IJStringUtil.UnescapeForConfig(sa[2]);
                                }
                                break;
                            case "radio":
                                if (version <= 1930) // 過去の互換性のため
                                {
                                    if (sa[1] == "radioButtonBrowserIE6" || sa[1] == "radioButtonBrowserIE7")
                                    {
                                        radioButtonBrowserIE.Checked = true;
                                    }
                                }
                                ((RadioButton)c[0]).Checked = bool.Parse(sa[2]);
                                break;
                            case "checkBox":
                                ((CheckBox)c[0]).Checked = bool.Parse(sa[2]);
                                break;
                            case "listBox":
                                ((ListBox)c[0]).SelectedIndex = int.Parse(sa[2]);
                                break;
                            case "dateTimePicker":
                                ((DateTimePicker)c[0]).Value = NicoUtil.StringToDate(sa[2]);
                                break;
                            case "comboBox":
                                ((ComboBox)c[0]).SelectedIndex = int.Parse(sa[2]);
                                break;
                            case "numericUpDown":
                                ((NumericUpDown)c[0]).Value = decimal.Parse(sa[2]);
                                break;
                        }
                    }
                }
                if (is_old_rankfile)
                {
                    textBoxInputRankFilePath.Text = is_old_rankfile_path;
                    checkBoxIsSameToInput.Checked = true;
                }
            }
        }

        private void GetAllControl(Control c, ref List<Control> list)
        {
            for (int i = 0; i < c.Controls.Count; ++i)
            {
                list.Add(c.Controls[i]);
                GetAllControl(c.Controls[i], ref list);
            }
        }

        private void GetControlList<T>(ref List<T> target_control_list) where T : Control
        {
            List<Control> control_list = new List<Control>();
            GetAllControl(this, ref control_list);
            for (int i = 0; i < control_list.Count; ++i)
            {
                if (control_list[i] is T)
                {
                    target_control_list.Add((T)control_list[i]);
                }
            }
        }

        private void SetPath()
        {
            List<TextBox> textbox_list = new List<TextBox>();
            GetControlList(ref textbox_list);
            foreach (TextBox c in textbox_list)
            {
                nicorank_mgr_.GetPathMgr().SetPath(c.Name, c.Text);
            }
        }

        private TranslatingOption GetFFmpegOption()
        {
            if (form_trans_option_ != null)
            {
                trans_detail_option_ = form_trans_option_.GetAppOption();
            }
            TranslatingOption translating_option = new TranslatingOption();

            switch (comboBoxTransFileKind.SelectedIndex)
            {
                case 0:
                    translating_option.trans_file_kind = TranslatingOption.TransFileKind.RankFile;
                    break;
                case 1:
                    translating_option.trans_file_kind = TranslatingOption.TransFileKind.Directory;
                    break;
                case 2:
                    translating_option.trans_file_kind = TranslatingOption.TransFileKind.File;
                    break;
            }
            translating_option.is_flv_to_avi = checkBoxIsFlvToAvi.Checked;
            translating_option.is_avi_include_audio = radioButtonIncludeAudio.Checked;
            translating_option.trans_avi_kind = trans_detail_option_.trans_avi_kind;

            translating_option.is_flv_to_wav = checkBoxIsFlvToWav.Checked;
            translating_option.is_flv_to_mp3 = checkBoxIsFlvToMp3.Checked;
            translating_option.is_flv_to_png = checkBoxIsFlvToPng.Checked;
            translating_option.is_flv_to_detail = checkBoxIsFlvToDetail.Checked;

            translating_option.fadein = (checkBoxTransIsFadeIn.Checked) ? IJStringUtil.ToDoubleWithDef(textBoxTransFadeIn.Text, -1.0) : -1.0;
            translating_option.fadeout = (checkBoxTransIsFadeOut.Checked) ? IJStringUtil.ToDoubleWithDef(textBoxTransFadeOut.Text, -1.0) : -1.0;
            translating_option.is_normalize = checkBoxTransIsNormalize.Checked;

            int width = trans_detail_option_.change_width;
            int height = trans_detail_option_.change_height;
            if (trans_detail_option_.is_change_size && width > 0 && height > 0)
            {
                translating_option.changing_width = width;
                translating_option.changing_height = height;
                translating_option.is_fix_aspect = trans_detail_option_.is_fix_aspect;
            }
            else
            {
                trans_detail_option_.change_width = 0;
                trans_detail_option_.change_height = 0;
            }

            translating_option.is_framerate_change = trans_detail_option_.is_framerate_change;
            translating_option.frame_rate = trans_detail_option_.frame_rate;

            translating_option.cut_start = (checkBoxTransIsCut.Checked ? IJStringUtil.ToDoubleWithDef(textBoxTransCutStart.Text, -1.0) : -1.0);
            translating_option.cut_end = (checkBoxTransIsCut.Checked ? IJStringUtil.ToDoubleWithDef(textBoxTransCutEnd.Text, -1.0) : -1.0);
            translating_option.cut_list_name = (checkBoxIsUsingCutList.Checked ? IJFile.GetAbsolutePath(textBoxCutListPath.Text) : "");

            translating_option.is_only_sm = trans_detail_option_.is_only_sm;
            translating_option.is_only_nm = trans_detail_option_.is_only_nm;

            translating_option.is_window_show = trans_detail_option_.is_show_window;
            translating_option.is_overwrite = checkBoxIsOverwrite.Checked;

            //ffmpeg_option.trans_detail_option = textBoxTransDetailOption.Text;

            translating_option.trans_before_file_or_dir = textBoxTransBeforeFileOrDir.Text;
            translating_option.trans_after_file_or_dir = textBoxTransAfterFileOrDir.Text;
            translating_option.app_path = new FFmpegAppPath(IJFile.GetAbsolutePath(textBoxFFmpegPath.Text), IJFile.GetAbsolutePath(textBoxWavfltPath.Text));
            translating_option.iooption = GetInputOutputOption();
            return translating_option;
        }

        private RankingMethod GetRankingMethod()
        {
            HoseiKind hosei_kind = HoseiKind.Nothing;
            if (radioButtonHoseiVocaran.Checked)
            {
                hosei_kind = HoseiKind.Vocaran;
            }
            else if (radioButtonHoseiNicoran.Checked)
            {
                hosei_kind = HoseiKind.Nicoran;
            }
            SortKind sort_kind = SortKind.Nothing;
            if (radioButtonSortMylist.Checked)
            {
                sort_kind = SortKind.Mylist;
            }
            else if (radioButtonSortPoint.Checked)
            {
                sort_kind = SortKind.Point;
            }
            if (checkBoxFilter.Checked)
            {
                return new RankingMethod(hosei_kind, sort_kind,
                    (int)numericUpDownMylistRate.Value,
                    true, nicorank_mgr_.GetPathMgr().GetFullPath(textBoxFilterPath.Text), checkBoxIsOutputFilteredVideo.Checked);
            }
            else
            {
                return new RankingMethod(hosei_kind, sort_kind, (int)numericUpDownMylistRate.Value);
            }
        }

        public InputOutputOption GetInputOutputOption()
        {
            InputOutputOption iooption = new InputOutputOption(radioButtonInputFromRankFile.Checked,
                (checkBoxIsSameToInput.Checked ? true : radioButtonOutputToRankFile.Checked),
                GetRankFileCustomFormat());
            if (radioButtonInputFromRankFile.Checked)
            {
                iooption.SetInputPath(nicorank_mgr_.GetPathMgr().GetFullPath(textBoxInputRankFilePath.Text));
            }
            else
            {
                iooption.SetInputText(textBoxInputRank.Text);
            }
            if (checkBoxIsSameToInput.Checked)
            {
                // 入力ファイルと出力ファイルを同じにする
                iooption.SetOutputPath(nicorank_mgr_.GetPathMgr().GetFullPath(textBoxInputRankFilePath.Text));
            }
            else if (radioButtonOutputToRankFile.Checked)
            {
                iooption.SetOutputPath(nicorank_mgr_.GetPathMgr().GetFullPath(textBoxOutputRankFilePath.Text));
            }
            else
            {
                iooption.SetOutputRankFileDelegate(SetTextBoxOutputRank);
            }
            return iooption;
        }

        private RankFileCustomFormat GetRankFileCustomFormat()
        {
            if (checkBoxIsRankFileCustomize.Checked)
            {
                return new RankFileCustomFormat(textBoxInputRankFileFormat.Text, textBoxOutputRankFileFormat.Text);
            }
            else
            {
                return new RankFileCustomFormat();
            }
        }

        private void MakeUserFile()
        {
            string str = IJStringUtil.EncryptString(textBoxUser.Text, "dailyvocaran") + "\t" +
                IJStringUtil.EncryptString(textBoxPassword.Text, "dailyvocaran");
            IJFile.Write(userdat_filename, str);
        }

        // 古いので消す
        public List<string> GetIdListFromTextBox()
        {
            string[] s_array = IJStringUtil.SplitWithCRLF(textBoxInputRank.Text);
            List<string> list = new List<string>();
            for (int i = 0; i < s_array.Length; ++i)
            {
                int start = s_array[i].IndexOf("sm");
                if (start < 0)
                {
                    start = s_array[i].IndexOf("nm");
                }
                if (start < 0)
                {
                    continue;
                }
                int end = start + 2;
                while (end < s_array[i].Length && s_array[i][end] >= '0' && s_array[i][end] <= '9')
                {
                    ++end;
                }
                list.Add(s_array[i].Substring(start, end - start));
            }
            return list;
        }

        private string GetExecTimeString(TimeSpan ts)
        {
            return ((int)ts.TotalDays).ToString() + "日" + ts.Hours.ToString("00") + "時間" +
                ts.Minutes.ToString("00") + "分" + ts.Seconds.ToString("00") + "秒後に実行します。";
        }

        private void DoTimerCommand(int index)
        {
            switch (index)
            {
                case 1:
                    buttonRankDl_Click(null, null);
                    break;
                case 2:
                    buttonTagSearchNew_Click(null, null);
                    break;
                case 3:
                    buttonUpdateDetail_Click(null, null);
                    break;
                case 4:
                    buttonMylistSearch_Click(null, null);
                    break;
                case 5:
                    buttonNewArrival_Click(null, null);
                    break;
                case 6:
                    buttonAnalyzeRanking_Click(null, null);
                    break;
            }
        }

        private void MakeCommentList(out List<string> tag_list, out List<bool> is_lock_list)
        {
            tag_list = new List<string>();
            is_lock_list = new List<bool>();
            if (textBoxTag1.Text != "")
            {
                tag_list.Add(textBoxTag1.Text);
                is_lock_list.Add(checkBoxTagLock1.Checked);
            }
            if (textBoxTag2.Text != "")
            {
                tag_list.Add(textBoxTag2.Text);
                is_lock_list.Add(checkBoxTagLock2.Checked);
            }
            if (textBoxTag3.Text != "")
            {
                tag_list.Add(textBoxTag3.Text);
                is_lock_list.Add(checkBoxTagLock3.Checked);
            }
            if (textBoxTag4.Text != "")
            {
                tag_list.Add(textBoxTag4.Text);
                is_lock_list.Add(checkBoxTagLock4.Checked);
            }
            if (textBoxTag5.Text != "")
            {
                tag_list.Add(textBoxTag5.Text);
                is_lock_list.Add(checkBoxTagLock5.Checked);
            }
        }

        private bool CheckFFmpeg()
        {
            if (!File.Exists(nicorank_mgr_.GetPathMgr().GetFullPath(textBoxFFmpegPath.Text)))
            {
                Write("FFmpeg のパスの設定が正しくありません。\r\n");
                string new_ffmpeg_path = Path.GetDirectoryName(Application.ExecutablePath) + "\\bin\\ffmpeg\\ffmpeg.exe";
                if (File.Exists(new_ffmpeg_path))
                {
                    textBoxFFmpegPath.Text = new_ffmpeg_path;
                    Write(new_ffmpeg_path + " に自動設定しました。\r\n");
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckTagSearchInterval()
        {
            if (textBoxTagSearchInterval.Text == "")
            {
                textBoxTagSearchInterval.Text = "8";
            }
            double interval_min = 8.0;
            double interval_max = 10.0;
            try
            {
                IJStringUtil.ParseDlInterval(textBoxTagSearchInterval.Text, ref interval_min, ref interval_max);
            }
            catch (FormatException)
            {
                textBoxInfo.AppendText("タグ検索の間隔の指定が正しくありません。\r\n");
                return false;
            }
            if (interval_min < 3.0)
            {
                textBoxInfo.AppendText("タグ検索の間隔は3秒未満にはできません。\r\n");
                return false;
            }
            return true;
        }

        private bool CheckGettingDetailInterval()
        {
            if (textBoxGettingDetailInterval.Text == "")
            {
                textBoxGettingDetailInterval.Text = "0.5";
            }
            double interval_min = 0.3;
            double interval_max = 0.5;
            try
            {
                IJStringUtil.ParseDlInterval(textBoxGettingDetailInterval.Text, ref interval_min, ref interval_max);
            }
            catch (FormatException)
            {
                textBoxInfo.AppendText("詳細情報取得の間隔の指定が正しくありません。\r\n");
                return false;
            }
            if (interval_min < 0.1)
            {
                textBoxInfo.AppendText("詳細情報取得の間隔は0.1秒未満にはできません。\r\n");
                return false;
            }
            return true;
        }

        // マイリスト番号かURLを入力して、マイリスト番号を返す
        private string GetMylistIdFromUrl(string mylist_id_or_uri)
        {
            return Regex.Match(mylist_id_or_uri, "(mylist/)?([0-9]+)").Groups[2].Value;
        }

        private bool CheckFileWrite()
        {
            string filename = "";
            string test_text = "testtest";
            try
            {
                filename = IJFile.GetTemporaryFileName(Directory.GetCurrentDirectory(), "txt");
                IJFile.Write(filename, test_text);
                string content = IJFile.Read(filename);
                return content == test_text;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (filename != "")
                    {
                        File.Delete(filename);
                    }
                }
                catch (Exception) { }
            }
        }

        private void StartThread(NicoRankManager.ThreadStarterDelegate ts_delegate, NicoRankManager.ThreadCompletedDelegate complated_delegate,
            bool check_output, params object[] param_array)
        {
            SetPath();
            IJLog.SetLogging(checkBoxIsOutputLog.Checked);

            if (check_output)
            {
                string output_filename = checkBoxIsSameToInput.Checked ?
                                             textBoxInputRankFilePath.Text : textBoxOutputRankFilePath.Text;
                if ((radioButtonOutputToRankFile.Checked || checkBoxIsSameToInput.Checked) &&
                    output_filename == "")
                {
                    MessageBox.Show(this, "ファイル名が入力されていません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (checkBoxConfirmFilter.Checked && checkBoxFilter.Checked) // フィルター使用確認
                {
                    using (MessageBoxWithCheckBox msgbox = new MessageBoxWithCheckBox())
                    {
                        if (checkBoxIsOutputFilteredVideo.Checked && !radioButtonOutputToTextBox.Checked)
                        {
                            string filter_filename = Path.GetDirectoryName(output_filename);
                            if (filter_filename != "" && !filter_filename.EndsWith("\\"))
                            {
                                filter_filename += "\\";
                            }
                            filter_filename += Path.GetFileNameWithoutExtension(output_filename) +
                                "_filter" + Path.GetExtension(output_filename);
                            msgbox.SetText("フィルターがオンになっています。\r\nフィルターによって除去された動画情報は " +
                                filter_filename +
                                " に保存されます。よろしいですか？", "確認", "今後確認しない");
                        }
                        else
                        {
                            msgbox.SetText("フィルターがオンになっています。\r\n" +
                                "フィルターによって除去された動画情報は失われてしまいます。よろしいですか？",
                                "確認", "今後確認しない");
                        }
                        DialogResult result = msgbox.ShowDialog(this);
                        if (result == DialogResult.Cancel || result == DialogResult.No)
                        {
                            return;
                        }
                        if (msgbox.CheckBoxState)
                        {
                            checkBoxConfirmFilter.Checked = false;
                        }
                    }
                }

                // ランクファイル上書き確認
                if (checkBoxConfirmOverWrite.Checked &&
                    ((radioButtonOutputToRankFile.Checked && File.Exists(output_filename)) || checkBoxIsSameToInput.Checked))
                {
                    using (MessageBoxWithCheckBox msgbox = new MessageBoxWithCheckBox())
                    {
                        if (checkBoxIsSameToInput.Checked)
                        {
                            msgbox.SetText(output_filename + " を上書きしますか？", "確認", "今後確認しない");
                        }
                        else
                        {
                            msgbox.SetText(output_filename + " は既に存在します。\r\n上書きしますか？", "確認", "今後確認しない");
                        }
                        DialogResult result = msgbox.ShowDialog(this);
                        if (result == DialogResult.Cancel || result == DialogResult.No)
                        {
                            return;
                        }
                        if (msgbox.CheckBoxState)
                        {
                            checkBoxConfirmOverWrite.Checked = false;
                        }
                    }
                }
            }

            nicorank_mgr_.StartNewThread(ts_delegate, complated_delegate, param_array);
        }

        private void StartThreadNotCatch(NicoRankManager.ThreadStarterDelegate ts_delegate, params object[] param_array)
        {
            SetPath();
            IJLog.SetLogging(checkBoxIsOutputLog.Checked);
            nicorank_mgr_.StartNewThreadNotCatch(ts_delegate, param_array);
        }

        private void ChangeSearchButtonText(AsyncCompletedEventArgs e) 
        {
            if (this.InvokeRequired)
            {
                MethodInvoker invoker = delegate
                {
                    this.buttonTagSearchNew.Text = "検索";
                };
                this.Invoke(invoker);
            }
            else
            {
                this.buttonTagSearchNew.Text = "検索";
            }
        }
    }
}
