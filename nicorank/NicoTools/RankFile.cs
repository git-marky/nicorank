// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IJLib;

namespace NicoTools
{
    public class RankFile
    {
        private List<Video> video_list_;
        private RankFileCustomFormat custom_format_;

        public RankFile(RankFileCustomFormat custom_format)
        {
            video_list_ = new List<Video>();
            custom_format_ = custom_format;
        }

        public RankFile(List<Video> video_list, RankFileCustomFormat custom_format)
        {
            video_list_ = video_list;
            custom_format_ = custom_format;
        }

        public RankFile(string rank_file_path, RankFileCustomFormat custom_format)
        {
            video_list_ = new List<Video>();
            custom_format_ = custom_format;
            Load(rank_file_path);
        }

        public string this[int index]
        {
            get
            {
                return video_list_[index].video_id;
            }
        }

        public int Count
        {
            get
            {
                return video_list_.Count;
            }
        }

        public Video GetVideo(int index)
        {
            return video_list_[index];
        }

        public List<Video> GetVideoList()
        {
            return video_list_;
        }

        public void Merge(RankFile rank_file)
        {
            for (int i = 0; i < rank_file.video_list_.Count; ++i)
            {
                if (RankFile.SearchVideo(video_list_, rank_file.video_list_[i].video_id) < 0)
                {
                    video_list_.Add(rank_file.video_list_[i]);
                }
            }
        }

        public void Save(string rank_filename, RankingMethod ranking_method)
        {
            Save(rank_filename, ranking_method.hosei_kind,
                ranking_method.mylist_rate, ranking_method.GetFilter());
        }

        public void Save(string rank_filename, HoseiKind hosei_kind, int mylist_rate, IFilterManager filter)
        {
            StringBuilder buff_notfiltered = new StringBuilder();
            StringBuilder buff_filtered = new StringBuilder();
            int rank_notfiltered = 1;
            int rank_filtered = 1;
            for (int i = 0; i < video_list_.Count; ++i)
            {
                filter.DoEffect(video_list_[i]);
                if (filter.IsThrough(video_list_[i]))
                {
                    if (custom_format_.IsUsingCustomFormat())
                    {
                        buff_notfiltered.Append(custom_format_.VideoToString(video_list_[i], rank_notfiltered));
                    }
                    else
                    {
                        buff_notfiltered.Append(video_list_[i].ToStringWithRank(rank_notfiltered, hosei_kind, mylist_rate));
                    }
                    ++rank_notfiltered;
                    buff_notfiltered.Append("\r\n");
                }
                else
                {
                    if (custom_format_.IsUsingCustomFormat())
                    {
                        buff_filtered.Append(custom_format_.VideoToString(video_list_[i], rank_filtered));
                    }
                    else
                    {
                        buff_filtered.Append(video_list_[i].ToStringWithRank(rank_filtered, hosei_kind, mylist_rate));
                    }
                    ++rank_filtered;
                    buff_filtered.Append("\r\n");
                }
            }
            IJFile.Write(rank_filename, buff_notfiltered.ToString());

            if (filter.IsOutputFilteredVideo())
            {
                string rank_str = buff_filtered.ToString();
                if (rank_str != "")
                {
                    IJFile.Write(Path.GetDirectoryName(rank_filename) + "\\" + Path.GetFileNameWithoutExtension(rank_filename) +
                        "_filter" + Path.GetExtension(rank_filename), rank_str);
                }
            }
        }

        public string ToString(RankingMethod ranking_method)
        {
            return ToString(ranking_method.hosei_kind,
                ranking_method.mylist_rate, ranking_method.GetFilter());
        }

        public string ToString(HoseiKind hosei_kind, int mylist_rate, IFilterManager filter)
        {
            StringBuilder buff_notfiltered = new StringBuilder();
            int rank_notfiltered = 1;
            for (int i = 0; i < video_list_.Count; ++i)
            {
                filter.DoEffect(video_list_[i]);
                if (filter.IsThrough(video_list_[i]))
                {
                    if (custom_format_.IsUsingCustomFormat())
                    {
                        buff_notfiltered.Append(custom_format_.VideoToString(video_list_[i], rank_notfiltered));
                    }
                    else
                    {
                        buff_notfiltered.Append(video_list_[i].ToStringWithRank(rank_notfiltered, hosei_kind, mylist_rate));
                    }
                    ++rank_notfiltered;
                    buff_notfiltered.Append("\r\n");
                }
            }
            return buff_notfiltered.ToString();
        }

