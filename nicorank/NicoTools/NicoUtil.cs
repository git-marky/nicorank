// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IJLib;
using System.Text.RegularExpressions;

namespace NicoTools
{
    public class NicoUtil
    {
        public enum FileType { Swf, Mp4, Other };

        private const int checking_error_num_ = 3;

        /// <summary>
        /// ニコニコ動画で使われる日付文字列を解析する。
        /// 2008年02月08日 07：21：32 または 2008/02/08 07:21:32 または 08/02/08 07:21 など。
        /// </summary>
        public static DateTime StringToDate(string date_str)
        {
            string[] format = { "yyyy年MM月dd日 HH：mm：ss", "yyyy/MM/dd HH:mm:ss", "yy/MM/dd HH:mm", "yyyy年MM月dd日 HH:mm" };
            return DateTime.ParseExact(date_str, format, null, System.Globalization.DateTimeStyles.None);
        }

        public static string DateToString(DateTime dt)
        {
            return dt.ToString("yyyy年MM月dd日 HH：mm：ss");
        }

        public static Video GetVideo(NicoNetwork network, string video_id, CancelObject cancel_object, MessageOut msgout)
        {
            Video video = null;
            for (int error_times = 0; ; ++error_times)
            {
                try
                {
                    video = new Video(network.GetThumbInfo(video_id));
                }
                catch (Exception)
                {
                    if (error_times >= checking_error_num_ - 1)
                    {
                        return Video.GetOtherStateVideo();
                    }
                    else
                    {
                        if (msgout != null)
                        {
                            msgout.Write("ニコニコ動画へのアクセスエラーが起きました。3秒後に再試行します。\r\n");
                        }
                        cancel_object.Wait(3000);
                        continue;
                    }
                }
                break;
            }
            return video;
        }

        // 文字列から sm1234567 など動画IDを探して取得する。
        public static string CutNicoVideoId(string str)
        {
            int dummy = 0;
            return CutNicoVideoId(str, ref dummy);
        }

        // 文字列から sm1234567 など動画IDを探して取得。index 文字目から探し始める。
        public static string CutNicoVideoId(string str, ref int index)
        {
            string[] prefix = { "sm", "nm", "fz", "yo", "ig", "ax", "na", "za", "yk", "sk", "fx", "cw", "zc", "zb", "ca", "zd", "so" };

            for (int i = index; i < str.Length; ++i)
            {
                for (int j = 0; j < prefix.Length; ++j)
                {
                    if (str[i] == prefix[j][0])
                    {
                        if (i + prefix[j].Length < str.Length && str.Substring(i, prefix[j].Length) == prefix[j])
                        {
                            int k = i + prefix[j].Length;
                            if ('0' <= str[k] && str[k] <= '9')
                            {
                                while (k < str.Length && '0' <= str[k] && str[k] <= '9')
                                {
                                    ++k;
                                }
                                index = k;
                                return str.Substring(i, k - i);
                            }
                        }
                    }
                }
            }
            index = str.Length;
            return "";
        }

        // ニコニコ動画IDを抜き出して取得。collection の1つの要素あたり、1つだけ抜き出す。
        public static List<string> GetNicoIdList(IEnumerable<string> collection)
        {
            List<string> list = new List<string>();
            foreach (string s in collection)
            {
                string id = NicoUtil.CutNicoVideoId(s);
                if (id != "")
                {
                    list.Add(id);
                }
                else
                {
                    Match m = Regex.Match(s, "([0-9]{8,11})");
                    if (m.Success)
                    {
                        list.Add(m.Groups[1].Value);
                    }
                }
            }

            return list;
        }

        public static FileType JudgeFileType(string filename)
        {
            byte[] data = new byte[128];
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            fs.Read(data, 0, data.Length);
            fs.Close();
            if (data[4] == (byte)'f' && data[5] == (byte)'t' && data[6] == (byte)'y' && data[7] == (byte)'p')
            {
                return FileType.Mp4;
            }
            else if (data[1] == (byte)'W' && data[2] == (byte)'S')
            {
                if (data[0] == (byte)'C' || data[0] == (byte)'F')
                {
                    return FileType.Swf;
                }
            }
            return FileType.Other;
        }

