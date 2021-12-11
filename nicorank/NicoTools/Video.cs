// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using IJLib;

namespace NicoTools
{
    // 動画の情報を表すクラス。面倒なのでメンバーはすべて public。
    public class Video
    {
        public string video_id = "";
        public string title = "";
        public string description = "";
        public string thumbnail_url = "";
        public DateTime submit_date = new DateTime();
        public RankPoint point;
        public string pname = "";
        public string length = "";
        public string last_res_body = "";
        public string user_id = "";
        public TagSet tag_set = new TagSet();

        public enum Status { OK, DELETED, NOT_FOUND, OTHER };

        private List<string> user_text_list_ = new List<string>();
        private Status status_ = Status.OK;

        public Video()
        {
            // 何もしない
        }

        public Video(string xml)
        {
            ParseGetThumbInfo(xml);
        }

        public bool IsStatusOK()
        {
            return status_ == Status.OK;
        }

        public Status GetStatus()
        {
            return status_;
        }

        public string GetErrorMessage(string video_id)
        {
            switch (status_)
            {
                case Video.Status.OK:
                    return "";
                case Video.Status.DELETED:
                    return video_id + " は削除されたようです。";
                case Video.Status.NOT_FOUND:
                    return video_id + " は見つかりません。";
                default:
                    return video_id + " の情報の取得に失敗しました。";
            }
        }

        public string ToStringForSpecial()
        {
            return video_id + "\t" + IJStringUtil.ToStringWithComma(point.view) + "\t" + IJStringUtil.ToStringWithComma(point.res) + "\t" + IJStringUtil.ToStringWithComma(point.mylist) +
                "\t" + title + "\t" + NicoUtil.DateToString(submit_date) +
                "\t" + tag_set.ToStringSpace();
        }

        public string ToStringWithRank(int rank, HoseiKind hosei_kind, int mylist_rate)
        {
            if (status_ == Status.DELETED)
            {
                return video_id + "\tDELETED";
            }
            else if (status_ == Status.NOT_FOUND)
            {
                return video_id + "\tDELETED";
            }
            else if (status_ == Status.OTHER)
            {
                return video_id + "";
            }
            else
            {
                StringBuilder buff = new StringBuilder();
                buff.Append(video_id);
                buff.Append("\t");
                buff.Append(rank.ToString());
                buff.Append("\t");
                buff.Append(IJStringUtil.ToStringWithComma(point.view));
                buff.Append("\t");
                buff.Append(IJStringUtil.ToStringWithComma(point.res));
                buff.Append("\t");
                buff.Append(IJStringUtil.ToStringWithComma(point.mylist));
                buff.Append("\t");
                buff.Append(point.GetHoseiString(hosei_kind, mylist_rate, 2, hosei_kind == HoseiKind.Nicoran));
                buff.Append("\t");
                buff.Append(point.GetMylistRateString());
                buff.Append("\t");
                buff.Append(IJStringUtil.ToStringWithComma(point.CalcScore(hosei_kind, mylist_rate)));
                buff.Append("\t");
                buff.Append(title);
                buff.Append("\t");
                buff.Append(NicoUtil.DateToString(submit_date));
                buff.Append("\t");
                buff.Append(video_id + ".png");
                buff.Append("\t");
                buff.Append(pname);
                buff.Append("\t");
                buff.Append(tag_set.ToString());
                return buff.ToString();
            }
        }

        public void Set(RankVideo rank_video)
        {
            video_id = rank_video.video_id;
            title = rank_video.title;
            submit_date = rank_video.submit_date;
            tag_set = rank_video.tag_set.Clone();
        }

        public RankVideo ToRankVideo()
        {
            RankVideo rank_video = new RankVideo();
            rank_video.video_id = video_id;
            rank_video.title = title;
            rank_video.submit_date = submit_date;
            rank_video.tag_set = tag_set.Clone();
            return rank_video;
        }

