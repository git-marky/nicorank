// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using IJLib;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace NicoTools
{
    public class NicoNetworkManager
    {
        private NicoNetwork niconico_network_;
        private NicoNetwork niconico_no_user_session_network_ = new NicoNetwork();
        private MessageOut msgout_;
        private CancelObject cancel_object_;
        public delegate void StringDelegate(string str);

        // 冗長検索のロジックを表すデリゲート
        private delegate List<Video> MakeListBySearchDelegate(SearchingTagOption option, ref int log_number);

        private StringDelegate SetDownloadInfo = null;

        public NicoNetworkManager(NicoNetwork network, MessageOut msgout, CancelObject cancel_object)
        {
            niconico_network_ = network;
            msgout_ = msgout;
            cancel_object_ = cancel_object;

            niconico_no_user_session_network_.ReloadCookie(NicoNetwork.CookieKind.None);
        }

        public void SetDelegateSetDonwloadInfo(StringDelegate dlg)
        {
            SetDownloadInfo = dlg;
        }

        public void CheckLogin()
        {
            if (niconico_network_.IsLoginNiconico())
            {
                msgout_.Write("ログインされています。\r\n");
            }
            else
            {
                msgout_.Write("ログインされていません。\r\n");
            }
        }

        public string DownloadRanking(DownloadKind download_kind, string rank_dl_dir)
        {
            msgout_.Write("ランキングのDLを開始します。\r\n");
            DateTime dt = DateTime.Now;
            if (rank_dl_dir[rank_dl_dir.Length - 1] != '\\')
            {
                rank_dl_dir += '\\';
            }

            rank_dl_dir += dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00")
              + dt.Hour.ToString("00");

            rank_dl_dir = IJFile.GetNoExistDirName(rank_dl_dir);
            Directory.CreateDirectory(rank_dl_dir);
            //niconico_network_.DownloadRanking(rank_dl_dir, download_kind, /*hour, */OnDownloadRankingEvent);
            // 2019/06/26 Update marky
            if (download_kind.IsRss)
            {
                niconico_network_.DownloadRanking(rank_dl_dir, download_kind, OnDownloadRankingEvent);
            }
            else
            {
                niconico_network_.DownloadRankingLog(rank_dl_dir, download_kind, OnDownloadRankingEvent);
            }
            msgout_.Write("すべてのランキングのDLが完了しました。\r\n");
            return rank_dl_dir;
        }

        // ランキングDL中に呼び出される
        public void OnDownloadRankingEvent(string message, int current, int total)
        {
            msgout_.Write("ランキングをDLしました。" + current + "/" + total + "\r\n");
            if (current < total)
            {
                cancel_object_.CheckCancel();
                cancel_object_.Wait(3000, 5000);
            }
        }

        public void MakeListAndWriteBySearchTag(InputOutputOption iooption, SearchingTagOption option,
                                                RankingMethod ranking_method)
        {
            RankFile rank_file = MakeListBySearchTag(option, ranking_method.GetFilter(), iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("リストを作成しました。\r\n");
        }

        public RankFile MakeListBySearchTag(SearchingTagOption option, IFilterManager filter, RankFileCustomFormat custom_format)
        {
            RankFile rank_file = new RankFile(custom_format);
            int log_number = 0;

            if (!string.IsNullOrEmpty(option.save_html_dir))
            {
                if (!System.IO.Directory.Exists(option.save_html_dir))
                {
                    System.IO.Directory.CreateDirectory(option.save_html_dir);
                }
            }

            msgout_.Write("タグ・キーワード検索を開始します。\r\n");

            // 冗長検索方法を判定
            MakeListBySearchDelegate search_method;

            if (option.is_searching_get_kind_api) // use API
            {
                search_method = MakeListBySearchAPI;
            }
            else // use HTML
            {
                switch (option.redundant_seatching_method)
                {
                    case RedundantSearchingMethod.Once:
                        search_method = MakeListBySearchOnce;
                        break;
                    case RedundantSearchingMethod.TwiceTakeFirst:
                        search_method = MakeListBySearchTwiceTakeFirst;
                        break;
                    case RedundantSearchingMethod.TwiceTakeLast:
                        search_method = MakeListBySearchTwiceTakeLast;
                        break;
                    case RedundantSearchingMethod.TwiceMergeResult:
                        search_method = MakeListBySearchTwiceMergeResult;
                        break;
                    case RedundantSearchingMethod.AtMostThreeTimes:
                        search_method = MakeListBySearchThreeTimes;
                        break;
                    default:
                        search_method = MakeListBySearchOnce;
                        break;
                }
            }

            List<Video> video_list = search_method(option, ref log_number);

            if (option.is_detail_getting)
            {
                GetDetail(video_list, option.detail_info_lower, filter, option.getting_detail_interval);
            }
            if (option.is_searching_get_kind_api) // Title が "false" になることがある不具合の回避
            {
                // Title が "false" になっているものが1つでもあれば、GetDetailForFixingTitle を実行
                bool is_need_fix = false;
                for (int i = 0; i < video_list.Count; ++i)
                {
                    if (video_list[i].title == "false")
                    {
                        is_need_fix = true;
                        break;
                    }
                }
                if (is_need_fix)
                {
                    GetDetailForFixingTitle(video_list, option.getting_detail_interval);
                }
            }
            for (int i = 0; i < video_list.Count; ++i)
            {
                rank_file.Add(video_list[i]);
            }
            return rank_file;
        }

        private List<Video> SearchTag(string tag_word, SearchingTagOption option, ref int log_number, int redundant_search_count, ref bool wait_required)
        {
            List<Video> ret_list = new List<Video>();
            msgout_.Write(tag_word + " の検索を開始します。\r\n");
            double tag_search_interval_lower = 10.0, tag_search_interval_upper = 12.0;
            IJStringUtil.ParseDlInterval(option.searching_interval, ref tag_search_interval_lower, ref tag_search_interval_upper);

            int start_page = (option.is_page_all ? 1 : option.page_start);
            int end_page = (option.is_page_all ? int.MaxValue : option.page_end);
            option.offset = 0;      // 2019/07/06 ADD marky
            option.last_value = ""; // 2019/07/06 ADD marky

            wait_required = false;

            for (int page = start_page; page <= end_page; ++page)
            {
                List<Video> current_list = GetPage(tag_word, page, option, ref log_number, option.is_searching_kind_tag, redundant_search_count, out wait_required);
                cancel_object_.CheckCancel();
                if (current_list.Count == 0)
                {
                    break;
                }
                for (int i = 0; i < current_list.Count; ++i)
                {
                    if (option.IsEndSearch(current_list[i]))
                    {
                        return ret_list;
                    }
                    if (option.IsConditionSatisfy(current_list[i]))
                    {
                        ret_list.Add(current_list[i]);
                    }

                    // 2019/07/06 ADD marky
                    option.offset += 1;
                    string value = "";
                    switch (option.GetSortMethod())
                    {
                        case NicoNetwork.SearchSortMethod.SubmitDate:
                            value = current_list[i].submit_date.ToString("yyyy-MM-dd'T'HH:mm:ss'%2B09:00'");
                            break;
                        case NicoNetwork.SearchSortMethod.View:
                            value = current_list[i].point.view.ToString();
                            break;
                        case NicoNetwork.SearchSortMethod.ResNew:
                            value = current_list[i].last_comment_time.Replace("+","%2B");
                            break;
                        case NicoNetwork.SearchSortMethod.Res:
                            value = current_list[i].point.res.ToString();
                            break;
                        case NicoNetwork.SearchSortMethod.Mylist:
                            value = current_list[i].point.mylist.ToString();
                            break;
                        case NicoNetwork.SearchSortMethod.Time:
                            value = current_list[i].length.ToString();
                            break;
                    }
                    if (option.last_value != value)
                    {
                        option.offset = 1;
                        option.last_value = value;
                    }
                }

                if (wait_required == true &&
                    page < end_page)
                {
                    cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
                }
            }
            return ret_list;
        }

        private List<Video> GetPage(string tag_word, int page, SearchingTagOption option, ref int log_number, bool is_searching_kind_tag, int redundant_search_count, out bool wait_required)
        {
            int error_times = 0;
            string str = "";
            msgout_.Write(page.ToString() + "ページ目を取得中…\r\n");

            // 検索チケット使用の場合は、ダウンロードしたページを保存するパスを取
            string download_file_path;
            if (option.ticket_id != null)
            {
                download_file_path = SearchingTicketManager.GetPageDownloadPath(option.ticket_id, redundant_search_count, page);
            }
            else
            {
                download_file_path = null;
            }

            // ページが既にダウンロード済みの場合は、ダウンロードしない
            if (File.Exists(download_file_path))
            {
                wait_required = false;
                str = IJFile.ReadUTF8(download_file_path);
            }
            else
            {
                // ニコニコ動画では、タグ（キーワード）検索の際、なぜかログインしているとアクセス制限がきつく、していないと弱い。
                // そのため、タグ検索のときだけログインしないというオプションを用意している。
                NicoNetwork network = (option.is_sending_user_session ? niconico_network_ : niconico_no_user_session_network_);
                wait_required = true;
                while (true)
                {
                    try
                    {
                        // 2019/07/06 ADD marky API検索を個別メソッド化
                        if (option.is_searching_get_kind_api)
                        {
                            str = network.GetSearchByAPI(tag_word, option);
                        }
                        else
                        {
                            if (is_searching_kind_tag)
                            {
                                str = network.GetSearchTag(tag_word, page, option.GetSortMethod(), option.GetSearchOrder(), option.is_searching_get_kind_api);
                            }
                            else
                            {
                                str = network.GetSearchKeyword(tag_word, page, option.GetSortMethod(), option.GetSearchOrder(), option.is_searching_get_kind_api);
                            }
                            if (str.IndexOf("ここから先をご利用いただくにはログインしてください") >= 0)
                            {
                                throw new NiconicoLoginException();
                            }
                        }
                    }
                    catch (NiconicoAccessDeniedException)
                    {
                        ++error_times;
                        if (error_times >= 5)
                        {
                            msgout_.Write("ニコニコ動画にアクセスを拒否されました。初めからやり直してください。\r\n");
                            throw;
                        }
                        else
                        {
                            msgout_.Write("ニコニコ動画にアクセスを拒否されました。1分待機した後に再試行します。\r\n");
                            cancel_object_.Wait(60000);
                            continue;
                        }
                    }
                    catch (System.Net.WebException e)
                    {
                        if (e.Status == System.Net.WebExceptionStatus.ProtocolError)
                        {
                            if (((System.Net.HttpWebResponse)e.Response).StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                            {
                                ++error_times;
                                if (error_times >= 5)
                                {
                                    msgout_.Write("ニコニコ動画へのアクセスエラー(503)が起きました。初めからやり直してください。\r\n");
                                    throw;
                                }
                                else
                                {
                                    msgout_.Write("ニコニコ動画へのアクセスエラー(503)が起きました。20秒後に再試行します。\r\n");
                                    cancel_object_.Wait(20000);
                                    continue;
                                }
                            }
                        }
                        throw;
                    }
                    break;
                }

                // 検索チケット使用の場合は、ダウンロードしたページをファイルに保存
                if (download_file_path != null)
                {
                    IJFile.WriteUTF8(download_file_path, str);
                }
            }
            msgout_.Write(page.ToString() + "ページ目を取得しました。\r\n");
            if (!string.IsNullOrEmpty(option.save_html_dir))
            {
                IJFile.WriteUTF8(Path.Combine(option.save_html_dir,  "search" + log_number.ToString() + ".html"), str);
            }
            cancel_object_.CheckCancel();
            ++log_number;
            if (option.is_searching_get_kind_api)
            {
                return ParseSearchFromAPI(str);
            }
            else
            {
                return ParseSearch(str);
            }
        }

        private void GetDetail(List<Video> video_list, int detail_info_lower, IFilterManager filter, string interval)
        {
            msgout_.Write("詳細情報を取得中…\r\n");
            double interval_lower = 0.3, interval_upper = 0.5;
            IJStringUtil.ParseDlInterval(interval, ref interval_lower, ref interval_upper);

            for (int i = 0; i < video_list.Count; ++i)
            {
                if (video_list[i].point.mylist >= detail_info_lower && filter.IsThrough(video_list[i]))
                {
                    Video video = NicoUtil.GetVideo(niconico_network_, video_list[i].video_id, cancel_object_, msgout_);
                    if (video.IsStatusOK())
                    {
                        int dummy;
                        video_list[i] = video;
                        video_list[i].pname = TagSet.GetPname(video_list[i].tag_set, out dummy);
                    }
                    else
                    {
                        msgout_.Write(video.GetErrorMessage(video_list[i].video_id) + "\r\n");
                    }
                    cancel_object_.Wait((int)(interval_lower * 1000), (int)(interval_upper * 1000));
                }
                cancel_object_.CheckCancel();
            }
            msgout_.Write("詳細情報を取得しました。\r\n");
        }

        // API 検索時に Title の項目が "false" になっているものを補正
        private void GetDetailForFixingTitle(List<Video> video_list, string interval)
        {
            msgout_.Write("タイトル補正のための詳細情報を取得中…\r\n");
            double interval_lower = 0.3, interval_upper = 0.5;
            IJStringUtil.ParseDlInterval(interval, ref interval_lower, ref interval_upper);

            for (int i = 0; i < video_list.Count; ++i)
            {
                if (video_list[i].title == "false") // Title の項目が "false" になっているものは補正する
                {
                    Video video = NicoUtil.GetVideo(niconico_network_, video_list[i].video_id, cancel_object_, msgout_);
                    if (video.IsStatusOK())
                    {
                        video_list[i].title = video.title;
                    }
                    else
                    {
                        msgout_.Write(video.GetErrorMessage(video_list[i].video_id) + "\r\n");
                    }
                    cancel_object_.Wait((int)(interval_lower * 1000), (int)(interval_upper * 1000));
                }
                cancel_object_.CheckCancel();
            }
            msgout_.Write("タイトル補正のための詳細情報を取得しました。\r\n");
        }

        public void MylistSearch(string mylist_str, InputOutputOption iooption, RankingMethod ranking_method)
        {
            List<Video> video_list = GetMylist(mylist_str);

            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("マイリスト取得が終了しました。\r\n");
        }

        public void UpdateRankFileByMylist(string mylist_str, InputOutputOption iooption, RankingMethod ranking_method)
        {
            List<Video> video_list = GetMylist(mylist_str);

            RankFile rank_file = iooption.GetRankFile();
            RankFile new_rank_file = new RankFile(iooption.GetRankFileCustomFormat());

            for (int i = 0; i < rank_file.Count; ++i)
            {
                Video video = rank_file.GetVideo(i);
                int index = RankFile.SearchVideo(video_list, video.video_id);
                if (index >= 0)
                {
                    video.title = video_list[index].title;
                    video.tag_set = video_list[index].tag_set;
                    video.point = video_list[index].point;
                    video.description = video_list[index].description;
                    new_rank_file.Add(video);
                }
            }
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("マイリスト更新が終了しました。\r\n");
        }

        public List<Video> GetMylist(string mylist_str)
        {
            msgout_.Write("マイリスト取得中…\r\n");
            List<string> mylist_number_list = new List<string>();

            string[] line = IJStringUtil.SplitWithCRLF(mylist_str);

            for (int i = 0; i < line.Length; ++i)
            {
                string str = line[i];
                if ('0' <= str[0] && str[0] <= '9')
                {
                    int end = 1;
                    while (end < str.Length && '0' <= str[end] && str[end] <= '9')
                    {
                        ++end;
                    }
                    mylist_number_list.Add(str.Substring(0, end));
                }
                int cp = 0;
                while (cp < str.Length && (cp = str.IndexOf("mylist/", cp)) >= 0)
                {
                    cp += "mylist/".Length;
                    int end = cp;
                    while (end < str.Length && '0' <= str[end] && str[end] <= '9')
                    {
                        ++end;
                    }
                    mylist_number_list.Add(str.Substring(cp, end - cp));
                    cp = end + 1;
                }
            }
            List<Video> video_list = new List<Video>();
            for (int i = 0; i < mylist_number_list.Count; ++i)
            {
                List<Video> temp_list = new List<Video>();
                NicoListManager.ParsePointRss(niconico_network_.GetMylistHtml(mylist_number_list[i], true), DateTime.Now, temp_list, false, true);
                msgout_.Write("マイリスト" + mylist_number_list[i] + "を取得しました。\r\n");
                video_list.AddRange(temp_list);
                if (i < mylist_number_list.Count - 1)
                {
                    cancel_object_.Wait(100);
                }
            }

            video_list = VideoListUtil.Distinct(video_list);
            return video_list;
        }

        public void GetNewArrival(int start, int end, InputOutputOption iooption, RankingMethod ranking_method)
        {
            msgout_.Write("新着投稿取得を開始します…\r\n");
            List<Video> video_list = new List<Video>();

            if (end < start)
            {
                int temp = end; end = start; start = temp;
            }

            for (int i = start; i <= end; ++i)
            {
                video_list.AddRange(ParseSearch(niconico_network_.GetNewArrival(i)));
                msgout_.Write("新着" + i + "ページ目を取得しました。\r\n");
                if (i < end)
                {
                    cancel_object_.Wait(i * 1000);
                }
            }

            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("新着投稿取得が終了しました。\r\n");
        }

        public void UpdateDetailInfo(InputOutputOption iooption, UpdateRankKind update_rank_kind,
            RankingMethod ranking_method, string interval)
        {
            RankFile rank_file = iooption.GetRankFile();
            RankFile new_rank_file = new RankFile(iooption.GetRankFileCustomFormat());
            msgout_.Write("情報の更新を開始します。\r\n");
            double interval_lower = 0.3, interval_upper = 0.5;
            IJStringUtil.ParseDlInterval(interval, ref interval_lower, ref interval_upper);

            for (int i = 0; i < rank_file.Count; ++i)
            {
                int dummy;
                Video rank_file_video = rank_file.GetVideo(i);
                Video video = NicoUtil.GetVideo(niconico_network_, rank_file[i], cancel_object_, msgout_);
                if (video.IsStatusOK())
                {
                    if (update_rank_kind == UpdateRankKind.AddingTag)
                    {
                        rank_file_video.tag_set.Add(video.tag_set);
                        //rank_file_video.pname = TagSet.GetPname(video.tag_set, out dummy);
                        rank_file_video.thumbnail_url = video.thumbnail_url;     // 2019/07/06 ADD marky
                        rank_file_video.genre = video.genre;                     // 2019/07/06 ADD marky
                        new_rank_file.Add(rank_file_video);
                    }
                    else
                    {
                        if (update_rank_kind == UpdateRankKind.ExceptPoint || update_rank_kind == UpdateRankKind.All)
                        {
                            rank_file_video.video_id = video.video_id;
                            rank_file_video.title = video.title;
                        }
                        if (update_rank_kind == UpdateRankKind.All)
                        {
                            rank_file_video.point = video.point;
                        }
                        rank_file_video.submit_date = video.submit_date;
                        rank_file_video.tag_set = video.tag_set;
                        //rank_file_video.pname = TagSet.GetPname(video.tag_set, out dummy);
                        rank_file_video.thumbnail_url = video.thumbnail_url;     // 2019/07/06 ADD marky
                        rank_file_video.genre = video.genre;                     // 2019/07/06 ADD marky
                        new_rank_file.Add(rank_file_video);
                    }
                    msgout_.Write(rank_file_video.video_id + " の情報を更新しました。\r\n");
                }
                else
                {
                    msgout_.Write(video.GetErrorMessage(rank_file_video.video_id) + "\r\n");
                }
                cancel_object_.Wait((int)(interval_lower * 1000), (int)(interval_upper * 1000));
            }
            new_rank_file.Sort(ranking_method);
            iooption.OutputRankFile(new_rank_file, ranking_method);

            msgout_.Write("ファイルに書き込みました。\r\n情報の更新を終了します。\r\n");
        }

        public string UpdateUserId(List<string> video_id_list, List<string> plist, string interval)
        {
            msgout_.Write("ユーザIDの取得を開始します。\r\n");
            double interval_lower = 0.3, interval_upper = 0.5;
            IJStringUtil.ParseDlInterval(interval, ref interval_lower, ref interval_upper);

            System.Text.StringBuilder buff = new System.Text.StringBuilder();

            for (int i = 0; i < video_id_list.Count; ++i)
            {
                Video video = NicoUtil.GetVideo(niconico_network_, video_id_list[i], cancel_object_, msgout_);
                if (video.IsStatusOK())
                {
                    buff.Append(video_id_list[i]).Append("\t").Append(video.user_id).Append("\t").Append(plist[i]).Append("\r\n");
                    msgout_.Write(video_id_list[i] + " の情報を取得しました。\r\n");
                }
                else
                {
                    msgout_.Write(video.GetErrorMessage(video_id_list[i]) + "\r\n");
                }
                cancel_object_.Wait((int)(interval_lower * 1000), (int)(interval_upper * 1000));
            }
            msgout_.Write("情報の更新を終了します。\r\n");
            return buff.ToString();
        }

        public void GetDetailInfo(List<string> video_id_list, InputOutputOption iooption, bool is_reading_input,
            RankingMethod ranking_method, string interval)
        {
            msgout_.Write("情報の取得を開始します。\r\n");
            double interval_lower = 0.3, interval_upper = 0.5;
            IJStringUtil.ParseDlInterval(interval, ref interval_lower, ref interval_upper);

            RankFile rank_file;
            if (is_reading_input)
            {
                rank_file = iooption.GetRankFile();
            }
            else
            {
                rank_file = new RankFile(iooption.GetRankFileCustomFormat());
            }

            for (int i = 0; i < video_id_list.Count; ++i)
            {
                Video video = NicoUtil.GetVideo(niconico_network_, video_id_list[i], cancel_object_, msgout_);
                if (video.IsStatusOK())
                {
                    int dummy;
                    video.pname = TagSet.GetPname(video.tag_set, out dummy);
                    // すでに動画がリストに含まれている場合は取得した情報で上書きする。
                    rank_file.AddOrOverwrite(video);
                    msgout_.Write(video_id_list[i] + " の情報を取得しました。\r\n");
                }
                else
                {
                    msgout_.Write(video.GetErrorMessage(video_id_list[i]) + "\r\n");
                }
                cancel_object_.Wait((int)(interval_lower * 1000), (int)(interval_upper * 1000));
            }
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("ファイルに書き込みました。\r\n情報の取得を終了します。\r\n");
        }

        public void DownloadFlv(InputOutputOption iooption, string dl_interval, string flv_save_dir, bool is_fixing_extension)
        {
            double interval_min = 30.0, interval_max = 30.0;
            IJStringUtil.ParseDlInterval(dl_interval, ref interval_min, ref interval_max);
            DownloadFlv(iooption, interval_min, interval_max, flv_save_dir, is_fixing_extension);
        }

        public void DownloadFlv(InputOutputOption iooption, double interval_min, double interval_max, string flv_save_dir, bool is_fixing_extension)
        {
            bool start_flag = true;
            bool error_flag = false;

            RankFile rank_file = iooption.GetRankFile();
            if (!System.IO.Directory.Exists(flv_save_dir))
            {
                System.IO.Directory.CreateDirectory(flv_save_dir);
            }
            for (int i = 0; i < rank_file.Count; ++i)
            {
                if (!File.Exists(flv_save_dir + rank_file[i] + ".flv") && !File.Exists(flv_save_dir + rank_file[i] + ".mp4") &&
                    !File.Exists(flv_save_dir + rank_file[i] + ".swf"))
                {
                    if (start_flag)
                    {
                        start_flag = false;
                    }
                    else
                    {
                        cancel_object_.CheckCancel();
                        msgout_.Write("次の動画DLまで待機中。\r\n");
                        cancel_object_.Wait((int)(interval_min * 1000), (int)(interval_max * 1000));
                    }
                    int try_times = 5;
                    for (int j = 0; j < try_times; ++j)
                    {
                        msgout_.Write("動画 " + rank_file[i] + " をDLしています…\r\n");
                        try
                        {
                            niconico_network_.DownloadAndSaveFlv(rank_file[i], flv_save_dir + rank_file[i] + ".flv", InformDownloading);
                            if (!is_fixing_extension)
                            {
                                RenameFlv(flv_save_dir + rank_file[i] + ".flv");
                            }
                        }
                        catch (MyCancelException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            msgout_.Write("動画 " + rank_file[i] + " のDLに失敗しました。\r\n");
                            msgout_.Write(e.Message + "\r\n");
                            if (j < try_times - 1)
                            {
                                cancel_object_.CheckCancel();
                                msgout_.Write("再試行します。\r\n");
                                cancel_object_.Wait(5000);
                            }
                            else
                            {
                                error_flag = true;
                            }
                            continue;
                        }
                        msgout_.Write("動画 " + rank_file[i] + " のDLが完了しました。\r\n");
                        if (SetDownloadInfo != null)
                        {
                            SetDownloadInfo("完了");
                        }
                        break;
                    }
                }
                else
                {
                    msgout_.Write(rank_file[i] + " は存在するのでDLしません。\r\n");
                }
            }
            if (error_flag)
            {
                msgout_.Write("動画DLを完了しましたが、一部動画をDLできませんでした。\r\n");
            }
            else
            {
                msgout_.Write("すべての動画DLを完了しました。\r\n");
            }
        }

        public void InformDownloading(ref bool is_cancel, long current_size, long file_size)
        {
            if (is_cancel)
            {
                if (SetDownloadInfo != null)
                {
                    SetDownloadInfo("");
                }
                cancel_object_.CheckCancel();
            }
            else
            {
                is_cancel = cancel_object_.IsCanceling();
                if (SetDownloadInfo != null)
                {
                    SetDownloadInfo((current_size / 1024).ToString() + "KB / " + (file_size / 1024).ToString() + "KB");
                }
            }
        }

        public void RenameFlvInDirectory(string dir_name)
        {
            string[] files = Directory.GetFiles(dir_name);
            RenameFlvAll(new List<string>(files), true);
        }

        private void RenameFlvAll(List<string> filename_list, bool is_show_message)
        {
            if (is_show_message)
            {
                msgout_.Write("ファイル名を変更しています。\r\n");
            }
            for (int i = 0; i < filename_list.Count; ++i)
            {
                RenameFlv(filename_list[i]);
            }
            if (is_show_message)
            {
                msgout_.Write("ファイル名を変更しました。\r\n");
            }
        }

        private void RenameFlv(string filename)
        {
            string extension = "";

            switch (NicoUtil.JudgeFileType(filename))
            {
              case NicoUtil.FileType.Mp4:
                extension = ".mp4";
                break;
              case NicoUtil.FileType.Swf:
                extension = ".swf";
                break;
            }

            if (extension != "")
            {
                string new_filename = Path.GetDirectoryName(filename) + "\\" +
                  Path.GetFileNameWithoutExtension(filename) + extension;
                if (File.Exists(new_filename))
                {
                    msgout_.Write(Path.GetFileName(new_filename) + " は存在するので名前は変更されませんでした。\r\n");
                }
                else
                {
                    File.Move(filename, new_filename);
                }
            }
        }

        public void DownloadThumbnail(InputOutputOption iooption, string thumbnail_dir)
        {
            const int try_num = 5;
            RankFile rank_file = iooption.GetRankFile();
            List<string> video_list = new List<string>();

            for (int i = rank_file.Count - 1; i >= 0; --i)
            {
                video_list.Add(rank_file[i]);
            }

            if (!System.IO.Directory.Exists(thumbnail_dir))
            {
                System.IO.Directory.CreateDirectory(thumbnail_dir);
            }
            msgout_.Write("サムネイルのダウンロードを開始します。\r\n");
            for (int i = 0; i < try_num; ++i)
            {
                for (int j = video_list.Count - 1; j >= 0; --j)
                {
                    string video_id = video_list[j];
                    if (!System.IO.File.Exists(thumbnail_dir + video_id + ".jpg"))
                    {
                        try
                        {
                            niconico_network_.SaveThumbnailWithVideoId(video_id, thumbnail_dir + video_id + ".jpg");
                            msgout_.Write("サムネイル " + video_id + ".jpg をDLしました。\r\n");
                            video_list.RemoveAt(j);
                        }
                        catch (System.Exception)
                        {
                            msgout_.Write("サムネイル " + video_id + ".jpg のDLに失敗しました。");
                            if (i < try_num - 1)
                            {
                                msgout_.Write("あとでもう一度試行します。\r\n");
                            }
                        }
                        cancel_object_.Wait(1500, 2000);
                    }
                    else
                    {
                        msgout_.Write(video_id + ".jpg は存在します。\r\n");
                        video_list.RemoveAt(j);
                    }
                }
            }
            msgout_.Write("サムネイルのDLを完了しました。\r\n");
        }

        public string DownloadNicoChart(string ranking_dir, DateTime start_date, DateTime end_date)
        {
            msgout_.Write("ランキングのDLを開始します。\r\n");
            start_date = new DateTime(start_date.Year, start_date.Month, start_date.Day);
            end_date = new DateTime(end_date.Year, end_date.Month, end_date.Day);
            DownloadKindNicoChart download_kind = new DownloadKindNicoChart();
            List<string> name_list = new List<string>();
            List<string> filename_list = new List<string>();
            download_kind.GetRankingNameList(ref name_list, ref filename_list);

            if (ranking_dir[ranking_dir.Length - 1] != '\\')
            {
                ranking_dir += '\\';
            }
            ranking_dir += "nicochart\\";

            IJNetwork network = new IJNetwork();

            for (DateTime dt = start_date; dt <= end_date; dt = dt.AddDays(1.0))
            {
                msgout_.Write(dt.ToString("yyyyMMdd") + " のランキングをDLします。\r\n");
                string dir_name = ranking_dir + dt.ToString("yyyyMMdd") + "\\";
                System.IO.Directory.CreateDirectory(dir_name);
                for (int i = 0; i < name_list.Count; ++i)
                {
                    string html = network.GetAndReadFromWebUTF8("http://www.nicochart.jp/ranking/" +
                                                                dt.ToString("yyyyMMdd") + name_list[i]);
                    IJFile.Write(dir_name + filename_list[i] + ".txt", html);
                    msgout_.Write("ランキングをDLしました。" + (i + 1).ToString() +
                                  "/" + name_list.Count.ToString() + "\r\n");
                    cancel_object_.CheckCancel();
                    cancel_object_.Wait(2000);
                }
            }
            msgout_.Write("すべてのランキングのDLが完了しました。\r\n");
            return ranking_dir;
        }

        public void GetDataFromNicoApi()
        {
            msgout_.Write(niconico_network_.GetDataFromNicoApi());
        }

        public static List<string> ParseTag(string html)
        {
            List<string> tag_list = new List<string>();
            int index = html.IndexOf("<meta name=\"keywords");
            if (index >= 0)
            {
                index = html.IndexOf("content", index);
                if (index >= 0)
                {
                    index = html.IndexOf('"', index) + 1;
                    int last = html.IndexOf('"', index);
                    if (index >= 0 && last >= 0)
                    {
                        int end;
                        while (index >= 0 && index < last)
                        {
                            end = html.IndexOf(',', index);
                            if (end < 0 || end >= last)
                            {
                                tag_list.Add(html.Substring(index, last - index));
                                break;
                            }
                            tag_list.Add(html.Substring(index, end - index));
                            index = end + 1;
                        }
                    }
                }
            }
            return tag_list;
        }

        public static List<Video> ParseSearch(string html)
        {
            return ParseSearch(html, -1);
        }

        // 前から start_num - 1 件は捨てる
        // start_num が -1 なら全件登録
        // (Thanks to Asarima-san and marky-san)
        public static List<Video> ParseSearch(string html, int start_num)
        {
            int index = -1;
            List<Video> list = new List<Video>();
            int count = 0;

            //while ((index = html.IndexOf("videoList01Wrap\">", index + 1)) >= 0)
            // 2018/12/12 Update marky 広告枠を取得しないようキーワード変更
            while ((index = html.IndexOf("data-video-id", index + 1)) >= 0)
            {
                Video video = new Video();

                //投稿日時
                index = html.IndexOf("video_uploaded", index + 1); // この行は 2015/11/1 に挿入。Thanks to marky-san.
                string dateStr = IJStringUtil.GetStringBetweenTag(ref index, "span", html).Trim();
                if (!DateTime.TryParseExact(dateStr, "MM/dd HH:mm", null, System.Globalization.DateTimeStyles.None, out video.submit_date))
                {
                    //video.submit_date = DateTime.ParseExact(dateStr, "yy/MM/dd HH:mm", null);
                    // 2018/12/12 Update marky 投稿年の表記変更に対応
                    video.submit_date = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", null);
                }

                //動画ID
                int start = html.IndexOf("watch/", index) + 6;
                int end = html.IndexOf('"', start);
                int c = html.IndexOf('?', start);
                if (end > c)
                {
                    end = c;
                }
                video.video_id = html.Substring(start, end - start);
                index = end;    // 2018/12/12 ADD marky 再生時間が取得出来ていなかったのでインデックスを進める

                //再生時間
                video.length = IJStringUtil.GetStringBetweenTag(ref index, "span", html);

                index = html.IndexOf("itemContent", index + 1);
                //タイトル
                video.title = IJStringUtil.UnescapeHtml(IJStringUtil.GetStringBetweenTag(ref index, "a", html));

                //再生
                string viewStr = IJStringUtil.GetStringBetweenTag(ref index, "span", html);
                video.point.view = IJStringUtil.ToIntFromCommaValue(viewStr);
                //コメント
                string resStr = IJStringUtil.GetStringBetweenTag(ref index, "span", html);
                video.point.res = IJStringUtil.ToIntFromCommaValue(resStr);
                //マイリスト
                string mylistStr = IJStringUtil.GetStringBetweenTag(ref index, "a", html);
                video.point.mylist = IJStringUtil.ToIntFromCommaValue(mylistStr);

                // 宣伝ポイント。将来実装するときのため
                //string comStr = 
                IJStringUtil.GetStringBetweenTag(ref index, "a", html); // 読み捨て
                //video.com = IJStringUtil.ToIntFromCommaValue(comStr);

                ++count;
                if (count >= start_num)
                {
                    list.Add(video);
                }
            }
            return list;
        }

        public static List<Video> ParseSearchFromAPI(string json)
        {
            return ParseSearchFromAPI(json, -1);
        }

        /*
        // 前から start_num - 1 件は捨てる
        // start_num が -1 なら全件登録
        public static List<Video> ParseSearchFromAPI(string json, int start_num)
        {
            List<Video> list = new List<Video>();
            int count = 0;

            string[] ar = json.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < ar.Length; ++i)
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(NiconicoAPIResult));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ar[i])))
                {
                    NiconicoAPIResult result = (NiconicoAPIResult)serializer.ReadObject(ms);
                    if (result.values != null)
                    {
                        for (int j = 0; j < result.values.Count; ++j)
                        {
                            if (!string.IsNullOrEmpty(result.values[j].cmsid))
                            {
                                Video video = new Video();
                                video.video_id = result.values[j].cmsid;
                                video.point.view = int.Parse(result.values[j].view_counter);
                                video.point.res = int.Parse(result.values[j].comment_counter);
                                video.point.mylist = int.Parse(result.values[j].mylist_counter);
                                video.title = result.values[j].title;
                                video.submit_date = DateTime.ParseExact(result.values[j].start_time, "yyyy-MM-dd HH:mm:ss",
                                    null, System.Globalization.DateTimeStyles.None);
                                video.thumbnail_url = result.values[j].thumbnail_url;
                                video.length = result.values[j].length_seconds;
                                video.tag_set.ParseBlank(result.values[j].tags);
                                ++count;
                                if (count >= start_num)
                                {
                                    list.Add(video);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
        */

        //2017-03-03 UPDATE marky 検索API v2
        // 前から start_num - 1 件は捨てる
        // start_num が -1 なら全件登録
        public static List<Video> ParseSearchFromAPI(string json, int start_num)
        {
            List<Video> list = new List<Video>();
            int count = 0;

            //Start 2017-03-06 DEL まどやさんの指摘を反映
            //string[] ar = json.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //for (int i = 0; i < ar.Length; ++i)
            //{
            //End 2017-03-06 DEL まどやさんの指摘を反映
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(NiconicoAPIResult));
                //using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ar[i])))
                //2017-03-06 UPDATE まどやさんの指摘を反映
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    NiconicoAPIResult result = (NiconicoAPIResult)serializer.ReadObject(ms);
                    if (result.data != null)
                    {
                        for (int j = 0; j < result.data.Count; ++j)
                        {
                            if (!string.IsNullOrEmpty(result.data[j].contentId))
                            {
                                Video video = new Video();
                                video.video_id = result.data[j].contentId;
                                video.point.view = int.Parse(result.data[j].viewCounter);
                                video.point.res = int.Parse(result.data[j].commentCounter);
                                video.point.mylist = int.Parse(result.data[j].mylistCounter);
                                //video.title = result.data[j].title;
                                // 2019/08/18 Update marky title:null対策
                                video.title = result.data[j].title ?? "";
                                video.submit_date = DateTime.Parse(result.data[j].startTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                video.thumbnail_url = result.data[j].thumbnailUrl;
                                video.length = result.data[j].lengthSeconds;
                                video.tag_set.ParseBlank(result.data[j].tags);
                                // 2019/07/06 ADD marky
                                video.last_comment_time = result.data[j].lastCommentTime;
                                video.genre = result.data[j].genre;
                                ++count;
                                if (count >= start_num)
                                {
                                    list.Add(video);
                                }
                            }
                        }
                    }
                }
            //}
            return list;
        }

        // 冗長検索ロジック(1回(冗長検索無し))
        private List<Video> MakeListBySearchAPI(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }

            // 重複を排除
            return VideoListUtil.Distinct(video_list);
        }

        // 冗長検索ロジック(1回(冗長検索無し))
        private List<Video> MakeListBySearchOnce(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }

            // 重複を排除
            return VideoListUtil.Distinct(video_list);
        }

        // 冗長検索ロジック(2回、1回目を優先)
        private List<Video> MakeListBySearchTwiceTakeFirst(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list_a = new List<Video>();
            List<Video> video_list_b = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            double tag_search_interval_lower = 10.0, tag_search_interval_upper = 12.0;
            IJStringUtil.ParseDlInterval(option.searching_interval, ref tag_search_interval_lower, ref tag_search_interval_upper);


            msgout_.Write("複数回検索1回目を開始します。\r\n");
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_a.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索1回目を終了しました。\r\n");

            if (wait_required)
            {
                cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
            }

            msgout_.Write("複数回検索2回目を開始します。\r\n");
            redundant_search_count++;
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_b.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索2回目を終了しました。\r\n");


            List<Video> video_list;

            // リストA内の重複を調べる
            if (!VideoListUtil.ContainsDuplicateId(video_list_a))
            {
                video_list = video_list_a;
            }
            else if (!VideoListUtil.ContainsDuplicateId(video_list_b))
            {
                video_list = video_list_b;
            }
            else
            {
                video_list = video_list_a;
            }

            return VideoListUtil.Distinct(video_list);
        }

        // 冗長検索ロジック(2回、2回目を優先)
        private List<Video> MakeListBySearchTwiceTakeLast(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list_a = new List<Video>();
            List<Video> video_list_b = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            double tag_search_interval_lower = 10.0, tag_search_interval_upper = 12.0;
            IJStringUtil.ParseDlInterval(option.searching_interval, ref tag_search_interval_lower, ref tag_search_interval_upper);

            msgout_.Write("複数回検索1回目を開始します。\r\n");
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_a.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索1回目を終了しました。\r\n");

            if (wait_required)
            {
                cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
            }

            msgout_.Write("複数回検索2回目を開始します。\r\n");
            redundant_search_count++;
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_b.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索2回目を終了しました。\r\n");


            List<Video> video_list;

            // リストB内の重複を調べる
            if (!VideoListUtil.ContainsDuplicateId(video_list_b))
            {
                video_list = video_list_b;
            }
            else if (!VideoListUtil.ContainsDuplicateId(video_list_a))
            {
                video_list = video_list_a;
            }
            else
            {
                video_list = video_list_b;
            }

            return VideoListUtil.Distinct(video_list);
        }

        // 冗長検索ロジック(2回、結果をマージ)
        private List<Video> MakeListBySearchTwiceMergeResult(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list_a = new List<Video>();
            List<Video> video_list_b = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            double tag_search_interval_lower = 10.0, tag_search_interval_upper = 12.0;
            IJStringUtil.ParseDlInterval(option.searching_interval, ref tag_search_interval_lower, ref tag_search_interval_upper);

            msgout_.Write("複数回検索1回目を開始します。\r\n");
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_a.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索1回目を終了しました。\r\n");

            if (wait_required)
            {
                cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
            }

            msgout_.Write("複数回検索2回目を開始します。\r\n");
            redundant_search_count++;
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_b.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索2回目を終了しました。\r\n");

            return VideoListUtil.Merge(video_list_a, video_list_b);
        }

        // 冗長検索ロジック(最大3回)
        private List<Video> MakeListBySearchThreeTimes(SearchingTagOption option, ref int log_number)
        {
            List<Video> video_list_a = new List<Video>();
            List<Video> video_list_b = new List<Video>();
            int redundant_search_count = 1;
            bool wait_required = false;

            double tag_search_interval_lower = 10.0, tag_search_interval_upper = 12.0;
            IJStringUtil.ParseDlInterval(option.searching_interval, ref tag_search_interval_lower, ref tag_search_interval_upper);

            msgout_.Write("複数回検索1回目を開始します。\r\n");
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_a.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索1回目を終了しました。\r\n");

            if (wait_required)
            {
                cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
            }

            msgout_.Write("複数回検索2回目を開始します。\r\n");
            redundant_search_count++;
            for (int i = 0; i < option.searching_tag_list.Count; ++i)
            {
                video_list_b.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
            }
            msgout_.Write("複数回検索2回目を終了しました。\r\n");

            List<Video> video_list;

            video_list_a = VideoListUtil.Distinct(video_list_a);
            video_list_b = VideoListUtil.Distinct(video_list_b);

            if (VideoListUtil.Equals(video_list_a, video_list_b))
            {
                return video_list_a;
            }
            else
            {
                if (wait_required)
                {
                    cancel_object_.Wait((int)(tag_search_interval_lower * 1000), (int)(tag_search_interval_upper * 1000));
                }

                msgout_.Write("複数回検索3回目を開始します。\r\n");
                redundant_search_count++;
                video_list = new List<Video>();
                for (int i = 0; i < option.searching_tag_list.Count; ++i)
                {
                    video_list.AddRange(SearchTag(option.searching_tag_list[i], option, ref log_number, redundant_search_count, ref wait_required));
                }
                msgout_.Write("複数回検索3回目を終了しました。\r\n");
                
                return VideoListUtil.Distinct(video_list);
            }
        }

        private class DownloadKindNicoChart : DownloadKind
        {
            public override void GetRankingNameList(ref List<string> name_list, ref List<string> filename_list)
            {
                for (int i = 0; i < target_name.Length; ++i)
                {
                    for (int m = 1; m <= 3; ++m)
                    {
                        string option = "";
                        if (m >= 2)
                        {
                            option = "page=" + m.ToString();
                        }
                        if (target_name[i] == "view" || target_name[i] == "res")
                        {
                            if (option != "")
                            {
                                option += "&";
                            }
                            option += ((target_name[i] == "view") ? "type=vd" : "type=rd");
                        }
                        if (option != "")
                        {
                            option = "?" + option;
                        }
                        if (name_list != null)
                        {
                            name_list.Add(option);
                        }
                        if (filename_list != null)
                        {
                            filename_list.Add(target_short_name[i] + "_" + m.ToString());
                        }
                    }
                }
            }
        }
    }

    /*
    [DataContract]
    class NiconicoAPIResult
    {
        [DataMember]
        public string dqnid = "";

        [DataMember]
        public string type = "";

        [DataMember]
        public List<ValueC> values = null;

        [DataMember]
        public string endofstream = "";

        [DataContract]
        public class ValueC
        {
            [DataMember]
            public string _rowid = "";

            [DataMember]
            public string cmsid = "";

            [DataMember]
            public string comment_counter = "";

            [DataMember]
            public string length_seconds = "";

            [DataMember]
            public string mylist_counter = "";

            [DataMember]
            public string start_time = "";

            [DataMember]
            public string tags = "";

            [DataMember]
            public string thumbnail_url = "";

            [DataMember]
            public string title = "";

            [DataMember]
            public string view_counter = "";
        }
    }
     */

    //2017-03-03 UPDATE marky 検索API v2
    [DataContract]
    class NiconicoAPIResult
    {
        [DataMember]
        public MetaC meta = null;

        [DataContract]
        public class MetaC
        {
            [DataMember]
            public string status = "";

            [DataMember]
            public string totalCount = "";

            [DataMember]
            public string id = "";
        }

        [DataMember]
        public List<DataC> data = null;

        [DataContract]
        public class DataC
        {
            [DataMember]
            public string contentId = "";

            [DataMember]
            public string title = "";

            //2018/02/27 DELETE marky 動画説明文にHTMLタグが入る仕様変更に対応
            //[DataMember]
            //public string description = "";

            [DataMember]
            public string tags = "";

            // 2019/07/06 DELETE marky
            //[DataMember]
            //public string categoryTags = "";

            [DataMember]
            public string viewCounter = "";

            [DataMember]
            public string mylistCounter = "";

            [DataMember]
            public string commentCounter = "";

            [DataMember]
            public string startTime = "";

            [DataMember]
            public string thumbnailUrl = "";

            [DataMember]
            public string lengthSeconds = "";

            // 2019/07/06 ADD marky
            [DataMember]
            public string lastCommentTime = "";

            // 2019/07/06 ADD marky
            [DataMember]
            public string genre = "";
        }
    }

}
