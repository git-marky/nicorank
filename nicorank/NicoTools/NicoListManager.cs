﻿// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using IJLib;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.Serialization;         // 2019/06/26 ADD marky
using System.Runtime.Serialization.Json;    // 2019/06/26 ADD marky

namespace NicoTools
{
    public class NicoListManager
    {
        public enum ParseRankingKind { TermPoint, TotalPoint };
        private enum PointKind { View, Res, Mylist };

        private MessageOut msgout_;

        public NicoListManager(MessageOut msgout)
        {
            msgout_ = msgout;
        }

        // is_save_to_rank_file : true ならランクファイル、false ならデータベース
        // is_point : true ならポイント方式、false なら実数方式
        public void AnalyzeRanking(InputOutputOption iooption, RankingMethod ranking_method, ParseRankingKind kind, string ranking_dir_name)
        {
            msgout_.Write("ランキング解析中…\r\n");

            //// ランキングHTMLを全ポイント解析しようとしている場合は警告メッセージを出力 2020/08/19 Del marky 新ランキング時に不要となった
            //if (kind == ParseRankingKind.TotalPoint && IsRankingHtml(ranking_dir_name))
            //{
            //    msgout_.Write("ランキングHTML解析では全ポイント解析は使用できません。期間ポイントを選択して解析しなおしてください。\r\n");
            //    return;
            //}

            List<Video> video_list = ParseRanking(ranking_dir_name, DateTime.Now, kind);
            if (ranking_method.sort_kind != SortKind.Nothing)
            {
                video_list.Sort(ranking_method.GetComparer());
            }
            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("ランキングの解析が終了しました。\r\n");
        }

        /// <summary>
        /// 指定されたディレクトリに含まれているファイルがランキングHTML
        /// かどうかを判定する。
        /// </summary>
        /// <param name="ranking_dir_name">ダウンロードしたランキングファイルが含まれるディレクトリ</param>
        /// <returns>ダウンロードしたランキングがHTMLの場合はtrue、その他の場合はfalse。</returns>
        private bool IsRankingHtml(string ranking_dir_name)
        {
            bool is_html = false;

            if (Directory.Exists(ranking_dir_name))
            {
                foreach (string ranking_file in Directory.GetFiles(ranking_dir_name))
                {
                    string first_line = IJFile.ReadFirstLineUTF8(ranking_file);

                    if (!first_line.StartsWith("<?xml"))
                    {
                        is_html = true;
                        break;
                    }

                    // 最初の1ファイルだけで判断する
                    break;
                }
            }

            return is_html;
        }

        public void AnalyzeRankingNicoChart(InputOutputOption iooption, RankingMethod ranking_method, string ranking_dir, DateTime start_date, DateTime end_date)
        {
            msgout_.Write("ランキング解析中…\r\n");

            List<Video> video_list = ParseRankingNicoChart(ranking_dir, start_date.Date, end_date.Date);
            if (ranking_method.sort_kind != SortKind.Nothing)
            {
                video_list.Sort(ranking_method.GetComparer());
            }
            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("ランキングの解析が終了しました。\r\n");
        }

        public void MakeDiff(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method, DateTime exclusion_date)
        {
            if (!File.Exists(rank_file_diff1_path))
            {
                throw new Exception("ランクファイル(1)が存在しません。");
            }
            if (!File.Exists(rank_file_diff2_path))
            {
                throw new Exception("ランクファイル(2)が存在しません。");
            }

            msgout_.Write("差分を計算中…\r\n");
            RankFile rank_file_diff1 = new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat());
            RankFile rank_file_diff2 = new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat());
            List<Video> video_list_diff2 = rank_file_diff2.GetVideoList();
            List<Video> video_list = new List<Video>();

