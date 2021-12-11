// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using IJLib;

namespace NicoTools
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFilterManager
    {
        bool IsThrough(Video video);
        bool IsOutputFilteredVideo();
        void DoEffect(Video video);
    }

    /// <summary>
    /// 
    /// </summary>
    public class RootFilter : IFilterManager
    {
        /// <summary>
        /// 
        /// </summary>
        public const string version_indicator = "version=2";

        IVideoFilter filter_;
        bool is_output_filtered_video_;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="is_output_filtered_video"></param>
        /// <param name="filter"></param>
        public RootFilter(bool is_output_filtered_video, IVideoFilter filter)
        {
            is_output_filtered_video_ = is_output_filtered_video;
            filter_ = filter;
        }

        #region IRootFilter Members

        public bool IsThrough(Video video)
        {
            return filter_.IsThrough(video);
        }

        public bool IsOutputFilteredVideo()
        {
            return is_output_filtered_video_;
        }

        public void DoEffect(Video video)
        {

        }

        #endregion
    }

    /// <summary>
    /// フィルターのインターフェース。
    /// </summary>
    public interface IVideoFilter
    {
        bool IsThrough(Video video);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AllowFilter : IVideoFilter
    {

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            return true;
        }

        #endregion
    }

    /// <summary>
    /// 複数のフィルターで AND 演算するためのフィルター。
    /// 1つでもfalseを返すフィルターがあった場合、falseを返却する。
    /// </summary>
    public class AndFilter : IVideoFilter
    {
        private List<IVideoFilter> sub_filters_;

        public AndFilter()
        {
            sub_filters_ = new List<IVideoFilter>();
        }

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            return sub_filters_.TrueForAll(
                delegate(IVideoFilter sub_filter)
                {
                    return sub_filter.IsThrough(video);
                });
        }

        #endregion

        /// <summary>
        /// AND で連結されるフィルターのリスト。
        /// </summary>
        public IList<IVideoFilter> SubFilters
        {
            get { return sub_filters_; }
        }
    }

    /// <summary>
    /// 複数のフィルターで OR 演算するためのフィルター。
    /// 1 つでも true を返すフィルターがあった場合、true を返却する。
    /// </summary>
    public class OrFilter : IVideoFilter
    {
        private List<IVideoFilter> sub_filters_;

        public OrFilter()
        {
            sub_filters_ = new List<IVideoFilter>();
        }

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            return sub_filters_.Exists(
                delegate(IVideoFilter sub_filter)
                {
                    return sub_filter.IsThrough(video);
                });

        }

        #endregion

        /// <summary>
        /// OR で連結されるフィルターのリスト。
        /// </summary>
        public IList<IVideoFilter> SubFilters
        {
            get { return sub_filters_; }
        }
    }

    /// <summary>
    /// フィルターに NOT 演算を適用するフィルター。
    /// 内部フィルターが true を返却するときは false を、false を返却するときは
    /// true を返却する。
    /// </summary>
    public class NotFilter : IVideoFilter
    {
        private IVideoFilter inner_filter_;

        /// <summary>
        /// 指定されたフィルターで NOT をするフィルターを生成する。
        /// </summary>
        /// <param name="inner_filter">NOT 演算を適用するフィルター。</param>
        public NotFilter(IVideoFilter inner_filter)
        {
            inner_filter_ = inner_filter;
        }

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            return !(inner_filter_.IsThrough(video));
        }

        #endregion

        /// <summary>
        /// NOT 演算を適用するフィルターを取得、または設定する。
        /// </summary>
        public IVideoFilter InnerFilter
        {
            get { return inner_filter_; }
            set { inner_filter_ = value; }
        }
    }

    /// <summary>
    /// 文字列の比較により判定を行うフィルターの基底クラス。
    /// </summary>
    /// <remarks>
    /// フィルターの条件として文字列の比較を使用するフィルターの基底クラスを表わす。
    /// 「正規表現」、「大文字、小文字を区別する・しない」、「部分一致・全体一致」の
    /// 3 つのオプションがある。
    /// 「正規表現」が指定された場合、他の 2 つのオプションは正規表現のパターン内で
    /// 表現できるので無視される。
    /// </remarks>
    public abstract class TextFilterBase : IVideoFilter
    {
        string pattern_;
        bool is_regex_;
        bool ignore_case_;
        bool match_all_;
        Regex regex_;

        /// <summary>
        /// パターンとオプションを指定して、クラスを初期化する。
        /// </summary>
        /// <param name="pattern">このフィルターのパターンを表わす文字列。</param>
        /// <param name="is_regex">パターンを正規表現と見做すかどうかを表わすオプション。</param>
        /// <param name="ignore_case">比較を行なう際、大文字を小文字を区別するかどうかを表わすオプション。</param>
        /// <param name="match_all">比較を行う際、全体一致が必要かどうかを表わすオプション</param>
        public TextFilterBase(string pattern, bool is_regex, bool ignore_case, bool match_all)
        {
            pattern_ = pattern;
            is_regex_ = is_regex;
            ignore_case_ = ignore_case;
            match_all_ = match_all;
            if (is_regex)
            {
                regex_ = new Regex(pattern);
            }
            else
            {
                regex_ = null;
            }
        }

        /// <summary>
        /// パターンと正規表現オプションを指定して、クラスを初期化する。
        /// 省略された 2 つのオプションにはいずれも false が設定される。
        /// </summary>
        /// <remark>
        /// このクラスを継承するクラスでは、省略されたオプションについて独自の既定値を
        /// 設定する場合がある。
        /// </remark>
        /// <param name="pattern">このフィルターのパターンを表わす文字列。</param>
        /// <param name="is_regex">パターンを正規表現と見做すかどうかを表わすオプション。</param>
        public TextFilterBase(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// パターンを指定して、クラスを初期化する。
        /// 省略された 3 つのオプションにははいずれも false が設定される。
        /// </summary>
        /// <remark>
        /// このクラスを継承するクラスでは、省略されたオプションについて独自の既定値を
        /// 設定する場合がある。
        /// </remark>
        /// <param name="pattern">このフィルターのパターンを表わす文字列。</param>
        public TextFilterBase(string pattern)
            : this(pattern, false, true, false)
        {

        }

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            bool result;
            string text = GetText(video);
            if (text == null)
            {
                return false;
            }

            if (is_regex_)
            {
                result = regex_.IsMatch(text);
            }
            else
            {
                if (ignore_case_)
                {
                    if (match_all_)
                    {
                        result = IJStringUtil.CompareString(text, pattern_);
                    }
                    else
                    {
                        result = IJStringUtil.IndexOfIgnoreCase(text, pattern_) >= 0;
                    }
                }
                else
                {
                    if (match_all_)
                    {
                        result = text.Equals(pattern_, StringComparison.Ordinal);
                    }
                    else
                    {
                        result = text.IndexOf(pattern_, StringComparison.Ordinal) >= 0;
                    }
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// フィルターを適用するオブジェクトから、
        /// パターンと比較を行うための文字列を取得する。
        /// </summary>
        /// <param name="item">フィルターを適用するオブジェクト。</param>
        /// <returns>パターンと比較を行う文字列。</returns>
        protected abstract string GetText(Video video);

        /// <summary>
        /// このフィルターの文字列パターンを取得、または設定する。
        /// </summary>
        public string Pattern
        {
            get { return pattern_; }
            set { pattern_ = value; }
        }

        /// <summary>
        /// フィルターのパターンを正規表現と見做すかどうかを表わすオプションを
        /// 取得、または設定する。
        /// </summary>
        public bool IsRegex
        {
            get { return is_regex_; }
            set { is_regex_ = value; }
        }

        /// <summary>
        /// 比較の際に大文字と小文字を区別するかどうかを表わすオプションを
        /// 取得、または設定する。
        /// </summary>
        public bool IgnoreCase
        {
            get { return ignore_case_; }
            set { ignore_case_ = value; }
        }

        /// <summary>
        /// 比較に全体一致が必要かどうかを表わすオプションを取得、または設定する。
        /// </summary>
        public bool MatchAll
        {
            get { return match_all_; }
            set { match_all_ = value; }
        }
    }

    /// <summary>
    /// 整数の範囲により判定を行なうフィルターを表わすクラス。
    /// </summary>
    public abstract class NumericalRangeFilterBase : IVideoFilter
    {
        private long lower_bound_;
        private long upper_bound_;

        /// <summary>
        /// 下限を上限を指定してクラスを初期化する。
        /// </summary>
        /// <remark>
        /// フィルターがtrueを返す範囲には下限、上限も含む。つまり下限が 3、 上限が 9
        /// の場合、このフィルターは 3 と 9 に対しては true を返却し、2 や 10 に対しては
        /// false を返却する。
        /// </remark>
        /// <param name="lower_bound">下限値。</param>
        /// <param name="upper_bound">上限値。</param>
        public NumericalRangeFilterBase(long lower_bound, long upper_bound)
        {
            lower_bound_ = lower_bound;
            upper_bound_ = upper_bound;
        }

        #region IVideoFilter Members

        public virtual bool IsThrough(Video video)
        {
            long item_value = GetNumericalValue(video);

            return ((lower_bound_ <= item_value) && (item_value <= upper_bound_));
        }

        #endregion

        /// <summary>
        /// フィルターを適用するオブジェクトから、下限上限と比較を行うための
        /// 整数値を取得する。
        /// </summary>
        /// <param name="item">フィルターを適用するオブジェクト</param>
        /// <returns>下限、上限と比較を行うための整数。</returns>
        protected abstract long GetNumericalValue(Video video);

        /// <summary>
        /// このフィルターの下限値を取得、または設定する。
        /// </summary>
        public long LowerBound
        {
            get { return lower_bound_; }
            set { lower_bound_ = value; }
        }

        /// <summary>
        /// このフィルターの上限値を取得、または設定する。
        /// </summary>
        public long UpperBound
        {
            get { return upper_bound_; }
            set { upper_bound_ = value; }
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="NicoTools.Video.video_id"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoIdFilter : TextFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        /// <param name="ignore_case"></param>
        /// <param name="match_all"></param>
        public VideoIdFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public VideoIdFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public VideoIdFilter(string pattern)
            : this(pattern, false, false, true)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override string GetText(Video video)
        {
            return video.video_id;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.title"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoTitleFilter : TextFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        /// <param name="ignore_case"></param>
        /// <param name="match_all"></param>
        public VideoTitleFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public VideoTitleFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public VideoTitleFilter(string pattern)
            : this(pattern, false, true, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override string GetText(Video video)
        {
            return video.title;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.description"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoDescriptionFilter : TextFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        /// <param name="ignore_case"></param>
        /// <param name="match_all"></param>
        public VideoDescriptionFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public VideoDescriptionFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public VideoDescriptionFilter(string pattern)
            : this(pattern, false, true, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override string GetText(Video video)
        {
            return video.description;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.submit_date"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoSubmitDateFilter : NumericalRangeFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min_date"></param>
        /// <param name="max_date"></param>
        public VideoSubmitDateFilter(DateTime min_date, DateTime max_date)
            : base(min_date.Ticks, max_date.Ticks)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override long GetNumericalValue(Video video)
        {
            return video.submit_date.Ticks;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime MinDate
        {
            get { return new DateTime(base.LowerBound); }
            set { base.LowerBound = value.Ticks; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime MaxDate
        {
            get { return new DateTime(base.UpperBound); }
            set { base.UpperBound = value.Ticks; }
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの再生数を対象としたフィルター。
    /// </summary>
    public class VideoViewFilter : NumericalRangeFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lower_bound"></param>
        /// <param name="upper_bound"></param>
        public VideoViewFilter(long lower_bound, long upper_bound)
            : base(lower_bound, upper_bound)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override long GetNumericalValue(Video video)
        {
            return video.point.view;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスのコメント数を対象としたフィルター。
    /// </summary>
    public class VideoCommentFilter : NumericalRangeFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lower_bound"></param>
        /// <param name="upper_bound"></param>
        public VideoCommentFilter(long lower_bound, long upper_bound)
            : base(lower_bound, upper_bound)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override long GetNumericalValue(Video video)
        {
            return video.point.res;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスのマイリスト数を対象としたフィルター。
    /// </summary>
    public class VideoMylistFilter : NumericalRangeFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lower_bound"></param>
        /// <param name="upper_bound"></param>
        public VideoMylistFilter(long lower_bound, long upper_bound)
            : base(lower_bound, upper_bound)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override long GetNumericalValue(Video video)
        {
            return video.point.mylist;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.pname"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoPNameFilter : TextFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        /// <param name="ignore_case"></param>
        /// <param name="match_all"></param>
        public VideoPNameFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public VideoPNameFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public VideoPNameFilter(string pattern)
            : this(pattern, false, true, true)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override string GetText(Video video)
        {
            return video.pname;
        }
    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.length"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoLengthFilter : NumericalRangeFilterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min_length"></param>
        /// <param name="max_length"></param>
        public VideoLengthFilter(TimeSpan min_length, TimeSpan max_length)
            : base(min_length.Ticks, max_length.Ticks)
        {

        }

        public override bool IsThrough(Video video)
        {
            if (string.IsNullOrEmpty(video.length))
            {
                return false;
            }

            return base.IsThrough(video);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        protected override long GetNumericalValue(Video video)
        {
            TimeSpan length = GetLengthFromString(video.length);
            return length.Ticks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length_string"></param>
        /// <returns></returns>
        private TimeSpan GetLengthFromString(string length_string)
        {
            string[] min_sec = length_string.Split(':');

            int min = int.Parse(min_sec[0]);
            int sec = int.Parse(min_sec[1]);

            return new TimeSpan(0, min, sec);
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MinLength
        {
            get { return new TimeSpan(base.LowerBound); }
            set { base.LowerBound = value.Ticks; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MaxLength
        {
            get { return new TimeSpan(base.UpperBound); }
            set { base.UpperBound = value.Ticks; }
        }

    }

    /// <summary>
    /// <see cref="Video"/> クラスの <see cref="Video.tag_set"/> を
    /// 対象としたフィルター。
    /// </summary>
    public class VideoTagFilter : IVideoFilter
    {
        string pattern_;
        bool is_regex_;
        bool ignore_case_;
        bool match_all_;
        Regex regex_;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        public VideoTagFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
        {
            pattern_ = pattern;
            is_regex_ = is_regex;
            ignore_case_ = ignore_case;
            match_all_ = match_all;
            if (is_regex)
            {
                regex_ = new Regex(pattern);
            }
            else
            {
                regex_ = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public VideoTagFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public VideoTagFilter(string pattern)
            : this(pattern, false, true, true)
        {

        }

        #region IVideoFilter Members

        public bool IsThrough(Video video)
        {
            bool result;

            if (is_regex_)
            {
                result = video.tag_set.IsInclude(TagEqualsWithRegex);
            }
            else
            {
                result = video.tag_set.IsInclude(TagEqualsWithOption);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected virtual bool TagEqualsWithRegex(string tag, bool is_locked)
        {
            return regex_.IsMatch(tag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected virtual bool TagEqualsWithOption(string tag, bool is_locked)
        {
            bool result;

            if (tag == null)
            {
                return false;
            }

            if (ignore_case_)
            {
                if (match_all_)
                {
                    result = IJStringUtil.CompareString(tag, pattern_);
                }
                else
                {
                    result = (IJStringUtil.IndexOfIgnoreCase(tag, pattern_) >= 0);
                }
            }
            else
            {
                if (match_all_)
                {
                    result = tag.Equals(pattern_, StringComparison.Ordinal);
                }
                else
                {
                    result = (tag.IndexOf(pattern_, StringComparison.Ordinal) >= 0);
                }
            }

            return result;
        }
        /// <summary>
        /// このフィルターの文字列パターンを取得、または設定する。
        /// </summary>
        public string Pattern
        {
            get { return pattern_; }
            set { pattern_ = value; }
        }

        /// <summary>
        /// フィルターのパターンを正規表現と見做すかどうかを表わすオプションを
        /// 取得、または設定する。
        /// </summary>
        public bool IsRegex
        {
            get { return is_regex_; }
            set { is_regex_ = value; }
        }

        /// <summary>
        /// 比較の際に大文字と小文字を区別するかどうかを表わすオプションを
        /// 取得、または設定する。
        /// </summary>
        public bool IgnoreCase
        {
            get { return ignore_case_; }
            set { ignore_case_ = value; }
        }

        /// <summary>
        /// 比較に全体一致が必要かどうかを表わすオプションを取得、または設定する。
        /// </summary>
        public bool MatchAll
        {
            get { return match_all_; }
            set { match_all_ = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LockedVideoTagFilter : VideoTagFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public LockedVideoTagFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public LockedVideoTagFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public LockedVideoTagFilter(string pattern)
            : this(pattern, false, true, true)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected override bool TagEqualsWithRegex(string tag, bool is_locked)
        {
            bool result;

            if (is_locked == true)
            {
                result = base.TagEqualsWithRegex(tag, is_locked);
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected override bool TagEqualsWithOption(string tag, bool is_locked)
        {
            bool result;

            if (is_locked == true)
            {
                result = base.TagEqualsWithOption(tag, is_locked);
            }
            else
            {
                result = false;
            }

            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UnlockedVideoTagFilter : VideoTagFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        public UnlockedVideoTagFilter(string pattern, bool is_regex, bool ignore_case, bool match_all)
            : base(pattern, is_regex, ignore_case, match_all)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="is_regex"></param>
        public UnlockedVideoTagFilter(string pattern, bool is_regex)
            : this(pattern, is_regex, false, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public UnlockedVideoTagFilter(string pattern)
            : this(pattern, false, true, true)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected override bool TagEqualsWithRegex(string tag, bool is_locked)
        {
            bool result;

            if (is_locked == false)
            {
                result = base.TagEqualsWithRegex(tag, is_locked);
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="is_locked"></param>
        /// <returns></returns>
        protected override bool TagEqualsWithOption(string tag, bool is_locked)
        {
            bool result;

            if (is_locked == false)
            {
                result = base.TagEqualsWithOption(tag, is_locked);
            }
            else
            {
                result = false;
            }

            return result;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <see cef="FilterParser.Parse" /> メソッドはスレッドアンセーフなので、
    /// 複数のスレッドで使用する場合は、排他処理をするか、スレッド毎に
    /// <see cef="FilterParser" /> のインスタンスを持たせること。
    /// </remarks>
    public class FilterParser
    {
        /// <summary>
        /// 
        /// </summary>
        private enum TokenType
        {
            None = 0,           // 不明なタイプ
            OpNot,              // NOT 演算子
            OpOr,               // OR 演算子
            OpAnd,              // AND 演算子
            OpenParen,          // 開き括弧「(」
            CloseParen,         // 閉じ括弧「)」
            Pattern             // フィルターパターン
        }

        // トークン解析に用いる正規表現
        // (?<token_pattern>
        //   (?<pattern_name>(?:id|title|description|submit|view|comment|mylist|pname|length|tag|ltag|utag)list)
        //   (?:(?<pattern_option_indicator>@)(?<pattern_option>[air]*))?:
        //   (?:(?<list_pattern_paren>\()[\r\n]+(?:(?<pattern_value>[^\r\n]+)[\r\n]+)*?\)(?:[\r\n]|$)
        //      |
        //      (?<list_pattern_paren>{)[\r\n]+(?:(?<pattern_value>[^\r\n]+)[\r\n]+)*?}(?:[\r\n]|$)))
        // |
        // (?<token_pattern>
        //   (?<pattern_name>submit):
        //   (?<pattern_value>\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} *, *\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}| *, *\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}|\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} *,))
        // |
        // (?<token_pattern>
        //   (?<pattern_name>id|title|description|submit|view|comment|mylist|pname|length|tag|ltag|utag)
        //   (?:(?<pattern_option_indicator>@)(?<pattern_option>[air]*))?:
        //   (?<pattern_value>[^") \r\n]+|"(?:[^"\\]|\\.)+"))
        // |
        // (?<token_not>-)
        // |
        // (?<token_or>\bor\b)
        // |
        // (?<token_and>\band\b)
        // |
        // (?<token_open_paren>\()
        // |
        // (?<token_close_paren>\))
        private const string token_regex =
            "(?<token_pattern>(?<pattern_name>(?:id|title|description|submit|view|comment|myl" +
            "ist|pname|length|tag|ltag|utag)list)(?:(?<pattern_option_indicator>@)(?<pattern_" +
            "option>[air]*))?:(?:(?<list_pattern_paren>\\()[\\r\\n]+(?:(?<pattern_value>[^\\r" +
            "\\n]+)[\\r\\n]+)*?\\)(?:[\\r\\n]|$)|(?<list_pattern_paren>{)[\\r\\n]+(?:(?<patter" +
            "n_value>[^\\r\\n]+)[\\r\\n]+)*?}(?:[\\r\\n]|$)))|(?<token_pattern>(?<pattern_name" +
            ">submit):(?<pattern_value>\\d{4}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2} *, *\\d{4}/" +
            "\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2}| *, *\\d{4}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2" +
            "}|\\d{4}/\\d{2}/\\d{2} \\d{2}:\\d{2}:\\d{2} *,))|(?<token_pattern>(?<pattern_nam" +
            "e>id|title|description|submit|view|comment|mylist|pname|length|tag|ltag|utag)(?:" +
            "(?<pattern_option_indicator>@)(?<pattern_option>[air]*))?:(?<pattern_value>[^\")" +
            " \\r\\n]+|\"(?:[^\"\\\\]|\\\\.)+\"))|(?<token_not>-)|(?<token_or>\\bor\\b)|(?<token_" +
            "and>\\band\\b)|(?<token_open_paren>\\()|(?<token_close_paren>\\))";

        // コメント行削除に用いる正規表現
        // (?:^|[\r\n])(?:#|//)(?:[^\r\n]*|$)
        private const string comment_line_regex = "(?:^|[\\r\\n])(?:#|//)(?:[^\\r\\n]*|$)";

        private string expression_;
        private MatchCollection tokens_;
        private Match token_;
        private int token_index_;
        private TokenType token_type_;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private delegate T Generator<T>();

        private IDictionary<string, Generator<IVideoFilter>> atomic_filter_parsers_;

        /// <summary>
        /// 
        /// </summary>
        public FilterParser()
        {
            atomic_filter_parsers_ = new Dictionary<string, Generator<IVideoFilter>>();
            atomic_filter_parsers_.Add("id", ParseVideoIdFilter);
            atomic_filter_parsers_.Add("title", ParseVideoTitleFilter);
            atomic_filter_parsers_.Add("description", ParseVideoDescriptionFilter);
            atomic_filter_parsers_.Add("submit", ParseVideoSubmitDateFilter);
            atomic_filter_parsers_.Add("view", ParseVideoViewFilter);
            atomic_filter_parsers_.Add("comment", ParseVideoCommentFilter);
            atomic_filter_parsers_.Add("mylist", ParseVideoMylistFilter);
            atomic_filter_parsers_.Add("pname", ParseVideoPNameFilter);
            atomic_filter_parsers_.Add("length", ParseVideoLengthFilter);
            atomic_filter_parsers_.Add("tag", ParseVideoTagFilter);
            atomic_filter_parsers_.Add("ltag", ParseVideoTagFilter);
            atomic_filter_parsers_.Add("utag", ParseVideoTagFilter);
            atomic_filter_parsers_.Add("idlist", ParseVideoIdListFilter);
            atomic_filter_parsers_.Add("titlelist", ParseVideoTitleListFilter);
            atomic_filter_parsers_.Add("descriptionlist", ParseVideoDescriptionListFilter);
            atomic_filter_parsers_.Add("submitlist", ParseVideoSubmitDateListFilter);
            atomic_filter_parsers_.Add("viewlist", ParseVideoViewListFilter);
            atomic_filter_parsers_.Add("commentlist", ParseVideoCommentListFilter);
            atomic_filter_parsers_.Add("mylistlist", ParseVideoMylistListFilter);
            atomic_filter_parsers_.Add("pnamelist", ParseVideoPNameListFilter);
            atomic_filter_parsers_.Add("lengthlist", ParseVideoLengthListFilter);
            atomic_filter_parsers_.Add("taglist", ParseVideoTagListFilter);
            atomic_filter_parsers_.Add("ltaglist", ParseVideoTagListFilter);
            atomic_filter_parsers_.Add("utaglist", ParseVideoTagListFilter);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetToken()
        {
            if (token_index_ < tokens_.Count)
            {
                token_ = tokens_[token_index_];
                token_type_ = GetTokenType(token_);
                token_index_++;
            }
            else
            {
                token_ = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private TokenType GetTokenType(Match token)
        {
            TokenType type;
            if (token.Groups["token_not"].Success)
            {
                type = TokenType.OpNot;
            }
            else if (token.Groups["token_or"].Success)
            {
                type = TokenType.OpOr;
            }
            else if (token.Groups["token_and"].Success)
            {
                type = TokenType.OpAnd;
            }
            else if (token.Groups["token_open_paren"].Success)
            {
                type = TokenType.OpenParen;
            }
            else if (token.Groups["token_close_paren"].Success)
            {
                type = TokenType.CloseParen;
            }
            else if (token.Groups["token_pattern"].Success)
            {
                type = TokenType.Pattern;
            }
            else
            {
                type = TokenType.None;
            }

            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IVideoFilter Parse(string expression)
        {
            expression_ = Regex.Replace(expression, comment_line_regex, string.Empty, RegexOptions.Multiline);

            PreCheck();

            tokens_ = Regex.Matches(expression_,
                                    token_regex,
                                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
            token_index_ = 0;

            GetToken();

            if (token_ == null)
            {
                // フィルター式から 1 つもトークンを取り出せない
                ThrowParseException(FilterParseException.ParseErrorType.NoExpressions);

                // 到達しない return
                return null;
            }

            IVideoFilter filter = ParseOrFilter();

            if (token_ != null)
            {
                // トークンを全て取り出す前に解析が終了してしまった
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                // 到達しない return
                return null;
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PreCheck()
        {
            string no_token_format = Regex.Replace(expression_,
                                                   token_regex,
                                                   string.Empty,
                                                   RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match match = Regex.Match(no_token_format, "[^\\s]+");
            if (match.Success)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="e"></param>
        private void ThrowParseException(FilterParseException.ParseErrorType error_type, Exception inner_exception)
        {
            string error_token = token_ != null ? token_.Value : "null";
            
            throw new FilterParseException(error_type, error_token, inner_exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorType"></param>
        private void ThrowParseException(FilterParseException.ParseErrorType errorType)
        {
            ThrowParseException(errorType, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseOrFilter()
        {
            // or 連結の一番初めのフィルター
            IVideoFilter or_filter_head = null;
            // 返却されるフィルター
            IVideoFilter filter = ParseAndFilter();

            // or が検出された場合は、返却されるフィルターを OrFilter にする
            // 最後まで or が検出されない場合は、filter をそのまま返却する
            while (token_type_ == TokenType.OpOr)
            {
                if (or_filter_head == null)
                {
                    // or が検出されたので、filter を OrFilter にする
                    or_filter_head = filter;
                    filter = new OrFilter();
                    // or の一番左のフィルターを設定する
                    ((OrFilter)filter).SubFilters.Add(or_filter_head);
                }

                // or の右側のフィルターを連結する
                GetToken();
                IVideoFilter or_filter_trail = ParseAndFilter();
                ((OrFilter)filter).SubFilters.Add(or_filter_trail);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseAndFilter()
        {
            // and 連結の一番初めのフィルター
            IVideoFilter and_filter_head = null;
            // 返却されるフィルター
            IVideoFilter filter = ParseNotFilter();

            // and が検出された場合は、返却されるフィルターを AndFilter にする
            // 最後まで and が検出されない場合は、filter をそのまま返却する
            while (token_type_ == TokenType.OpAnd)
            {
                if (and_filter_head == null)
                {
                    // and が検出されたので、filter を AndFilter にする
                    and_filter_head = filter;
                    filter = new AndFilter();
                    // and の一番左のフィルターを設定する
                    ((AndFilter)filter).SubFilters.Add(and_filter_head);
                }

                // and の右側のフィルターを連結する
                GetToken();
                IVideoFilter and_filter_trail = ParseNotFilter();
                ((AndFilter)filter).SubFilters.Add(and_filter_trail);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseNotFilter()
        {
            // 返却されるフィルター
            IVideoFilter filter;

            if (token_type_ == TokenType.OpNot)
            {
                // not が検出された場合は NotFilter を返却する
                GetToken();
                IVideoFilter inner_filter = ParseParenFilter();
                filter = new NotFilter(inner_filter);
            }
            else
            {
                // not が検出されない場合は ParseParenFilter の結果をそのまま返却する
                filter = ParseParenFilter();
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseParenFilter()
        {
            IVideoFilter filter;

            if (token_type_ == TokenType.OpenParen)
            {
                // 「(」が検出された場合は再帰し、or の解析をする
                GetToken();
                filter = ParseOrFilter();

                // 「)」が検出されない場合はエラーとする
                if (token_type_ != TokenType.CloseParen)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.MissingClosingParen);
                }

                GetToken();
            }
            else
            {
                // 個々のフィルターを解析する
                filter = ParseAtomicFilter();
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseAtomicFilter()
        {
            IVideoFilter filter;

            if (token_ == null)
            {
                // パースするべきフィルター要素がない
                ThrowParseException(FilterParseException.ParseErrorType.IncompleteExpression);

                return null;
            }

            string pattern_name = token_.Groups["pattern_name"].Value.ToLower();

            Generator<IVideoFilter> atomic_filter_parser;
            if (!atomic_filter_parsers_.TryGetValue(pattern_name, out atomic_filter_parser))
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            filter = atomic_filter_parser();
            GetToken();

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoIdFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            string pattern_value = ParsePatternValue();

            
            if (is_option_present)
            {
                // オプションの指定有り

                if (is_regex_option_present)
                {
                    try
                    {
                        // Regex オプションが指定されているので、他のオプションは無視
                        filter = new VideoIdFilter(pattern_value, true);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                        return null;
                    }
                }
                else
                {
                    // Regex オプションが指定されていないので、他のオプションも設定
                    filter = new VideoIdFilter(pattern_value,
                                               is_regex_option_present,
                                               is_ignore_case_option_preset,
                                               is_match_all_option_present);
                }
            }
            else
            {
                // オプションが全て省略されているのでデフォルトオプションを用いる
                filter = new VideoIdFilter(pattern_value);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoTitleFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            string pattern_value = ParsePatternValue();

            
            if (is_option_present)
            {
                // オプションの指定有り

                if (is_regex_option_present)
                {
                    try
                    {
                        // Regex オプションが指定されているので、他のオプションは無視
                        filter = new VideoTitleFilter(pattern_value, true);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                        return null;
                    }
                }
                else
                {
                    // Regex オプションが指定されていないので、他のオプションも設定
                    filter = new VideoTitleFilter(pattern_value,
                                               is_regex_option_present,
                                               is_ignore_case_option_preset,
                                               is_match_all_option_present);
                }
            }
            else
            {
                // オプションが全て省略されているのでデフォルトオプションを用いる
                filter = new VideoTitleFilter(pattern_value);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoDescriptionFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            string pattern_value = ParsePatternValue();

            
            if (is_option_present)
            {
                // オプションの指定有り

                if (is_regex_option_present)
                {
                    try
                    {
                        // Regex オプションが指定されているので、他のオプションは無視
                        filter = new VideoDescriptionFilter(pattern_value, true);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                        return null;
                    }
                }
                else
                {
                    // Regex オプションが指定されていないので、他のオプションも設定
                    filter = new VideoDescriptionFilter(pattern_value,
                                               is_regex_option_present,
                                               is_ignore_case_option_preset,
                                               is_match_all_option_present);
                }
            }
            else
            {
                // オプションが全て省略されているのでデフォルトオプションを用いる
                filter = new VideoDescriptionFilter(pattern_value);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoSubmitDateFilter()
        {
            IVideoFilter filter;

            string pattern_value = ParsePatternValue();

            string[] splited_value = pattern_value.Split(',');
            if (splited_value.Length != 2)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            string min_date_str = splited_value[0].Trim();
            string max_date_str = splited_value[1].Trim();

            DateTime min_date;
            DateTime max_date;

            if (min_date_str.Length != 0)
            {
                if(!DateTime.TryParse(min_date_str, out min_date))
                {
                    ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                    return null;
                }

                if (max_date_str.Length != 0)
                {
                    if(!DateTime.TryParse(max_date_str, out max_date))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                        return null;
                    }
                }
                else
                {
                    max_date = DateTime.MaxValue;
                }
            }
            else
            {
                min_date = DateTime.MinValue;

                if (max_date_str.Length != 0)
                {
                    if(!DateTime.TryParse(max_date_str, out max_date))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                        return null;
                    }
                }
                else
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }
            }

            if (max_date < min_date)
            {
                DateTime temp_date = min_date;
                min_date = max_date;
                max_date = temp_date;
            }

            filter = new VideoSubmitDateFilter(min_date, max_date);

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoViewFilter()
        {
            IVideoFilter filter;

            string pattern_value = ParsePatternValue();

            string[] splited_value = pattern_value.Split(',');
            if (splited_value.Length != 2)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            int min_value;
            int max_value;

            string min_value_str = splited_value[0].Trim();
            string max_value_str = splited_value[1].Trim();

            if (min_value_str.Length != 0)
            {
                if (!int.TryParse(min_value_str, out min_value))
                {
                    ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                    return null;
                }
                
                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    max_value = int.MaxValue;
                }
            }
            else
            {
                min_value = int.MinValue;

                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }
            }

            if (max_value < min_value)
            {
                int temp_value = min_value;
                min_value = max_value;
                max_value = temp_value;
            }

            filter =  new VideoViewFilter(min_value, max_value);

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoCommentFilter()
        {
            IVideoFilter filter;

            string pattern_value = ParsePatternValue();

            string[] splited_value = pattern_value.Split(',');
            if (splited_value.Length != 2)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            int min_value;
            int max_value;

            string min_value_str = splited_value[0].Trim();
            string max_value_str = splited_value[1].Trim();

            if (min_value_str.Length != 0)
            {
                if (!int.TryParse(min_value_str, out min_value))
                {
                    ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                    return null;
                }
                
                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    max_value = int.MaxValue;
                }
            }
            else
            {
                min_value = int.MinValue;

                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }
            }

            if (max_value < min_value)
            {
                int temp_value = min_value;
                min_value = max_value;
                max_value = temp_value;
            }

            filter =  new VideoCommentFilter(min_value, max_value);

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoMylistFilter()
        {
            IVideoFilter filter;

            string pattern_value = ParsePatternValue();

            string[] splited_value = pattern_value.Split(',');
            if (splited_value.Length != 2)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            int min_value;
            int max_value;

            string min_value_str = splited_value[0].Trim();
            string max_value_str = splited_value[1].Trim();

            if (min_value_str.Length != 0)
            {
                if (!int.TryParse(min_value_str, out min_value))
                {
                    ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                    return null;
                }
                
                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    max_value = int.MaxValue;
                }
            }
            else
            {
                min_value = int.MinValue;

                if (max_value_str.Length != 0)
                {
                    if (!int.TryParse(max_value_str, out max_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }
                }
                else
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }
            }

            if (max_value < min_value)
            {
                int temp_value = min_value;
                min_value = max_value;
                max_value = temp_value;
            }

            filter =  new VideoMylistFilter(min_value, max_value);

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoPNameFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            string pattern_value = ParsePatternValue();

            
            if (is_option_present)
            {
                // オプションの指定有り

                if (is_regex_option_present)
                {
                    try
                    {
                        // Regex オプションが指定されているので、他のオプションは無視
                        filter = new VideoPNameFilter(pattern_value, true);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                        return null;
                    }
                }
                else
                {
                    // Regex オプションが指定されていないので、他のオプションも設定
                    filter = new VideoPNameFilter(pattern_value,
                                               is_regex_option_present,
                                               is_ignore_case_option_preset,
                                               is_match_all_option_present);
                }
            }
            else
            {
                // オプションが全て省略されているのでデフォルトオプションを用いる
                filter = new VideoPNameFilter(pattern_value);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoLengthFilter()
        {
            IVideoFilter filter;

            string pattern_value = ParsePatternValue();

            string[] splited_value = pattern_value.Split(',');
            if (splited_value.Length != 2)
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            TimeSpan min_length;
            TimeSpan max_length;

            string min_length_str = splited_value[0].Trim();
            string max_length_str = splited_value[1].Trim();

            if (min_length_str.Length != 0)
            {
                min_length = CreateTimeSpan(min_length_str);

                if (max_length_str.Length != 0)
                {
                    max_length = CreateTimeSpan(max_length_str);
                }
                else
                {
                    max_length = TimeSpan.MaxValue;
                }
            }
            else
            {
                min_length = TimeSpan.MinValue;

                if (max_length_str.Length != 0)
                {
                    max_length = CreateTimeSpan(max_length_str);
                }
                else
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }
            }

            if (max_length < min_length)
            {
                TimeSpan temp_length = min_length;
                min_length = max_length;
                max_length = temp_length;
            }

            filter = new VideoLengthFilter(min_length, max_length);

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoTagFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            string pattern_value = ParsePatternValue();

            switch (token_.Groups["pattern_name"].Value.ToLower())
            { 
                case "tag":
                    if (is_option_present)
                    {
                        // オプションの指定有り

                        if (is_regex_option_present)
                        {
                            try
                            {
                                // Regex オプションが指定されているので、他のオプションは無視
                                filter = new VideoTagFilter(pattern_value, true);
                            }
                            catch (ArgumentException e)
                            {
                                ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                return null;
                            }
                        }
                        else
                        {
                            // Regex オプションが指定されていないので、他のオプションも設定
                            filter = new VideoTagFilter(pattern_value,
                                is_regex_option_present,
                                is_ignore_case_option_preset,
                                is_match_all_option_present);
                        }
                    }
                    else
                    {
                        // オプションが全て省略されているのでデフォルトオプションを用いる
                        filter = new VideoTagFilter(pattern_value);
                    }
                    break;
                case "ltag":
                    if (is_option_present)
                    {
                        // オプションの指定有り

                        if (is_regex_option_present)
                        {
                            try
                            {
                                // Regex オプションが指定されているので、他のオプションは無視
                                filter = new LockedVideoTagFilter(pattern_value, true);
                            }
                            catch (ArgumentException e)
                            {
                                ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                return null;
                            }
                        }
                        else
                        {
                            // Regex オプションが指定されていないので、他のオプションも設定
                            filter = new LockedVideoTagFilter(pattern_value,
                                is_regex_option_present,
                                is_ignore_case_option_preset,
                                is_match_all_option_present);
                        }
                    }
                    else
                    {
                        // オプションが全て省略されているのでデフォルトオプションを用いる
                        filter = new LockedVideoTagFilter(pattern_value);
                    }
                    break;
                case "utag":
                    if (is_option_present)
                    {
                        // オプションの指定有り

                        if (is_regex_option_present)
                        {
                            try
                            {
                                // Regex オプションが指定されているので、他のオプションは無視
                                filter = new UnlockedVideoTagFilter(pattern_value, true);
                            }
                            catch (ArgumentException e)
                            {
                                ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                return null;
                            }
                        }
                        else
                        {
                            // Regex オプションが指定されていないので、他のオプションも設定
                            filter = new UnlockedVideoTagFilter(pattern_value,
                                is_regex_option_present,
                                is_ignore_case_option_preset,
                                is_match_all_option_present);
                        }
                    }
                    else
                    {
                        // オプションが全て省略されているのでデフォルトオプションを用いる
                        filter = new UnlockedVideoTagFilter(pattern_value);
                    }
                    break;
                default:
                    filter = null;
                    break;
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoIdListFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                if (is_option_present)
                {
                    // オプションの指定有り

                    if (is_regex_option_present)
                    {
                        try
                        {
                            // Regex オプションが指定されているので、他のオプションは無視
                            sub_filter = new VideoIdFilter(pattern_value, true);
                        }
                        catch (ArgumentException e)
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                            return null;
                        }
                    }
                    else
                    {
                        // Regex オプションが指定されていないので、他のオプションも設定
                        sub_filter = new VideoIdFilter(pattern_value,
                            is_regex_option_present,
                            is_ignore_case_option_preset,
                            is_match_all_option_present);
                    }
                }
                else
                {
                    // オプションが全て省略されているのでデフォルトオプションを用いる
                    sub_filter = new VideoIdFilter(pattern_value);
                }

                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoTitleListFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                if (is_option_present)
                {
                    // オプションの指定有り

                    if (is_regex_option_present)
                    {
                        try
                        {
                            // Regex オプションが指定されているので、他のオプションは無視
                            sub_filter = new VideoTitleFilter(pattern_value, true);
                        }
                        catch (ArgumentException e)
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                            return null;
                        }
                    }
                    else
                    {
                        // Regex オプションが指定されていないので、他のオプションも設定
                        sub_filter = new VideoTitleFilter(pattern_value,
                            is_regex_option_present,
                            is_ignore_case_option_preset,
                            is_match_all_option_present);
                    }
                }
                else
                {
                    // オプションが全て省略されているのでデフォルトオプションを用いる
                    sub_filter = new VideoTitleFilter(pattern_value);
                }

                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoDescriptionListFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                if (is_option_present)
                {
                    // オプションの指定有り

                    if (is_regex_option_present)
                    {
                        try
                        {
                            // Regex オプションが指定されているので、他のオプションは無視
                            sub_filter = new VideoDescriptionFilter(pattern_value, true);
                        }
                        catch (ArgumentException e)
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                            return null;
                        }
                    }
                    else
                    {
                        // Regex オプションが指定されていないので、他のオプションも設定
                        sub_filter = new VideoDescriptionFilter(pattern_value,
                            is_regex_option_present,
                            is_ignore_case_option_preset,
                            is_match_all_option_present);
                    }
                }
                else
                {
                    // オプションが全て省略されているのでデフォルトオプションを用いる
                    sub_filter = new VideoDescriptionFilter(pattern_value);
                }

                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoSubmitDateListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                string[] splited_value = pattern_value.Split(',');
                if (splited_value.Length != 2)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }

                string min_date_str = splited_value[0].Trim();
                string max_date_str = splited_value[1].Trim();

                DateTime min_date;
                DateTime max_date;

                if (min_date_str.Length != 0)
                {
                    if (!DateTime.TryParse(min_date_str, out min_date))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                        return null;
                    }

                    if (max_date_str.Length != 0)
                    {
                        if (!DateTime.TryParse(max_date_str, out max_date))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                            return null;
                        }
                    }
                    else
                    {
                        max_date = DateTime.MaxValue;
                    }
                }
                else
                {
                    min_date = DateTime.MinValue;

                    if (max_date_str.Length != 0)
                    {
                        if (!DateTime.TryParse(max_date_str, out max_date))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.DateTimeFormat);

                            return null;
                        }
                    }
                    else
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                        return null;
                    }
                }

                if (max_date < min_date)
                {
                    DateTime temp_date = min_date;
                    min_date = max_date;
                    max_date = temp_date;
                }

                sub_filter = new VideoSubmitDateFilter(min_date, max_date);
                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoViewListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                string[] splited_value = pattern_value.Split(',');
                if (splited_value.Length != 2)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }

                int min_value;
                int max_value;

                string min_value_str = splited_value[0].Trim();
                string max_value_str = splited_value[1].Trim();

                if (min_value_str.Length != 0)
                {
                    if (!int.TryParse(min_value_str, out min_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        max_value = int.MaxValue;
                    }
                }
                else
                {
                    min_value = int.MinValue;

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                        return null;
                    }
                }

                if (max_value < min_value)
                {
                    int temp_value = min_value;
                    min_value = max_value;
                    max_value = temp_value;
                }

                sub_filter = new VideoViewFilter(min_value, max_value);
                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoCommentListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                string[] splited_value = pattern_value.Split(',');
                if (splited_value.Length != 2)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }

                int min_value;
                int max_value;

                string min_value_str = splited_value[0].Trim();
                string max_value_str = splited_value[1].Trim();

                if (min_value_str.Length != 0)
                {
                    if (!int.TryParse(min_value_str, out min_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        max_value = int.MaxValue;
                    }
                }
                else
                {
                    min_value = int.MinValue;

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                        return null;
                    }
                }

                if (max_value < min_value)
                {
                    int temp_value = min_value;
                    min_value = max_value;
                    max_value = temp_value;
                }

                sub_filter = new VideoCommentFilter(min_value, max_value);
                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoMylistListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                string[] splited_value = pattern_value.Split(',');
                if (splited_value.Length != 2)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }

                int min_value;
                int max_value;

                string min_value_str = splited_value[0].Trim();
                string max_value_str = splited_value[1].Trim();

                if (min_value_str.Length != 0)
                {
                    if (!int.TryParse(min_value_str, out min_value))
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                        return null;
                    }

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        max_value = int.MaxValue;
                    }
                }
                else
                {
                    min_value = int.MinValue;

                    if (max_value_str.Length != 0)
                    {
                        if (!int.TryParse(max_value_str, out max_value))
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.NumberFormat);

                            return null;
                        }
                    }
                    else
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                        return null;
                    }
                }

                if (max_value < min_value)
                {
                    int temp_value = min_value;
                    min_value = max_value;
                    max_value = temp_value;
                }

                sub_filter = new VideoMylistFilter(min_value, max_value);
                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoPNameListFilter()
        {
            IVideoFilter filter;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                if (is_option_present)
                {
                    // オプションの指定有り

                    if (is_regex_option_present)
                    {
                        try
                        {
                            // Regex オプションが指定されているので、他のオプションは無視
                            sub_filter = new VideoPNameFilter(pattern_value, true);
                        }
                        catch (ArgumentException e)
                        {
                            ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                            return null;
                        }
                    }
                    else
                    {
                        // Regex オプションが指定されていないので、他のオプションも設定
                        sub_filter = new VideoPNameFilter(pattern_value,
                            is_regex_option_present,
                            is_ignore_case_option_preset,
                            is_match_all_option_present);
                    }
                }
                else
                {
                    // オプションが全て省略されているのでデフォルトオプションを用いる
                    sub_filter = new VideoPNameFilter(pattern_value);
                }

                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IVideoFilter ParseVideoLengthListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;
                string[] splited_value = pattern_value.Split(',');
                if (splited_value.Length != 2)
                {
                    ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                    return null;
                }

                TimeSpan min_length;
                TimeSpan max_length;

                string min_length_str = splited_value[0].Trim();
                string max_length_str = splited_value[1].Trim();

                if (min_length_str.Length != 0)
                {
                    min_length = CreateTimeSpan(min_length_str);

                    if (max_length_str.Length != 0)
                    {
                        max_length = CreateTimeSpan(max_length_str);
                    }
                    else
                    {
                        max_length = TimeSpan.MaxValue;
                    }
                }
                else
                {
                    min_length = TimeSpan.MinValue;

                    if (max_length_str.Length != 0)
                    {
                        max_length = CreateTimeSpan(max_length_str);
                    }
                    else
                    {
                        ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                        return null;
                    }
                }

                if (max_length < min_length)
                {
                    TimeSpan temp_length = min_length;
                    min_length = max_length;
                    max_length = temp_length;
                }

                sub_filter = new VideoLengthFilter(min_length, max_length);

                sub_filters.Add(sub_filter);
            }

            return filter;
        }

        private IVideoFilter ParseVideoTagListFilter()
        {
            IVideoFilter filter;
            IVideoFilter sub_filter;
            IList<IVideoFilter> sub_filters;

            bool is_regex_option_present;
            bool is_ignore_case_option_preset;
            bool is_match_all_option_present;

            bool is_option_present = ParsePatternOptions(out is_regex_option_present,
                                                         out is_ignore_case_option_preset,
                                                         out is_match_all_option_present);

            if (token_.Groups["list_pattern_paren"].Value == "(")
            {
                filter = new AndFilter();
                sub_filters = ((AndFilter)filter).SubFilters;
            }
            else if (token_.Groups["list_pattern_paren"].Value == "{")
            {
                filter = new OrFilter();
                sub_filters = ((OrFilter)filter).SubFilters;
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.Syntax);

                return null;
            }

            foreach (Capture capture in token_.Groups["pattern_value"].Captures)
            {
                string pattern_value = capture.Value;

                switch (token_.Groups["pattern_name"].Value.ToLower())
                {
                    case "taglist":
                        if (is_option_present)
                        {
                            // オプションの指定有り

                            if (is_regex_option_present)
                            {
                                try
                                {
                                    // Regex オプションが指定されているので、他のオプションは無視
                                    sub_filter = new VideoTagFilter(pattern_value, true);
                                }
                                catch (ArgumentException e)
                                {
                                    ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                    return null;
                                }
                            }
                            else
                            {
                                // Regex オプションが指定されていないので、他のオプションも設定
                                sub_filter = new VideoTagFilter(pattern_value,
                                    is_regex_option_present,
                                    is_ignore_case_option_preset,
                                    is_match_all_option_present);
                            }
                        }
                        else
                        {
                            // オプションが全て省略されているのでデフォルトオプションを用いる
                            sub_filter = new VideoTagFilter(pattern_value);
                        }
                        sub_filters.Add(sub_filter);
                        break;
                    case "ltaglist":
                        if (is_option_present)
                        {
                            // オプションの指定有り

                            if (is_regex_option_present)
                            {
                                try
                                {
                                    // Regex オプションが指定されているので、他のオプションは無視
                                    sub_filter = new LockedVideoTagFilter(pattern_value, true);
                                }
                                catch (ArgumentException e)
                                {
                                    ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                    return null;
                                }
                            }
                            else
                            {
                                // Regex オプションが指定されていないので、他のオプションも設定
                                sub_filter = new LockedVideoTagFilter(pattern_value,
                                    is_regex_option_present,
                                    is_ignore_case_option_preset,
                                    is_match_all_option_present);
                            }
                        }
                        else
                        {
                            // オプションが全て省略されているのでデフォルトオプションを用いる
                            sub_filter = new LockedVideoTagFilter(pattern_value);
                        }
                        sub_filters.Add(sub_filter);
                        break;
                    case "utaglist":
                        if (is_option_present)
                        {
                            // オプションの指定有り

                            if (is_regex_option_present)
                            {
                                try
                                {
                                    // Regex オプションが指定されているので、他のオプションは無視
                                    sub_filter = new UnlockedVideoTagFilter(pattern_value, true);
                                }
                                catch (ArgumentException e)
                                {
                                    ThrowParseException(FilterParseException.ParseErrorType.RegexFormat, e);
                                    return null;
                                }
                            }
                            else
                            {
                                // Regex オプションが指定されていないので、他のオプションも設定
                                sub_filter = new UnlockedVideoTagFilter(pattern_value,
                                    is_regex_option_present,
                                    is_ignore_case_option_preset,
                                    is_match_all_option_present);
                            }
                        }
                        else
                        {
                            // オプションが全て省略されているのでデフォルトオプションを用いる
                            sub_filter = new UnlockedVideoTagFilter(pattern_value);
                        }
                        sub_filters.Add(sub_filter);
                        break;
                    default:
                        filter = null;
                        break;
                }
            }

            return filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="is_regex_option_present"></param>
        /// <param name="is_ignore_case_option_preset"></param>
        /// <param name="is_match_all_option_present"></param>
        /// <returns></returns>
        private bool ParsePatternOptions(out bool is_regex_option_present, out bool is_ignore_case_option_preset, out bool is_match_all_option_present)
        {
            bool is_option_present = token_.Groups["pattern_option_indicator"].Success;
            if (is_option_present)
            {
                string pattern_option = token_.Groups["pattern_option"].Value;
                is_regex_option_present = IJStringUtil.IndexOfIgnoreCase(pattern_option, "r") >= 0;
                is_ignore_case_option_preset = IJStringUtil.IndexOfIgnoreCase(pattern_option, "i") >= 0;
                is_match_all_option_present = IJStringUtil.IndexOfIgnoreCase(pattern_option, "a") >= 0;
            }
            else
            {
                is_regex_option_present = false;
                is_ignore_case_option_preset = false;
                is_match_all_option_present = false;
            }

            return is_option_present;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string ParsePatternValue()
        {
            string pattern_value;
            string pattern_value_org = token_.Groups["pattern_value"].Value;

            if (pattern_value_org[0] == '"')
            {
                pattern_value = pattern_value_org.Substring(1, pattern_value_org.Length - 2)
                    .Replace("\\\"", "\"");
            }
            else
            {
                pattern_value = pattern_value_org;
            }

            return pattern_value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time_str"></param>
        /// <returns></returns>
        private TimeSpan CreateTimeSpan(string time_str)
        {
            TimeSpan time;

            string minute_str;
            string second_str;

            string[] splited_time_str = time_str.Split(':');

            if (splited_time_str.Length == 2)
            {
                minute_str = splited_time_str[0].Trim();
                second_str = splited_time_str[1].Trim();
            }
            else
            {
                ThrowParseException(FilterParseException.ParseErrorType.TimeSpanFormat);

                return default(TimeSpan);
            }

            int minute;
            int second;

            if (!int.TryParse(minute_str, out minute))
            {
                ThrowParseException(FilterParseException.ParseErrorType.TimeSpanFormat);

                return default(TimeSpan);
            }
            if (!int.TryParse(second_str, out second))
            {
                ThrowParseException(FilterParseException.ParseErrorType.TimeSpanFormat);

                return default(TimeSpan);
            }

            try {
                time = new TimeSpan(0, minute, second);
            }
            catch (ArgumentOutOfRangeException e)
            {
                ThrowParseException(FilterParseException.ParseErrorType.TimeSpanFormat, e);

                return default(TimeSpan);
            }

            return time;
        }
    }
    

    /// <summary>
    /// 
    /// </summary>
    public class FilterParseException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public enum ParseErrorType
        {
            Unknown,
            NoExpressions,
            IncompleteExpression,
            Syntax,
            MissingClosingParen,
            DateTimeFormat,
            TimeSpanFormat,
            NumberFormat,
            RegexFormat
        }

        private static IDictionary<FilterParseException.ParseErrorType, string> error_messages_;
        static FilterParseException()
        {
            error_messages_ = new Dictionary<FilterParseException.ParseErrorType, string>();
            error_messages_.Add(ParseErrorType.Unknown, "フィルターのパースエラーが発生しました。");
            error_messages_.Add(ParseErrorType.NoExpressions, "フィルターの式が空です。");
            error_messages_.Add(ParseErrorType.IncompleteExpression, "フィルター式が不完全です。");
            error_messages_.Add(ParseErrorType.Syntax, "フィルター式の文法に間違いがあります。");
            error_messages_.Add(ParseErrorType.MissingClosingParen, "開き括弧「(」に対応する閉じ括弧「)」がありません。");
            error_messages_.Add(ParseErrorType.DateTimeFormat, "日時に変換できない文字列です。");
            error_messages_.Add(ParseErrorType.TimeSpanFormat, "時間間隔に変換できない文字列です。");
            error_messages_.Add(ParseErrorType.NumberFormat, "整数に変換できない文字列です。");
            error_messages_.Add(ParseErrorType.RegexFormat, "正規表現の書式に間違いがあります。");
        }

        private ParseErrorType error_type_;

        /// <summary>
        /// 
        /// </summary>
        public FilterParseException() : this(ParseErrorType.Unknown, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public FilterParseException(ParseErrorType error_type, string error_token) : this(error_type, error_token, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner_exception"></param>
        public FilterParseException(ParseErrorType error_type, string error_token, Exception inner_exception)
            : base(GetErrorMessage(error_type, error_token), inner_exception)
        {
            error_type_ = error_type;
        }

        private static string GetErrorMessage(ParseErrorType error_type, string error_token)
        {
            string message;
            if (!error_messages_.TryGetValue(error_type, out message))
            {
                message = error_messages_[ParseErrorType.Unknown];
            }

            if (error_token == null)
            {
                error_token = "null";
            }

            return string.Format("{0} ({1})", message, error_token);
        }

        public ParseErrorType ErrorType {
            get { return error_type_; }
        }
    }
}