        public void Add(Video video)
        {
            video_list_.Add(video);
        }

        /// <summary>
        /// ランクファイルに video を追加または上書きする。
        /// </summary>
        /// <remarks>
        /// ランクファイルに、引数の video と同じ ID を持つ動画がない場合には、リストに video を追加し、
        /// ある場合は、その動画の情報を引数の video で上書きする。
        /// </remarks>
        /// <param name="video">追加または上書きする動画。</param>
        /// <returns>
        /// 引数の video と同じ ID の動画がランクファイルにすでに含まれ、上書きされた場合は true、
        /// その他の場合は false。
        /// </returns>
        public bool AddOrOverwrite(Video video)
        {
            int removed_count = VideoListUtil.RemoveAll(video_list_, video);
            video_list_.Add(video);
            return (removed_count != 0);
        }

        public void Sort(RankingMethod ranking_method)
        {
            if (ranking_method.sort_kind != SortKind.Nothing)
            {
                video_list_.Sort(ranking_method.GetComparer());
            }
        }

        public void Load(string rank_file_path)
        {
            string str = IJFile.Read(rank_file_path);
            Parse(str);
        }

        public void Reload(string rank_file_path)
        {
            Clear();
            Load(rank_file_path);
        }

        public void Clear()
        {
            video_list_ = new List<Video>();
        }

        public void Parse(string str)
        {
            string[] line = IJStringUtil.SplitWithCRLF(str);

            for (int i = 0; i < line.Length; ++i)
            {
                if (custom_format_.IsUsingCustomFormat())
                {
                    Video video = custom_format_.GetVideo(line[i]);
                    video_list_.Add(video);
                }
                else
                {
                    string[] s_array = line[i].Split('\t');
                    string[] info = new string[18];
                    for (int j = 0; j < 18; ++j)
                    {
                        info[j] = (j < s_array.Length) ? s_array[j] : "";
                    }
                    if (RankFile.SearchVideo(video_list_, info[0]) < 0) // 存在しないなら
                    {
                        Video video = new Video();
                        video.video_id = info[0];
                        video.point.view = IJStringUtil.ToIntFromCommaValueWithDef(info[2], 0);
                        video.point.res = IJStringUtil.ToIntFromCommaValueWithDef(info[3], 0);
                        video.point.mylist = IJStringUtil.ToIntFromCommaValueWithDef(info[4], 0);
                        video.title = info[8];
                        if (info[9] != "")
                        {
                            video.submit_date = NicoUtil.StringToDate(info[9]);
                        }
                        video.pname = info[11];
                        video.tag_set.Parse(info[12]);

                        video_list_.Add(video);
                    }
                }
            }
        }

        public void ParseFromIdList(string str)
        {
            List<string> id_list = NicoUtil.GetNicoIdList(IJStringUtil.SplitWithCRLF(str));

            foreach (string s in id_list)
            {
                Video video = new Video();
                video.video_id = s;
                video_list_.Add(video);
            }
        }

