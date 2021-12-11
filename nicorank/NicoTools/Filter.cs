// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Text;
using IJLib;

namespace NicoTools
{
    /// <summary>
    /// フィルターを表すクラス
    /// </summary>
    public class Filter : IFilterManager
    {
        private bool is_output_filtered_video_; // フィルター「された」動画を出力するかどうか
        private List<FilterElement> element_list_ = new List<FilterElement>();

        public Filter()
        {
            is_output_filtered_video_ = false;
        }

        public Filter(bool is_output_filtered_video)
        {
            is_output_filtered_video_ = is_output_filtered_video;
        }

        public bool IsOutputFilteredVideo()
        {
            return is_output_filtered_video_;
        }

        /// <summary>
        /// フィルターを解析する（フィルターを使う前にこの関数を呼び出す必要がある）
        /// </summary>
        /// <param name="str">フィルターファイルの中身</param>
        public void Parse(string str)
        {
            string[] line = IJStringUtil.SplitWithCRLFAndEraseComment(str);

            int start_blacket = -1;

            for (int i = 0; i < line.Length; ++i)
            {
                if (line[i].StartsWith("["))
                {
                    if (start_blacket >= 0) // 最初は無視
                    {
                        element_list_.Add(FilterElementFactory.MakeElement(line, start_blacket, i));
                    }
                    start_blacket = i;
                }
            }
            // 最後のブロックの処理
            if (start_blacket >= 0)
            {
                element_list_.Add(FilterElementFactory.MakeElement(line, start_blacket, line.Length));
            }
        }

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < element_list_.Count; ++i)
            {
                buff.Append(element_list_[i].ToString() + "\r\n");
            }
            return buff.ToString();
        }

        /// <summary>
        /// フィルターを通るかどうか検査する。
        /// </summary>
        /// <param name="video">検査する Video クラス</param>
        /// <returns>検査結果（通った場合は true）</returns>
        public bool IsThrough(Video video)
        {
            for (int i = 0; i < element_list_.Count; ++i)
            {
                // 一つでも通らなければアウト
                if (!element_list_[i].IsThrough(video))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Video クラスにエフェクトをかける
        /// </summary>
        /// <param name="video">エフェクトをかける Video クラス</param>
        public void DoEffect(Video video)
        {
            for (int i = 0; i < element_list_.Count; ++i)
            {
                element_list_[i].DoEffect(video);
            }
        }
    }

    class FilterElementFactory
    {
        public static FilterElement MakeElement(string[] line, int start_line, int end_line)
        {
            int index = line[start_line].IndexOf(']');
            if (index < 0)
            {
                throw new FormatException();
            }
            switch (line[start_line].Substring(0, index + 1).ToLower())
            {
                case "[ng video id]":
                    return new FilterElementVideoId(false, line, start_line + 1, end_line);
                case "[pickup video id]":
                    return new FilterElementVideoId(true, line, start_line + 1, end_line);
                case "[ng title]":
                    return new FilterElementTitle(false, line, start_line + 1, end_line);
                case "[pickup title]":
                    return new FilterElementTitle(true, line, start_line + 1, end_line);
                case "[ng tag]":
                    return new FilterTag(false, line, start_line + 1, end_line);
                case "[pickup tag]":
                    return new FilterTag(true, line, start_line + 1, end_line);
                case "[ng submit_date]":
                    return new FilterElementSubmitDate(false, line, start_line + 1, end_line);
                case "[pickup submit_date]":
                    return new FilterElementSubmitDate(true, line, start_line + 1, end_line);
                case "[ng view]":
                    return new FilterElementView(false, line, start_line + 1, end_line);
                case "[pickup view]":
                    return new FilterElementView(true, line, start_line + 1, end_line);
                case "[ng comment]":
                    return new FilterElementComment(false, line, start_line + 1, end_line);
                case "[pickup comment]":
                    return new FilterElementComment(true, line, start_line + 1, end_line);
                case "[ng mylist]":
                    return new FilterElementMylist(false, line, start_line + 1, end_line);
                case "[pickup mylist]":
                    return new FilterElementMylist(true, line, start_line + 1, end_line);
                case "[delete tag]":
                    return new FilterDeleteTag(true, line, start_line + 1, end_line);
                case "[delete !tag]":
                    return new FilterDeleteTag(false, line, start_line + 1, end_line);
            }
            throw new FormatException();
        }
    }

    public abstract class FilterElement
    {
        protected bool is_affirmation_;
        protected List<string> word_list_ = new List<string>();

        protected FilterElement(bool is_affirmation, string[] line, int start_line, int end_line)
        {
            is_affirmation_ = is_affirmation;
            for (int i = start_line; i < end_line; ++i)
            {
                word_list_.Add(line[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < word_list_.Count; ++i)
            {
                buff.Append(word_list_[i] + "\r\n");
            }
            return buff.ToString();
        }

        public virtual void DoEffect(Video video)
        {
            return;
        }

        public abstract bool IsThrough(Video video);
    }

    class FilterElementVideoId : FilterElement
    {
        public FilterElementVideoId(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup video id]\r\n" + base.ToString();
            }
            else
            {
                return "[ng video id]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < word_list_.Count; ++i)
            {
                if (word_list_[i] == video.video_id)
                {
                    return is_affirmation_;
                }
            }
            return !is_affirmation_;
        }
    }

    class FilterElementTitle : FilterElement
    {
        public FilterElementTitle(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup title]\r\n" + base.ToString();
            }
            else
            {
                return "[ng title]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < word_list_.Count; ++i)
            {
                if (IJStringUtil.IndexOfIgnoreCase(video.title, word_list_[i]) >= 0)
                {
                    return is_affirmation_;
                }
            }
            return !is_affirmation_;
        }
    }

    class FilterTag : FilterElement
    {
        public FilterTag(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup tag]\r\n" + base.ToString();
            }
            else
            {
                return "[ng tag]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < word_list_.Count; ++i)
            {
                if (video.tag_set.IsInclude(word_list_[i]))
                {
                    return is_affirmation_;
                }
            }
            return !is_affirmation_;
        }
    }

    class FilterElementSubmitDate : FilterElement
    {
        private List<DateTime> start_datetime_list_ = new List<DateTime>();
        private List<DateTime> end_datetime_list_ = new List<DateTime>();

        public FilterElementSubmitDate(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line)
        {
            for (int i = 0; i < word_list_.Count; ++i)
            {
                int sep = word_list_[i].IndexOf(',');
                if (sep >= 0)
                {
                    string dts1 = word_list_[i].Substring(0, sep).Trim();
                    string dts2 = word_list_[i].Substring(sep + 1).Trim();
                    DateTime dt1;
                    DateTime dt2;

                    if (dts1 != "")
                    {
                        dt1 = DateTime.ParseExact(dts1, "yyyy/MM/dd HH:mm:ss", null);
                    }
                    else
                    {
                        dt1 = DateTime.MinValue;
                    }
                    if (dts2 != "")
                    {
                        dt2 = DateTime.ParseExact(dts2, "yyyy/MM/dd HH:mm:ss", null);
                    }
                    else
                    {
                        dt2 = DateTime.MaxValue;
                    }
                    if (dt2 < dt1)
                    {
                        DateTime temp = dt1;
                        dt1 = dt2;
                        dt2 = temp;
                    }
                    start_datetime_list_.Add(dt1);
                    end_datetime_list_.Add(dt2);
                }
                else
                {
                    start_datetime_list_.Add(DateTime.ParseExact(word_list_[i].Trim(), "yyyy/MM/dd HH:mm:ss", null));
                    end_datetime_list_.Add(DateTime.MaxValue);
                }
            }
        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup submit_date]\r\n" + base.ToString();
            }
            else
            {
                return "[ng submit_date]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < start_datetime_list_.Count; ++i)
            {
                if (!(start_datetime_list_[i] <= video.submit_date && video.submit_date <= end_datetime_list_[i]))
                {
                    return !is_affirmation_;
                }
            }
            return is_affirmation_;
        }
    }

    abstract class FilterElementValue : FilterElement
    {
        protected List<int> start_value_list_ = new List<int>();
        protected List<int> end_value_list_ = new List<int>();

        public FilterElementValue(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line)
        {
            for (int i = 0; i < word_list_.Count; ++i)
            {
                int sep = word_list_[i].IndexOf(',');
                if (sep >= 0)
                {
                    string dts1 = word_list_[i].Substring(0, sep).Trim();
                    string dts2 = word_list_[i].Substring(sep + 1).Trim();
                    int v1;
                    int v2;

                    if (dts1 != "")
                    {
                        v1 = int.Parse(dts1);
                    }
                    else
                    {
                        v1 = int.MinValue;
                    }
                    if (dts2 != "")
                    {
                        v2 = int.Parse(dts2);
                    }
                    else
                    {
                        v2 = int.MaxValue;
                    }
                    if (v2 < v1)
                    {
                        int temp = v1;
                        v1 = v2;
                        v2 = temp;
                    }
                    start_value_list_.Add(v1);
                    end_value_list_.Add(v2);
                }
                else
                {
                    start_value_list_.Add(int.Parse(word_list_[i].Trim()));
                    end_value_list_.Add(int.MaxValue);
                }
            }
        }
    }

    class FilterElementView : FilterElementValue
    {
        public FilterElementView(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup view]\r\n" + base.ToString();
            }
            else
            {
                return "[ng view]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < start_value_list_.Count; ++i)
            {
                if (!(start_value_list_[i] <= video.point.view && video.point.view <= end_value_list_[i]))
                {
                    return !is_affirmation_;
                }
            }
            return is_affirmation_;
        }
    }

    class FilterElementComment : FilterElementValue
    {
        public FilterElementComment(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup comment]\r\n" + base.ToString();
            }
            else
            {
                return "[ng comment]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < start_value_list_.Count; ++i)
            {
                if (!(start_value_list_[i] <= video.point.res && video.point.res <= end_value_list_[i]))
                {
                    return !is_affirmation_;
                }
            }
            return is_affirmation_;
        }
    }

    class FilterElementMylist : FilterElementValue
    {
        public FilterElementMylist(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[pickup mylist]\r\n" + base.ToString();
            }
            else
            {
                return "[ng mylist]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            for (int i = 0; i < start_value_list_.Count; ++i)
            {
                if (!(start_value_list_[i] <= video.point.mylist && video.point.mylist <= end_value_list_[i]))
                {
                    return !is_affirmation_;
                }
            }
            return is_affirmation_;
        }
    }

    class FilterDeleteTag : FilterElement
    {
        public FilterDeleteTag(bool is_affirmation, string[] line, int start_line, int end_line)
            : base(is_affirmation, line, start_line, end_line) 
        {

        }

        public override string ToString()
        {
            if (is_affirmation_)
            {
                return "[delete tag]\r\n" + base.ToString();
            }
            else
            {
                return "[delete !tag]\r\n" + base.ToString();
            }
        }

        public override bool IsThrough(Video video)
        {
            return true;
        }

        public override void DoEffect(Video video)
        {
            if (is_affirmation_)
            {
                for (int i = 0; i < word_list_.Count; ++i)
                {
                    video.tag_set.DeleteTag(word_list_[i]);
                }
            }
        }
    }
}