        public static string GetReplacedString(string text, string[] string_array)
        {
            List<string[]> list = new List<string[]>();
            list.Add(string_array);
            return GetReplacedString(text, list);
        }

        /// <summary>
        /// #記法を置き換える。
        /// "#m:n" を element[m - 1][n - 1] で置き換えたテキストを取得
        /// </summary>
        /// <param name="text">置換元文字列</param>
        /// <param name="element">置換するデータ</param>
        /// <returns>置き換えられた文字列</returns>
        public static string GetReplacedString(string text, List<string[]> element)
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == '#' && i < text.Length - 1 && text[i + 1] == '#')
                {
                    buff.Append((char)'#');
                    ++i;
                }
                else if (text[i] == '#')
                {
                    int row = 0;
                    int column = 0;
                    string format_string = "";
                    ParseSharp(text, ref i, ref row, ref column, ref format_string);
                    if (row >= 0) // -1 なら普通のテキストと解釈
                    {
                        while (row - 1 < element.Count && column - 1 >= element[row - 1].Length)
                        {
                            column -= element[row - 1].Length;
                            ++row;
                        }
                        string value = ((row - 1 < element.Count && column - 1 < element[row - 1].Length) ? element[row - 1][column - 1] : "");
                        if (value != "")
                        {
                            if (format_string != "")
                            {
                                format_string = "{0," + format_string + "}";
                                if (value.IndexOf('.') >= 0)
                                {
                                    buff.Append(String.Format(format_string, double.Parse(value)));
                                }
                                else
                                {
                                    buff.Append(String.Format(format_string, IJStringUtil.ToIntFromCommaValue(value)));
                                }
                            }
                            else
                            {
                                buff.Append(value);
                            }
                        }
                    }
                    else
                    {
                        buff.Append((char)text[i]);
                    }
                }
                else
                {
                    buff.Append((char)text[i]);
                }
            }
            return buff.ToString();
        }

        public static void ParseSharp(string text, ref int index, ref int row, ref int column, ref string format)
        {
            if (index + 1 < text.Length && '0' <= text[index + 1] && text[index + 1] <= '9')
            {
                format = "";
                int start = index + 1;
                int end = index + 1;
                while (end < text.Length && '0' <= text[end] && text[end] <= '9')
                {
                    ++end;
                }
                int num1 = int.Parse(text.Substring(start, end - start));
                if (end < text.Length && text[end] == ':')
                {
                    start = end + 1;
                    end = start;
                    while (end < text.Length && '0' <= text[end] && text[end] <= '9')
                    {
                        ++end;
                    }
                    row = num1;
                    column = int.Parse(text.Substring(start, end - start));
                }
                else
                {
                    row = 1;
                    column = num1;
                }
                if (end < text.Length && text[end] == '{')
                {
                    start = end + 1;
                    end = text.IndexOf('}', start);
                    format = text.Substring(start, end - start);
                    ++end;
                }
                index = end - 1;
            }
            else
            {
                row = -1;
            }
        }

        public static string Test_GetReplacedString()
        {
            string[] element = { "0", "1", "-1", "123", "1,234", "1,234,567",
                                   "0.99", "0.0", "abc", "1.234", "1.2345678" };
            string[] text = { "abc", "abc#1", "abc#2d", "abc\\#", "abc#1#2", "#2",
                            "abc#2{0:0000}", "abc#4{0:0000}#2{0:0000}", "abc#5{0:00000}",
                            "#3", "#3{0:0000}", "#6{0:0000}", "#7{0:0.00}",
                            "#11{0:0.00}", "#11{0:0.000}", "#6{0:#,###}", "#7{0:E5}"};
            string[] answer = { "abc", "abc0", "abc1d", "abc#", "abc01", "1",
                              "abc0001", "abc01230001", "abc01234",
                              "-1", "-0001", "1234567", "0.99",
                              "1.23", "1.235", "1,234,567", "9.90000E-001"};
            StringBuilder buff = new StringBuilder();
            List<string[]> list = new List<string[]>();
            list.Add(element);
            for (int i = 0; i < text.Length; ++i)
            {
                string r = GetReplacedString(text[i], list);
                buff.Append((answer[i] == r).ToString());
                buff.Append(" " + answer[i] + " " + r);
                buff.Append("\r\n");
            }
            return buff.ToString();
        }
    }

    public class NicoMylist
    {
        private NicoNetwork niconico_network_;
        private MessageOut msgout_;
        private CancelObject cancel_object_;

        public NicoMylist(NicoNetwork network, MessageOut msgout, CancelObject cancel_object)
        {
            niconico_network_ = network;
            msgout_ = msgout;
            cancel_object_ = cancel_object;
        }

        public string MakeNewMylistGroup(bool is_setting_public, string title, string description, int order, int color)
        {
            string mylist_id;
            niconico_network_.MakeNewAndUpdateMylistGroup(is_setting_public, title, description, order, color, out mylist_id);
            msgout_.Write("マイリストを新規作成しました。\r\n");
            return mylist_id;
        }

        public string UpdateMylistGroup(string mylist_id, bool is_setting_public, string title, string description, int order, int color)
        {
            string str = niconico_network_.UpdateMylistGroup(mylist_id, is_setting_public, title, description, order, color);
            msgout_.Write("マイリストを更新しました。\r\n");
            return str;
        }

        public void AddMylist(InputOutputOption iooption, string mylist_id)
        {
            const int try_times = 5;
            const int large_wait_time = 50000;
            const int large_wait_num = 70;
            RankFile rank_file = iooption.GetRankFile();

            if (rank_file.Count == 0)
            {
                msgout_.Write("追加する動画がありません。\r\n");
                return;
            }

            msgout_.Write("マイリスト " + mylist_id + " への追加を開始します。\r\n");
            // 最小3秒、最大10秒
            int wait_time = Math.Min(Math.Max(rank_file.Count * 1000 * 8 / 10, 3000), 10000);
            int total_wait_time = (wait_time * rank_file.Count + large_wait_time * (rank_file.Count / large_wait_num)) / 1000 / 60 + 1;
            msgout_.Write(rank_file.Count + "件の追加には推定" + total_wait_time.ToString() + "分かかります。\r\n");

            List<Video> exist_video_list = new List<Video>();
            NicoListManager.ParsePointRss(niconico_network_.GetMylistHtml(mylist_id, true), DateTime.Now, exist_video_list, false, true);
            cancel_object_.Wait(1000);

            for (int i = 0; i < rank_file.Count; ++i)
            {
                if (RankFile.SearchVideo(exist_video_list, rank_file[i]) >= 0)
                {
                    msgout_.Write(rank_file[i] + " はすでに存在します。\r\n");
                    continue;
                }
                for (int j = 0; j < try_times; ++j)
                {
                    try
                    {
                        niconico_network_.AddMylist(mylist_id, rank_file[i]);
                        msgout_.Write(rank_file[i] + " をマイリストに追加しました。\r\n");
                    }
                    catch (NiconicoAddingMylistExistException) // 上でチェックしているが念のためもう一度チェック
                    {
                        msgout_.Write(rank_file[i] + " はすでに存在します。\r\n");
                    }
                    catch (Exception e)
                    {
                        if (j < try_times - 1)
                        {
                            msgout_.Write("エラー：" + e.Message + "\r\n3秒後に再試行します。\r\n");
                            cancel_object_.Wait(3000);
                            continue;
                        }
                        else
                        {
                            msgout_.Write("エラー：" + e.Message + "\r\n");
                            cancel_object_.Wait(3000);
                        }
                    }
                    if (i < rank_file.Count - 1)
                    {
                        msgout_.Write("待機しています。\r\n");
                        // 基本的には wait_time ミリ秒だけ待つが、large_wait_num 回に1回は
                        // large_wait_time ミリ秒待つ。
                        if ((i + 1) % large_wait_num == 0)
                        {
                            cancel_object_.Wait(large_wait_time, large_wait_time + 1000);
                        }
                        else
                        {
                            cancel_object_.Wait(wait_time, wait_time + 1000);
                        }
                    }
                    break;
                }
            }
            msgout_.Write("マイリストへの追加を終了します。\r\n");
        }

        public void AddMultipleMylist(InputOutputOption iooption, List<string> mylist_id_list, List<int> mylist_count_list)
        {
            if (mylist_id_list.Count == 0) {
                return;
            }

            const int try_times = 5;
            const int large_wait_time = 50000;
            const int large_wait_num = 70;
            RankFile rank_file = iooption.GetRankFile();

            if (rank_file.Count == 0)
            {
                msgout_.Write("追加する動画がありません。\r\n");
                return;
            }

            msgout_.Write("マイリストへの追加を開始します。\r\n");
            // 最小3秒、最大10秒
            int wait_time = Math.Min(Math.Max(rank_file.Count * 1000 * 8 / 10, 3000), 10000);
            int total_wait_time = (wait_time * rank_file.Count + large_wait_time * (rank_file.Count / large_wait_num)) / 1000 / 60 + 1;
            msgout_.Write(rank_file.Count + "件の追加には推定" + total_wait_time.ToString() + "分かかります。\r\n");

            //List<Video> exist_video_list = new List<Video>();
            //NicoListManager.ParsePointRss(niconico_network_.GetMylistHtml(mylist_id, true), DateTime.Now, exist_video_list, false, true);
            //cancel_object_.Wait(1000);

            string mylist_id = mylist_id_list[0];
            int count = mylist_count_list[0];
            int current_mylist_index = 0;

            for (int i = 0; i < rank_file.Count; ++i)
            {
                //if (RankFile.SearchVideo(exist_video_list, rank_file[i]) >= 0)
                //{
                //    msgout_.Write(rank_file[i] + " はすでに存在します。\r\n");
                //    continue;
                //}
                for (int j = 0; j < try_times; ++j)
                {
                    try
                    {
                        niconico_network_.AddMylist(mylist_id, rank_file[i]);
                        msgout_.Write(rank_file[i] + " をマイリストに追加しました。\r\n");
                    }
                    catch (NiconicoAddingMylistExistException) // 上でチェックしているが念のためもう一度チェック
                    {
                        msgout_.Write(rank_file[i] + " はすでに存在します。\r\n");
                    }
                    catch (Exception e)
                    {
                        if (j < try_times - 1)
                        {
                            msgout_.Write("エラー：" + e.Message + "\r\n3秒後に再試行します。\r\n");
                            cancel_object_.Wait(3000);
                            continue;
                        }
                        else
                        {
                            msgout_.Write("エラー：" + e.Message + "\r\n");
                            cancel_object_.Wait(3000);
                        }
                    }
                    if (i < rank_file.Count - 1)
                    {
                        msgout_.Write("待機しています。\r\n");
                        // 基本的には wait_time ミリ秒だけ待つが、large_wait_num 回に1回は
                        // large_wait_time ミリ秒待つ。
                        if ((i + 1) % large_wait_num == 0)
                        {
                            cancel_object_.Wait(large_wait_time, large_wait_time + 1000);
                        }
                        else
                        {
                            cancel_object_.Wait(wait_time, wait_time + 1000);
                        }
                    }
                    break;
                }
                --count;
                if (count == 0)
                {
                    ++current_mylist_index;
                    if (current_mylist_index >= mylist_id_list.Count)
                    {
                        break;
                    }
                    mylist_id = mylist_id_list[current_mylist_index];
                    count = mylist_count_list[current_mylist_index];
                }
            }
            msgout_.Write("マイリストへの追加を終了します。\r\n");
        }

        public void UpdateMylistDescription(string mylist_id, string text, InputOutputOption iooption)
        {
            msgout_.Write("マイリストの説明の更新を開始します。\r\n");
            string[] line = IJStringUtil.SplitWithCRLF(iooption.GetRawText());

            List<string> add_list = new List<string>();
            List<string> description_list = new List<string>();
            for (int i = 0; i < line.Length; ++i)
            {
                string[] s_array = line[i].Split('\t');
                add_list.Add(s_array[0]);
                description_list.Add(NicoUtil.GetReplacedString(text, s_array).Replace("\\n", "\r\n"));
            }
            niconico_network_.UpdateMylistDescription(mylist_id, add_list, description_list, OnUpdateMylistDescriptionEvent);
            msgout_.Write("マイリストの説明の更新を終了します。\r\n");
        }

        private void OnUpdateMylistDescriptionEvent(string message, int current, int total)
        {
            msgout_.Write(message + "の説明を追加しました。\r\n");
            cancel_object_.CheckCancel();
            if (current < total - 1)
            {
                if (current % 10 == 0)
                {
                    msgout_.Write("待機しています。\r\n");
                    cancel_object_.Wait(30000);
                }
                else
                {
                    cancel_object_.Wait(3000, 5000);
                }
            }
        }

        public void PreviewMylistDescription(string text, string rank_file_path)
        {
            string[] line = IJStringUtil.SplitWithCRLF(IJFile.Read(rank_file_path));

            List<string> add_list = new List<string>();
            List<string> description_list = new List<string>();
            for (int i = 0; i < line.Length; ++i)
            {
                string[] s_array = line[i].Split('\t');
                add_list.Add(s_array[0]);
                description_list.Add(NicoUtil.GetReplacedString(text, s_array));
            }
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < add_list.Count; ++i)
            {
                buff.Append(add_list[i]);
                buff.Append("\r\n---------\r\n");
                buff.Append(description_list[i]);
                buff.Append("\r\n--------------------------------------------------------\r\n");
            }
            msgout_.Write(buff.ToString());
        }

        public string GetMyMylistList()
        {
            List<MylistInfo> mylist_info_list = niconico_network_.GetMylistInfoListFromMypage();
            StringBuilder buff = new StringBuilder();

            for (int i = 0; i < mylist_info_list.Count; ++i)
            {
                buff.Append(mylist_info_list[i].mylist_id).Append("\t");
                if (mylist_info_list[i].is_public)
                {
                    buff.Append("公開\t");
                }
                else
                {
                    buff.Append("非公開\t");
                }
                buff.Append(mylist_info_list[i].title).Append("\t").Append(mylist_info_list[i].description).Append("\r\n");
            }
            return buff.ToString();
        }
    }

    public class NicoTagManager
    {
        private NicoNetwork niconico_network_;
        private MessageOut msgout_;
        private CancelObject cancel_object_;

        public NicoTagManager(NicoNetwork network, MessageOut msgout, CancelObject cancel_object)
        {
            niconico_network_ = network;
            msgout_ = msgout;
            cancel_object_ = cancel_object;
        }

        public void AddTags(List<string> tag_list, List<bool> is_lock_list, string video_id)
        {
            msgout_.Write("タグ付けを開始します。動画ID = " + video_id + "\r\n");
            niconico_network_.AddTag(tag_list, is_lock_list, video_id, OnAddTagsEvent);
            msgout_.Write("タグ付けを終了します。\r\n");
        }

        private void OnAddTagsEvent(string message, int current, int total)
        {
            msgout_.WriteLine(message);
            cancel_object_.CheckCancel();
            cancel_object_.Wait(5000);
        }
    }

    public class NicoCommentManager
    {
        private NicoNetwork niconico_network_;
        private MessageOut msgout_;
        private CancelObject cancel_object_;

        public NicoCommentManager(NicoNetwork network, MessageOut msgout, CancelObject cancel_object)
        {
            niconico_network_ = network;
            msgout_ = msgout;
            cancel_object_ = cancel_object;
        }

        public void PostComment(string video_id, string comment, double time)
        {
            msgout_.Write("コメントを送信しています。\r\n");
            niconico_network_.PostComment(video_id, comment, (int)(time * 100));
            msgout_.Write("コメントを送信しました。\r\n");
        }

        private void OnAddTagsEvent(string message, int current, int total)
        {
            msgout_.Write(message);
            cancel_object_.CheckCancel();
            cancel_object_.Wait(2000);
        }
    }
}