        // GetThumbInfo で取得した xml を解析してメンバーに設定
        public void ParseGetThumbInfo(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement element = doc.DocumentElement;
            if (element.Attributes["status"].Value == "ok")
            {
                status_ = Status.OK;
                for (XmlNode node = element.FirstChild.FirstChild;
                    node != null; node = node.NextSibling)
                {
                    switch (node.Name)
                    {
                        case "video_id":
                            video_id = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "title":
                            title = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "description":
                            description = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "thumbnail_url":
                            thumbnail_url = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "first_retrieve":
                            submit_date = DateTime.ParseExact(IJStringUtil.UnescapeHtml(node.InnerText), "yyyy-MM-ddTHH:mm:ss+09:00", null);
                            break;
                        case "view_counter":
                            point.view = int.Parse(IJStringUtil.UnescapeHtml(node.InnerText));
                            break;
                        case "comment_num":
                            point.res = int.Parse(IJStringUtil.UnescapeHtml(node.InnerText));
                            break;
                        case "mylist_counter":
                            point.mylist = int.Parse(IJStringUtil.UnescapeHtml(node.InnerText));
                            break;
                        case "length":
                            length = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "last_res_body":
                            last_res_body = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "user_id":
                            user_id = IJStringUtil.UnescapeHtml(node.InnerText);
                            break;
                        case "tags":
                            if (node.Attributes["domain"] != null && node.Attributes["domain"].Value == "jp")
                            {
                                for (XmlNode tagnode = node.FirstChild;
                                    tagnode != null; tagnode = tagnode.NextSibling)
                                {
                                    bool is_lock;
                                    if (tagnode.Attributes["lock"] != null && tagnode.Attributes["lock"].Value == "1")
                                    {
                                        is_lock = true;
                                    }
                                    else
                                    {
                                        is_lock = false;
                                    }
                                    tag_set.Add(IJStringUtil.UnescapeHtml(tagnode.InnerText), is_lock);
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                if (element.FirstChild.FirstChild.InnerText == "NOT_FOUND")
                {
                    status_ = Status.NOT_FOUND;
                }
                else if (element.FirstChild.FirstChild.InnerText == "DELETED")
                {
                    status_ = Status.DELETED;
                }
                else
                {
                    status_ = Status.OTHER;
                }
            }
        }

        public void ParseExtThumb(string html)
        {
            int index;

            if ((index = html.IndexOf("<p class=\"TXT10")) >= 0)
            {
                point.view = IJStringUtil.ToIntFromCommaValue(IJStringUtil.GetStringBetweenTag(ref index, "strong", html));
                point.res = IJStringUtil.ToIntFromCommaValue(IJStringUtil.GetStringBetweenTag(ref index, "strong", html));
                point.mylist = IJStringUtil.ToIntFromCommaValue(IJStringUtil.GetStringBetweenTag(ref index, "strong", html));
                length = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                submit_date = DateTime.ParseExact(IJStringUtil.GetStringBetweenTag(ref index, "strong", html), "yy/MM/dd HH:mm", null);
                title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);
            }
            else
            {
                status_ = Status.OTHER;
            }
        }

        public void SetUserText(int index, string text)
        {
            while (index >= user_text_list_.Count)
            {
                user_text_list_.Add("");
            }
            user_text_list_[index] = text;
        }

        public string GetUserText(int index)
        {
            if (index < user_text_list_.Count)
            {
                return user_text_list_[index];
            }
            else
            {
                return "";
            }
        }

        public static bool IdEquals(Video video1, Video video2)
        {
            if (video1 == video2)
            {
                return true;
            }

            if (video1 != null &&
                video2 != null &&
                video1.video_id == video2.video_id)
            {
                return true;
            }

            return false;
        }

        public static Video GetOtherStateVideo()
        {
            Video video = new Video();
            video.status_ = Status.OTHER;
            return video;
        }
    }

    // Video をマイリスト数降順でソートするためのクラス。
    public class VideoMylistComparer : IComparer<Video>
    {
        public int Compare(Video x, Video y)
        {
            return y.point.mylist - x.point.mylist;
        }
    }

    // Video をマイリスト数降順でソートするためのクラス。
    public class VideoMylistRateComparer : IComparer<Video>
    {
        public int Compare(Video x, Video y)
        {
            return (int)((y.point.GetMylistRate() - x.point.GetMylistRate()) * 1000);
        }
    }

    // Video をポイント降順でソートするためのクラス。
    public class VideoScoreComparer : IComparer<Video>
    {
        private int mylist_rate_;
        private HoseiKind hosei_kind_;

        public VideoScoreComparer(HoseiKind hosei_kind, int mylist_rate)
        {
            hosei_kind_ = hosei_kind;
            mylist_rate_ = mylist_rate;
        }

        public int Compare(Video x, Video y)
        {
            return y.point.CalcScore(hosei_kind_, mylist_rate_) -
                x.point.CalcScore(hosei_kind_, mylist_rate_);
        }
    }

    // Video をポイント降順でソートするためのクラス。
    public class VideoSubmitDateComparer : IComparer<Video>
    {
        public int Compare(Video x, Video y)
        {
            return x.submit_date.CompareTo(y.submit_date);
        }
    }

    public delegate bool TagEqualityComparison(string tag, bool is_locked);

    public class TagSet
    {
        private List<TagElement> tag_list_ = new List<TagElement>();

        private static Font tag_font_ = null;
        private static Bitmap bitmap_ = null;
        private static Graphics graphics_ = null;

        private static string[] pname_list_ = null;

        public int Count
        {
            get { return tag_list_.Count; }
        }

        public void Add(string tag, bool is_lock)
        {
            tag_list_.Add(new TagElement(tag, is_lock));
        }

        public void Add(TagSet tag_set)
        {
            for (int i = 0; i < tag_set.tag_list_.Count; ++i)
            {
                int j;
                for (j = 0; j < tag_list_.Count; ++j)
                {
                    if (IJStringUtil.CompareString(tag_list_[j].GetRawTag(), tag_set.tag_list_[i].GetRawTag()))
                    {
                        break;
                    }
                }
                if (j >= tag_list_.Count)
                {
                    tag_list_.Add(tag_set.tag_list_[i]);
                }
            }
        }

        public TagSet Clone()
        {
            TagSet tag_set = new TagSet();
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                tag_set.Add(tag_list_[i].GetRawTag(), tag_list_[i].IsLock());
            }
            return tag_set;
        }

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                buff.Append(tag_list_[i].Get());
                if (i < tag_list_.Count - 1)
                {
                    buff.Append("\\n");
                }
            }
            return buff.ToString();
        }