        public void LoadForSpecial(string rank_file_path)
        {
            string str = "";
            try
            {
                str = IJFile.Read(rank_file_path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }
            string[] line = IJStringUtil.SplitWithCRLF(str);
            if (video_list_ == null)
            {
                video_list_ = new List<Video>();
            }

            for (int i = 0; i < line.Length; ++i)
            {
                string[] s_array = line[i].Split('\t');
                if (RankFile.SearchVideo(video_list_, s_array[0]) < 0) // 存在しないなら
                {
                    Video video = new Video();
                    video.video_id = s_array[0];
                    try
                    {
                        video.point.view = IJStringUtil.ToIntFromCommaValue(s_array[3]);
                        video.point.res = IJStringUtil.ToIntFromCommaValue(s_array[4]);
                        video.point.mylist = IJStringUtil.ToIntFromCommaValue(s_array[5]);
                        video.title = s_array[6];
                        video.submit_date = NicoUtil.StringToDate(s_array[7]);
                        video.tag_set.ParseBlank(s_array[14]);
                    }
                    catch (System.IndexOutOfRangeException) { }
                    video_list_.Add(video);
                }
            }
        }

        public string CategorizePname(string plist, out string kekka)
        {
            string[] pname_array = IJStringUtil.SplitWithCRLF(plist);
            StringBuilder buff = new StringBuilder();
            StringBuilder buff_kekka = new StringBuilder();

            for (int i = 0; i < pname_array.Length; i += 3)
            {
                StringBuilder buff_pname = new StringBuilder();
                int view = 0;
                int res = 0;
                int mylist = 0;
                for (int j = 0; j < video_list_.Count; ++j)
                {

                    if (video_list_[j].tag_set.IsInclude(pname_array[i + 1]) || video_list_[j].tag_set.IsInclude(pname_array[i + 2]))
                    {
                        buff_pname.Append("\t");
                        buff_pname.Append(video_list_[j].ToStringForSpecial());
                        buff_pname.Append("\r\n");
                        view += video_list_[j].point.view;
                        res += video_list_[j].point.res;
                        mylist += video_list_[j].point.mylist;
                    }
                }
                string s = (i / 3 + 1).ToString() + "\t" + pname_array[i + 1] + "\t" + pname_array[i + 2] + "\t" + view.ToString() +
                     "\t" + res.ToString() + "\t" + mylist.ToString() + "\r\n";
                buff.Append(s);
                buff_kekka.Append(s);
                buff.Append(buff_pname);
            }
            kekka = buff_kekka.ToString();
            return buff.ToString();
        }

        public static int SearchVideo(List<Video> list, string video_id)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].video_id.Equals(video_id))
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public enum UpdateRankKind { Tag, ExceptPoint, All, AddingTag };
    public enum SortKind { Point, Mylist, Nothing };

    public class RankingMethod
    {
        public HoseiKind hosei_kind;
        public SortKind sort_kind;
        public int mylist_rate;
        public bool is_using_filter;
        public string filter_filename;
        public bool is_output_filtered_video;

        public RankingMethod(HoseiKind hosei_kind_a, SortKind sort_kind_a, int mylist_rate_a)
        {
            hosei_kind = hosei_kind_a;
            sort_kind = sort_kind_a;
            mylist_rate = mylist_rate_a;
            is_using_filter = false;
            filter_filename = "";
        }

        public RankingMethod(HoseiKind hosei_kind_a, SortKind sort_kind_a, int mylist_rate_a, bool is_using_filter_a,
            string filter_filename_a, bool is_output_filtered_video_a)
        {
            hosei_kind = hosei_kind_a;
            sort_kind = sort_kind_a;
            mylist_rate = mylist_rate_a;
            is_using_filter = is_using_filter_a;
            filter_filename = filter_filename_a;
            is_output_filtered_video = is_output_filtered_video_a;
        }

        public IComparer<Video> GetComparer()
        {
            if (sort_kind == SortKind.Mylist)
            {
                return new VideoMylistComparer();
            }
            else if (sort_kind == SortKind.Point)
            {
                return new VideoScoreComparer(hosei_kind, mylist_rate);
            }
            else
            {
                throw new System.Exception("並べ替え方式の設定が間違えています。");
            }
        }

        public IFilterManager GetFilter()
        {
            IFilterManager filter;
            if (is_using_filter && filter_filename != "" && File.Exists(filter_filename))
            {
                try
                {
                    string first_line = IJFile.ReadFirstLineUTF8(filter_filename);

                    // フィルター書式のバージョンを調べる(ファイルの最初の行が「version=2」であるかどうか)
                    bool is_version2 = IJStringUtil.CompareString(first_line, RootFilter.version_indicator);

                    string str;

                    if (is_version2)
                    {
                        // 新フィルターはUTF-8で読み込む
                        string str_with_version = IJFile.ReadUTF8(filter_filename);

                        // 先頭の「version=2」を削除する
                        int start_index = str_with_version.IndexOfAny(new char[] { '\r', '\n' }, 0);
                        if (start_index + 1 <= str_with_version.Length - 1)
                        {
                            str = str_with_version.Substring(start_index + 1);
                        }
                        else
                        {
                            str = string.Empty;
                        }

                        filter = GetFilter2(str);
                    }
                    else
                    {
                        str = IJFile.Read(filter_filename);
                        filter = GetFilter1(str);
                    }

                }
                catch (Exception)
                {
                    throw new FormatException("フィルターの書式が間違えています。");
                }
                return filter;
            }
            return new Filter(is_output_filtered_video);
        }