            for (int i = 0; i < rank_file_diff1.Count; ++i)
            {
                Video video = rank_file_diff1.GetVideo(i);
                int index = RankFile.SearchVideo(video_list_diff2, video.video_id);
                if (index >= 0)
                {
                    video.point -= video_list_diff2[index].point;
                    // 2019/09/26 ADD marky 前回の値を継承
                    if (video.thumbnail_url.Equals("") && video_list_diff2[index].thumbnail_url != "")
                    {
                        video.thumbnail_url = video_list_diff2[index].thumbnail_url;
                    }
                    if (video.genre.Equals("") && video_list_diff2[index].genre != "")
                    {
                        video.genre = video_list_diff2[index].genre;
                    }
                    if (video.tag_set.ToString().Equals("") && video_list_diff2[index].tag_set.ToString() != "")
                    {
                        video.tag_set = video_list_diff2[index].tag_set;
                    }
                    // 2021/05/20 ADD marky
                    if (video.user_id.Equals("") && video_list_diff2[index].user_id != "")
                    {
                        video.user_id = video_list_diff2[index].user_id;
                    }
                    if (video_list_diff2[index].like != "")
                    {
                        if (video.like.Equals(""))
                        {
                            video.like = video_list_diff2[index].like; //いいね！を継承
                        }
                        else
                        {
                            video.like = (int.Parse(video.like) - int.Parse(video_list_diff2[index].like)).ToString(); //いいね！差分を計算
                        }
                    }

                    video_list.Add(video);
                }
                else
                {
                    if (exclusion_date <= video.submit_date)
                    {
                        video_list.Add(video);
                    }
                }

            }
            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("差分を計算しました。\r\n");
        }

        public void MakeDiffB(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method)
        {
            if (!File.Exists(rank_file_diff1_path))
            {
                throw new Exception("ランクファイル(1)が存在しません。");
            }
            if (!File.Exists(rank_file_diff2_path))
            {
                throw new Exception("ランクファイル(2)が存在しません。");
            }

            msgout_.Write("差分を計算中…\r\n");
            RankFile rank_file_diff1 = new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat());
            RankFile rank_file_diff2 = new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat());
            List<Video> video_list_diff2 = rank_file_diff2.GetVideoList();
            List<Video> video_list = new List<Video>();

            for (int i = 0; i < rank_file_diff1.Count; ++i)
            {
                Video video = rank_file_diff1.GetVideo(i);
                int index = RankFile.SearchVideo(video_list_diff2, video.video_id);
                if (index < 0)
                {
                    video_list.Add(video);
                }
            }
            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("差分を計算しました。\r\n");
        }

        public void MakeDup(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method)
        {
            if (!File.Exists(rank_file_diff1_path))
            {
                throw new Exception("ランクファイル(1)が存在しません。");
            }
            if (!File.Exists(rank_file_diff2_path))
            {
                throw new Exception("ランクファイル(2)が存在しません。");
            }

            msgout_.Write("重複チェック中…\r\n");
            RankFile rank_file_diff1 = new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat());
            RankFile rank_file_diff2 = new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat());
            List<Video> video_list_diff2 = rank_file_diff2.GetVideoList();
            List<Video> video_list = new List<Video>();

            for (int i = 0; i < rank_file_diff1.Count; ++i)
            {
                Video video = rank_file_diff1.GetVideo(i);
                int index = RankFile.SearchVideo(video_list_diff2, video.video_id);
                if (index >= 0)
                {
                    video_list.Add(video);
                }
            }
            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("重複チェックが終了しました。\r\n");
        }

        public void MergeRankFileA(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method)
        {
            bool exists_rank_file1 = File.Exists(rank_file_diff1_path);
            bool exists_rank_file2 = File.Exists(rank_file_diff2_path);
            if (!exists_rank_file1 && !exists_rank_file2)
            {
                throw new Exception("ランクファイル(1),(2)が存在しません。");
            }

            msgout_.Write("マージ中…\r\n");
            if (!exists_rank_file1 && rank_file_diff1_path != "")
            {
                msgout_.WriteLine("ランクファイル(1)は存在しません。");
            }
            if (!exists_rank_file2 && rank_file_diff2_path != "")
            {
                msgout_.WriteLine("ランクファイル(2)は存在しません。");
            }
            RankFile rank_file_diff1 = (exists_rank_file1 ? new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat()) : null); 
            RankFile rank_file_diff2 = (exists_rank_file2 ? new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat()) : null);

            List<Video> video_list = new List<Video>();

            if (exists_rank_file1)
            {
                MergeToList(rank_file_diff1, video_list, ranking_method);
            }
            if (exists_rank_file2)
            {
                MergeToList(rank_file_diff2, video_list, ranking_method);
            }

            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("マージが終了しました。\r\n");
        }

        private void MergeToList(RankFile rank_file, List<Video> video_list, RankingMethod ranking_method)
        {
            for (int i = 0; i < rank_file.Count; ++i)
            {
                Video video = rank_file.GetVideo(i);
                int index = RankFile.SearchVideo(video_list, video.video_id);
                if (index >= 0)
                {
                    int point_new = video.point.CalcScore(ranking_method);
                    int point_old = video_list[index].point.CalcScore(ranking_method);
                    if (point_new > point_old)
                    {
                        video_list.RemoveAt(index);
                        video_list.Add(video);
                    }
                }
                else
                {
                    video_list.Add(video);
                }
            }
        }

        public void MergeRankFileB(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method)
        {
            bool exists_rank_file1 = File.Exists(rank_file_diff1_path);
            bool exists_rank_file2 = File.Exists(rank_file_diff2_path);
            if (!exists_rank_file1 && !exists_rank_file2)
            {
                throw new Exception("ランクファイル(1),(2)が存在しません。");
            }

            msgout_.Write("マージ中…\r\n");
            if (!exists_rank_file1 && rank_file_diff1_path != "")
            {
                msgout_.WriteLine("ランクファイル(1)は存在しません。");
            }
            if (!exists_rank_file2 && rank_file_diff2_path != "")
            {
                msgout_.WriteLine("ランクファイル(2)は存在しません。");
            }
            RankFile rank_file_diff1 = (exists_rank_file1 ? new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat()) : null);
            RankFile rank_file_diff2 = (exists_rank_file2 ? new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat()) : null);

            List<Video> video_list = new List<Video>();

            if (exists_rank_file1)
            {
                for (int i = 0; i < rank_file_diff1.Count; ++i)
                {
                    Video video = rank_file_diff1.GetVideo(i);
                    if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                    {
                        video_list.Add(video);
                    }
                }
            }
            if (exists_rank_file2)
            {
                for (int i = 0; i < rank_file_diff2.Count; ++i)
                {
                    Video video = rank_file_diff2.GetVideo(i);
                    if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                    {
                        video_list.Add(video);
                    }
                }
            }

            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("マージが終了しました。\r\n");
        }

        public void UpdatePoint(string rank_file_diff1_path, string rank_file_diff2_path, InputOutputOption iooption, RankingMethod ranking_method)
        {
            bool exists_rank_file1 = File.Exists(rank_file_diff1_path);
            if (!exists_rank_file1)
            {
                throw new Exception("ランクファイル(1)が存在しません。");
            }

            bool exists_rank_file2 = File.Exists(rank_file_diff2_path);
            if (!exists_rank_file2)
            {
                throw new Exception("ランクファイル(2)が存在しません。");
            }

            msgout_.Write("ポイント更新中…\r\n");
            
            RankFile rank_file_diff1 = new RankFile(rank_file_diff1_path, iooption.GetRankFileCustomFormat());
            RankFile rank_file_diff2 = new RankFile(rank_file_diff2_path, iooption.GetRankFileCustomFormat());

            List<Video> video_list_diff2 = rank_file_diff2.GetVideoList();
            List<Video> video_list = new List<Video>();

            for (int i = 0; i < rank_file_diff1.Count; ++i)
            {
                Video video = rank_file_diff1.GetVideo(i);
                int index = RankFile.SearchVideo(rank_file_diff2.GetVideoList(), video.video_id);
                if (index >= 0)
                {
                    video.title = video_list_diff2[index].title;
                    video.tag_set = video_list_diff2[index].tag_set;
                    video.point = video_list_diff2[index].point;
                    video.description = video_list_diff2[index].description;
                    video_list.Add(video);
                }
            }

            RankFile rank_file = new RankFile(video_list, iooption.GetRankFileCustomFormat());
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("ポイント更新が終了しました。\r\n");
        }

        public void SortRankFile(InputOutputOption iooption, RankingMethod ranking_method)
        {
            RankFile rank_file = iooption.GetRankFile();
            rank_file.Sort(ranking_method);
            iooption.OutputRankFile(rank_file, ranking_method);
            msgout_.Write("再計算しました。\r\n");
        }

        public void CalculateSum(InputOutputOption iooption, RankingMethod ranking_method)
        {
            RankFile rank_file = iooption.GetRankFile();

            msgout_.Write("------------------------------------------\r\n");
            msgout_.Write("内訳: 再生数, コメント数, マイリスト数, ポイント\r\n");

            RankPoint point = new RankPoint();
            point.view = point.res = point.mylist = 0;

            List<Video> video_list = rank_file.GetVideoList();
            List<int> view_list = new List<int>();
            List<int> res_list = new List<int>();
            List<int> mylist_list = new List<int>();
            List<int> point_list = new List<int>();

            for (int i = 0; i < video_list.Count; ++i)
            {
                point += video_list[i].point;
                view_list.Add(video_list[i].point.view);
                res_list.Add(video_list[i].point.res);
                mylist_list.Add(video_list[i].point.mylist);
                point_list.Add(video_list[i].point.CalcScore(ranking_method));
            }

            msgout_.Write("合計: " + point.view + ", " + point.res + ", " + point.mylist + ", " + point.CalcScore(ranking_method) + "\r\n");
            if (video_list.Count > 0)
            {
                msgout_.Write("平均: " + ((double)point.view / video_list.Count).ToString("0.00") + ", " +
                    ((double)point.res / video_list.Count).ToString("0.00") + ", " +
                    ((double)point.mylist / video_list.Count).ToString("0.00") + ", " +
                    ((double)point.CalcScore(ranking_method) / video_list.Count).ToString("0.00") + "\r\n");
                msgout_.Write("中央値: " + CalculateMedian(view_list) + ", " + CalculateMedian(res_list) + ", "
                     + CalculateMedian(mylist_list) + ", " + CalculateMedian(point_list) + "\r\n");
                msgout_.Write("最大値: " + view_list[view_list.Count - 1] + ", " + res_list[res_list.Count - 1] + ", "
                     + mylist_list[mylist_list.Count - 1] + ", " + point_list[point_list.Count - 1] + "\r\n");
                msgout_.Write("最小値: " + view_list[0] + ", " + res_list[0] + ", "
                     + mylist_list[0] + ", " + point_list[0] + "\r\n");
            }
        }

        private double CalculateMedian(List<int> list)
        {
            list.Sort();
            if (list.Count % 2 == 0)
            {
                return (double)(list[list.Count / 2 - 1] + list[list.Count / 2]) / 2;
            }
            else
            {
                return (double)list[(list.Count - 1) / 2];
            }
        }

        public static List<Video> ParseRanking(string dir_name, DateTime getting_dt, ParseRankingKind kind)
        {
            string[] files = System.IO.Directory.GetFiles(dir_name);
            List<Video> video_list = new List<Video>();
            Dictionary<string, Video> video_dic = new Dictionary<string, Video>();
            for (int i = 0; i < files.Length; ++i)
            {
                int t = System.Environment.TickCount;
                string html = IJFile.ReadUTF8(files[i]);
                //if (html.StartsWith("<?xml"))
                // 2020/04/04 Update marky 拡張子でRSSを判定に変更
                if (Path.GetExtension(files[i]).Equals(".xml"))
                {
                    ParsePointRss(html, getting_dt, video_list, kind == ParseRankingKind.TotalPoint, false);
                }
                else
                {
                    //switch (kind)
                    //{
                    //    case ParseRankingKind.TermPoint:
                    //        ParseRankingTermPointHtml(html, getting_dt, video_list, video_dic);
                    //        break;
                    //    case ParseRankingKind.TotalPoint:
                    //        throw new InvalidOperationException("ランキングHTMLは全ポイント解析できません。");
                    //}
                    // 2019/06/26 Update marky
                    ParseGenreTagLogFile(html, getting_dt, video_list);
                }
                System.Diagnostics.Debug.Write((System.Environment.TickCount - t).ToString() + ", ");
            }

            return video_list;
        }

        // ランキングまたはマイリストのRSSを解析する
        // is_mylist が true ならマイリスト、false ならランキング
        public static void ParsePointRss(string html, DateTime getting_dt, List<Video> video_list, bool is_total, bool is_mylist)
        {
            int index = -1;


            string genre = "";
            if (!is_mylist)
            {
                // nico-memoが存在する場合は2回目でnico-thumbnailを取得
                index = html.IndexOf("<title>", index + 1);
                string title = IJStringUtil.GetStringBetweenTag(ref index, "title", html);
                Match t = Regex.Match(title, "(.*)の動画ランキング");
                if (t.Success)
                {
                    genre = t.Groups[1].Value;
                }
            }

            while ((index = html.IndexOf("<item>", index + 1)) >= 0)
            {
                Video video = new Video();

                video.point.getting_date = getting_dt;
                video.title = IJStringUtil.GetStringBetweenTag(ref index, "title", html);
                if (!is_mylist)
                {
                    video.title = video.title.Substring(video.title.IndexOf('：') + 1);
                }
                string link = IJStringUtil.GetStringBetweenTag(ref index, "link", html);
                //video.video_id = link.Substring(link.LastIndexOf('/') + 1);
                // 2020/08/04 Update marky 7月27日リニューアル後？末尾にパラメータが付くようになったため除去
                //video.video_id = link.Substring(link.LastIndexOf('/') + 1).Replace("?ref=rss_mylist_rss2", "");
                // 2023/01/18 Update marky パラメータの変更"?ref=rss_mylist_rss2.0"に対応、汎用的にパラメータを除去
                video.video_id = link.Substring(link.LastIndexOf('/') + 1);
                if (video.video_id.LastIndexOf('?') > 0 ) {
                    video.video_id = video.video_id.Substring(0, video.video_id.LastIndexOf('?')); 
                }

                // 2020/08/10 ADD marky 7月27日リニューアル後？ランキングRSSにもパラメータが付くようになったため除去
                if (is_mylist)
                {
                    // 2023/01/18 del marky パラメータの変更に対応
                    //video.video_id = link.Substring(link.LastIndexOf('/') + 1).Replace("?ref=rss_mylist_rss2", "");

                    // 2021/09/15 ADD marky マイリストRSSでは公式動画がスレッドIDとなっていたためsoのIDに変換
                    int id;
                    if (int.TryParse(video.video_id, out id))   // IDが数字だけの場合
                    {
                        NicoNetwork network = new NicoNetwork();
                        Video video2 = new Video(network.GetThumbInfo(video.video_id));
                        video.video_id = video2.video_id;
                    }
                }
                // 2023/01/18 del marky パラメータの変更に対応
                //else
                //{
                //    video.video_id = link.Substring(link.LastIndexOf('/') + 1).Replace("?ref=rss_specified_ranking_rss2", "");
                //}
                // 2020/08/10 ADD marky end

                //IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                // 2021/12/08 Update marky start nico-thumbnailを取得
                string thumbnail = IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                Match m = Regex.Match(thumbnail, "src=\"([^\"]*)\"");
                if (m.Success)
                {
                    video.thumbnail_url = m.Groups[1].Value;
                }
                else if (is_mylist)
                {
                    // nico-memoが存在する場合は2回目でnico-thumbnailを取得
                    thumbnail = IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                    m = Regex.Match(thumbnail, "src=\"([^\"]*)\"");
                    if (m.Success)
                    {
                        video.thumbnail_url = m.Groups[1].Value.Replace(".M","");
                    }
                }
                // 2021/12/08 Update marky end

                video.description = IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                // 2019/06/26 DEL marky
                //if (!is_mylist) // 読み飛ばし
                //{
                //    IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                //}
                video.length = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                string date_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                video.submit_date = NicoUtil.StringToDate(date_str);

                if (!is_mylist)
                {
                    //「合計」をSkip
                    IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                    // 2019/06/26 DEL marky
                    //if (!is_total)
                    //{
                    //    for (int i = 0; i < 4; ++i) // 4回読み飛ばす
                    //    {
                    //        IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                    //    }
                    //}
                }

                string view_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                string res_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                // 2021/12/08 ADD marky
                if (!is_mylist)
                {
                    string like_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);  
                    video.like = IJStringUtil.ToIntFromCommaValueWithDef(like_str, 0).ToString();
                }
                string mylist_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                video.point.view = IJStringUtil.ToIntFromCommaValue(view_str);
                video.point.res = IJStringUtil.ToIntFromCommaValue(res_str);
                video.point.mylist = IJStringUtil.ToIntFromCommaValue(mylist_str);
                // 2021/12/08 ADD marky
                video.genre = genre;
                if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                {
                    video_list.Add(video);
                }
            }
        }

        // 2023/01/21 ADD marky
        // マイリストのJsonを解析する
        public static void ParseMylistJson(string json, List<Video> video_list, ref bool isNext)
        {
            int index = -1;
            string hasNext;

            //string genre = "";

            while ((index = json.IndexOf("\"watchId\":\"", index + 1)) >= 0)
            {
                Video video = new Video();

                video.video_id = IJStringUtil.GetValueByKey(ref index, "watchId", json);
                video.description = IJStringUtil.GetValueByKey(ref index, "description", json);
                video.title = IJStringUtil.GetValueByKey(ref index, "title", json);

                string date_str = IJStringUtil.GetValueByKey(ref index, "registeredAt", json);
                video.submit_date = DateTime.Parse(date_str, null, System.Globalization.DateTimeStyles.RoundtripKind);


                string view_str = IJStringUtil.GetValueByKey(ref index, "view", json);
                string res_str = IJStringUtil.GetValueByKey(ref index, "comment", json);
                string mylist_str = IJStringUtil.GetValueByKey(ref index, "mylist", json);
                video.point.view = int.Parse(view_str);
                video.point.res = int.Parse(res_str);
                video.point.mylist = int.Parse(mylist_str);
                video.like = IJStringUtil.GetValueByKey(ref index, "like", json);

                video.thumbnail_url = IJStringUtil.GetValueByKey(ref index, "url", json);

                video.length = IJStringUtil.GetValueByKey(ref index, "duration", json);
                video.last_res_body = IJStringUtil.GetValueByKey(ref index, "latestCommentSummary", json);

                video.user_id = IJStringUtil.GetValueByKey(ref index, "id", json);
                video.user_name = IJStringUtil.GetValueByKey(ref index, "name", json);

                //video.genre = genre;

                if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                {
                    video_list.Add(video);
                }
            }

            //続きがあるか
            index = 0;
            hasNext = IJStringUtil.GetValueByKey(ref index, "hasNext", json);
            if (bool.TryParse(hasNext, out bool ret))
            {
                isNext = ret;
            }
            else
            {
                isNext = false;
            }

        }

        // video_dic は高速化のため
        public static void ParseRankingTermPointHtml(string html, DateTime getting_dt, List<Video> video_list, Dictionary<string, Video> video_dic)
        {
            PointKind point_kind = JudgePointKind(html);

            foreach (int index in EnumerateRankingHtmlVideoInfoIndex(html))
            {
                Video video;
                int value;
                DateTime date;
                int view;
                int res;
                int mylist;
                string video_id;
                string title;

                ParseRankingHtmlVideoInfo(html, index, out value, out date, out view, out res, out mylist, out video_id, out title);

                if (!video_dic.TryGetValue(video_id, out video))
                {
                    video = new Video();
                    video.point.getting_date = getting_dt;
                    video.submit_date = date;
                    video.point.view = 0;
                    video.point.res = 0;
                    video.point.mylist = 0;
                    video.video_id = video_id;
                    video.title = title;
                    video_list.Add(video);
                    video_dic.Add(video_id, video);
                }
                switch (point_kind)
                {
                    case PointKind.View:
                        video.point.view = value;
                        break;
                    case PointKind.Res:
                        video.point.res = value;
                        break;
                    case PointKind.Mylist:
                        video.point.mylist = value;
                        break;
                }
            }
        }

        // 2019-06-26 ADD marky
        private static void ParseGenreTagLogFile(string json, DateTime getting_dt, List<Video> video_list)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GenreTagLogList[]));
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    GenreTagLogList[] result = (GenreTagLogList[])serializer.ReadObject(ms);
                    if (result != null)
                    {
                        StringBuilder buff = new StringBuilder();
                        for (int j = 0; j < result.Length; ++j)
                        {
                            Video video = new Video();

                            video.point.getting_date = getting_dt;
                            video.video_id = result[j].id;
                            video.point.view = result[j].count.view; ;
                            video.point.res = result[j].count.comment;
                            video.point.mylist = result[j].count.mylist;
                            video.like = result[j].count.like;          // 2021/07/30 ADD marky
                            if (video.like == null){ video.like = ""; } // 2021/07/30 ADD marky 07/30以前はlike未定義
                            video.thumbnail_url = result[j].thumbnail.url;  // 2021/12/08 ADD marky
                            video.title = result[j].title;
                            video.submit_date = DateTime.Parse(result[j].registeredAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                            {
                                video_list.Add(video);
                            }

                        }
                    }
                }
            }
            catch
            {
                throw new NiconicoFormatException("ランキングファイルが無効です。");           
            }
        }
        
        private static IEnumerable<int> EnumerateRankingHtmlVideoInfoIndex(string html)
        {
            int index = -1;

            //while ((index = html.IndexOf("<table width=\"648\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" summary=\"\">", index + 1)) >= 0)
            //GINZA対応  2013/10/14 UPDATE marky
            while ((index = html.IndexOf("<div class=\"rankingNumWrap\">", index + 1)) >= 0)
            {
                yield return index;
            }
        }

        //private static void ParseRankingHtmlVideoInfo(string html, int index,
        //    out int value,
        //    out DateTime date,
        //    out int view,
        //    out int res,
        //    out int mylist,
        //    out string video_id,
        //    out string title)
        //{
        //    int ps = html.IndexOf("watch/", index) + 6;
        //    int pe = html.IndexOf('"', ps);
        //    video_id = html.Substring(ps, pe - ps);

        //    index = html.IndexOf("<strong", index) + 1;
        //    string value_str = IJStringUtil.GetStringBetweenTag(ref index, "span", html);

        //    string date_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);

        //    // ニコニコ動画(9)のランキングHTMLにはランキング対象の期間ポイント以外は表示されない
        //    string view_str = "0";
        //    string res_str = "0";
        //    string mylist_str = "0";

        //    title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);

        //    value = IJStringUtil.ToIntFromCommaValue(value_str);
        //    date = NicoUtil.StringToDate(date_str);
        //    view = IJStringUtil.ToIntFromCommaValue(view_str);
        //    res = IJStringUtil.ToIntFromCommaValue(res_str);
        //    mylist = IJStringUtil.ToIntFromCommaValue(mylist_str);
        //}

        //HTMLランキングファイル動画情報取得 GINZA対応 2013/10/14 marky
        private static void ParseRankingHtmlVideoInfo(string html, int index,
            out int value,
            out DateTime date,
            out int view,
            out int res,
            out int mylist,
            out string video_id,
            out string title)
        {
            //ポイント
            int ps = html.IndexOf("rankingPt\">+", index) + 12;
            int pe = html.IndexOf('<', ps);
            string value_str = html.Substring(ps, pe - ps);

            //投稿日時
            string date_str = IJStringUtil.GetStringBetweenTag(ref index, "span", html);

            //動画ID
            ps = html.IndexOf("data-id=\"", index) + 9;
            pe = html.IndexOf('"', ps);
            video_id = html.Substring(ps, pe - ps);

            //タイトル
            index = html.IndexOf("<p class=\"itemTitle ranking\">", index) + 1;
            title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);

            // ニコニコ動画(9)のランキングHTMLにはランキング対象の期間ポイント以外は表示されない
            string view_str = "0";
            string res_str = "0";
            string mylist_str = "0";

            value = IJStringUtil.ToIntFromCommaValue(value_str);
            //date = NicoUtil.StringToDate(date_str);
            date = DateTime.ParseExact(date_str, "yyyy/MM/dd HH:mm", null);
            view = IJStringUtil.ToIntFromCommaValue(view_str);
            res = IJStringUtil.ToIntFromCommaValue(res_str);
            mylist = IJStringUtil.ToIntFromCommaValue(mylist_str);
        }

        private static PointKind JudgePointKind(string html)
        {
            int end = html.IndexOf("</title>");
            string str = html.Substring(0, end);
            if (str.IndexOf("再生ランキング") >= 0)
            {
                return PointKind.View;
            }
            if (str.IndexOf("コメントランキング") >= 0)
            {
                return PointKind.Res;
            }
            if (str.IndexOf("マイリストランキング") >= 0)
            {
                return PointKind.Mylist;
            }
            throw new FormatException("HTMLの解析に失敗しました（ポイント種類判定）。");
        }

        public static void ParseRankingTermPointHtmlVersion1(string html, DateTime getting_dt, List<Video> video_list, string filename)
        {
            int index = -1;

            while ((index = html.IndexOf("<td class=\"rank_num\"", index + 1)) >= 0)
            {
                IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                string valueStr = IJStringUtil.GetStringBetweenTag(ref index, "p", html);
                IJStringUtil.GetStringBetweenTag(ref index, "strong", html); // 読み飛ばす
                string dateStr = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                //video.submit_date = IJStringUtil.StringToDate(dateStr);
                string viewStr = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                if (viewStr.Equals("投稿者コメント")) // 投稿者コメントの部分はとばす
                {
                    viewStr = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                }
                string resStr = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                string mylistStr = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                /*point.view = IJStringUtil.GetInt(viewStr);
                point.res = IJStringUtil.GetInt(resStr);
                point.mylist = IJStringUtil.GetInt(mylistStr);*/
                int ps = html.IndexOf("watch/", index) + 6;
                int pe = html.IndexOf('"', ps);
                string video_id = html.Substring(ps, pe - ps);
                string title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);
                Video video = null;
                for (int i = 0; i < video_list.Count; ++i)
                {
                    if (video_list[i].video_id == video_id)
                    {
                        video = video_list[i];
                        break;
                    }
                }
                if (video == null)
                {
                    video = new Video();
                    video.point.view = 0;
                    video.point.res = 0;
                    video.point.mylist = 0;
                    video.point.getting_date = getting_dt;
                    video.submit_date = NicoUtil.StringToDate(dateStr);
                    video.video_id = video_id;
                    video.title = title;
                    video_list.Add(video);
                }
                if (filename.IndexOf("vie") >= 0)
                {
                    video.point.view = IJStringUtil.ToIntFromCommaValue(valueStr);
                }
                else if (filename.IndexOf("res") >= 0)
                {
                    video.point.res = IJStringUtil.ToIntFromCommaValue(valueStr);
                }
                else if (filename.IndexOf("myl") >= 0)
                {
                    video.point.mylist = IJStringUtil.ToIntFromCommaValue(valueStr);
                }
                else
                {
                    throw new System.Exception("ファイルの名前の形式が正しくありません");
                }
            }
        }

        public static List<Video> ParseRankingNicoChart(string dir_name, DateTime start_date, DateTime end_date)
        {
            if (dir_name[dir_name.Length - 1] != '\\')
            {
                dir_name += '\\';
            }
            List<Video> video_list = new List<Video>();
            for (DateTime dt = end_date; dt >= start_date; dt = dt.AddDays(-1.0))
            {
                string[] files = System.IO.Directory.GetFiles(dir_name + dt.ToString("yyyyMMdd"));
                for (int i = 0; i < files.Length; ++i)
                {
                    string html = IJFile.Read(files[i]);
                    ParseRankingNicoChartOnePage(html, video_list);
                }
            }
            return video_list;
        }

        public static void ParseRankingNicoChartOnePage(string html, List<Video> video_list)
        {
            int index = 0;

            while ((index = html.IndexOf("<li id=\"rank", index)) >= 0)
            {
                Video video = new Video();

                // 2023/11/30 ADD marky
                index = html.IndexOf("<li class=\"first-retrieve", index);
                string date_str = IJStringUtil.GetStringBetweenTag(ref index, "li", html);
                date_str = date_str.Replace("  <strong class=\"new\">New!</strong>", "").Replace(":", "：");
                video.submit_date = NicoUtil.StringToDate(date_str);
                index = html.IndexOf("<li class=\"total", index);

                string view_str = IJStringUtil.GetStringBetweenTag(ref index, "em", html);
                string res_str = IJStringUtil.GetStringBetweenTag(ref index, "em", html);
                string mylist_str = IJStringUtil.GetStringBetweenTag(ref index, "em", html);
                video.point.view = IJStringUtil.ToIntFromCommaValue(view_str);
                video.point.res = IJStringUtil.ToIntFromCommaValue(res_str);
                video.point.mylist = IJStringUtil.ToIntFromCommaValue(mylist_str);
                int ps = html.IndexOf("watch/", index) + 6;
                int pe = html.IndexOf('"', ps);
                video.video_id = html.Substring(ps, pe - ps);
                // 2023/11/30 ADD marky
                index = html.IndexOf("<li class=\"title", index);
                video.title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);
                // 2023/11/30 DEL marky
                //index = html.IndexOf("<li class=\"release", index) - 1;
                //string date_str = IJStringUtil.GetStringBetweenTag(ref index, "li", html);
                //video.submit_date = NicoUtil.StringToDate(date_str);
                if (RankFile.SearchVideo(video_list, video.video_id) < 0)
                {
                    video_list.Add(video);
                }
            }
        }

        public static List<Video> ParseMyVideoHtml(string html)
        {
            if (html.IndexOf("ここから先をご利用いただくにはログインしてください") >= 0)
            {
                throw new NiconicoLoginException();
            }

            List<Video> video_list = new List<Video>();
            
            // 動画が1つもない場合の処理が必要

            int index = 0;

            while ((index = html.IndexOf("<p class=\"menu_box\">", index)) >= 0)
            {
                Video video = new Video();
                string date_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                video.submit_date = DateTime.ParseExact(date_str, "yy年MM月dd日 HH:mm", null);

                string view_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                string res_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                string mylist_str = IJStringUtil.GetStringBetweenTag(ref index, "strong", html);
                video.point.view = IJStringUtil.ToIntFromCommaValue(view_str);
                video.point.res = IJStringUtil.ToIntFromCommaValue(res_str);
                video.point.mylist = IJStringUtil.ToIntFromCommaValue(mylist_str);
                int start = html.IndexOf("watch/", index) + "watch/".Length;
                int end = html.IndexOf('"', start);
                video.video_id = html.Substring(start, end - start);
                video.title = IJStringUtil.GetStringBetweenTag(ref index, "a", html);
                video.description = IJStringUtil.GetStringBetweenTag(ref index, "p", html); // description の欄を仮に使う
                video_list.Add(video);
            }
            return video_list;
        }
    }
}