        public string ToStringWithSplitter(string splitter)
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                buff.Append(tag_list_[i].Get());
                if (i < tag_list_.Count - 1)
                {
                    buff.Append(splitter);
                }
            }
            return buff.ToString();
        }

        public string ToStringSpace()
        {
            return ToStringWithSplitter(" ");
        }

        // 引数で与えた正規表現にマッチするタグを取得
        public string ExtractTag(string match)
        {
            Regex regex = new Regex(match);
            return ExtractTag(regex);
        }

        public string ExtractTag(Regex regex)
        {
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                string raw_tag = tag_list_[i].GetRawTag();
                if (regex.IsMatch(raw_tag))
                {
                    return raw_tag;
                }
            }
            return "";
        }

        public void AddFromList(string[] s_array)
        {
            for (int i = 0; i < s_array.Length; ++i)
            {
                tag_list_.Add(new TagElement(s_array[i]));
            }
        }

        public void Parse(string str)
        {
            string[] splitter = { "\\n", "  ", " " };
            string[] s_array = str.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            AddFromList(s_array);
        }

        public void ParseBlank(string str)
        {
            Parse(str, " ");
        }

        public void Parse(string str, string splitter)
        {
            string[] s_array = str.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
            AddFromList(s_array);
        }

        public string GetId(string tag)
        {
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if (tag_list_[i].GetRawTag() == tag)
                {
                    return tag_list_[i].GetId();
                }
            }
            return "";
        }

        private static bool IsExclusion(string str, string[] exclusion_tag)
        {
            for (int i = 0; i < exclusion_tag.Length; ++i)
            {
                if (IJStringUtil.CompareString(str, exclusion_tag[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public void DeleteTag(string tag)
        {
            for (int i = tag_list_.Count - 1; i >= 0; --i)
            {
                if (IJStringUtil.CompareString(tag_list_[i].GetRawTag(), tag))
                {
                    tag_list_.RemoveAt(i);
                }
            }
        }

        private static void LoadTagFont()
        {
            if (tag_font_ == null)
            {
                tag_font_ = new System.Drawing.Font("メイリオ", 9, FontStyle.Bold);
            }
            if (graphics_ == null)
            {
                bitmap_ = new Bitmap(1, 1);
                graphics_ = Graphics.FromImage(bitmap_);
            }
        }

        private static float GetStringWidth(string text)
        {
            return graphics_.MeasureString(text, tag_font_).Width;
        }

        private static List<TagPair> ArrangeTag(List<TagElement> tag_list)
        {
            List<TagPair> tags = new List<TagPair>();
            List<TagPair> tag_lock = new List<TagPair>();
            List<TagPair> tag_normal = new List<TagPair>();
            List<TagPair> tag_large = new List<TagPair>(); // 長いタグ
            List<TagPair> tag_including_link = new List<TagPair>(); // 「リンク」を含んだタグ

            LoadTagFont();

            for (int i = 0; i < tag_list.Count; ++i)
            {
                TagPair pair = new TagPair(tag_list[i].Get(), GetStringWidth(tag_list[i].Get()));
                if (tag_list[i].IsLock())
                {
                    tag_lock.Add(pair);
                }
                else if (pair.length >= 180.0)
                {
                    tag_large.Add(pair);
                }
                else if (pair.tag.IndexOf("リンク") >= 0)
                {
                    tag_including_link.Add(pair);
                }
                else
                {
                    tag_normal.Add(pair);
                }
            }
            tags.AddRange(tag_lock);
            tags.AddRange(tag_normal);
            tags.AddRange(tag_large);
            tags.AddRange(tag_including_link);
            return tags;
        }

        public string GetDisplayingTag()
        {
            return GetDisplayingTagInner(tag_list_);
        }

        private static string GetDisplayingTagInner(List<TagElement> tag_list)
        {
            List<TagPair> tags = ArrangeTag(tag_list);

            List<string> line1 = new List<string>();
            List<string> line2 = new List<string>();
            float line1_length = 0.0F;
            float line2_length = 0.0F;
            float space_length = GetStringWidth("  ");

            for (int i = 0; i < tags.Count; ++i)
            {
                if (line1_length + tags[i].length < 320.0)
                {
                    line1.Add(tags[i].tag);
                    line1_length += tags[i].length + space_length;
                }
                else if (line2_length + tags[i].length < 320.0)
                {
                    line2.Add(tags[i].tag);
                    line2_length += tags[i].length + space_length;
                }
            }
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < line1.Count; ++i)
            {
                buff.Append(line1[i]);
                if (i < line1.Count - 1)
                {
                    buff.Append("  ");
                }
            }
            buff.Append("\\n");
            for (int i = 0; i < line2.Count; ++i)
            {
                buff.Append(line2[i]);
                if (i < line2.Count - 1)
                {
                    buff.Append("  ");
                }
            }
            return buff.ToString();
        }

        // タグリストから P名を推定する
        public static string GetPname(TagSet tag_set, out int index)
        {
            if (pname_list_ == null) // 初回にリストをロードする
            {
                if (File.Exists("pnamelist.txt"))
                {
                    pname_list_ = IJStringUtil.SplitWithCRLF(IJFile.Read("pnamelist.txt"));
                }
                else
                {
                    pname_list_ = new string[0];
                }
            }

            for (int i = 0; i < tag_set.tag_list_.Count; ++i)
            {
                for (int j = 0; j < pname_list_.Length; ++j)
                {
                    if (IJStringUtil.CompareString(tag_set.tag_list_[i].GetRawTag(), pname_list_[j]))
                    {
                        index = i;
                        return pname_list_[j];
                    }
                }
            }
            // リストから見つからない場合は後ろにPのついたタグを探す
            for (int i = 0; i < tag_set.tag_list_.Count; ++i)
            {
                if (tag_set.tag_list_[i].GetRawTag().EndsWith("P", StringComparison.InvariantCultureIgnoreCase) ||
                    tag_set.tag_list_[i].GetRawTag().EndsWith("Ｐ", StringComparison.InvariantCultureIgnoreCase))
                {
                    index = i;
                    return tag_set.tag_list_[i].GetRawTag();
                }
            }
            index = -1;
            return ""; // それでも見つからないときは空文字を返す
        }

        public bool IsInclude(string tag)
        {
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if (IJStringUtil.CompareString(tag, tag_list_[i].GetRawTag()))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsIncludeAndLocked(string tag)
        {
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if (IJStringUtil.CompareString(tag, tag_list_[i].GetRawTag()) && tag_list_[i].IsLock())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInclude(TagEqualityComparison match)
        {
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if (match(tag_list_[i].GetRawTag(), tag_list_[i].IsLock()) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public void ParseHtml(string html)
        {
            int index = 0;

            while ((index = html.IndexOf("<input type=\"hidden\" name=\"cmd\" value=\"lock\">", index)) >= 0)
            {
                index = html.IndexOf("<input", index + 1);
                index = html.IndexOf("value=", index + 1);
                int start = html.IndexOf('"', index) + 1;
                int end = html.IndexOf('"', start);
                string tag = html.Substring(start, end - start);

                index = html.IndexOf("value=", end + 1);
                start = html.IndexOf('"', index) + 1;
                end = html.IndexOf('"', start);
                string id = html.Substring(start, end - start);

                index = html.IndexOf("value=", end + 1);
                start = html.IndexOf('"', index) + 1;
                end = html.IndexOf('"', start);
                string lock_str = html.Substring(start, end - start);

                // lock_str == "0" で正しい
                tag_list_.Add(new TagElement(tag, (lock_str == "0"), id));
            }
        }

        // タグ編集を開始するときに得られるHTMLを解析する
        public void ParseEditHtml(string html)
        {
            int index;
            int next = 0;
            const string lock_str = "<span style=\"color:#F90;\">★</span>";
            const string category_str = " <strong style=\"color:#F30;\">カテゴリ</strong>";

            while ((next = html.IndexOf("<td id=\"tag_edit_status", next)) >= 0)
            {
                index = html.LastIndexOf("<td>", next);
                string tag = IJStringUtil.GetStringBetweenTag(ref index, "td", html);
                bool is_lock = (tag.IndexOf(lock_str) >= 0);
                tag = tag.Replace(lock_str, "").Replace(category_str, "");
                if (tag.EndsWith(" "))
                {
                    tag = tag.Remove(tag.Length - 1);
                }
                index = html.IndexOf("name=\"id\"", index);
                index = html.IndexOf("value", index);
                index = html.IndexOf('"', index) + 1;
                int end = html.IndexOf('"', index);
                string id = html.Substring(index, end - index);
                tag_list_.Add(new TagElement(tag, is_lock, id));
                ++next;
            }
        }

        private class TagPair
        {
            public string tag;
            public float length;

            public TagPair(string t, float len)
            {
                tag = t;
                length = len;
            }
        }

        private class TagElement
        {
            private string tag_;
            private bool is_lock_;
            private string id_;

            private const string lock_str_ = "★";

            public TagElement(string tag)
            {
                Set(tag);
            }

            public TagElement(string tag, bool is_lock)
            {
                Set(tag, is_lock);
            }

            public TagElement(string tag, bool is_lock, string id)
            {
                Set(tag, is_lock, id);
            }

            public string Get()
            {
                return (is_lock_ ? lock_str_ : "") + tag_;
            }

            public string GetRawTag()
            {
                return tag_;
            }

            public string GetId()
            {
                return id_;
            }

            public bool IsLock()
            {
                return is_lock_;
            }

            public void Set(string tag)
            {
                if (tag.StartsWith(lock_str_))
                {
                    is_lock_ = true;
                    tag_ = tag.Substring(lock_str_.Length);
                }
                else
                {
                    is_lock_ = false;
                    tag_ = tag;
                }
            }

            public void Set(string tag, bool is_lock)
            {
                tag_ = tag;
                is_lock_ = is_lock;
            }

            public void Set(string tag, bool is_lock, string id)
            {
                tag_ = tag;
                is_lock_ = is_lock;
                id_ = id;
            }
        }
    }

    public enum HoseiKind { Vocaran, Nicoran, DailyVocaran, Nothing };

    // 再生数、コメント数、マイリスト数を組にしたもの。 getting_date は数値を取得した日時
    public struct RankPoint
    {
        public DateTime getting_date; // 数値を取得した日時
        public int view; // 再生数
        public int res;  // コメント数
        public int mylist; // マイリスト数

        public int CalcScore(RankingMethod ranking_method)
        {
            return CalcScore(ranking_method.hosei_kind, ranking_method.mylist_rate);
        }

        public static RankPoint operator +(RankPoint lhs, RankPoint rhs)
        {
            RankPoint point = new RankPoint();
            point.view = lhs.view + rhs.view;
            point.res = lhs.res + rhs.res;
            point.mylist = lhs.mylist + rhs.mylist;
            return point;
        }

        public static RankPoint operator -(RankPoint lhs, RankPoint rhs)
        {
            RankPoint point = new RankPoint();
            point.view = lhs.view - rhs.view;
            point.res = lhs.res - rhs.res;
            point.mylist = lhs.mylist - rhs.mylist;
            return point;
        }

        // 総ポイント数を取得
        public int CalcScore(HoseiKind hosei_kind, int mylist_rate)
        {
            switch (hosei_kind)
            {
                case HoseiKind.Vocaran:
                    // 補正値は小数第3位切り捨て
                    return view + res * (int)(GetHosei(hosei_kind, mylist_rate) * 100.0) / 100 + mylist * mylist_rate;
                case HoseiKind.Nicoran:
                    // 補正値は小数第3位切り上げ
                    return view + res * (int)(GetHosei(hosei_kind, mylist_rate) * 100.0 + 0.999999) / 100 + mylist * mylist_rate;
                //// 補正値は小数第7位切り捨て（過去）
                //return view + (int)((long)res * (long)(GetHosei(hosei_kind, mylist_rate) * 1000000.0) / 1000000) + mylist * mylist_rate;
                case HoseiKind.DailyVocaran: // 場当たり的対応。将来的には計算式をユーザがカスタマイズできるようにする
                    return view + Math.Min(res * 3, view) + mylist * mylist_rate;
                case HoseiKind.Nothing:
                    return view + res + mylist * mylist_rate;
            }
            return -1;
        }

        public double GetHosei(RankingMethod ranking_method)
        {
            return GetHosei(ranking_method.hosei_kind, ranking_method.mylist_rate);
        }

        // 補正値を取得
        public double GetHosei(HoseiKind hosei_kind, int mylist_rate)
        {
            switch (hosei_kind)
            {
                case HoseiKind.Vocaran:
                    if (view + mylist == 0)
                    {
                        return 0.01;
                    }
                    else if (view + res + mylist == 0)
                    {
                        return 0.00;
                    }
                    else
                    {
                        return (double)(view + mylist) / (view + res + mylist);
                    }
                case HoseiKind.Nicoran:
                    if (view + mylist == 0)
                    {
                        return 0.1;
                    }
                    else if (view >= res)
                    {
                        return 1.0;
                    }
                    else if (view + res + mylist * mylist_rate == 0)
                    {
                        return 0.0;
                    }
                    else
                    {
                        double value = (double)(view + view + mylist * mylist_rate) / (view + res + mylist * mylist_rate);
                        if (value < 0.1)
                        {
                            return 0.1;
                        }
                        else
                        {
                            return value;
                        }
                    }
                case HoseiKind.DailyVocaran:
                case HoseiKind.Nothing:
                    return 1.0;
            }
            return 0.0;
        }

        // 補正値を文字列にしたもの（小数第２位まで）を取得
        public string GetHoseiString(HoseiKind hosei_kind, int mylist_rate, int ketasuu, bool is_ceil)
        {
            double hosei = GetHosei(hosei_kind, mylist_rate);
            hosei = (double)((int)(hosei * Math.Pow(10, ketasuu) + (is_ceil ? 0.999999 : 0))) / Math.Pow(10, ketasuu);
            string format = "{0:0.";
            for (int i = 0; i < ketasuu; ++i)
            {
                format += "0";
            }
            format += "}";
            return String.Format(format, hosei);
        }

        // マイリスト率を取得
        public double GetMylistRate()
        {
            if (view == 0)
            {
                return 0.0;
            }
            else
            {
                return (double)mylist * 100 / view;
            }
        }

        // マイリスト率を文字列にしたもの（小数第２位まで）を取得
        public string GetMylistRateString()
        {
            int rate = (int)(GetMylistRate() * 100.0);
            bool sign = (rate >= 0);
            if (!sign)
            {
                rate = -rate;
            }
            return ((!sign) ? "-" : "") + (rate / 100).ToString() + "." + (rate / 10 % 10).ToString() +
                (rate % 10).ToString();
        }

        public override string ToString()
        {
            return NicoUtil.DateToString(getting_date) + "#" + view + "#" + res + "#" + mylist;
        }

        public string ToStringPoint()
        {
            return view.ToString() + "\t" + res + "\t" + mylist;
        }

        public string ToStringForPrint()
        {
            return NicoUtil.DateToString(getting_date) + "\t" + view + "\t" + res + "\t" + mylist;
        }

        public void Parse(string str)
        {
            string[] splitter = { "#" };
            string[] sArray = str.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            getting_date = NicoUtil.StringToDate(sArray[0]);
            view = int.Parse(sArray[1]);
            res = int.Parse(sArray[2]);
            mylist = int.Parse(sArray[3]);
        }

        // マイリスト率が 2％を超えているかを調べる
        public bool IsMylistRateSatisfy()
        {
            return (double)mylist / view * 100.0 >= 2.0;
        }
    }

    // １つの動画情報と複数の RankPoint を持つクラス。データベース用。
    public class RankVideo
    {
        public string video_id = "";
        public string title = "";
        public DateTime submit_date;
        public TagSet tag_set = new TagSet();
        public string length = "";
        private List<RankPoint> val_list_ = new List<RankPoint>();
        private const string delimiter1_ = "!&&!";
        private const string delimiter2_ = "$&&$";

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            buff.Append(video_id);
            buff.Append(delimiter1_);
            buff.Append(title);
            buff.Append(delimiter1_);
            buff.Append(NicoUtil.DateToString(submit_date));
            buff.Append(delimiter1_);
            buff.Append(tag_set.ToString());
            buff.Append(delimiter1_);
            for (int i = 0; i < val_list_.Count; ++i)
            {
                buff.Append(val_list_[i].ToString());
                if (i < val_list_.Count - 1)
                {
                    buff.Append(delimiter2_);
                }
            }
            buff.Append(delimiter1_);
            buff.Append(length);
            return buff.ToString();
        }

        public string ToStringForPrint()
        {
            StringBuilder buff = new StringBuilder();
            buff.Append(Filter() ? "○\t" : "×\t");
            buff.Append(video_id);
            buff.Append("\t");
            buff.Append(title);
            buff.Append("\t");
            buff.Append(NicoUtil.DateToString(submit_date));
            return buff.ToString();
        }

        public void ParseVideo(string str)
        {
            string[] splitter1 = { delimiter1_ };
            string[] splitter2 = { delimiter2_ };
            string[] sArray = str.Split(splitter1, StringSplitOptions.None);
            video_id = sArray[0];
            title = sArray[1];
            submit_date = NicoUtil.StringToDate(sArray[2]);
            string[] rankArray;
            tag_set.Parse(sArray[3]);
            rankArray = sArray[4].Split(splitter2, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < rankArray.Length; ++i)
            {
                RankPoint point = new RankPoint();
                point.Parse(rankArray[i]);
                val_list_.Add(point);
            }
            if (sArray.Length >= 6)
            {
                length = sArray[5];
            }
        }

        public bool EqualsId(RankVideo video)
        {
            return video_id.Equals(video.video_id);
        }

        public void AddRankVal(RankPoint point)
        {
            for (int i = 0; i < val_list_.Count; ++i)
            {
                if (point.getting_date.Equals(val_list_[i].getting_date))
                {
                    return;
                }
            }
            val_list_.Add(point);
        }

        public RankPoint GetNewestRankPoint()
        {
            return val_list_[val_list_.Count - 1];
        }

        public RankPoint GetRankPoint(DateTime date)
        {
            for (int i = 0; i < val_list_.Count; ++i)
            {
                if (date.Date == val_list_[i].getting_date.Date)
                {
                    return val_list_[i];
                }
            }
            return new RankPoint();
        }

        private bool IsEqualsDate(DateTime dt1, DateTime dt2)
        {
            return (dt1.Year == dt2.Year && dt1.Month == dt2.Month &&
                dt1.Day == dt2.Day);
        }

        // is_one_day : 評価期間が1日かどうか（false なら投稿日から現在）
        public RankPoint GetNewEvalRankVal(bool is_one_day, DateTime current)
        {
            RankPoint rank_value = new RankPoint();
            rank_value.view = 0;
            rank_value.res = 0;
            rank_value.mylist = 0;

            if (is_one_day)
            {
                if (val_list_.Count >= 2)
                {
                    RankPoint prev1 = val_list_[val_list_.Count - 1];
                    RankPoint prev2 = val_list_[val_list_.Count - 2];
                    DateTime dt2 = prev2.getting_date.AddDays(1.0);
                    if (IsEqualsDate(prev1.getting_date, current) && IsEqualsDate(dt2, current))
                    {
                        rank_value.view = prev1.view - prev2.view;
                        rank_value.res = prev1.res - prev2.res;
                        rank_value.mylist = prev1.mylist - prev2.mylist;
                    }
                }
            }
            else
            {
                if (val_list_.Count >= 1)
                {
                    RankPoint prev1 = val_list_[val_list_.Count - 1];
                    if (IsEqualsDate(prev1.getting_date, current))
                    {
                        rank_value.view = prev1.view;
                        rank_value.res = prev1.res;
                        rank_value.mylist = prev1.mylist;
                    }
                }
            }
            return rank_value;
        }

        // 1時間の集計
        public RankPoint GetRankValWithOneHourEval()
        {
            RankPoint rank_value = new RankPoint();
            rank_value.view = 0;
            rank_value.res = 0;
            rank_value.mylist = 0;

            if (val_list_.Count >= 2)
            {
                RankPoint prev1 = val_list_[val_list_.Count - 1];
                RankPoint prev2 = val_list_[val_list_.Count - 2];
                rank_value.view = prev1.view - prev2.view;
                rank_value.res = prev1.res - prev2.res;
                rank_value.mylist = prev1.mylist - prev2.mylist;
            }
            else if (val_list_[0].getting_date.Hour == 19)
            {
                RankPoint prev1 = val_list_[val_list_.Count - 1];
                if (prev1.view < 80000)
                {
                    rank_value.view = prev1.view;
                    rank_value.res = prev1.res;
                    rank_value.mylist = prev1.mylist;
                }
            }
            return rank_value;
        }

        // 指定した日時とその前日の両方にデータがある場合のみ、
        // 当日マイナス前日のデータを取得。それ以外の場合は 0 ポイントとなる。
        public RankPoint GetRankPointOfOneDay(DateTime date)
        {
            DateTime prev_one_day = date.AddDays(-1.0);
            RankPoint today = new RankPoint();
            RankPoint yesterday = new RankPoint();
            today.view = -1;
            yesterday.view = -1;
            for (int i = 0; i < val_list_.Count; ++i)
            {
                if (val_list_[i].getting_date.Date == date.Date)
                {
                    today = val_list_[i];
                }
                else if (val_list_[i].getting_date.Date == prev_one_day.Date)
                {
                    yesterday = val_list_[i];
                }
            }
            RankPoint point = new RankPoint();
            if (today.view == -1 || yesterday.view == -1)
            {
                point.view = 0;
                point.res = 0;
                point.mylist = 0;
            }
            else
            {
                point.view = today.view - yesterday.view;
                point.res = today.res - yesterday.res;
                point.mylist = today.mylist - yesterday.mylist;
            }
            return point;
        }

        public bool Filter()
        {
            throw new NotSupportedException();
        }
    }

    public static class VideoListUtil
    {
        public static bool ContainsDuplicateId(List<Video> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (Video.IdEquals(list[i], list[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<Video> Distinct(List<Video> list)
        {
            List<Video> distinct_list = new List<Video>();

            foreach (Video video in list)
            {
                Predicate<Video> pred = delegate(Video v) {
                    return Video.IdEquals(video, v);
                };

                if (!distinct_list.Exists(pred))
                {
                    distinct_list.Add(video);
                }
            }

            return distinct_list;
        }

        public static List<Video> Merge(List<Video> list_a, List<Video> list_b)
        {
            List<Video> merged_list = new List<Video>();

            foreach (Video video in list_a)
            {
                Predicate<Video> pred = delegate(Video v)
                {
                    return Video.IdEquals(video, v);
                };

                if (!merged_list.Exists(pred))
                {
                    merged_list.Add(video);
                }
            }

            foreach (Video video in list_b)
            {
                Predicate<Video> pred = delegate(Video v)
                {
                    return Video.IdEquals(video, v);
                };

                if (!merged_list.Exists(pred))
                {
                    merged_list.Add(video);
                }
            }

            return merged_list;
        }

        /// <summary>
        /// list_a と list_b が「等価」かどうかを判定。
        /// list_a に含まれる動画がすべて list_b にも含まれ、list_b に含まれる動画がすべて list_a にも
        /// 含まれるなら、list_a と list_b は等価とみなす。並び順は考慮しない。
        /// list_a に同じ動画IDが2つ以上含まれる場合は正しく動作しない（list_b も同様）。
        /// </summary>
        public static bool Equals(List<Video> list_a, List<Video> list_b)
        {
            if (list_a.Count != list_b.Count)
            {
                return false;
            }

            foreach (Video video_b in list_b)
            {
                Predicate<Video> pred = delegate(Video video_a)
                {
                    return Video.IdEquals(video_a, video_b);
                };

                if (!list_a.Exists(pred))
                {
                    return false;
                }
            }

            return true;
        }

        public static int RemoveAll(List<Video> list, Video video)
        {
            Predicate<Video> match = delegate(Video v)
            {
                return Video.IdEquals(video, v);
            };
            return list.RemoveAll(match);
        }
    }
}