        private IFilterManager GetFilter1(string str)
        {
            Filter filter = new Filter(is_output_filtered_video);
            filter.Parse(str);
            return filter;
        }

        private IFilterManager GetFilter2(string str)
        {
            FilterParser parser = new FilterParser();
            IVideoFilter video_filter = parser.Parse(str);
            IFilterManager filter = new RootFilter(is_output_filtered_video, video_filter);

            return filter;
        }
    }

    public class SearchingTagOptionOldVersion
    {
        public enum AddingKind { Overwrite, Append, AlreadyWrite };

        public List<string> searching_tag_list;
        public DateTime date_from;
        public DateTime date_to;

        public bool is_detail_getting;
        public int detail_info_lower;

        public AddingKind adding_kind;

        public int sort_kind_num;
        public int search_tag_num_start;
        public int search_tag_num_end;
        public int search_tag_upper;

        public string searching_interval;
        public string getting_detail_interval;

        public void SetTagList(string str)
        {
            searching_tag_list = new List<string>(IJStringUtil.SplitWithCRLF(str));
        }

        public void SetDateFrom(string str)
        {
            date_from = NicoUtil.StringToDate(str);
        }

        public void SetDateTo(string str)
        {
            date_to = NicoUtil.StringToDate(str);
        }

        public void SetDetailInfoLower(string str)
        {
            detail_info_lower = IJStringUtil.ToNumberWithDef(str, 0);
        }

        public void SetSearchTagUpper(string str)
        {
            search_tag_upper = IJStringUtil.ToNumberWithDef(str, 0);
        }

        public bool IsConditionSatisfy(Video video)
        {
            /*if (is_using_detail)
            {
                if (getting_kind == GettingKind.Upper)
                {
                    switch (sort_kind_num)
                    {
                        case 0:
                            return date_from <= video.submit_date;
                        case 1:
                            return date_to >= video.submit_date;
                        case 2:
                            return video.point.view >= search_tag_upper;
                        case 3:
                            return video.point.view <= search_tag_upper;
                        case 6:
                            return video.point.res >= search_tag_upper;
                        case 7:
                            return video.point.res <= search_tag_upper;
                        case 8:
                            return video.point.mylist >= search_tag_upper;
                        case 9:
                            return video.point.mylist <= search_tag_upper;
                        default:
                            return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {*/
                return date_from <= video.submit_date;
            //}
        }

        public string GetUrlOption()
        {
            /*if (is_using_detail)
            {
                string[] sort = { "sort=f", "sort=v", "", "sort=r", "sort=m", "sort=l" };
                string url_option = sort[sort_kind_num / 2];
                if (sort_kind_num % 2 == 1)
                {
                    if (url_option != "")
                    {
                        url_option += "&";
                    }
                    url_option += "order=a";
                }
                return (url_option == "") ? "" : ("?" + url_option);
            }
            else
            {*/
                return "?sort=f";
            //}
        }
    }

    public class SearchingTagOption
    {
        public List<string> searching_tag_list;

        public bool is_searching_get_kind_api;

        public bool is_searching_kind_tag;

        public bool is_detail_getting;
        public int detail_info_lower;

        public int sort_kind_num;
        public bool is_page_all;

        public int page_start;
        public int page_end;

        public bool is_using_condition;

        public DateTime date_from;
        public DateTime date_to;

        public int condition_lower;
        public int condition_upper;

        public string searching_interval;
        public string getting_detail_interval;

        public string save_html_dir;

        public bool is_sending_user_session;

        // 検索チケットID
        public string ticket_id;
        // 検索チケット作成フラグ
        public bool is_create_ticket;

        // 冗長検索方法
        public RedundantSearchingMethod redundant_seatching_method = RedundantSearchingMethod.Once;

        public void SetTagList(string str)
        {
            searching_tag_list = new List<string>(IJStringUtil.SplitWithCRLF(str));

            ticket_id = null;
        }

        public bool IsEndSearch(Video video)
        {
            if (is_using_condition)
            {
                switch (sort_kind_num)
                {
                    case 0:
                        return video.submit_date < date_from;
                    case 1:
                        return video.submit_date > date_to;
                    case 2:
                        return video.point.view < condition_lower;
                    case 3:
                        return video.point.view > condition_upper;
                    case 6:
                        return video.point.res < condition_lower;
                    case 7:
                        return video.point.res > condition_upper;
                    case 8:
                        return video.point.mylist < condition_lower;
                    case 9:
                        return video.point.mylist > condition_upper;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsConditionSatisfy(Video video)
        {
            if (is_using_condition)
            {
                switch (sort_kind_num)
                {
                    case 0: case 1:
                        return date_from <= video.submit_date && video.submit_date <= date_to;
                    case 2: case 3:
                        return condition_lower <= video.point.view && video.point.view <= condition_upper;
                    case 6: case 7:
                        return condition_lower <= video.point.res && video.point.res <= condition_upper;
                    case 8: case 9:
                        return condition_lower <= video.point.mylist && video.point.mylist <= condition_upper;
                    default:
                        return true;
                }
            }
            else
            {
                return true;
            }
        }

        public NicoNetwork.SearchSortMethod GetSortMethod()
        {
            NicoNetwork.SearchSortMethod[] method = { NicoNetwork.SearchSortMethod.SubmitDate, NicoNetwork.SearchSortMethod.View,
                                                    NicoNetwork.SearchSortMethod.ResNew, NicoNetwork.SearchSortMethod.Res,
                                                    NicoNetwork.SearchSortMethod.Mylist, NicoNetwork.SearchSortMethod.Time};
            return method[sort_kind_num / 2];
        }

        public NicoNetwork.SearchOrder GetSearchOrder()
        {
            return (sort_kind_num % 2 == 0) ? NicoNetwork.SearchOrder.Desc : NicoNetwork.SearchOrder.Asc;
        }

        public void SetRedundantSearchMethod(int index)
        {
            switch (index)
            {
                case 0:
                    redundant_seatching_method = RedundantSearchingMethod.Once;
                    break;
                case 1:
                    redundant_seatching_method = RedundantSearchingMethod.TwiceTakeFirst;
                    break;
                case 2:
                    redundant_seatching_method = RedundantSearchingMethod.TwiceTakeLast;
                    break;
                case 3:
                    redundant_seatching_method = RedundantSearchingMethod.TwiceMergeResult;
                    break;
                case 4:
                    redundant_seatching_method = RedundantSearchingMethod.AtMostThreeTimes;
                    break;
                default:
                    redundant_seatching_method = RedundantSearchingMethod.Once;
                    break;
            }
        }

        // 古いので消す
        public string GetUrlOption()
        {
            string[] sort = { "sort=f", "sort=v", "", "sort=r", "sort=m", "sort=l" };
            string url_option = sort[sort_kind_num / 2];
            if (sort_kind_num % 2 == 1)
            {
                if (url_option != "")
                {
                    url_option += "&";
                }
                url_option += "order=a";
            }
            return (url_option == "") ? "" : ("?" + url_option);
        }
    }

    public enum RedundantSearchingMethod
    {
        Once,
        TwiceTakeFirst,
        TwiceTakeLast,
        TwiceMergeResult,
        AtMostThreeTimes
    }

    public class InputOutputOption
    {
        private string input_path_;
        private string output_path_;
        private bool is_input_from_file_;
        private bool is_output_from_file_;
        private bool is_input_from_stdin_ = false;

        private string input_text_;
        private RankFileCustomFormat custom_format_ = null;

        public delegate void OutputRankFileDelegate(string rank_file_str);

        private OutputRankFileDelegate OnOutputRankFile = null;

        public InputOutputOption(bool is_input_from_file, bool is_output_from_file)
            : this(is_input_from_file, is_output_from_file, new RankFileCustomFormat())
        {
            // nothing
        }

        public InputOutputOption(bool is_input_from_file, bool is_output_from_file, RankFileCustomFormat custom_format) 
        {
            is_input_from_file_ = is_input_from_file;
            is_output_from_file_ = is_output_from_file;
            custom_format_ = custom_format;
        }

        public InputOutputOption(string input_path, string output_path)
            : this(input_path, output_path, new RankFileCustomFormat())
        {
            // nothing
        }

        public InputOutputOption(string input_path, string output_path, RankFileCustomFormat custom_format)
        {
            is_input_from_file_ = true;
            is_output_from_file_ = true;
            input_path_ = input_path;
            output_path_ = output_path;
            custom_format_ = custom_format;
        }

        public void SetInputPath(string input_path)
        {
            input_path_ = input_path;
        }

        public void SetOutputPath(string output_path)
        {
            output_path_ = output_path;
        }

        public void SetInputText(string input_text)
        {
            input_text_ = input_text;
        }

        public void SetOutputRankFileDelegate(OutputRankFileDelegate dlg)
        {
            OnOutputRankFile = dlg;
        }

        public void SetInputFromStdin()
        {
            is_input_from_stdin_ = true;
        }

        public virtual RankFile GetRankFile()
        {
            if (is_input_from_file_)
            {
                if (!File.Exists(input_path_))
                {
                    throw new Exception("入力ランクファイルが存在しません。");
                }
                return new RankFile(input_path_, custom_format_);
            }
            else
            {
                if (is_input_from_stdin_)
                {
                    StreamReader reader = null;
                    try
                    {
                        reader = new StreamReader(System.Console.OpenStandardInput());
                        input_text_ = reader.ReadToEnd();
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                    is_input_from_stdin_ = false;
                }
                RankFile rank_file = new RankFile(custom_format_);
                string separator = custom_format_.GetInputSeparator();
                if (input_text_.IndexOf(separator) >= 0)
                {
                    rank_file.Parse(input_text_);
                }
                else
                {
                    rank_file.ParseFromIdList(input_text_);
                }
                return rank_file;
            }
        }

        public virtual string GetRawText()
        {
            if (is_input_from_file_)
            {
                if (!File.Exists(input_path_))
                {
                    throw new FileNotFoundException("入力ランクファイルが存在しません。");
                }
                return IJFile.Read(input_path_);
            }
            else
            {
                return input_text_;
            }
        }

        public void OutputRankFile(RankFile rank_file, RankingMethod ranking_method)
        {
            if (is_output_from_file_)
            {
                rank_file.Save(output_path_, ranking_method);
            }
            else
            {
                if (OnOutputRankFile != null)
                {
                    OnOutputRankFile(rank_file.ToString(ranking_method));
                }
            }
        }

        public RankFileCustomFormat GetRankFileCustomFormat()
        {
            return custom_format_;
        }
    }

    public class InputRankFile : InputOutputOption
    {
        RankFile rank_file_;

        public InputRankFile(RankFile rank_file) : base(false, false)
        {
            rank_file_ = rank_file;
        }

        public void SetRankFile(RankFile rank_file)
        {
            rank_file_ = rank_file;
        }

        public override RankFile GetRankFile()
        {
            return rank_file_;
        }
    }

    public class PathManager
    {
        private Dictionary<string, string> dictionary_ = new Dictionary<string, string>();
        private string base_dir_;
        private DateTime today_;

        public void SetPath(string path_kind, string path_name)
        {
            dictionary_[path_kind] = path_name;
        }

        public void SetBaseDir(string base_dir)
        {
            base_dir_ = base_dir;
            if (base_dir_.Length >= 1 && base_dir_[base_dir_.Length - 1] != '\\')
            {
                base_dir_ += '\\';
            }
        }

        public string GetPath(string path_kind)
        {
            string path = dictionary_[path_kind].Replace("%%date8%%", today_.ToString("yyyyMMdd"));
            if (path.Length >= 3 && ('A' <= path[0] && path[0] <= 'Z' || 'a' <= path[0] && path[0] <= 'z') && path[1] == ':' && path[2] == '\\')
            {// 絶対パスならそのまま返す
                return path;
            }
            else
            {
                return base_dir_ + path;
            }
        }

        public string GetDir(string path_kind)
        {
            string dir = GetPath(path_kind);
            if (dir[dir.Length - 1] != '\\')
            {
                dir += '\\';
            }
            return dir;
        }

        public string GetBaseDir()
        {
            return base_dir_;
        }

        public void SetDate(DateTime date)
        {
            today_ = date;
        }

        public string GetFullPath(string filename)
        {
            if (filename.Length >= 3 && ('A' <= filename[0] && filename[0] <= 'Z' || 'a' <= filename[0] && filename[0] <= 'z') && filename[1] == ':' && filename[2] == '\\')
            {// 絶対パスならそのまま返す
                return filename;
            }
            else
            {
                return base_dir_ + filename;
            }
        }
    }

    public class NicoPathManager : PathManager
    {
        public string GetInputRankFilePath()
        {
            return GetPath("textBoxInputRankFilePath");
        }

        public string GetOutputRankFilePath()
        {
            return GetPath("textBoxOutputRankFilePath");
        }

        public string GetRankDlDir()
        {
            return GetPath("textBoxRankDlDir");
        }

        public string GetSavedRankDir()
        {
            return GetDir("textBoxSavedRankDir");
        }

        public string GetFlvDlDir()
        {
            return GetDir("textBoxFlvDlDir");
        }

        public string GetThumbnailDir()
        {
            return GetDir("textBoxThumbnailDir");
        }

        public string GetTransAfterDir()
        {
            return GetDir("textBoxTransAfterFileOrDir");
        }

        public string GetLayoutPath()
        {
            return GetPath("textBoxLayoutPath");
        }

        public string GetRankPicDir()
        {
            return GetDir("textBoxRankPicDir");
        }

        public string GetGeneratedAviDir()
        {
            return GetDir("textBoxGeneratedAviDir");
        }

        public string GetUploadPath()
        {
            return GetPath("textBoxUploadPath");
        }

        public string GetTemplateDir()
        {
            return GetDir("textBoxTemplateDir");
        }

        public string GetFFMpegPath()
        {
            return GetPath("textBoxFFmpegPath");
        }

        public string GetWFltPath()
        {
            return GetPath("textBoxWavfltPath");
        }

        public string GetVDMPath()
        {
            return GetPath("textBoxVDMPath");
        }

        public string GetMencPath()
        {
            return GetPath("textBoxMencPath");
        }

        public string GetUWSCPath()
        {
            return GetPath("textBoxUWSCPath");
        }

        public string GetSwfPath()
        {
            return GetPath("textBoxSwfPath");
        }

        public string GetHugFlashPath()
        {
            return GetPath("textBoxHugFlashPath");
        }

        public string GetSimpleTransBeforePath()
        {
            return GetPath("textBoxSimpleTransBeforePath");
        }

        public string GetSimpleTransAfterPath()
        {
            return GetPath("textBoxSimpleTransAfterPath");
        }

        public string GetCutListPath()
        {
            return GetPath("textBoxCutListPath");
        }

        public string GetDiff1Path()
        {
            return GetPath("textBoxDiff1Path");
        }

        public string GetDiff2Path()
        {
            return GetPath("textBoxDiff2Path");
        }

        public string GetDTransBeforePath()
        {
            return GetPath("textBoxDTransBeforePath");
        }

        public string GetDTransAfterPath()
        {
            return GetPath("textBoxDTransAfterPath");
        }

        public string GetSavedRankNicoChartDir()
        {
            return GetDir("textBoxSavedRankNicoChartDir");
        }

        public string GetFilterPath()
        {
            return GetPath("textBoxFilterPath");
        }

        public string GetScriptInputPath()
        {
            return GetPath("textBoxScriptInputPath");
        }

        public string GetAviSynthScriptPath()
        {
            return GetPath("textBoxAviSynthScriptPath");
        }

        public string GetAviFromScriptPath()
        {
            return GetPath("textBoxAviFromScriptPath");
        }

        public string GetRenameDir()
        {
            return GetDir("textBoxRenameDir");
        }
    }
}
