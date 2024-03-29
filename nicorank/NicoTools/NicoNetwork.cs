﻿// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using IJLib;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;     //2018/09/14 Add marky Dmcサーバ対応
using System.Runtime.Serialization.Json;//2018/09/14 Add marky Dmcサーバ対応
using System.Timers;                    //2018/09/14 Add marky Dmcサーバ対応
using System.Linq;
using Org.BouncyCastle.Crypto;              //2022/08/07 Add marky Chrome AES対応
using Org.BouncyCastle.Crypto.Modes;        //2022/08/07 Add marky Chrome AES対応
using Org.BouncyCastle.Crypto.Engines;      //2022/08/07 Add marky Chrome AES対応
using Org.BouncyCastle.Crypto.Parameters;   //2022/08/07 Add marky Chrome AES対応

namespace NicoTools
{
    /// <summary>
    /// ニコニコ動画から各種情報を取得するためのクラス。
    /// </summary>
    /// <remarks>
    /// ニコニコ動画から公式ランキングHTML取得、FLV取得などの機能を持つクラス。ログインが必要な操作は事前にログインの必要がある。
    /// </remarks>
    public class NicoNetwork
    {
        /// <summary>
        /// ブラウザからクッキーを自動取得して設定するときにブラウザの種類を指定するための列挙型
        /// </summary>
        //public enum CookieKind { None, IE, Firefox3, Opera, Chrome };
        //2022/08/07 Update marky Chrome AES対応
        public enum CookieKind { None, Edge, Firefox3, Opera, Chrome };

        public enum SearchSortMethod { SubmitDate, View, ResNew, Res, Mylist, Time };
        public enum SearchOrder { Asc, Desc };

        public delegate void NetworkWaitDelegate(string message, int current, int total);

        private static Random random_ = new Random(); // for Wait

        //private CookieKind cookie_kind_ = CookieKind.IE; // クッキーを読み込むブラウザ
        // 2022/08/07 Update marky
        private CookieKind cookie_kind_ = CookieKind.None; // クッキーを読み込むブラウザ
        private IJNetwork network_; // ネットワーク通信用
        private bool is_loaded_cookie_ = false; // 通信を一度でもしたことがあるなら true になる
        private int wait_millisecond_; // デフォルトのイベント関数で Sleep するときのミリ秒数を保存
        private bool is_no_cache_ = false; // HTTP通信でキャッシュをしないように HttpWebRequest に強制させるか
        private string firefox_profile_dir_ = ""; // Firefox のプロファイルディレクトリ（空ならデフォルト値を採用）

        //private const string nicovideo_uri_ = "http://www.nicovideo.jp"; // ニコニコ動画URL
        private const string nicovideo_uri_ = "https://www.nicovideo.jp"; // ニコニコ動画URL     // 2019/06/26 Update marky
        //private const string nicovideo_ext_uri_ = "http://ext.nicovideo.jp";
        private const string nicovideo_ext_uri_ = "https://ext.nicovideo.jp";  // 2020/08/27 Update marky API呼び出しをhttps化
        private const string ranklog_url_ = "https://dcdn.cdn.nimg.jp/nicovideo/old-ranking/";   //過去ログURL 2019/06/26 ADD marky

        private const string nicovideo_cookie_domain_ = ".nicovideo.jp"; // ニコニコ動画クッキードメイン

        // ユーザエージェント（ニコニコ動画ではデータ取得アプリのユーザエージェントに連絡先を含めることを推奨している）
        private const string nicovideo_user_agent_ = "Niconico_Ranking_Maker/%%version%% (by rankingloid at daily-vocaran.info)";

        private string issuer_ = "Niconico_Ranking_Maker/%%version%%";

        //2021/12/18 ADD marky
        private readonly string[] mylist_sort = { "addedAt", "mylistComment", "title", "registeredAt", "viewCount", "lastCommentTime", "commentCount", "mylistCount", "duration" };

        public NicoNetwork()
        {
            network_ = new IJNetwork();
            string version = System.Windows.Forms.Application.ProductVersion;
            version = version.Remove(version.Length - 4, 4); // 後ろの ".0.0" を削る
            network_.UserAgent = nicovideo_user_agent_.Replace("%%version%%", version);
            issuer_ = issuer_.Replace("%%version%%", version);
            // 2023/11/30 ADD marky TLS1.2化（規定値はSSL 3.0|TLS 1.0）
            // 対象のフレームワークを.NET Framework 4.5.2以上にすることで以下記述が可能
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }

        /// <summary>
        /// HTTP通信でキャッシュをしないように HttpWebRequest に強制させるか
        /// </summary>
        public bool NoCache
        {
            get { return is_no_cache_; }
            set { is_no_cache_ = value; }
        }

        public string ProfileDir
        {
            get { return firefox_profile_dir_; }
            set { firefox_profile_dir_ = value; }
        }

        /// <summary>
        /// ニコニコ動画にログインする
        /// </summary>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインに成功したかどうか</returns>
 
        public bool LoginNiconico(string username, string password)
        {
            network_.ClearCookie();

            string post_data = IJNetwork.ConstructPostData(
                "mail",     username,
                "password", password);
            string str = network_.PostAndReadFromWebUTF8("https://secure.nicovideo.jp/secure/login?site=niconico", post_data);
            is_loaded_cookie_ = true;

            //if (str.IndexOf("入力したメールアドレスまたはパスワードが間違っています") >= 0) // login failed
            //2018/09/14 Update marky
            if (str.IndexOf("メールアドレスまたはパスワードが間違っています") >= 0) // login failed
            { 
                return false;
            }
            else
            {
                //Match m = Regex.Match(str, "var User = \\{ id: [0-9]+");
                //2018/09/14 Update marky
                Match m = Regex.Match(str, "user.user_id = parseInt\\('[0-9]+");

                if (m.Success) // login succeeded
                {
                    return true;
                }
                else // html parse error
                {
                    throw new NiconicoFormatException();
                }
            }
        }
        
        /// <summary>
        /// ニコニコ動画にログインされていない場合はログインする
        /// </summary>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインに成功したかどうか。すでにログイン済みの場合は true を返す</returns> 
        public bool CheckAndLoginNiconico(string username, string password)
        {
            if (!IsLoginNiconico())
            {
                return LoginNiconico(username, password);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// ニコニコ動画にログインされているかをチェックする。
        /// </summary>
        /// <returns>ログインされているかどうか</returns> 
        public bool IsLoginNiconico()
        {
            CheckCookie();

            string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_);

            //if (str.IndexOf("var User = { id: false") < 0)
            //{
            //    is_loaded_cookie_ = true;
            //    return true;
            //}
            //else if (str.IndexOf("User = { id: ") >= 0)
            //{
            //    return false;
            //}
            // 2019/01/21 Update marky
            if (str.IndexOf("user.login_status = 'login'") > 0)
            {
                is_loaded_cookie_ = true;
                return true;
            }
            else if (str.IndexOf("user.login_status") >= 0)
            {
                return false;
            }
            else
            {
                throw new NiconicoLoginException();
            }
        }

        /// <summary>
        /// クッキーを読み込むブラウザの種類を設定する
        /// </summary>
        /// <param name="kind">クッキーを読み込むブラウザの種類</param>
        public void SetCookieKind(CookieKind kind)
        {
            cookie_kind_ = kind;
        }

        /// <summary>
        /// 内部のクッキーを空にする
        /// </summary>
        public void ClearCookie()
        {
            network_.ClearCookie();
            is_loaded_cookie_ = false;
        }

        /// <summary>
        /// 内部のクッキーを空にして、クッキーを読み込むブラウザの種類を設定し、
        /// そのブラウザから実際にクッキーを読み込む
        /// </summary>
        /// <param name="kind">クッキーを読み込むブラウザの種類</param>
        public void ReloadCookie(CookieKind kind)
        {
            ClearCookie();
            SetCookieKind(kind);
            LoadCookies();
        }

        /// <summary>
        /// 現在の user_session を取得
        /// </summary>
        /// <returns></returns>
        public string GetUserSession()
        {
            CookieCollection cookie_collection = network_.GetCookie(nicovideo_uri_);
            if (cookie_collection["user_session"] != null)
            {
                return cookie_collection["user_session"].Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// user_session を設定
        /// </summary>
        /// <param name="user_session">user_session</param>
        public void ResetUserSession(string user_session)
        {
            network_.ClearCookie();
            string cookie_string = string.Format("user_session={0}; domain={1}; path=/", user_session, nicovideo_cookie_domain_);
            network_.SetCookie(nicovideo_uri_, cookie_string);
            is_loaded_cookie_ = true;
        }

        /// <summary>
        /// ニコニコ動画公式ランキングHTMLまたはRSSをダウンロードする。
        /// </summary>
        /// <param name="saved_dir">ランキングHTMLまたはRSSを保存するディレクトリ</param>
        /// <param name="download_kind">ダウンロードするランキングの種類</param>
        /// <param name="wait_millisecond">1件ごとの待ち時間（ミリ秒）</param>
        public void DownloadRanking(string saved_dir, DownloadKind download_kind, int wait_millisecond)
        {
            wait_millisecond_ = wait_millisecond;
            DownloadRanking(saved_dir, download_kind, OnDefaultWaitEvent);
        }

        /// <summary>
        /// ニコニコ動画公式ランキングHTMLまたはRSSをダウンロードする。
        /// </summary>
        /// <param name="saved_dir">ランキングHTMLまたはRSSを保存するディレクトリ</param>
        /// <param name="download_kind">ダウンロードするランキングの種類</param>
        /// <param name="dlg">1件ダウンロードするごとに呼び出されるイベント関数</param>
        public void DownloadRanking(string saved_dir, DownloadKind download_kind, NetworkWaitDelegate dlg)
        {
            if (!saved_dir.EndsWith("\\") && saved_dir != "")
            {
                saved_dir += "\\";
            }

            if (saved_dir != "")
            {
                Directory.CreateDirectory(saved_dir);
            }

            List<string> name_list = new List<string>();
            List<string> filename_list = new List<string>();

            //download_kind.GetRankingNameList(ref name_list, ref filename_list);
            // 2019/06/26 Update marky
            download_kind.GetRankingRssList(ref name_list, ref filename_list);

            for (int i = 0; i < name_list.Count; ++i)
            {
                DownloadRankingOnePage(name_list[i], saved_dir, filename_list[i], -1, download_kind.IsRss);
                if (dlg != null)
                {
                    dlg(name_list[i], i + 1, name_list.Count);
                }
            }
        }

        /// <summary>
        /// ニコニコ動画公式ランキング過去ログをダウンロードする。 2019/06/26 ADD marky
        /// </summary>
        /// <param name="saved_dir">ランキング過去ログを保存するディレクトリ</param>
        /// <param name="download_kind">ダウンロードするランキングの種類</param>
        /// <param name="ranking_method">ランキングファイルオプション</param>
        /// <param name="dlg">1件ダウンロードするごとに呼び出されるイベント関数</param>
        public void DownloadRankingLog(string saved_dir, DownloadKind download_kind, NetworkWaitDelegate dlg)
        {
            if (!saved_dir.EndsWith("\\") && saved_dir != "")
            {
                saved_dir += "\\";
            }

            if (saved_dir != "")
            {
                Directory.CreateDirectory(saved_dir);
            }

            List<string> name_list = new List<string>();
            List<string> filename_list = new List<string>();

            download_kind.GetRankingLogList(ref name_list, ref filename_list);

            for (int i = 0; i < name_list.Count; ++i)
            {
                DownloadRankingLog(name_list[i], saved_dir, filename_list[i]);
                if (dlg != null)
                {
                    dlg(name_list[i], i + 1, name_list.Count);
                }
            }
        }

        /// <summary>
        /// 動画をダウンロードする
        /// </summary>
        /// <param name="video_id">ダウンロードする動画ID</param>
        /// <param name="save_flv_filename">ダウンロードしたファイルを保存するファイル名</param>
        /// <param name="dlg">ダウンロード最中に呼び出されるコールバックメソッド。null でもよい。</param>
        /// <exception cref="System.Exception">erer</exception>
        public void DownloadAndSaveFlv_old(string video_id, string save_flv_filename, IJNetwork.DownloadingEventDelegate dlg)
        {
            CheckCookie();
            GetVideoPage(video_id); // クッキーのために HTML のページを取得しておく

            Thread.Sleep(500);
            VideoInfo video_info = new VideoInfo(GetVideoInfo(video_id));

            if (video_info.url == "")
            {
                throw new NiconicoAccessFailedException();
            }

            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            network_.SetDownloadingEventDelegate(dlg);

            try
            {
                Thread.Sleep(1000);
                network_.PostAndSaveToFile(video_info.url, "", save_flv_filename);
            }
            finally
            {
                network_.Reset();
            }
        }

        /// <summary>
        /// 動画をダウンロードする
        /// </summary>
        /// <param name="video_id">ダウンロードする動画ID</param>
        /// <param name="save_flv_dir">ダウンロードしたファイルを保存するファイル名</param>
        /// <param name="dlg">ダウンロード最中に呼び出されるコールバックメソッド。null でもよい。</param>
        /// <param name="ffmpeg_dir">ffmepgの実行パス</param>
        /// <exception cref="System.Exception">erer</exception>
        //public void DownloadAndSaveFlv(string video_id, string save_flv_filename, IJNetwork.DownloadingEventDelegate dlg)
        //2024/02/26 Update marky CMAF対応
        public void DownloadAndSaveFlv(string video_id, string save_flv_dir, IJNetwork.DownloadingEventDelegate dlg, string ffmpeg_dir)
        {

            string api_url = "";
            string res = "";

            CheckCookie();
            String page = GetVideoPage(video_id); // クッキーのために HTML のページを取得しておく

            //smileInfo&quot;:{&quot;url&quot;:&quot;https:\\/\\/smile-pow64.sv.nicovideo.jp\\/smile?m=33806951.42570&quot;,
            //int index = page.IndexOf("smileInfo");
            //int start = page.IndexOf("url", index) +16;
            //int end = page.IndexOf(",", start) - 7;

            //String video_url = page.Substring(start, end - start);

            //if (video_url == "")
            //{
            //    throw new NiconicoAccessFailedException();
            //}

            //video_url = video_url.Replace("\\", "");  //リテラル文字除去

            int index = page.IndexOf("js-initial-watch-data");
            if (index == -1)
            {
                throw new NiconicoAccessFailedException();
            }

            //<div id="js-initial-watch-data" data-api-data="{
            String startstr = "data-api-data=\"";
            //}" hidden></div>
            String endtstr = "\" hidden></div>";
            if (page.IndexOf(startstr, index) == -1)
            {
                throw new NiconicoAccessFailedException();
            }
            int start = page.IndexOf(startstr, index) + startstr.Length;
            int end = page.IndexOf(endtstr, start);
            if (end == -1)
            {
                throw new NiconicoAccessFailedException();
            }

            String data = page.Substring(start, end - start);

            data = data.Replace("&quot;","\"");  //リテラル文字除去

            //startstr = "\"dmcInfo\":";
            //endtstr = ",\"backCommentType";
            //start = data.IndexOf(startstr) + startstr.Length;
            //end = data.IndexOf(endtstr, start);

            //data = data.Substring(start, end - start);

            ////sessionが取得できないケース
            //if (data.IndexOf("\"dmcInfo\":null") != -1)
            //{
            //    DownloadAndSaveFlv_old(video_id, save_flv_filename, dlg);
            //    return;
            //}

            //startstr = "\"session_api\":";
            //endtstr = ",\"storyboard_session_api";
            // 2021/03/16 Update marky sessionフォーマット変更
            startstr = "\"session\":";
            endtstr = ",\"storyboard";
            if (data.IndexOf(startstr) == -1)
            {
                // 2024/02/26 ADD marky CMAF対応 start
                //"watchTrackId":"TmF3Ip1rR1_1708519448256"},
                index = 0;
                string TrackId = IJStringUtil.GetValueByKey(ref index, "watchTrackId", data);
                if (TrackId.Equals(""))
                {
                    throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
                }

                //"accessRightKey":"XXXXXXXXX"},
                index = 0;
                string RightKey = IJStringUtil.GetValueByKey(ref index, "accessRightKey", data);
                if (RightKey.Equals(""))
                {
                    startstr = "\"billingType\":\"member_only\"";
                    if (data.IndexOf(startstr) > 0)
                    {
                        throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。有料動画の可能性があります。");
                    }
                    else
                    {
                        throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
                    }
                }

                string rate = GetVideoAudioRate(data);

                DownloadAndSaveFlv_new(video_id, save_flv_dir, dlg, TrackId, RightKey, rate, ffmpeg_dir);
                return;
                // 2024/02/26 ADD marky CMAF対応 end
            }
            start = data.IndexOf(startstr) + startstr.Length;
            end = data.IndexOf(endtstr, start);
            if (end == -1)
            {
                throw new NiconicoAccessFailedException();
            }

            data = data.Substring(start, end - start);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(sessionAPI));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                sessionAPI result = (sessionAPI)serializer.ReadObject(ms);
                if (result.urls == null)
                {
                    throw new NiconicoAccessFailedException();
                }

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                StringBuilder post_str = new StringBuilder();
                using (TextWriter tw = new StringWriter(post_str))
                using (XmlWriter writer = XmlWriter.Create(tw, settings))
                {
                    writer.WriteStartElement("session");

                    writer.WriteStartElement("recipe_id");
                    writer.WriteString(result.recipeId);
                    writer.WriteEndElement(); // <recipe_id ... />

                    writer.WriteStartElement("content_id");
                    writer.WriteString(result.contentId);
                    writer.WriteEndElement(); // <content_id ... />

                    writer.WriteStartElement("content_type");
                    writer.WriteString("movie");
                    writer.WriteEndElement(); // <content_type ... />

                    writer.WriteStartElement("protocol");

                    writer.WriteStartElement("name");
                    //writer.WriteString(result.protocols[0]);
                    writer.WriteString("http"); //固定でいいんじゃね？
                    writer.WriteEndElement(); // <name ... />

                    writer.WriteStartElement("parameters");
                    writer.WriteStartElement("http_parameters");

                    writer.WriteStartElement("method");
                    writer.WriteString("GET");
                    writer.WriteEndElement(); // <method ... />

                    writer.WriteStartElement("parameters");
                    writer.WriteStartElement("http_output_download_parameters");

                    writer.WriteStartElement("file_extension");
                    //writer.WriteString("flv");
                    writer.WriteString("mp4");
                    writer.WriteEndElement(); // <file_extension ... />

                    writer.WriteStartElement("use_well_known_port");
                    writer.WriteString((result.urls[0].isWellKnownPort ? "yes" : "no"));
                    writer.WriteEndElement(); // <use_well_known_port ... />

                    writer.WriteEndElement(); // <http_output_download_parameters ... />
                    writer.WriteEndElement(); // <parameters ... />
                    writer.WriteEndElement(); // <http_parameters ... />
                    writer.WriteEndElement(); // <parameters ... />

                    writer.WriteEndElement(); // <protocol ... />

                    writer.WriteStartElement("priority");
                    writer.WriteString(result.priority.ToString());
                    writer.WriteEndElement(); // <priority ... />

                    writer.WriteStartElement("content_src_id_sets");
                    writer.WriteStartElement("content_src_id_set");
                    writer.WriteStartElement("content_src_ids");
                    writer.WriteStartElement("src_id_to_mux");

                    writer.WriteStartElement("video_src_ids");
                    foreach (string e in result.videos){
                        writer.WriteStartElement("string");
                        writer.WriteString(e);
                        writer.WriteEndElement(); // <string ... />
                    };
                    writer.WriteEndElement(); // <video_src_ids ... />

                    writer.WriteStartElement("audio_src_ids");
                    foreach (string e in result.audios)
                    {
                        writer.WriteStartElement("string");
                        writer.WriteString(e);
                        writer.WriteEndElement(); // <string ... />
                    };
                    writer.WriteEndElement(); // <audio_src_ids ... />

                    writer.WriteEndElement(); // <src_id_to_mux ... />
                    writer.WriteEndElement(); // <content_src_ids ... />
                    writer.WriteEndElement(); // <content_src_id_set ... />
                    writer.WriteEndElement(); // <content_src_id_sets ... />

                    writer.WriteStartElement("keep_method");
                    writer.WriteStartElement("heartbeat");

                    writer.WriteStartElement("lifetime");
                    writer.WriteString(result.heartbeatLifetime.ToString());
                    writer.WriteEndElement(); // <lifetime ... />

                    writer.WriteEndElement(); // <heartbeat ... />
                    writer.WriteEndElement(); // <keep_method ... />

                    writer.WriteStartElement("timing_constraint");
                    writer.WriteString("unlimited");
                    writer.WriteEndElement(); // <timing_constraint ... />

                    writer.WriteStartElement("session_operation_auth");
                    writer.WriteStartElement("session_operation_auth_by_signature");

                    writer.WriteStartElement("token");
                    writer.WriteString(result.token);
                    writer.WriteEndElement(); // <token ... />

                    writer.WriteStartElement("signature");
                    writer.WriteString(result.signature);
                    writer.WriteEndElement(); // <signature ... />

                    writer.WriteEndElement(); // <session_operation_auth_by_signature ... />
                    writer.WriteEndElement(); // <session_operation_auth ... />

                    writer.WriteStartElement("content_auth");

                    writer.WriteStartElement("auth_type");
                    writer.WriteString(result.authTypes.http);
                    writer.WriteEndElement(); // <auth_type ... />

                    writer.WriteStartElement("service_id");
                    writer.WriteString("nicovideo");
                    writer.WriteEndElement(); // <service_id ... />

                    writer.WriteStartElement("service_user_id");
                    writer.WriteString(result.serviceUserId);
                    writer.WriteEndElement(); // <service_user_id ... />

                    writer.WriteStartElement("max_content_count");
                    writer.WriteString("10");
                    writer.WriteEndElement(); // <max_content_count ... />

                    writer.WriteStartElement("content_key_timeout");
                    writer.WriteString(result.contentKeyTimeout.ToString());
                    writer.WriteEndElement(); // <content_key_timeout ... />

                    writer.WriteEndElement(); // <content_auth ... />

                    writer.WriteStartElement("client_info");

                    writer.WriteStartElement("player_id");
                    writer.WriteString(result.playerId);
                    writer.WriteEndElement(); // <player_id ... />

                    writer.WriteEndElement(); // <client_info ... />

                    writer.WriteEndElement(); // <session ... />
                }

                api_url = result.urls[0].url;
                res = network_.PostAndReadFromWebUTF8(api_url + "?_format=xml", post_str.ToString());
            }

            if (res == "")
            {
                throw new NiconicoAccessFailedException();
            }

            String video_url = "";
            String id = "";
            String post_url = "";
            String post_data = "";

            Match m = Regex.Match(res, "<content_uri>([^<]*)</content_uri>");
            if (m.Success)
            {
                video_url = m.Groups[1].Value;
            }
            else
            {
                throw new NiconicoAccessFailedException();
            }


            Match m2 = Regex.Match(res, "<id>([^<]*)</id>");
            if (m2.Success)
            {
                id = m2.Groups[1].Value;
                post_url = api_url + "/" + id + "?_format=xml&_method=PUT";
            }
            else
            {
                throw new NiconicoAccessFailedException();
            }

            Match m3 = Regex.Match(res, "<session>(.+)</session>");
            if (m3.Success)
            {
                post_data = m3.Groups[0].Value;
            }
            else
            {
                throw new NiconicoAccessFailedException();
            }

            //network_.SetContentType("video/mp4");
            //network_.SetReferer("");
            network_.SetDownloadingEventDelegate(dlg);

            // 開始時に間隔を指定する
            var timer = new System.Timers.Timer(60000);

            // Elapsedイベントにタイマー発生時の処理を設定する
            timer.Elapsed += (sender, e) =>
            {
                network_.PostAndReadFromWebUTF8(post_url, post_data);
            };

            // タイマーを開始する
            timer.Start();

            try
            {
                Thread.Sleep(1000);
                //network_.GetAndSaveToFile(video_url, save_flv_filename);
                //2024/02/26 Update marky CMAF対応
                network_.GetAndSaveToFile(video_url, save_flv_dir + video_id + ".mp4");
            }
            finally
            {
                // タイマーを停止する
                timer.Stop();

                // 資源の解放
                using (timer) { }

                network_.Reset();
            }
        }

        /// <summary>
        /// 映像、音声の利用可能なビットレートを取得する 2024/02/26 ADD marky CMAF対応
        /// </summary>
        private string GetVideoAudioRate(string data)
        {
            int index = data.IndexOf("\"media\":{\"domand\":{\"videos\":[");
            if (index == -1)
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }

            //映像最高解像度
            string video_rate = IJStringUtil.GetValueByKey(ref index, "id", data);
            if (video_rate.Equals(""))
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }

            //利用可能？
            string isAvailable = IJStringUtil.GetValueByKey(ref index, "isAvailable", data);
            if (isAvailable.Equals(""))
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }

            if (isAvailable.Equals("false"))
            {
                video_rate = IJStringUtil.GetValueByKey(ref index, "id", data);
                //映像次点解像度
                if (video_rate.Equals(""))
                {
                    throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
                }

            }

            //"audios":[{"id":"audio-aac-XXXkbps",を探す
            String str = "\"audios\"";
            if (data.IndexOf(str, index) == -1)
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }
            index = data.IndexOf(str, index);

            //音声最高解像度
            string audio_rate = IJStringUtil.GetValueByKey(ref index, "id", data);
            if (audio_rate.Equals(""))
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }

            //利用可能？（不要かも）
            isAvailable = IJStringUtil.GetValueByKey(ref index, "isAvailable", data);
            if (isAvailable.Equals(""))
            {
                throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
            }

            if (isAvailable.Equals("false"))
            {
                audio_rate = IJStringUtil.GetValueByKey(ref index, "id", data);
                //音声次点解像度
                if (audio_rate.Equals(""))
                {
                    throw new NiconicoAccessFailedException("ファイル情報の取得に失敗しました。仕様が変わった可能性があります。");
                }

            }

            return "\"" + video_rate + "\",\"" + audio_rate + "\"";
        }

        /// <summary>
        /// 動画をダウンロードする 2024/02/26 ADD marky CMAF対応
        /// </summary>
        /// <param name="video_id">ダウンロードする動画ID</param>
        /// <param name="save_dir">ダウンロードしたファイルを保存するフォルダ名</param>
        /// <param name="dlg">ダウンロード最中に呼び出されるコールバックメソッド。null でもよい。</param>
        /// <param name="TrackId">トラックID</param>
        /// <param name="RightKey">ライトキー</param>
        /// <param name="rate">映像、音声の取得ビットレート</param>
        /// <param name="ffmpeg_dir">ffmpeg.exeの保存パス名</param>
        /// <exception cref="System.Exception">erer</exception>
        public void DownloadAndSaveFlv_new(string video_id, string save_dir, IJNetwork.DownloadingEventDelegate dlg, string TrackId, string RightKey, string rate, string ffmpeg_dir)
        {

            if (!File.Exists(ffmpeg_dir))
            {
                throw new NiconicoAccessFailedException("FFmpeg のパスの設定が正しくありません。");
            }

            network_.AddCustomHeader("Access-Control-Request-Headers: content-type,x-access-right-key,x-frontend-id,x-frontend-version,x-request-with");
            network_.AddCustomHeader("Access-Control-Request-Method: POST");
            network_.AddCustomHeader("Origin: " + nicovideo_uri_);
            network_.SetReferer(nicovideo_uri_);
            string video_url = "https://nvapi.nicovideo.jp/v1/watch/" + video_id + "/access-rights/hls?actionTrackId=" + TrackId;

            string str = network_.OptionsToWeb(video_url);
            network_.Reset();

            if (!str.Equals("OK"))
            {
                throw new NiconicoAccessFailedException("動画ファイルの取得に失敗しました。");
            }

            network_.SetContentTypeJSON();
            network_.AddCustomHeader("Origin: " + nicovideo_uri_);
            network_.SetReferer(nicovideo_uri_);
            network_.AddCustomHeader("X-Access-Right-Key: " + RightKey);
            network_.AddCustomHeader("X-Frontend-Id: 6");
            network_.AddCustomHeader("X-Frontend-Version: 0");
            network_.AddCustomHeader("X-Request-With: " + nicovideo_uri_);
            //string json = "{\"outputs\":[[\"video-h264-720p\",\"audio-aac-128kbps\"]]}";    //ひとまず固定値で
            //string json = "{\"outputs\":[[\"video-h264-1080p\",\"audio-aac-192kbps\"]]}";    //ひとまず固定値で
            string json = "{\"outputs\":[[" + rate + "]]}";    //取得ビットレート
            try
            {
                str = network_.PostAndReadFromWebUTF8(video_url, json);
            }
            catch (Exception e)
            {
                throw new NiconicoAccessFailedException("動画ファイルの取得に失敗しました。" + e.Message);
            }
            finally
            {
                network_.Reset();
            }

            if (str.IndexOf("\"status\":201") < 0) // "status":201 Created
            {
                throw new NiconicoAccessFailedException("動画ファイルの取得に失敗しました。");
            }

            int index = 0;
            //"contentUrl": "https://delivery.domand.nicovideo.jp/hlsbid/xxxxx/playlists/variants/xxxxx.m3u8?session=XXXXX&Policy=XXXX&Key-Pair-Id=XXXXXX",
            string contentUrl = IJStringUtil.GetValueByKey(ref index, "contentUrl", str);
            if (contentUrl.Equals(""))
            {
                throw new NiconicoAccessFailedException("動画ファイルの取得に失敗しました。");
            }
            //network_.GetAndSaveToFile(contentUrl, save_dir + video_id + ".m3u8");
            string m3u8File = network_.GetAndReadFromWebUTF8(contentUrl); // xxxxx.m3u8
            string[] m3u8Pages = m3u8File.Split('\n');
            index = m3u8Pages[3].LastIndexOf("URI=\"") + 5;
            string m3u8aURL = m3u8Pages[3].Substring(index, m3u8Pages[3].Length - 1 - index); //URI="audio-aac-192kbps.m3u8"
            string m3u8vURL = m3u8Pages[5];

            string m3u8aFile = network_.GetAndReadFromWebUTF8(m3u8aURL); // audio-aac-192kbps.m3u8
            string[] m3u8aPages = m3u8aFile.Split('\n');

            index = ("#EXT-X-MAP:URI=\"").Length;
            string initaURL = m3u8aPages[5].Substring(index, m3u8aPages[5].Length - 1 - index); //#EXT-X-MAP:URI="https://XXX/init1.cmfa?session=XXX"
            string initaFile = initaURL.Substring(0, initaURL.LastIndexOf('?'));
            index = initaFile.LastIndexOf('/') + 1;
            initaFile = initaFile.Substring(index, initaFile.Length - index);  //init1.cmfa

            index = ("#EXT-X-KEY:METHOD=AES-128,URI=\"").Length;
            string keyaURL = m3u8aPages[6].Substring(index, m3u8aPages[6].LastIndexOf('\"') - index); //#EXT-X-KEY:METHOD=AES-128,URI="https://XXX/audio-aac-192kbps.key?session=XXX",IV=XXX
            string keyaFile = keyaURL.Substring(0, keyaURL.LastIndexOf('?'));
            index = keyaFile.LastIndexOf('/') + 1;
            keyaFile = keyaFile.Substring(index, keyaFile.Length - index);  //audio-aac-192kbps.key

            int m3u8aCount = (m3u8aPages.Length - 9) / 2; //ヘッダー7行、フッター2行を除き、#EXTINF行を除く
            string[] cmfaFile = new String[m3u8aCount];

            string m3u8vFile = network_.GetAndReadFromWebUTF8(m3u8vURL); // video-h264-720p.m3u8
            string[] m3u8vPages = m3u8vFile.Split('\n');

            index = ("#EXT-X-MAP:URI=\"").Length;
            string initvURL = m3u8vPages[5].Substring(index, m3u8vPages[5].Length - 1 - index); //#EXT-X-MAP:URI="https://XXX/init1.cmfv?session=XXX"
            string initvFile = initvURL.Substring(0, initvURL.LastIndexOf('?'));
            index = initvFile.LastIndexOf('/') + 1;
            initvFile = initvFile.Substring(index, initvFile.Length - index);  //init1.cmfv

            index = ("#EXT-X-KEY:METHOD=AES-128,URI=\"").Length;
            string keyvURL = m3u8vPages[6].Substring(index, m3u8vPages[6].LastIndexOf('\"') - index); //#EXT-X-KEY:METHOD=AES-128,URI=""https://XXX/video-h264-720p.key?session=XXX",IV=XXX
            string keyvFile = keyvURL.Substring(0, keyvURL.LastIndexOf('?'));
            index = keyvFile.LastIndexOf('/') + 1;
            keyvFile = keyvFile.Substring(index, keyvFile.Length - index);  //video-h264-720p.key

            int m3u8vCount = (m3u8vPages.Length - 9) / 2; //ヘッダー7行、フッター2行を除き、#EXTINF行を除く
            string[] cmfvFile = new String[m3u8vCount];

            //network_.SetDownloadingEventDelegate(dlg);

            //一時フォルダパス
            //string temp_dir = save_dir + "temp\\";
            string temp_dir = Environment.GetEnvironmentVariable("temp") + "\\" + video_id + "\\";

            try
            {
                Thread.Sleep(1000);

                //一時フォルダ作成
                Directory.CreateDirectory(temp_dir);

                string cmf = "";

                //audio-aac-192kbps.key
                network_.GetAndSaveToFile(initaURL, temp_dir + initaFile);
                network_.GetAndSaveToFile(keyaURL, temp_dir + keyaFile);

                for (int i = 0; i <= m3u8aCount - 1; i++)
                {
                    cmf = m3u8aPages[i * 2 + 8];
                    cmfaFile[i] = cmf.Substring(0, cmf.LastIndexOf('?'));   //https://XXX/1.cmfa?session=XXX
                    index = cmfaFile[i].LastIndexOf('/') + 1;
                    cmfaFile[i] = cmfaFile[i].Substring(index, cmfaFile[i].Length - index);  //1.cmfa

                    network_.GetAndSaveToFile(cmf, temp_dir + cmfaFile[i]);

                    bool is_cancel = false;
                    dlg(ref is_cancel, i + 1, m3u8aCount + m3u8vCount, true);
                    if (is_cancel)
                    {
                        //一時ファイル削除
                        Directory.Delete(temp_dir, true);
                        dlg(ref is_cancel, 0, 0, true);
                    }
                }

                // video-h264-720p.m3u8
                network_.GetAndSaveToFile(initvURL, temp_dir + initvFile);
                network_.GetAndSaveToFile(keyvURL, temp_dir + keyvFile);

                for (int i = 0; i <= m3u8vCount - 1; i++)
                {
                    cmf = m3u8vPages[i * 2 + 8];
                    cmfvFile[i] = cmf.Substring(0, cmf.LastIndexOf('?'));   //https://XXX/1.cmfv?session=XXX
                    index = cmfvFile[i].LastIndexOf('/') + 1;
                    cmfvFile[i] = cmfvFile[i].Substring(index, cmfvFile[i].Length - index);  //1.cmfv

                    network_.GetAndSaveToFile(cmf, temp_dir + cmfvFile[i]);

                    bool is_cancel = false;
                    dlg(ref is_cancel, m3u8aCount + i + 1, m3u8aCount + m3u8vCount, true);
                    if (is_cancel)
                    {
                        //一時ファイル削除
                        Directory.Delete(temp_dir, true);
                        dlg(ref is_cancel, 0,0, true);
                    }
                }

                // xxxxx.m3u8
                StringBuilder buff = new StringBuilder();
                for (int i = 0; i <= m3u8Pages.Length - 1; i++)
                {
                    switch (i)
                    {
                        case 3:
                            buff.Append(m3u8Pages[i].Replace(m3u8aURL, temp_dir + video_id + "a.m3u8").Replace("\\", "\\\\") + "\r\n");
                            break;
                        case 5:
                            buff.Append(temp_dir + video_id + "v.m3u8" + "\r\n");
                            break;
                        default:
                            buff.Append(m3u8Pages[i] + "\r\n");
                            break;
                    }
                    
                }
                IJFile.WriteUTF8_NoBOM(temp_dir + video_id + ".m3u8", buff.ToString());

                // audio-aac-192kbps.m3u8
                buff = new StringBuilder();
                for (int i = 0; i <= m3u8aPages.Length - 1; i++)
                {
                    switch (i)
                    {
                        case int n when n <= 4:
                            buff.Append(m3u8aPages[i] + "\r\n");
                             break;
                        case 5:
                            buff.Append(m3u8aPages[i].Replace(initaURL, temp_dir + initaFile).Replace("\\", "\\\\") + "\r\n");
                            break;
                        case 6:
                            buff.Append(m3u8aPages[i].Replace(keyaURL, temp_dir + keyaFile).Replace("\\", "\\\\") + "\r\n");
                            break;
                        default:
                            if ((i - 6) % 2 == 0 && !m3u8aPages[i].Equals(""))
                            {
                                buff.Append(temp_dir + cmfaFile[(i - 6) / 2 - 1] + "\r\n");
                            }
                            else
                            {
                                buff.Append(m3u8aPages[i] + "\r\n");
                            }
                            break;
                    }
                }
                IJFile.WriteUTF8_NoBOM(temp_dir + video_id + "a.m3u8", buff.ToString());

                // video-h264-720p.m3u8
                buff = new StringBuilder();
                for (int i = 0; i <= m3u8vPages.Length - 1; i++)
                {
                    switch (i)
                    {
                        case int n when n <= 4:
                            buff.Append(m3u8vPages[i] + "\r\n");
                            break;
                        case 5:
                            buff.Append(m3u8vPages[i].Replace(initvURL, temp_dir + initvFile).Replace("\\", "\\\\") + "\r\n");
                            break;
                        case 6:
                            buff.Append(m3u8vPages[i].Replace(keyvURL, temp_dir + keyvFile).Replace("\\", "\\\\") + "\r\n");
                            break;
                        default:
                            if ((i - 6) % 2 == 0 && !m3u8vPages[i].Equals(""))
                            {
                                buff.Append(temp_dir + cmfvFile[(i - 6) / 2 - 1] + "\r\n");
                            }
                            else
                            {
                                buff.Append(m3u8vPages[i] + "\r\n");
                            }
                            break;
                    }
                }
                IJFile.WriteUTF8_NoBOM(temp_dir + video_id + "v.m3u8", buff.ToString());

                //ffmpeg変換
                nicorank.FFmpegAppPath ffPath = new nicorank.FFmpegAppPath(ffmpeg_dir,"");
                nicorank.FFmpeg ffm = new nicorank.FFmpeg(ffPath, "");
                ffm.TranslateToMp4(temp_dir + video_id + ".m3u8", save_dir + video_id + ".mp4");

            }
            catch (Exception e)
            {
                bool is_cancel = true;
                dlg(ref is_cancel, 0, 0, true);
                throw new Exception(e.Message);
            }
            finally
            {
                //一時ファイル削除
                Directory.Delete(temp_dir, true);
                network_.Reset();
            }

            return;
        }

        public string PostSessionAPI(string api_url, string post)
        {
            return network_.PostAndReadFromWebUTF8(api_url, post);
        }

        /// <summary>
        /// 動画のHTMLページを取得する
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>取得したHTML</returns>
        public string GetVideoPage(string video_id)
        {
            CheckCookie();
            return network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/watch/" + video_id);
        }

        ///// <summary>
        ///// 動画のHTMLページを取得する2021/09/15 DEL marky マイリスト登録ページ(/mylist_add/video/)の廃止に対応
        ///// </summary>
        ///// <param name="video_id">動画ID</param>
        ///// <returns>取得したHTML</returns>
        //public string GetMylistAddPage(string video_id)
        //{
        //    CheckCookie();
        //    return network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/mylist_add/video/" + video_id);
        //}

        /// <summary>
        /// getflv API で動画情報を取得
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>getflv API の結果文字列</returns>
        public string GetVideoInfo(string video_id)
        {
            CheckCookie();
            string postfix_and = "";
            string postfix_q = "";
            if (video_id.StartsWith("nm"))
            {
                postfix_and = "&as3=1";
                postfix_q = "?as3=1";
            }
            string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/api/getflv/" + video_id + postfix_q);
            if (str.StartsWith("closed=1&done=true"))
            {
                Thread.Sleep(500);
                str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/api/getflv?v=" + video_id + postfix_and);
            }
            else if (str.IndexOf("error=invalid_v1") >= 0) // so で始まるIDの動画用
            {
                // 連続アクセス規制回避
                Thread.Sleep(3 * 1000);
                string thread_id = GetThreadId(video_id);
                if (thread_id != "")
                {
                    Thread.Sleep(500);
                    return network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/api/getflv?v=" + thread_id + postfix_and);
                }
            }
            return str;
        }

        /// <summary>
        /// 動画IDから thread id を取得
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>thread id（取得できなければ空文字）</returns>
        public string GetThreadId(string video_id)
        {
            string new_uri = network_.GetLocationHeader(nicovideo_uri_ + "/watch/" + video_id);
            int last_separate = new_uri.LastIndexOf('/');
            if (last_separate >= 0)
            {
                return new_uri.Substring(last_separate + 1); // thread id を取得
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// getthumbinfo API で動画情報を取得
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>getthumbinfo API の結果文字列</returns>
        public string GetThumbInfo(string video_id)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                return network_.GetAndReadFromWebUTF8(nicovideo_ext_uri_ + "/api/getthumbinfo/" + video_id);
            }
            finally
            {
                network_.Reset();
            }
        }

        /// <summary>
        /// ext.nicovideo.jp/thumb からサムネイルページを取得
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>取得した文字列</returns>
        public string GetExtThumb(string video_id)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                return network_.GetAndReadFromWebUTF8(nicovideo_ext_uri_ + "/thumb/" + video_id);
            }
            finally
            {
                network_.Reset();
            }
        }

        /// <summary>
        /// サムネイルをダウンロードしてファイルに保存
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="filename">ファイル名</param>
        public void SaveThumbnailWithVideoId(string video_id, string filename)
        {
            CheckCookie();
            string url = "";
            // 2019/11/30 Update marky 公式動画からスレッドIDへのリダイレクトが不要に
            //if (video_id.StartsWith("so"))
            //{
            //    video_id = GetThreadId(video_id);
            //    if (video_id == "")
            //    {
            //        throw new FormatException("サムネイルのURLの取得に失敗しました。");
            //    }
            //}
            //if ('0' <= video_id[0] && video_id[0] <= '9')
            //{
            //    string html = GetExtThumb(video_id);
            //    Match m = Regex.Match(html, "<img alt=\"\" src=\"([^\"]*)\" class=\"video_img\"\\>");
            //    if (m.Success)
            //    {
            //        url = m.Groups[1].Value;
            //    }
            //}
            //else
            //{
                string xml = GetThumbInfo(video_id);
                Match m = Regex.Match(xml, "<thumbnail_url>([^<]*)</thumbnail_url>");
                if (m.Success)
                {
                    url = m.Groups[1].Value;
                }
            //}
            
            if (url != "")
            {
                Thread.Sleep(500);
                SaveThumbnail(url, filename);
            }
            else
            {
                throw new FormatException("サムネイルのURLの取得に失敗しました。");
            }
        }

        /// <summary>
        /// サムネイルをダウンロードしてファイルに保存
        /// </summary>
        /// <param name="video_id">サムネイルURL</param>
        /// <param name="filename">ファイル名</param>
        public void SaveThumbnail(string url, string filename)
        {
            CheckCookie();
            network_.GetAndSaveToFile(url, filename);
        }

        /// <summary>
        /// マイ動画ページを取得する
        /// </summary>
        /// <returns>取得したHTML</returns>
        public string GetMyVideoPage()
        {
            CheckCookie();
            return network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/my/video");
        }

        ///// <summary>
        ///// 指定したワードでキーワード検索を行う 2020/02/16 Del marky
        ///// </summary>
        ///// <param name="search_word">検索ワード</param>
        ///// <param name="page">ページ数（1から始まる）</param>
        ///// <param name="sort_method">検索時の並べ方指定</param>
        ///// <param name="order">昇順 or 降順</param>
        ///// <param name="is_use_api">API利用有無</param>
        ///// <returns>検索結果のHTML</returns>
        //public string GetSearchKeyword(string search_word, int page, SearchSortMethod sort_method, SearchOrder order, bool is_use_api)
        //{
        //    if (is_use_api)
        //    {
        //        return GetSearchKeywordOrTagByAPI(search_word, page, sort_method, order, false);
        //    }
        //    else
        //    {
        //        return GetSearchKeywordOrTag(search_word, page, sort_method, order, false);
        //    }
        //}

        ///// <summary>
        ///// 指定したワードでタグ検索を行う 2020/02/16 Del marky
        ///// </summary>
        ///// <param name="search_word">検索ワード</param>
        ///// <param name="page">ページ数（1から始まる）</param>
        ///// <param name="sort_method">検索時の並べ方指定</param>
        ///// <param name="order">昇順 or 降順</param>
        ///// <param name="is_use_api">API利用有無</param>
        ///// <returns>検索結果のHTML</returns>
        //public string GetSearchTag(string search_word, int page, SearchSortMethod sort_method, SearchOrder order, bool is_use_api)
        //{
        //    if (is_use_api)
        //    {
        //        return GetSearchKeywordOrTagByAPI(search_word, page, sort_method, order, true);
        //    }
        //    else
        //    {
        //        return GetSearchKeywordOrTag(search_word, page, sort_method, order, true);
        //    }
        //}

        /// <summary>
        /// 指定したワードでAPI検索を行う 2019/07/06 ADD marky
        /// </summary>
        /// <param name="search_word">検索ワード</param>
        /// <param name="option">検索時のオプションクラス</param>
        /// <returns>検索結果のjson</returns>
        public string GetSearchByAPI(string search_word, SearchingTagOption option)
        {
            return GetSearchKeywordOrTagByAPI(search_word, option);
        }

        /// <summary>
        /// 指定したワードでHTML検索を行う 2020/02/16 ADD marky
        /// </summary>
        /// <param name="search_word">検索ワード</param>
        /// <param name="page">ページ数（1から始まる）</param>
        /// <param name="option">検索時のオプションクラス</param>
        /// <param name="is_tag">タグ検索有無</param>
        /// <returns>検索結果のHTML</returns>
        public string GetSearchByHTML(string search_word, int page, SearchingTagOption option, bool is_tag)
        {
            return GetSearchKeywordOrTag(search_word, page, option, is_tag);
        }

        /// <summary>
        /// 新着動画情報を取得する
        /// </summary>
        /// <param name="page">ページ数（1から始まる）</param>
        /// <returns>取得結果のHTML</returns>
        public string GetNewArrival(int page)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/newarrival" +
                    (page >= 2 ? "?page=" + page : ""));
                CheckDenied(str);
                return str;
            }
            finally
            {
                network_.Reset();
            }
        }

        /// <summary>
        /// 指定したマイリストIDのHTMLページを取得する
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <returns>取得結果のHTML</returns>
        public string GetMylistHtml(string mylist_id, bool is_rss)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/mylist/" + mylist_id + (is_rss ? "?rss=2.0&numbers=1" : ""));
                CheckDenied(str);
                return str;
            }
            finally
            {
                network_.Reset();
            }
        }

        // 2023/01/21 ADD marky
        /// <summary>
        /// 指定したマイリストIDのJsonデータから動画リストを取得する（ログインユーザ作成分のみ取得可能）
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <param name="mylist_video">マイリストの動画リスト</param>
        public void GetMylistJson(string mylist_id, List<Video> mylist_video)
        {
            CheckCookie();

            int page = 1;
            bool isNext = true;

            while (isNext)
            {
                network_.AddCustomHeader("Access-Control-Request-Headers: x-frontend-id,x-frontend-version,x-niconico-language");
                network_.AddCustomHeader("Access-Control-Request-Method: GET");
                //network_.SetReferer(nicovideo_uri_ + "/my/mylist/" + "?ref=pc_mypage_menu");
                network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                string str = network_.OptionsToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "?pageSize=100&page=" + page.ToString());
                network_.Reset();

                if (str.Equals("OK"))
                {
                    network_.AddCustomHeader("X-Frontend-Id: 6");
                    network_.AddCustomHeader("X-Frontend-Version: 0");
                    network_.AddCustomHeader("X-Niconico-Language: ja-jp");
                    //network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                    try
                    {
                        str = network_.GetAndReadFromWebUTF8("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "?pageSize=100&page=" + page.ToString());
                    }
                    catch { str = ""; }
                    finally
                    {
                        network_.Reset();
                    }

                    if (!str.Equals(""))
                    {
                        NicoListManager.ParseMylistJson(str, mylist_video, ref isNext);
                        page++;
                    }
                    else { break; }
                }
                else { break; }
            }
        }

        /// <summary>
        /// マイページのマイリスト編集画面からマイリストの情報を取得する
        /// </summary>
        /// <returns>マイリスト情報リスト</returns>
        public List<MylistInfo> GetMylistInfoListFromMypage()
        {
            CheckCookie();
            string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/my/mylist");
            CheckDenied(str);

            List<MylistInfo> mylist_info_list = new List<MylistInfo>();

            Match mylist_match = Regex.Match(str, @"MylistGroup.preload\(\[(.+)\]\);");

            if (mylist_match.Success)
            {
                MatchCollection list_info_matches = Regex.Matches(mylist_match.Value,
                    @"{""id"":""(.+?)"",""user_id"":"".*?"",""name"":""(.*?)"",""description"":""(.*?)"","
                    + @"""public"":""(.*?)"",""default_sort"":"".*?"",""create_time"":.*?,"
                    + @"""update_time"":.*?,""sort_order"":"".*?"",""icon_id"":"".*?""}",
                    RegexOptions.Singleline);

                foreach (Match m in list_info_matches)
                {
                    MylistInfo mylist_info = new MylistInfo();
                    mylist_info.mylist_id = m.Groups[1].Value;
                    mylist_info.title = Regex.Unescape(Uri.UnescapeDataString(m.Groups[2].Value));
                    mylist_info.number_of_item = 0;
                    mylist_info.is_public = (m.Groups[4].Value == "1");
                    mylist_info.description = Regex.Unescape(Uri.UnescapeDataString(m.Groups[3].Value));
                    mylist_info_list.Add(mylist_info);
                }
            }
            return mylist_info_list;
        }

        /// <summary>
        /// 自分のマイリストページ（トップページ）を取得
        /// </summary>
        /// <returns>取得したHTML</returns>
        public string GetMylistPageFromMyPage()
        {
            CheckCookie();
            return network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/my/mylist");
        }

        ///// <summary>
        ///// マイリストを新規作成する。
        ///// </summary>
        ///// <param name="title">新規作成するマイリストのタイトル</param>
        ///// <returns>サーバからの応答</returns>
        //public string MakeNewMylistGroup(string title)
        //{
        //    CheckCookie();

        //    string html = GetMylistPageFromMyPage();

        //    string token = GetToken(html);
            
        //    Thread.Sleep(1000);

        //    return MakeNewMylistGroup(title, token);
        //}

        //public string MakeNewMylistGroup(string title, string token)
        //{
        //    CheckCookie();

        //    string post_data = IJNetwork.ConstructPostData(
        //        "name",         title,
        //        "default_sort", "1",
        //        "token",        token);
        //    return network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/api/mylistgroup/add", post_data);
        //}

        // 2021/12/18 Update marky マイリストAPI(/api/mylistgroup/)の廃止に対応
        /// <summary>
        /// マイリストを新規作成する。
        /// </summary>
        /// <param name="is_setting_public">マイリストを公開するかどうか</param>
        /// <param name="title">マイリストタイトル</param>
        /// <param name="description">マイリスト説明文</param>
        /// <param name="order">マイリストの項目の並び順</param>
        /// <param name="mylist_id">マイリストID</param>
        public void MakeNewMylistGroup(bool is_setting_public, string title, string description, int order, out string mylist_id)
        {
            CheckCookie();

            network_.AddCustomHeader("Access-Control-Request-Headers: x-frontend-id,x-frontend-version,x-niconico-language,x-request-with");
            network_.AddCustomHeader("Access-Control-Request-Method: POST");
            network_.AddCustomHeader("Origin: " + nicovideo_uri_);
            network_.SetReferer(nicovideo_uri_ + "/my/mylist");
            string str = network_.OptionsToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists");
            network_.Reset();

            if (str.Equals("OK"))
            {
                network_.SetReferer(nicovideo_uri_ + "/my/mylist");
                network_.AddCustomHeader("X-Frontend-Id: 6");
                network_.AddCustomHeader("X-Frontend-Version: 0");
                network_.AddCustomHeader("X-Niconico-Language: ja-jp");
                network_.AddCustomHeader("X-Request-With: " + nicovideo_uri_);
                network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                try
                {
                    //string post_data = "";
                    //string url_option = mylist_sort[order / 2];

                    //post_data = post_data + "?name=" + Uri.EscapeDataString(title);
                    //post_data = post_data + "&isPublic=" + (is_setting_public ? "true" : "false");
                    //post_data = post_data + "&defaultSortKey=" + url_option;
                    //post_data = post_data + "&defaultSortOrder=" + (order < 6 ? (order % 2 == 0 ? "asc" : "desc") : (order % 2 == 0 ? "desc" : "asc"));
                    //post_data = post_data + "&description=" + Uri.EscapeDataString(description);

                    string post_data = IJNetwork.ConstructPostData(
                        "name", title,
                        "isPublic", (is_setting_public ? "true" : "false"),
                        "defaultSortKey", mylist_sort[order / 2],
                        "defaultSortOrder", (order < 6 ? (order % 2 == 0 ? "asc" : "desc") : (order % 2 == 0 ? "desc" : "asc")),
                        "description", description);

                    str = network_.PostAndReadFromWebUTF8("https://nvapi.nicovideo.jp/v1/users/me/mylists", post_data);
                }
                finally
                {
                    network_.Reset();
                }

                if (str.IndexOf("\"status\":200") >= 0) // "status":200 OK
                {
                    //Match m = Regex.Match(str, "{\"mylistId\":(.*)}}");
                    // 2022/08/07 Update marky 階層化に対応 {\"mylistId\":73556289,\"mylist\":{\"id\":73556289,\"name\":・・・
                    Match m = Regex.Match(str, "{\"mylistId\":([0-9]+)\x2C");
                    if (m.Success)
                    {
                        mylist_id = m.Groups[1].Value;
                    }
                    else
                    {
                        throw new NiconicoAddingMylistFailedException("マイリストの新規作成に失敗しました。マイリストIDの取得に失敗しました。");
                    }
                    return;
                }
                else
                {
                    throw new NiconicoAddingMylistFailedException("マイリストの新規作成に失敗しました。サーバからエラーが返ってきました。");
                }
            }

            throw new NiconicoAddingMylistFailedException("マイリストの新規作成に失敗しました。サーバからエラーが返ってきました。");

        }

        ///// <summary>
        ///// マイリストの冒頭の説明文などを更新
        ///// </summary>
        ///// <param name="mylist_id">マイリストID</param>
        ///// <param name="is_setting_public">マイリストを公開するかどうか</param>
        ///// <param name="title">マイリストタイトル</param>
        ///// <param name="description">マイリスト説明文</param>
        ///// <param name="order">マイリストの項目の並び順</param>
        ///// <param name="color">マイリストの色</param>
        ///// <returns>サーバからの応答</returns>
        //public string UpdateMylistGroup(string mylist_id, bool is_setting_public, string title, string description, int order, int color)
        //{
        //    CheckCookie();

        //    string html = GetMylistPageFromMyPage();

        //    string token = GetToken(html);

        //    Thread.Sleep(1000);
        //    return UpdateMylistGroup(mylist_id, is_setting_public, title, description, order, color, token);
        //}

        ///// <summary>
        ///// マイリストの冒頭の説明文などを更新
        ///// </summary>
        ///// <param name="mylist_id">マイリストID</param>
        ///// <param name="is_setting_public">マイリストを公開するかどうか</param>
        ///// <param name="title">マイリストタイトル</param>
        ///// <param name="description">マイリスト説明文</param>
        ///// <param name="order">マイリストの項目の並び順</param>
        ///// <param name="color">マイリストの色</param>
        ///// <param name="token">トークン</param>
        ///// <returns>サーバからの応答</returns>
        //public string UpdateMylistGroup(string mylist_id, bool is_setting_public, string title, string description, int order, int color, string token)
        //{
        //    CheckCookie();

        //    string post_data = IJNetwork.ConstructPostData(
        //        "group_id",     mylist_id,
        //        "name",         title,
        //        "description",  description,
        //        "public",       (is_setting_public ? "1" : "0"),
        //        "default_sort", order.ToString(),
        //        "icon_id",      color.ToString(),
        //        "token",        token);

        //    return network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/api/mylistgroup/update", post_data);
        //}

        // 2021/12/18 Update marky マイリストAPI(/api/mylistgroup/)の廃止に対応
        /// <summary>
        /// マイリストの冒頭の説明文などを更新
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <param name="is_setting_public">マイリストを公開するかどうか</param>
        /// <param name="title">マイリストタイトル</param>
        /// <param name="description">マイリスト説明文</param>
        /// <param name="order">マイリストの項目の並び順</param>
        public string UpdateMylistGroup(string mylist_id, bool is_setting_public, string title, string description, int order)
        {
            CheckCookie();

            network_.AddCustomHeader("Access-Control-Request-Headers: x-frontend-id,x-frontend-version,x-niconico-language,x-request-with");
            network_.AddCustomHeader("Access-Control-Request-Method: PUT");
            network_.AddCustomHeader("Origin: " + nicovideo_uri_);
            network_.SetReferer(nicovideo_uri_ + "/my/mylist/" + mylist_id);
            string str = network_.OptionsToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id);
            network_.Reset();

            if (str.Equals("OK"))
            {
                network_.SetReferer(nicovideo_uri_ + "/my/mylist/" + mylist_id);
                network_.AddCustomHeader("X-Frontend-Id: 6");
                network_.AddCustomHeader("X-Frontend-Version: 0");
                network_.AddCustomHeader("X-Niconico-Language: ja-jp");
                network_.AddCustomHeader("X-Request-With: " + nicovideo_uri_);
                network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                try
                {
                    string put_data = IJNetwork.ConstructPostData(
                        "name", title,
                        "isPublic", (is_setting_public ? "true" : "false"),
                        "defaultSortKey", mylist_sort[order / 2],
                        "defaultSortOrder", (order < 6 ? (order % 2 == 0 ? "asc" : "desc") : (order % 2 == 0 ? "desc" : "asc")),
                        "description", description);

                    str = network_.PutToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id, put_data);
                }
                finally
                {
                    network_.Reset();
                }

                if (str.Equals("OK"))
                {
                    return str;
                }
                else
                {
                    throw new NiconicoAddingMylistFailedException("マイリストの更新に失敗しました。サーバからエラーが返ってきました。");
                }
            }
            throw new NiconicoAddingMylistFailedException("マイリストの更新に失敗しました。サーバからエラーが返ってきました。");
        }

        // 2021/12/18 Delete marky マイリストAPI(/api/mylistgroup/)の廃止に対応
        //public string MakeNewAndUpdateMylistGroup(bool is_setting_public, string title, string description, int order, int color, out string mylist_id)
        //{
        //    CheckCookie();

        //    string html = GetMylistPageFromMyPage();

        //    string token = GetToken(html);

        //    Thread.Sleep(1000);
        //    string str = MakeNewMylistGroup(title, token);

        //    if (str.IndexOf("\"status\":\"ok\"") >= 0)
        //    {
        //        int index = str.IndexOf("\"id\"");
        //        index = str.IndexOf(':', index) + 1;
        //        int end = str.IndexOf(',', index);
        //        mylist_id = str.Substring(index, end - index);
        //        Thread.Sleep(1000);
        //        str = UpdateMylistGroup(mylist_id, is_setting_public, title, description, order, color, token);
        //    }
        //    else
        //    {
        //        mylist_id = "";
        //    }

        //    return str;
        //}

        ///// <summary>
        ///// 指定した動画をマイリストに加える。token を取得するため、最初に動画のページを取りに行く。
        ///// </summary>
        ///// <param name="mylist_id">マイリストID</param>
        ///// <param name="video_id">動画ID</param>
        ///// <returns>サーバからの応答</returns>
        //public string AddMylist(string mylist_id, string video_id)
        //{
        //    CheckCookie();
        //    string html = GetMylistAddPage(video_id);

        //    Match match = Regex.Match(html, "<input type=\"hidden\" name=\"item_id\" value=\"([0-9]+)\">");

        //    if (!match.Success)
        //    {
        //        if (html.IndexOf("このアイテムはマイリストに登録できません。") >= 0)
        //        {
        //            throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。このアイテムはマイリストに登録できません。");
        //        }
        //        else
        //        {
        //            throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。html の解析ができませんでした。");
        //        }
        //    }

        //    string item_id = match.Groups[1].Value;

        //    //--- 2014/10/19 UPDATE marky マイリスト追加の仕様変更対応
        //    //string token = GetToken(html);
        //    string token = GetTokenAddPage(html);
        //    //---

        //    Thread.Sleep(500);
        //    string str = AddMylist(mylist_id, item_id, "", token);

        //    if (str.IndexOf("\"code\":\"EXIST\"") >= 0)
        //    {
        //        throw new NiconicoAddingMylistExistException();
        //    }
        //    if (str.IndexOf("\"status\":\"ok\"") < 0)
        //    {
        //        throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。サーバからエラーが返ってきました。");
        //    }
        //    return str;
        //}

        ///// <summary>
        ///// 指定した動画をマイリストに加える
        ///// </summary>
        ///// <param name="mylist_id">マイリストID</param>
        ///// <param name="item_id">アイテムID（thread ID？）</param>
        ///// <param name="token">token（動画追加ページのHTMLに書かれている認証キーみたいなもの）</param>
        ///// <returns>サーバからの応答</returns>
        //public string AddMylist(string mylist_id, string item_id, string description, string token)
        //{
        //    CheckCookie();
        //    string post_data = IJNetwork.ConstructPostData(
        //        "group_id",    mylist_id,
        //        "item_type",   "0",
        //        "item_id",     item_id,
        //        "description", description,
        //        "token",       token);
        //    string str = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/api/mylist/add", post_data);
        //    return str;
        //}
        // 2021/09/15 Update marky マイリスト登録ページ(/mylist_add/video/)の廃止に対応
        /// <summary>
        /// 指定した動画をマイリストに加える。
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <param name="video_id">動画ID</param>
        public void AddMylist(string mylist_id, string video_id)
        {
            CheckCookie();

            network_.AddCustomHeader("Access-Control-Request-Headers: x-frontend-id,x-frontend-version,x-niconico-language,x-request-with");
            network_.AddCustomHeader("Access-Control-Request-Method: POST");
            //network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            network_.AddCustomHeader("Origin: " + nicovideo_uri_);
            string str = network_.OptionsToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "/items?itemId=" + video_id + "&description=");
            network_.Reset();

            if (str.Equals("OK"))
            {
                network_.AddCustomHeader("X-Frontend-Id: 6");
                network_.AddCustomHeader("X-Frontend-Version: 0");
                network_.AddCustomHeader("X-Request-With: " + nicovideo_uri_);
                //network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
                //network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                try
                {
                    str = network_.PostAndReadFromWebUTF8("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "/items?itemId=" + video_id + "&description=", "");
                }
                finally
                {
                    network_.Reset();
                }

                if (str.IndexOf("\"status\":201") >=0) // "status":201 Created
                {
                    return;
                }else if (str.IndexOf("\"status\":200") >= 0) // "status":200 OK
                {
                    throw new NiconicoAddingMylistExistException();
                }
            }

            throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。サーバからエラーが返ってきました。");
        }



        /// <summary>
        /// マイリストの個々の動画の説明文を更新する。1件だけではなくて、同じマイリスト内の複数の動画に対して説明文を更新できる。
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <param name="video_id_list">動画IDのリスト</param>
        /// <param name="description_list">説明文のリスト。動画IDのリストと対応がとれている必要がある</param>
        /// <param name="wait_millisecond">1件追加した後の待ち時間</param>
        public void UpdateMylistDescription(string mylist_id, List<string> video_id_list,
            List<string> description_list, int wait_millisecond)
        {
            wait_millisecond_ = wait_millisecond;
            UpdateMylistDescription(mylist_id, video_id_list, description_list, OnDefaultWaitEvent);
        }

        ///// <summary>
        ///// マイリストの個々の動画の説明文を更新する。1件だけではなくて、同じマイリスト内の複数の動画に対して説明文を更新できる。
        ///// </summary>
        ///// <param name="mylist_id">マイリストID</param>
        ///// <param name="video_id_list">動画IDのリスト</param>
        ///// <param name="description_list">説明文のリスト。動画IDのリストと対応がとれている必要がある</param>
        ///// <param name="wait_millisecond">1件追加した後に呼び出されるイベント関数</param>
        //public void UpdateMylistDescription(string mylist_id, List<string> video_id_list,
        //    List<string> description_list, NetworkWaitDelegate dlg)
        //{
        //    CheckCookie();

        //    string html = GetMylistPageFromMyPage();

        //    string token = GetToken(html);

        //    Thread.Sleep(1000);
        //    string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/mylist/" + mylist_id);
        //    Dictionary<string, string> id_key_pair = ParseMylistEditAndGetKey(str);

        //    for (int i = 0; i < video_id_list.Count; ++i)
        //    {
        //        string value;
        //        if (id_key_pair.TryGetValue(video_id_list[i], out value))
        //        {
        //            string post_data = IJNetwork.ConstructPostData(
        //                "group_id",    mylist_id,
        //                "item_type",   "0",
        //                "item_id",     id_key_pair[video_id_list[i]],
        //                "description", description_list[i],
        //                "token",       token);
        //            network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/api/mylist/update/", post_data);
        //            if (dlg != null)
        //            {
        //                dlg(video_id_list[i], i + 1, video_id_list.Count);
        //            }
        //        }
        //    }
        //}
        // 2020/08/04 Update marky 7月27日リニューアルに対応
        /// <summary>
        /// マイリストの個々の動画の説明文を更新する。1件だけではなくて、同じマイリスト内の複数の動画に対して説明文を更新できる。
        /// </summary>
        /// <param name="mylist_id">マイリストID</param>
        /// <param name="video_id_list">動画IDのリスト</param>
        /// <param name="description_list">説明文のリスト。動画IDのリストと対応がとれている必要がある</param>
        /// <param name="dlg">1件追加した後に呼び出されるイベント関数</param>
        public void UpdateMylistDescription(string mylist_id, List<string> video_id_list,
            List<string> description_list, NetworkWaitDelegate dlg)
        {
            CheckCookie();

            //マイリストRSSからIDリスト取得
            List<Video> mylist_video = new List<Video>();
            //NicoListManager.ParsePointRss(GetMylistHtml(mylist_id, true), DateTime.Now, mylist_video, false, true);
            // 2023/01/21 Update marky
            GetMylistJson(mylist_id, mylist_video);

            for (int i = 0; i < video_id_list.Count; ++i)
            {
                //// 2020/10/20 ADD marky 公式動画はマイリストRSSのlinkがスレッドIDとなっているため変換
                //string video_id = video_id_list[i];
                //if (video_id.StartsWith("so"))
                //{
                //    video_id = ParseGetVideoInfo(GetVideoInfo2(video_id));
                //    // 2021/05/24 Update marky GetVideoInfo2がjsonを返すようになった ↓は有料動画を取得できないからボツ
                //    //VideoInfo video_info = new VideoInfo(GetVideoInfo(video_id));
                //    //video_id = video_info.thread_id;
                //}

                //if (mylist_video.Find(x => x.video_id == video_id) != null)
                // 2021/09/15 Update marky ParsePointRss内で公式動画をsoのIDに戻したので上記変換は不要に
                if (mylist_video.Find(x => x.video_id == video_id_list[i]) != null)
                {
                    network_.AddCustomHeader("Access-Control-Request-Headers: x-frontend-id,x-frontend-version,x-niconico-language,x-request-with");
                    network_.AddCustomHeader("Access-Control-Request-Method: PUT");
                    //network_.SetReferer(nicovideo_uri_ + "/my/mylist/" + mylist_id + "?ref=pc_mypage_mylist");
                    network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                    string str = network_.OptionsToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "/items/" + video_id_list[i]);
                    network_.Reset();

                    if (str.Equals("OK"))
                    {
                        network_.AddCustomHeader("X-Frontend-Id: 6");
                        network_.AddCustomHeader("X-Frontend-Version: 0");
                        network_.AddCustomHeader("X-Niconico-Language: ja-jp");
                        network_.AddCustomHeader("X-Request-With: " + nicovideo_uri_);
                        //network_.SetReferer(nicovideo_uri_ + "/my/mylist/" + mylist_id + "?ref=pc_mypage_mylist");
                        //network_.AddCustomHeader("Origin: " + nicovideo_uri_);
                        string put_data = IJNetwork.ConstructPostData("description", description_list[i]);
                        try
                        {
                            str = network_.PutToWeb("https://nvapi.nicovideo.jp/v1/users/me/mylists/" + mylist_id + "/items/" + video_id_list[i], put_data);
                        }
                        finally
                        {
                            network_.Reset();
                        }

                        if (str.Equals("OK") && dlg != null)
                        {
                            dlg(video_id_list[i], i + 1, video_id_list.Count);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// video.info API で動画情報を取得 2020/10/20 ADD marky
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <returns>video.info API の結果xml</returns>
        public string GetVideoInfo2(string video_id)
        {
            CheckCookie();
            return network_.GetAndReadFromWebUTF8("http://api.ce.nicovideo.jp/nicoapi/v1/video.info?v=" + video_id);
        }

        //// GetVideoInfo2 で取得した xml を解析してthread id を返す 2020/10/20 ADD marky 
        //public string ParseGetVideoInfo(string xml)
        //{
        //    string thread_id = "";

        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml(xml);
        //    XmlElement element = doc.DocumentElement;
        //    if (element.Attributes["status"].Value == "ok")
        //    {
        //        for (XmlNode node = element.FirstChild;
        //            node != null; node = node.NextSibling)
        //        {
        //            switch (node.Name)
        //            {
        //                case "thread":
        //                    for (XmlNode threadnode = node.FirstChild;
        //                        threadnode != null; threadnode = threadnode.NextSibling)
        //                    {
        //                        switch (threadnode.Name)
        //                        {
        //                            case "id":
        //                                thread_id = IJStringUtil.UnescapeHtml(threadnode.InnerText);
        //                                break;
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    return thread_id;
        //}
        // 2021/05/24 Update marky GetVideoInfo2がjsonを返すようになった
        public string ParseGetVideoInfo(string json)
        {
            string thread_id = "";

            int start = json.IndexOf("\"thread\":{\"id\":\"") + 16;
            if (start < 0)
            {
                return "";
            }
            int end = json.IndexOf("\"", start);
            if (end < 0)
            {
                return "";
            }
            else
            {
                thread_id = json.Substring(start, end - start);
            }

            return thread_id;
        }

        public void DeleteMylist(string mylist_id, List<string> video_id_list)
        {
            EditMylist("delete", mylist_id, "", video_id_list);
        }

        public void MoveMylist(string mylist_id, string dest_mylist_id, List<string> video_id_list)
        {
            EditMylist("move", mylist_id, dest_mylist_id, video_id_list);
        }

        public void CopyMylist(string mylist_id, string dest_mylist_id, List<string> video_id_list)
        {
            EditMylist("copy", mylist_id, dest_mylist_id, video_id_list);
        }

        public void EditMylist(string command, string mylist_id, string dest_mylist_id, List<string> video_id_list)
        {
            CheckCookie();
            string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/mylist_edit/" + mylist_id);
            Dictionary<string, string> id_key_pair = ParseMylistEditAndGetKey(str);

            List<string> post_items = new List<string>();

            post_items.Add("action");
            post_items.Add(command);

            if (dest_mylist_id != "")
            {
                post_items.Add("dest");
                post_items.Add(dest_mylist_id);
            }
            foreach (string video_id in video_id_list)
            {
                string value;
                if (id_key_pair.TryGetValue(video_id, out value))
                {
                    post_items.Add("threads[]");
                    post_items.Add(value);
                }
            }

            string post_data = IJNetwork.ConstructPostData(post_items.ToArray());

            network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/mylist_edit/" + mylist_id, post_data);
        }

        /// <summary>
        /// 指定した動画にタグをつける
        /// </summary>
        /// <param name="tag_list">タグのリスト</param>
        /// <param name="is_lock_list">タグをロックするかどうかを表す論理値のリスト。タグのリストと対応が取れている必要がある</param>
        /// <param name="video_id">タグをつける動画ID</param>
        /// <param name="dlg">1件タグを追加した、またはロックした後に呼び出されるイベント関数</param>
        public void AddTag(List<string> tag_list, List<bool> is_lock_list, string video_id, NetworkWaitDelegate dlg)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            string s = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, "");
            Thread.Sleep(2000);
            try
            {
                StringBuilder tag_str = new StringBuilder();
                for (int i = 0; i < tag_list.Count; ++i)
                {
                    tag_str.Append(tag_list[i]);
                    if (i < tag_list.Count - 1)
                    {
                        tag_str.Append(" ");
                    }
                }
                string post_data = IJNetwork.ConstructPostData(
                    "cmd",     "add",
                    "tag",     tag_str.ToString(),
                    "tag_add", "追加");
                string html = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                if (dlg != null)
                {
                    dlg("タグを加えました。", 0, 0);
                }

                for (int i = 0; i < tag_list.Count && i < is_lock_list.Count; ++i)
                {
                    if (is_lock_list[i])
                    {
                        string id = GetTagId(html, tag_list[i]);
                        if (id != "")
                        {
                            post_data = IJNetwork.ConstructPostData(
                                "cmd",        "lock",
                                "tag",        tag_list[i],
                                "id",         id,
                                "owner_lock", "1",
                                "tag_lock",   "ロックする★");
                            network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                            if (dlg != null)
                            {
                                dlg(tag_list[i] + " をロックしました。", 0, 0);
                            }
                        }
                    }
                }
            }
            finally
            {
                network_.Reset();
            }
        }

        public void UnlockTag(string tag_name, string video_id)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            try
            {
                string html = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id,
                            "");
                string id = GetTagId(html, tag_name);
                if (id != "")
                {
                    string post_data = IJNetwork.ConstructPostData(
                        "cmd", "lock",
                        "tag", tag_name,
                        "id", id,
                        "owner_lock", "0",
                        "tag_unlock", "ロック解除");
                    network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                }
            }
            finally
            {
                network_.Reset();
            }
        }

        public void SetCategoryTag(string tag_name, string video_id)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            try
            {
                string html = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id,
                            "");
                string id = GetTagId(html, tag_name);
                if (id != "")
                {
                    string post_data = IJNetwork.ConstructPostData(
                        "cmd", "set_category",
                        "tag", tag_name,
                        "id", id,
                        "tag_unlock", "カテゴリに設定");
                    network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                }
            }
            finally
            {
                network_.Reset();
            }
        }

        public void UnsetCategoryTag(string tag_name, string video_id)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            try
            {
                string html = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id,
                            "");
                string id = GetTagId(html, tag_name);
                if (id != "")
                {
                    string post_data = IJNetwork.ConstructPostData(
                        "cmd", "unset_category",
                        "tag", tag_name,
                        "id", id,
                        "tag_unlock", "カテゴリを解除");
                    network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                }
            }
            finally
            {
                network_.Reset();
            }
        }

        public void RemoveTag(string tag_name, string video_id)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            try
            {
                string html = network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id,
                            "");
                string id = GetTagId(html, tag_name);
                if (id != "")
                {
                    string post_data = IJNetwork.ConstructPostData(
                        "cmd",        "remove",
                        "tag",        tag_name,
                        "id",         id,
                        "tag_remove", "削除");
                    network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, post_data);
                }
            }
            finally
            {
                network_.Reset();
            }
        }

        public string GetTagEditHtml(string video_id)
        {
            CheckCookie();
            network_.AddCustomHeader("X-Requested-With: XMLHttpRequest");
            network_.AddCustomHeader("X-Prototype-Version: 1.5.1.1");
            network_.SetReferer(nicovideo_uri_ + "/watch/" + video_id);
            try
            {
                return network_.PostAndReadFromWebUTF8(nicovideo_uri_ + "/tag_edit/" + video_id, "");
            }
            finally
            {
                network_.Reset();
            }
        }

        /// <summary>
        /// wayback を取得
        /// </summary>
        /// <param name="thread_id">thread_id</param>
        /// <returns>wayback</returns>
        public string GetWayback(string thread_id)
        {
            CheckCookie();
            string way = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/api/getwaybackkey?thread=" + thread_id);
            string[] key = way.Split('=');
            if (key.Length < 2 || key[0] != "waybackkey")
            {
                throw new NiconicoAccessFailedException();
            }
            return key[1];
        }

        /// <summary>
        /// コメントを取得する。内部で GetVideoInfo を呼び出す。連続してこの関数を呼び出す場合は
        /// 引数に VideoInfo を指定するバージョンを使う方がよい（その場合は手動で GetVideoInfo を呼び出す）。
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="res_from">取得件数（最新から100件取得する場合は -100 を指定）</param>
        /// <returns>コメントXML</returns>
        public string GetComment(string video_id, int res_from)
        {
            return GetComment(video_id, new DateTime(), res_from);
        }

        /// <summary>
        /// コメントを取得する。内部で GetVideoInfo と GetWayback を呼び出す。
        /// 日時を指定した取得はプレミアム会員専用。連続してこの関数を呼び出す場合は
        /// 引数に VideoInfo を指定するバージョンを使う方がよい（その場合は手動で GetVideoInfo を呼び出す）。
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="datetime">日時指定</param>
        /// <param name="res_from">取得件数（指定した日時から100件取得する場合は -100 を指定）</param>
        /// <returns>コメントXML</returns>
        public string GetComment(string video_id, DateTime datetime, int res_from)
        {
            VideoInfo video_info = new VideoInfo(GetVideoInfo(video_id));
            Thread.Sleep(500);
            string wayback = "";
            if (datetime.Year != 1)
            {
                wayback = GetWayback(video_info.thread_id);
                Thread.Sleep(500);
            }
            return GetComment(video_id, video_info, wayback, datetime, res_from);
        }

        /// <summary>
        /// コメントを取得する。GetVideoInfo を引数で指定する
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="VideoInfo">GetVideoInfo で取得した VideoInfo オブジェクト</param>
        /// <param name="res_from">取得件数（最新から100件取得する場合は -100 を指定）</param>
        /// <returns>コメントXML</returns>
        public string GetComment(string video_id, VideoInfo video_info, int res_from)
        {
            return GetComment(video_id, video_info.thread_id, "", video_info.ms, new DateTime(), res_from, video_info.user_id);
        }

        /// <summary>
        /// コメントを取得する。GetVideoInfo と wayback を引数で指定する。
        /// 日時を指定した取得はプレミアム会員専用。
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="video_info">GetVideoInfo で取得した VideoInfo オブジェクト</param>
        /// <param name="wayback">wayback（GetWayback で取得した値）</param>
        /// <param name="datetime">日時指定</param>
        /// <param name="res_from">取得件数（指定した日時から100件取得する場合は -100 を指定）</param>
        /// <returns>コメントXML</returns>
        public string GetComment(string video_id, VideoInfo video_info, string wayback, DateTime datetime, int res_from)
        {
            return GetComment(video_id, video_info.thread_id, wayback, video_info.ms, datetime, res_from, video_info.user_id);
        }

        /// <summary>
        /// コメントを取得する。
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="thread_id">thread_id</param>
        /// <param name="wayback">wayback（GetWayback で取得した値）</param>
        /// <param name="message_server">メッセージサーバ</param>
        /// <param name="datetime">日時指定</param>
        /// <param name="res_from">取得件数（指定した日時から100件取得する場合は -100 を指定）</param>
        /// <param name="user_id">ユーザID</param>
        /// <returns></returns>
        public string GetComment(string video_id, string thread_id, string wayback, string message_server, DateTime datetime, int res_from, string user_id)
        {
            CheckCookie();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            StringBuilder post_str = new StringBuilder();
            using (TextWriter tw = new StringWriter(post_str))
            using (XmlWriter writer = XmlWriter.Create(tw, settings))
            {
                writer.WriteStartElement("thread");

                if (datetime.Year != 1) // datetime が空でないなら
                {
                    writer.WriteAttributeString("user_id", user_id);
                    writer.WriteAttributeString("when", NicoUserSession.DateToUnix(datetime));
                    writer.WriteAttributeString("waybackkey", wayback);
                }

                writer.WriteAttributeString("res_from", res_from.ToString());
                writer.WriteAttributeString("version", "20061206");
                writer.WriteAttributeString("thread", thread_id);

                writer.WriteEndElement(); // <thread ... />
            }

            return network_.PostAndReadFromWebUTF8(message_server, post_str.ToString());
        }

        /// <summary>
        /// コメントを投稿する
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="comment">コメント</param>
        /// <param name="vpos">コメントの時間（位置）。1/100秒単位で指定</param>
        /// <returns>サーバからの応答</returns>
        public string PostComment(string video_id, string comment, int vpos)
        {
            CheckCookie();
            network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/watch/" + video_id); // 必要かどうか不明。一応呼ぶ
            Thread.Sleep(1000);
            VideoInfo video_info = new VideoInfo(GetVideoInfo(video_id));
            Thread.Sleep(1000);
            string comment_xml = GetComment(video_id, video_info, -100);
            Thread.Sleep(1000);
            return PostComment(video_id, video_info, GetTicket(comment_xml), comment, GetLastResNo(comment_xml) / 1000, vpos);
        }

        /// <summary>
        /// コメントを投稿する
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="video_info">VideoInfo オブジェクト</param>
        /// <param name="ticket">ticket</param>
        /// <param name="comment">コメント</param>
        /// <param name="block_no">block_no（最後のコメントNo ÷ 100 らしい）</param>
        /// <param name="vpos">コメントの時間（位置）。1/100秒単位で指定</param>
        /// <returns>サーバからの応答</returns>
        public string PostComment(string video_id, VideoInfo video_info, string ticket, string comment, int block_no, int vpos)
        {
            string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/api/getpostkey?thread=" +
                video_info.thread_id + "&block_no=" + block_no);
            if (str.StartsWith("postkey="))
            {
                Thread.Sleep(1000);
                return PostComment(video_id, video_info, ticket, str.Substring("postkey=".Length), comment, vpos);
            }
            else
            {
                throw new NiconicoAccessFailedException();
            }
        }

        /// <summary>
        /// コメントを投稿する
        /// </summary>
        /// <param name="video_id">動画ID</param>
        /// <param name="video_info">VideoInfo オブジェクト</param>
        /// <param name="ticket">ticket</param>
        /// <param name="postkey">postkey</param>
        /// <param name="comment">コメント</param>
        /// <param name="vpos">コメントの時間（位置）。1/100秒単位で指定</param>
        /// <returns>サーバからの応答</returns>
        public string PostComment(string video_id, VideoInfo video_info, string ticket, string postkey, string comment, int vpos)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            StringBuilder post_data = new StringBuilder();
            using (TextWriter tw = new StringWriter(post_data))
            using (XmlWriter writer = XmlWriter.Create(tw, settings))
            {
                writer.WriteStartElement("chat");

                writer.WriteAttributeString("premium", (video_info.is_premium ? "1" : "0"));
                writer.WriteAttributeString("postkey", postkey);
                writer.WriteAttributeString("user_id", video_info.user_id);
                writer.WriteAttributeString("ticket", ticket);
                writer.WriteAttributeString("mail", " 184");
                writer.WriteAttributeString("vpos", vpos.ToString());
                writer.WriteAttributeString("thread", video_info.thread_id);

                writer.WriteString(comment);

                writer.WriteEndElement(); // </chat>
            }

            string referer_query = IJNetwork.ConstructPostData(
                "ts",                 video_info.thread_id,
                "is_video_owner",     "1",
                "wv_id",              video_id,
                "deleted",            "8",
                "open_src",           "true",
                "movie_type",         "flv",
                "button_threshold",   "0",
                "player_version_xml", "1218448842");

            network_.SetReferer(nicovideo_uri_ + "/swf/nicoplayer.swf?" + referer_query);
            byte[] post_data_byte = Encoding.UTF8.GetBytes(post_data.ToString());
            try
            {
                return network_.PostAndReadFromWebUTF8(video_info.ms, post_data_byte);
            }
            finally
            {
                network_.SetReferer("");
            }
        }

        // ジャンル＋人気のタグ ファイル一覧を取得
        // 2019/06/26 ADD marky
        public string GetGenreTag(DateTime getdate)
        {
            string url = ranklog_url_ + "daily/" + getdate.ToString("yyyy-MM-dd") + "/file_name_list.json";
            ////テスト用
            //string json = File.ReadAllText("D:\\dev\\file_name_list.json", Encoding.UTF8); 
            //string url = ranklog_url_ + "daily/" + getdate.ToString("yyyy-MM-dd") + "_05/file_name_list.json";
            string json = network_.GetAndReadFromWebUTF8(url);

            return json;
        }

        public string GetDataFromNicoApi() // 実験用メソッド
        {
            network_.SetContentTypeJSON();
            string str = network_.PostAndReadFromWebUTF8("http://api.search.nicovideo.jp/api/snapshot/", "{\"query\":\"初音ミク\",\"service\":[\"video\"],\"search\":[\"title\",\"description\",\"tags\"],\"join\":[\"cmsid\",\"title\",\"tags\",\"start_time\",\"thumbnail_url\",\"view_counter\",\"comment_counter\",\"mylist_counter\",\"length_seconds\"],\"filters\":[{\"type\":\"equal\",\"field\":\"music_download\",\"value\":true}],\"from\":0,\"size\":3,\"sort_by\":\"view_counter\",\"issuer\":\"your service/application name\"}");
            network_.SetDefaultContentType();
            return str;
        }

        //-------------------------------------------- private method -----------------------------------------------

        private void LoadCookies()
        {
            string user_session = "";
            switch (cookie_kind_)
            {
                //case CookieKind.IE:
                //    user_session = NicoUserSession.GetUserSessionFromIE(nicovideo_uri_);
                //    break;
                // 2022/08/07 Update marky
                case CookieKind.Edge:
                    user_session = NicoUserSession.GetUserSessionFromEdge();
                    break;
                case CookieKind.Firefox3:
                    user_session = NicoUserSession.GetUserSessionFromFilefox3(firefox_profile_dir_);
                    break;
                case CookieKind.Opera:
                    user_session = NicoUserSession.GetUserSessionFromOpera();
                    break;
                case CookieKind.Chrome:
                    user_session = NicoUserSession.GetUserSessionFromChrome();
                    break;
            }
            if (user_session != "")
            {
                ResetUserSession(user_session);
            }
            is_loaded_cookie_ = true;
        }

        private void CheckCookie()
        {
            if (!is_loaded_cookie_)
            {
                LoadCookies();
            }
        }

        // 連続アクセスで拒否されていないか調べる
        private void CheckDenied(string html)
        {
            if (html.IndexOf("連続アクセスはご遠慮ください") >= 0)
            {
                throw new NiconicoAccessDeniedException();
            }
        }

        private void OnDefaultWaitEvent(string message, int current, int total)
        {
            if (current < total)
            {
                Thread.Sleep(wait_millisecond_);
            }
        }

        private void DownloadRankingOnePage(string url, string dir_name, string filename, int hour, bool is_xml)
        {
            DateTime current_datetime = DateTime.Now;

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                string html = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/ranking/" + url);

                //2019/12/02 ADD marky ファイル名に使用できない文字を削除する
                char[] removeChars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
                foreach (char c in removeChars)
                {
                    filename = filename.Replace(c.ToString(), "");
                }

                string save_filename = dir_name + filename + current_datetime.ToString("yyyyMMddHHmm");
                save_filename += (is_xml ? ".xml" : ".html");

                File.WriteAllText(save_filename, html, Encoding.UTF8);
            }
            finally
            {
                network_.Reset();
            }
        }

        // 2019/06/26 ADD marky
        private void DownloadRankingLog(string url, string dir_name, string filename)
        {
            DateTime current_datetime = DateTime.Now;

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }

            try
            {
                ////テスト用
                //string json = File.ReadAllText("D:\\dev\\entertainment.json", Encoding.UTF8);
                string json = network_.GetAndReadFromWebUTF8(ranklog_url_ + url);
                //2019/12/02 ADD marky ファイル名に使用できない文字を削除する
                char[] removeChars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
                foreach (char c in removeChars)
                {
                    filename = filename.Replace(c.ToString(), "");
                } 
                string save_filename = dir_name + filename + current_datetime.ToString("yyyyMMddHHmm") + ".json";

                File.WriteAllText(save_filename, json, Encoding.UTF8);
            }
            finally
            {
                network_.Reset();
            }
        }

        //private string GetSearchKeywordOrTag(string word, int page, SearchSortMethod sort_method, SearchOrder order, bool is_tag)
        //2020/02/16 Update marky
        private string GetSearchKeywordOrTag(string word, int page, SearchingTagOption option, bool is_tag)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                string escapedWord = Uri.EscapeDataString(word).Replace("%20", "+");
                //string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/" + (is_tag ? "tag" : "search") +
                //    "/" + escapedWord + GetOption(page, sort_method, order, is_tag));
                //2020/02/16 Update marky
                string str = network_.GetAndReadFromWebUTF8(nicovideo_uri_ + "/" + (is_tag ? "tag" : "search") +
                    "/" + escapedWord + GetOption(page, option));
                CheckDenied(str);
                return str;
            }
            finally
            {
                network_.Reset();
            }
        }

        /*
        private string GetSearchKeywordOrTagByAPI(string word, int page, SearchSortMethod sort_method, SearchOrder order, bool is_tag)
        {
            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                //string escapedWord = Uri.EscapeDataString(word).Replace("%20", "+");
                string json = "{\"query\":\"";
                //json += escapedWord;
                json += word;
                json += "\",\"service\":[\"video\"],\"search\":";
                if (is_tag) { // tag search
                    json += "[\"tags_exact\"]";
                } else { // keyword search
                    json += "[\"title\",\"description\",\"tags\"]";
                }
                // description と last_res_body は取得しない
                json += ",\"join\":[\"cmsid\",\"title\",\"tags\",\"start_time\",\"thumbnail_url\",\"view_counter\"," +
                    "\"comment_counter\",\"mylist_counter\",\"length_seconds\"],";
                //json += "\"filters\":[{\"type\":\"equal\",\"field\":\"music_download\",\"value\":true}],"

                json += "\"sort_by\":\"";
                switch (sort_method)
                {
                    case SearchSortMethod.ResNew:
                        json += "last_comment_time";
                        break;
                    case SearchSortMethod.View:
                        json += "view_counter";
                        break;
                    case SearchSortMethod.SubmitDate:
                        json += "start_time";
                        break;
                    case SearchSortMethod.Mylist:
                        json += "mylist_counter";
                        break;
                    case SearchSortMethod.Res:
                        json += "comment_counter";
                        break;
                    case SearchSortMethod.Time:
                        json += "length_seconds";
                        break;
                }
                json += "\",";

                if (order == SearchOrder.Asc)
                {
                    json += "\"order\":\"asc\",";
                }
                json += "\"from\":";
                json += ((page - 1) * 100).ToString();
                json += ",\"size\":100,";
                json += "\"issuer\":\"" + issuer_ + "\"}";

                network_.SetContentTypeJSON();
                string str = network_.PostAndReadFromWebUTF8("http://api.search.nicovideo.jp/api/snapshot/", json);
                network_.SetDefaultContentType();

                //CheckDenied(str);
                return str;
            }
            finally
            {
                network_.Reset();
            }
        }
        */

        ////2017-03-03 UPDATE marky 検索API v2  2020/02/16 DEL marky
        //private string GetSearchKeywordOrTagByAPI(string word, int page, SearchSortMethod sort_method, SearchOrder order, bool is_tag)
        //{
        //    CheckCookie();

        //    if (is_no_cache_)
        //    {
        //        network_.SetMaxAgeZero();
        //    }
        //    try
        //    {
        //        string json = "q=";
        //        json += word;
        //        json += "&targets=";
        //        if (is_tag)
        //        { // tag search
        //            json += "tagsExact";
        //        }
        //        else
        //        { // keyword search
        //            json += "title,description,tags";
        //        }
        //        //json += "&fields=contentId,title,description,tags,categoryTags,viewCounter,mylistCounter,commentCounter,startTime,thumbnailUrl,lengthSeconds";
        //        //2018/02/27 UPDATE marky 動画説明文にHTMLタグが入る仕様変更に対応
        //        json += "&fields=contentId,title,tags,categoryTags,viewCounter,mylistCounter,commentCounter,startTime,thumbnailUrl,lengthSeconds";

        //        json += "&_sort=";
        //        if (order == SearchOrder.Asc)
        //        {
        //            //json += "+";
        //            //2017-03-06 UPDATE まどやさんの指摘を反映
        //            json += "%2B";
        //        }
        //        else
        //        {
        //            json += "-";
        //        }
        //        switch (sort_method)
        //        {
        //            case SearchSortMethod.ResNew:
        //                json += "lastCommentTime";
        //                break;
        //            case SearchSortMethod.View:
        //                json += "viewCounter";
        //                break;
        //            case SearchSortMethod.SubmitDate:
        //                json += "startTime";
        //                break;
        //            case SearchSortMethod.Mylist:
        //                json += "mylistCounter";
        //                break;
        //            case SearchSortMethod.Res:
        //                json += "commentCounter";
        //                break;
        //            case SearchSortMethod.Time:
        //                json += "lengthSeconds";
        //                break;
        //        }

        //        json += "&_offset=";
        //        json += ((page - 1) * 100).ToString();
        //        json += "&_limit=100";
        //        json += "&_context=" + issuer_ ;

        //        network_.SetDefaultContentType();
        //        string str = network_.GetAndReadFromWebUTF8("http://api.search.nicovideo.jp/api/v2/snapshot/video/contents/search?" + json);

        //        //CheckDenied(str);
        //        return str;
        //    }
        //    finally
        //    {
        //        network_.Reset();
        //    }
        //}

        //2019/07/06 ADD marky 検索API v3 10万件以上に対応
        private string GetSearchKeywordOrTagByAPI(string word, SearchingTagOption option)
        {
            SearchSortMethod sort_method = option.GetSortMethod();
            SearchOrder order = option.GetSearchOrder();
            bool is_using_condition = option.is_using_condition;
            bool is_using_date = option.is_using_date;  // 2020/11/24 ADD marky
            string date_from = option.date_from.ToString("yyyy-MM-dd'T'HH:mm:ss'%2B09:00'");
            string date_to = option.date_to.ToString("yyyy-MM-dd'T'HH:mm:ss'%2B09:00'");
            string condition_lower = option.condition_lower.ToString();
            string condition_upper = option.condition_upper.ToString();
            string offset = option.offset.ToString();
            string last_value = option.last_value;
            // 2020/02/11 ADD marky ジャンル追加
            string genre = option.genre;

            CheckCookie();

            if (is_no_cache_)
            {
                network_.SetMaxAgeZero();
            }
            try
            {
                string json = "q=";
                json += word;
                json += "&targets=";
                if (option.is_searching_kind_tag)
                { // tag search
                    json += "tagsExact";
                }
                else
                { // keyword search
                    json += "title,description,tags";
                }
                //json += "&fields=contentId,title,tags,viewCounter,mylistCounter,commentCounter,startTime,thumbnailUrl,lengthSeconds,lastCommentTime,genre";
                // 2021/05/20 Add marky
                json += "&fields=contentId,title,tags,viewCounter,mylistCounter,commentCounter,startTime,thumbnailUrl,lengthSeconds,lastCommentTime,genre,userId,channelId,likeCounter";

                json += "&_sort=";
                if (order == SearchOrder.Asc)
                {
                    json += "%2B";
                }
                else
                {
                    json += "-";
                }
                switch (sort_method)
                {
                    case SearchSortMethod.ResNew:
                        json += "lastCommentTime";
                        break;
                    case SearchSortMethod.View:
                        json += "viewCounter";
                        break;
                    case SearchSortMethod.SubmitDate:
                        json += "startTime";
                        break;
                    case SearchSortMethod.Mylist:
                        json += "mylistCounter";
                        break;
                    case SearchSortMethod.Res:
                        json += "commentCounter";
                        break;
                    case SearchSortMethod.Time:
                        json += "lengthSeconds";
                        break;
                }

                json += "&_offset=";
                json += offset;

                if (!last_value.Equals(""))
                {
                    json += "&filters";
                    switch (sort_method)
                    {
                        case SearchSortMethod.SubmitDate:
                            json += "[startTime]";
                            break;
                        case SearchSortMethod.View:
                            json += "[viewCounter]";
                            break;
                        case SearchSortMethod.ResNew:
                            json += "[lastCommentTime]";
                            break;
                        case SearchSortMethod.Res:
                            json += "[commentCounter]";
                            break;
                        case SearchSortMethod.Mylist:
                            json += "[mylistCounter]";
                            break;
                        case SearchSortMethod.Time:
                            json += "[lengthSeconds]";
                            break;
                    }
                    if (order == SearchOrder.Asc)
                    {
                        json += "[gte]=";
                    }
                    else
                    {
                        json += "[lte]=";
                    }
                    json += last_value;
                }

                if (is_using_condition)
                {
                    switch (option.sort_kind_num)
                    {
                        case 0:
                            json += "&filters[startTime][gte]=" + date_from + (last_value.Equals("") ? "&filters[startTime][lte]=" + date_to : "");
                            break;
                        case 1:
                            json += (last_value.Equals("") ? "&filters[startTime][gte]=" + date_from : "") + "&filters[startTime][lte]=" + date_to;
                            break;
                        case 2:
                            json += "&filters[viewCounter][gte]=" + condition_lower + (last_value.Equals("") ? "&filters[viewCounter][lte]=" + condition_upper : "");
                            break;
                        case 3:
                            json += (last_value.Equals("") ? "&filters[viewCounter][gte]=" + condition_lower : "") + "&filters[viewCounter][lte]=" + condition_upper;
                            break;
                        case 6:
                            json += "&filters[commentCounter][gte]=" + condition_lower + (last_value.Equals("") ? "&filters[commentCounter][lte]=" + condition_upper : "");
                            break;
                        case 7:
                            json += (last_value.Equals("") ? "&filters[commentCounter][gte]=" + condition_lower : "") + "&filters[commentCounter][lte]=" + condition_upper;
                            break;
                        case 8:
                            json += "&filters[mylistCounter][gte]=" + condition_lower + (last_value.Equals("") ? "&filters[mylistCounter][lte]=" + condition_upper : "");
                            break;
                        case 9:
                            json += (last_value.Equals("") ? "&filters[mylistCounter][gte]=" + condition_lower : "") + "&filters[mylistCounter][lte]=" + condition_upper;
                            break;
                    }
                    // 2020/11/24 ADD marky 日時指定を併用可能に
                    if (is_using_date)
                    {
                        switch (option.sort_kind_num)
                        {
                            case 0:
                            case 1:
                                break;
                            default:
                                json += "&filters[startTime][gte]=" + date_from + "&filters[startTime][lte]=" + date_to;
                                break;
                        }
                    }
                }

                // 2020/02/11 ADD marky ジャンル追加
                if (!genre.Equals("全ジャンル"))
                {
                    json += "&filters[genre][0]=" + genre;
                }

                json += "&_limit=100";
                json += "&_context=" + issuer_;

                network_.SetDefaultContentType();
                //string str = network_.GetAndReadFromWebUTF8("http://api.search.nicovideo.jp/api/v2/snapshot/video/contents/search?" + json);
                //// 2020/08/27 Update marky https化
                //string str = network_.GetAndReadFromWebUTF8("https://api.search.nicovideo.jp/api/v2/snapshot/video/contents/search?" + json);
                // 2024/03/01 Update marky URL変更
                string str = network_.GetAndReadFromWebUTF8("https://snapshot.search.nicovideo.jp/api/v2/snapshot/video/contents/search?" + json);

                //CheckDenied(str);
                return str;
            }
            finally
            {
                network_.Reset();
            }
        }

        ///// <summary>
        ///// タグ検索、キーワード検索のURLオプションを取得 2020/02/16 DEL marky
        ///// </summary>
        ///// <param name="page">ページ数</param>
        ///// <param name="sort_method">検索時の並べ方指定</param>
        ///// <param name="order">昇順 or 降順</param>
        ///// <param name="is_tag">タグ検索の場合 true、キーワード検索の場合 false</param>
        ///// <returns></returns>
        //private static string GetOption(int page, SearchSortMethod sort_method, SearchOrder order, bool is_tag)
        //{
        //    string option = "";
        //    if (page >= 2)
        //    {
        //        option += "?page=" + page.ToString();
        //    }
        //    switch (sort_method)
        //    {
        //        case SearchSortMethod.SubmitDate:
        //            option += ((option != "" ? "&" : "?") + "sort=f");
        //            break;
        //        case SearchSortMethod.View:
        //            option += ((option != "" ? "&" : "?") + "sort=v");
        //            break;
        //        case SearchSortMethod.Res:
        //            option += ((option != "" ? "&" : "?") + "sort=r");
        //            break;
        //        case SearchSortMethod.ResNew:
        //            //if (!is_tag) // タグ検索のときはオプションをつけない
        //            //{
        //            //2018/09/02 Update marky タグ検索でもオプションが必須になった
        //                option += ((option != "" ? "&" : "?") + "sort=n");
        //            //}
        //            break;
        //        case SearchSortMethod.Mylist:
        //            option += ((option != "" ? "&" : "?") + "sort=m");
        //            break;
        //        case SearchSortMethod.Time:
        //            option += ((option != "" ? "&" : "?") + "sort=l");
        //            break;
        //    }
        //    if (order == SearchOrder.Asc)
        //    {
        //        option += ((option != "" ? "&" : "?") + "order=a");
        //    }
        //    else
        //    {
        //        //if (!is_tag) // キーワード検索のときだけつける
        //        //{
        //        //2018/09/02 Update marky タグ検索の降順でもorderが必須になった
        //            option += ((option != "" ? "&" : "?") + "order=d");
        //        //}
        //    }
        //    return option;
        //}

        /// <summary>
        /// タグ検索、キーワード検索のURLオプションを取得v2 2020/02/16 ADD
        /// </summary>
        /// <param name="page">ページ数</param>
        /// <param name="option">オプションクラス</param>
        /// <returns>URLオプション文字列</returns>
        private static string GetOption(int page, SearchingTagOption option)
        {
            SearchSortMethod sort_method = option.GetSortMethod();
            SearchOrder order = option.GetSearchOrder();
            bool is_using_condition = option.is_using_condition;
            bool is_using_date = option.is_using_date;      // 2020/11/24 ADD marky 日時指定を併用可能に
            string date_from = option.date_from.ToString("yyyy-MM-dd");
            string date_to = option.date_to.ToString("yyyy-MM-dd");
            string genre_id = option.genre_id;
            string str = "";

            if (page >= 2)
            {
                str += "?page=" + page.ToString();
            }

            if (is_using_condition)
            {
                switch (option.sort_kind_num)
                {
                    case 0:
                    case 1:
                        str += (str != "" ? "&" : "?") ;
                        str += "start=" + date_from + "&end=" + date_to;
                        break;
                    // 2020/11/24 ADD marky 日時指定を併用可能に
                    default:
                        if (is_using_date)
                        {
                            str += (str != "" ? "&" : "?");
                            str += "start=" + date_from + "&end=" + date_to;
                        }
                        break;
                }
            }

            if (!genre_id.Equals("all"))
            {
                str += ((str != "" ? "&" : "?") + "genre=" + genre_id);
            }
            switch (sort_method)
            {
                case SearchSortMethod.SubmitDate:
                    str += ((str != "" ? "&" : "?") + "sort=f");
                    break;
                case SearchSortMethod.View:
                    str += ((str != "" ? "&" : "?") + "sort=v");
                    break;
                case SearchSortMethod.Res:
                    str += ((str != "" ? "&" : "?") + "sort=r");
                    break;
                case SearchSortMethod.ResNew:
                    str += ((str != "" ? "&" : "?") + "sort=n");
                    break;
                case SearchSortMethod.Mylist:
                    str += ((str != "" ? "&" : "?") + "sort=m");
                    break;
                case SearchSortMethod.Time:
                    str += ((str != "" ? "&" : "?") + "sort=l");
                    break;
            }
            if (order == SearchOrder.Asc)
            {
                str += ((str != "" ? "&" : "?") + "order=a");
            }
            else
            {
                str += ((str != "" ? "&" : "?") + "order=d");
            }
            return str;
        }

        private static Dictionary<string, string> ParseMylistEditAndGetKey(string html)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            int index = html.IndexOf("Mylist.preload");

            while ((index = html.IndexOf("\"item_id\"", index + 1)) >= 0)
            {
                int end = html.IndexOf('}', index);
                string str = html.Substring(index, end - index);

                Match match = Regex.Match(str, "\"item_id\":\"([0-9]+)\".*\"video_id\":\"([a-z0-9]+)\".*\"watch_id\":\"([a-z0-9]+)\"");

                if (match.Success)
                {
                    if (!dic.ContainsKey(match.Groups[2].Value))
                    {
                        dic.Add(match.Groups[2].Value, match.Groups[1].Value); // video_id と item_id のペア
                    }
                    if (!dic.ContainsKey(match.Groups[3].Value))
                    {
                        dic.Add(match.Groups[3].Value, match.Groups[1].Value); // watch_id と item_id のペア
                    }
                }
            }
            return dic;
        }

        // HTMLを解析して token を取得
        private static string GetToken(string html)
        {
            //Match match = Regex.Match(html, "NicoAPI.token = \"([-0-9a-f]+)\";");
            // 2020/08/03 Update marky 7月27日リニューアル後の新規マイリスト作成時用
            Match match = Regex.Match(html, "csrfToken&quot;:&quot;([-0-9a-f]+)&quot;,&quot;tagrepoCsrfToken");

            if (!match.Success)
            {
                throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。html の解析ができませんでした。");
            }

            return match.Groups[1].Value;
        }

        // HTMLを解析して token を取得 マイリスト追加ページ用 2014/10/19 ADD marky
        private static string GetTokenAddPage(string html)
        {
            //2014/10/31 UPDATE marky 再度のソース変更に対応
            //Match match = Regex.Match(html, "NicoAPI.token=\"([-0-9a-f]+)\";");
            Match match = Regex.Match(html, "NicoAPI.token = '([-0-9a-f]+)';");

            if (!match.Success)
            {
                throw new NiconicoAddingMylistFailedException("マイリストの追加に失敗しました。html の解析ができませんでした。");
            }

            return match.Groups[1].Value;
        }

        // コメントXMLを解析して ticket を取得
        private static string GetTicket(string xml)
        {
            int start = xml.IndexOf("ticket=\"") + "ticket=\"".Length;
            if (start < 0)
            {
                throw new NiconicoAccessFailedException();
            }
            int end = xml.IndexOf('"', start);
            return xml.Substring(start, end - start);
        }

        // コメントXMLを解析して最後のレス番号を取得
        private static int GetLastResNo(string xml)
        {
            int start = xml.LastIndexOf("no=\"") + "no=\"".Length;
            if (start < 0)
            {
                throw new NiconicoAccessFailedException();
            }
            int end = xml.IndexOf('"', start);
            return int.Parse(xml.Substring(start, end - start));
        }

        // タグHTMLを解析して指定したタグのIDを取得
        private static string GetTagId(string html, string tag_name)
        {
            int start = html.IndexOf("name=\"tag\" value=\"" + tag_name);
            if (start < 0)
            {
                return "";
            }
            start = html.IndexOf("name=\"id\" value=\"", start);
            if (start < 0)
            {
                return "";
            }
            start += "name=\"id\" value=\"".Length;
            int end = html.IndexOf('"', start);
            return html.Substring(start, end - start);
        }

    }

    /// <summary>
    /// ニコニコAPI の getflv で取得できる情報を格納するクラス
    /// </summary>
    public class VideoInfo
    {
        public string thread_id = "";
        public string l = "";
        public string url = "";
        public string link = "";
        public string ms = "";
        public string user_id = "";
        public bool is_premium = false;
        public string nickname = "";
        public string time = "";
        public bool done = false;

        public VideoInfo()
        {
            // 何もしない
        }

        public VideoInfo(string xml)
        {
            Parse(xml);
        }

        public void Parse(string xml)
        {
            string[] option_array = xml.Split('&');
            for (int i = 0; i < option_array.Length; ++i)
            {
                string[] name_value_pair = option_array[i].Split('=');
                Set(Uri.UnescapeDataString(name_value_pair[0]), Uri.UnescapeDataString(name_value_pair[1]));
            }
        }

        public void Set(string name, string value)
        {
            switch (name)
            {
                case "thread_id":
                    thread_id = value;
                    break;
                case "l":
                    l = value;
                    break;
                case "url":
                    url = value;
                    break;
                case "link":
                    link = value;
                    break;
                case "ms":
                    ms = value;
                    break;
                case "user_id":
                    user_id = value;
                    break;
                case "is_premium":
                    is_premium = value.Equals("1");
                    break;
                case "nickname":
                    nickname = value;
                    break;
                case "time":
                    time = value;
                    break;
                case "done":
                    done = value.Equals("true");
                    break;
            }
        }
    }

    /// <summary>
    /// マイリストの情報を表す構造体
    /// </summary>
    public struct MylistInfo
    {
        public string mylist_id;
        public string title;
        public string description;
        public int number_of_item;
        public bool is_public;
    }

    /// <summary>
    /// ランキングカテゴリの情報を表す構造体
    /// </summary>
    public struct CategoryItem
    {
        public string id;
        public string short_name;
        public string name;
        public int[] page;
        public string genre; // 2019/06/26 ADD marky
    }

    /// <summary>
    /// ランキングジャンル、人気のタグの情報を表す構造体 2019/06/26 ADD marky
    /// </summary>
    public struct GenreTagItem
    {
        public string id;
        public string genre;
        public string tag;
        public string file;
        public string name;
    }

    /// <summary>
    /// ニコニコ動画公式ランキングをダウンロードする時に、種類を指定するためのクラス
    /// </summary>
    // (Thanks to Asarima-san)
    public class DownloadKind
    {
        public enum FormatKind { Html, Rss };

        protected static string[] target_name = { "fav", "view", "res", "mylist" };
        protected static string[] target_short_name = { "fav", "vie", "res", "myl" };
        protected static string[] duration_name = { "total", "monthly", "weekly", "daily", "hourly" };
        protected static string[] duration_short_name = { "tot", "mon", "wek", "day", "hou" };
        protected static string[] term_name = { "total", "month", "week", "24h", "hour" }; //新RSS用 2019/06/26 ADD marky
        
        protected List<CategoryItem> category_list;
        protected DateTime getdate_; //過去ログ日付 2019/06/26 ADD marky
        protected int gettime_;      //過去ログ生成時間 2019/06/29 ADD marky
        protected bool[] target_ = new bool[target_name.Length];
        protected bool[] duration_ = new bool[duration_name.Length];

        protected FormatKind format_kind_ = FormatKind.Html;

        public DownloadKind()
        {
            for (int i = 0; i < target_.Length; ++i)
            {
                target_[i] = false;
            }
            for (int i = 0; i < duration_.Length; ++i)
            {
                duration_[i] = false;
            }
        }

        public List<CategoryItem> CategoryList
        {
            get { return category_list; }
            set { category_list = value; }
        }

        public bool IsRss
        {
            get { return format_kind_ == FormatKind.Rss; }
            set { format_kind_ = (value ? FormatKind.Rss : FormatKind.Html); }
        }

        //過去ログ日付 2019/06/26 ADD marky
        public DateTime GetDate
        {
            get { return getdate_; }
            set { getdate_ = value; }
        }

        //過去ログ日付 2019/06/29 ADD marky
        public int GetTime
        {
            get { return gettime_; }
            set { gettime_ = value; }
        }

        /// <summary>
        /// ダウンロードするランキングの期間を設定。true を指定するとその期間のランキングをDLする
        /// </summary>
        /// <param name="total">総合</param>
        /// <param name="month">月間</param>
        /// <param name="week">週間</param>
        /// <param name="day">デイリー</param>
        /// <param name="hour">毎時</param>
        public void SetDuration(bool total, bool month, bool week, bool day, bool hour)
        {
            duration_[0] = total;
            duration_[1] = month;
            duration_[2] = week;
            duration_[3] = day;
            duration_[4] = hour;
        }

        /// <summary>
        /// ダウンロードするランキングの対象を設定。
        /// </summary>
        /// <param name="fav">総合</param>
        /// <param name="view">再生</param>
        /// <param name="res">コメント</param>
        /// <param name="mylist">マイリスト</param>
        public void SetTarget(bool fav, bool view, bool res, bool mylist)
        {
            target_[0] = fav;
            target_[1] = view;
            target_[2] = res;
            target_[3] = mylist;
        }

        /// <summary>
        /// ダウンロードするランキングのURLをリストで取得
        /// </summary>
        /// <param name="name_list">URL のリスト</param>
        /// <param name="filename_list">ある規則に従ったファイル名のリスト</param>
        public virtual void GetRankingNameList(ref List<string> name_list, ref List<string> filename_list)
        {
            for (int i = 0; i < target_name.Length; ++i)
            {
                if (!target_[i])
                {
                    continue;
                }
                for (int j = 0; j < duration_name.Length; ++j)
                {
                    if (!duration_[j])
                    {
                        continue;
                    }
                    for (int k = 0; k < category_list.Count; ++k)
                    {
                        int end = category_list[k].page[j];
                        for (int m = 1; m <= end; ++m)
                        {
                            string option = "";
                            if (m >= 2)
                            {
                                option = "page=" + m.ToString();
                            }
                            if (format_kind_ == FormatKind.Rss)
                            {
                                if (option != "")
                                {
                                    option += "&";
                                }
                                option += "rss=2.0";
                            }
                            if (option != "")
                            {
                                option = "?" + option;
                            }
                            if (name_list != null)
                            {
                                name_list.Add(target_name[i] + "/" + duration_name[j] + "/" +
                                        category_list[k].id + option);
                            }
                            if (filename_list != null)
                            {
                                filename_list.Add(duration_short_name[j] + "_" + category_list[k].short_name + "_" +
                                    target_short_name[i] + "_" + m.ToString() + "_");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ダウンロードするランキングRSSのURLをリストで取得 2019/06/26 ADD marky
        /// </summary>
        /// <param name="name_list">URL のリスト</param>
        /// <param name="filename_list">ある規則に従ったファイル名のリスト</param>
        public virtual void GetRankingRssList(ref List<string> name_list, ref List<string> filename_list)
        {
            for (int j = 0; j < duration_name.Length; ++j)
            {
                if (!duration_[j])
                {
                    continue;
                }
                for (int k = 0; k < category_list.Count; ++k)
                {

                    // 2020/08/19 ADD marky RSSを全ページ取得する
                    int end = category_list[k].page[j];
                    for (int m = 1; m <= end; ++m)
                    {

                        string genre = category_list[k].id;
                        string option = "?";
                        if (category_list[k].short_name != "")  //人気のタグの場合
                        {
                            genre = genre.Substring(0, genre.Length - 3);
                            string name = category_list[k].name;
                            option += "tag=";
                            //option += name.Substring(name.IndexOf("：") + 1);
                            //2020/01/19 Update marky タグ名をURLエンコードする
                            string tag = name.Substring(name.IndexOf("：") + 1);
                            tag = Uri.EscapeDataString(tag).Replace("%20", "+");
                            option += tag;
                        }
                        option += "&term=" + term_name[j];

                        // 2020/08/19 ADD marky RSSを全ページ取得する
                        if (m >= 2)
                        {
                            option += "&page=" + m.ToString();
                        }

                        option += "&rss=2.0&lang=ja-jp";
                        if (name_list != null)
                        {
                            name_list.Add("genre/" + genre + option);
                        }
                        if (filename_list != null)
                        {
                            //filename_list.Add(duration_short_name[j] + "_" + category_list[k].name + "_");
                            // 2020/08/19 Update marky RSSを全ページ取得する
                            filename_list.Add(duration_short_name[j] + "_" + category_list[k].name + "_" + m.ToString() + "_");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ダウンロードするランキング過去ログのファイル名をリストで取得 2019/06/26 ADD marky
        /// </summary>
        /// <param name="name_list">URL のリスト</param>
        /// <param name="filename_list">ある規則に従ったファイル名のリスト</param>
        public virtual void GetRankingLogList(ref List<string> name_list, ref List<string> filename_list)
        {
            DateTime logdate;
            string logtime;

            for (int j = 0; j < duration_name.Length; ++j)
            {
                if (!duration_[j])
                {
                    continue;
                }
                switch (duration_name[j])
                {
                    case "monthly":
                        if (gettime_ == 5) { continue; } // 5時は24時と全期間のみ 2019/06/29 ADD
                        logdate = new DateTime(getdate_.Year, getdate_.Month, 1);
                        break;
                    case "weekly":
                        if (gettime_ == 5) { continue; } // 5時は24時と全期間のみ 2019/06/29 ADD
                        string[] week = { "月", "火", "水", "木", "金", "土", "日" };
                        logdate = getdate_.AddDays(Array.IndexOf(week,getdate_.ToString("ddd")) * (-1));
                        break;
                    case "hourly":  //毎時はスキップ
                        continue;
                    default:
                        logdate = getdate_;
                        break;
                }
                // 2019/06/29 ADD
                logtime = logdate.ToString("yyyy-MM-dd");
                if (gettime_ == 5) { logtime += "_05"; }

                for (int k = 0; k < category_list.Count; ++k)
                {
                    if (category_list[k].short_name != "" && duration_name[j] != "daily") // 人気のタグはdailyのみ
                    {
                        continue;
                    }
                    if (name_list != null)
                    {
                        //name_list.Add(duration_name[j] + "/" + logdate.ToString("yyyy-MM-dd") + "/" + category_list[k].id + ".json");
                        // 2019/06/29 Update
                        name_list.Add(duration_name[j] + "/" + logtime + "/" + category_list[k].id + ".json");
                    }
                    if (filename_list != null)
                    {
                        //filename_list.Add(duration_short_name[j] + "_" + category_list[k].name + "_" + logdate.ToString("yyyy-MM-dd") + "_");
                        // 2019/06/29 Update
                        filename_list.Add(duration_short_name[j] + "_" + category_list[k].name + "_" + logtime + "_");
                    }
                }
            }
        }

    }

    /// <summary>
    /// ブラウザからクッキー内の user_session を取得するクラス。かなりいいかげんな実装
    /// </summary>
    static class NicoUserSession
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetCookie(string url, string name, StringBuilder data, ref uint size);

        [DllImport("ieframe.dll")]
        private extern static int IEGetProtectedModeCookie(string url, string name, StringBuilder data, ref uint size, int flags);

        //2019/01/28 ADD marky
        [DllImport("Wininet", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool InternetGetCookieEx(string url, string name, StringBuilder data, ref uint size, int flags, IntPtr reserved);

        private const int INTERNET_COOKIE_THIRD_PARTY = 0x10;
        private const int INTERNET_COOKIE_HTTPONLY = 0x00002000;   //2019/01/28 ADD marky

        /// <summary>
        /// IE6 からクッキーを取得
        /// </summary>
        /// <param name="url">取得するクッキーに関連づけられたURL</param>
        /// <returns>クッキー文字列</returns>
        public static string GetCookieFromIEApi(string url)
        {
            uint size = 4096;
            StringBuilder buff = new StringBuilder(new String(' ', (int)size), (int)size);
            InternetGetCookie(url, null, buff, ref size);
            //2019/01/28 UPDATE marky
            //bool result = InternetGetCookieEx(url, null, buff, ref size, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero);
            return buff.ToString().Replace(';', ',');
        }

        public static string GetProtectedCookieFromIEApi(string url)
        {
            uint size = 4096;
            StringBuilder buff = new StringBuilder(new String(' ', (int)size), (int)size);
            IEGetProtectedModeCookie(url, null, buff, ref size, INTERNET_COOKIE_THIRD_PARTY);
            //2019/01/28 UPDATE marky
            //int result = IEGetProtectedModeCookie(url, null, buff, ref size, INTERNET_COOKIE_HTTPONLY);
            return buff.ToString().Replace(';', ',');
        }

        ///// <summary>
        ///// IE から user_session を取得 2022/08/07 DEL marky
        ///// </summary>
        ///// <param name="url">サイト（ニコニコ動画）のURL</param>
        ///// <returns>user_session</returns>
        //public static string GetUserSessionFromIE(string url)
        //{
        //    string user_session = "";
        //    string cookie;
        //    if (System.Environment.OSVersion.Version.Major >= 6) // Windows Vista or later
        //    {
        //        try
        //        {
        //            // まずは integrity level が低（保護モード）のクッキーを取得してみる。
        //            // これは IE8 以降でしか動かない
        //            // （動作未確認）
        //            cookie = GetProtectedCookieFromIEApi(url);
        //            user_session = CutUserSession(cookie);
        //        }
        //        catch (Exception) { }
        //    }
        //    if (user_session != "")
        //    {
        //        return user_session;
        //    }
        //    try
        //    {
        //        // 次に integrity level が中（通常モード）のクッキーを取得してみる。
        //        cookie = GetCookieFromIEApi(url);
        //        user_session = CutUserSession(cookie);
        //    }
        //    catch (Exception) { }
        //    if (user_session != "")
        //    {
        //        return user_session;
        //    }
        //    // それでも取得できなければ直接クッキーファイルを解析
        //    return GetUserSessionFromIECookieFile();
        //}

        /// <summary>
        /// Firefox3 から user_session を取得。
        /// Firefox のプロファイルディレクトリを firefox_profile_dir に指定する。
        /// Firefox のプロファイルディレクトリからデータを読み込めなかったときは、FileNotFoundException を投げる。
        /// それ以外のエラーが起こった場合、例外を投げずに空文字を返す。
        /// </summary>
        /// <returns>user_session</returns>
        public static string GetUserSessionFromFilefox3(string firefox_profile_dir)
        {
            string cand_user_session = "";
            int cand_expire_time = 0;
            bool is_firefox_profile_dir_empty = string.IsNullOrEmpty(firefox_profile_dir);

            try
            {
                if (string.IsNullOrEmpty(firefox_profile_dir))
                {
                    firefox_profile_dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Mozilla\\Firefox\\Profiles\\";
                }
                string sqlist_filename = SearchFile("cookies.sqlite", firefox_profile_dir);
                if (sqlist_filename == "" || !File.Exists(sqlist_filename))
                {
                    throw new FileNotFoundException();
                }
                FileStream fs = new FileStream(sqlist_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                for (int i = 0; i < data.Length; ++i)
                {
                    int pos = i;
                    if (!MatchString(data, "user_session_", ref pos))
                    {
                        continue;
                    }
                    //2019/01/28 ADD Start marky user_session_secureは無視
                    if (MatchString(data, "secure", ref pos))
                    {
                        continue;
                    }
                    //2019/01/28 ADD End
                    if (!MatchDigitUnderscore(data, ref pos))
                    {
                        i = pos - 1;
                        continue;
                    }
                    string user_session = "";
                    for (int k = i; k < pos; ++k)
                    {
                        user_session += (char)data[k];
                    }
                    if (!MatchString(data, ".nicovideo.jp/", ref pos))
                    {
                        i = pos - 1;
                        continue;
                    }
                    if (pos + 4 > data.Length)
                    {
                        i = pos - 1;
                        continue;
                    }
                    // クッキーの有効時刻（unixtime）を取得
                    int expire_time = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];

                    // 2022/08/07 DEL marky 日付が10年？となった
                    //DateTime dt = IJStringUtil.UnixToDate(expire_time);
                    //DateTime now = DateTime.Now;
                    //
                    //if (dt < now.AddYears(-1) || now.AddYears(1) < dt) // 現在から1年前後以外の日付は無効とみなす
                    //{
                    //    i = pos - 1;
                    //    continue;
                    //}

                    if (expire_time > cand_expire_time) // クッキーの有効時刻が最も遅いものを採用
                    {
                        cand_expire_time = expire_time;
                        cand_user_session = user_session;
                    }
                    i = pos - 1;
                }
            }
            catch (FileNotFoundException exception)
            {
                if (!is_firefox_profile_dir_empty)
                {
                    throw exception;
                }
            }
            catch (Exception) { }
            return cand_user_session;
        }

        private static bool MatchString(byte[] data, string match_string, ref int index)
        {
            if (index + match_string.Length > data.Length)
            {
                return false;
            }

            int pos = index;

            for (int i = 0; i < match_string.Length; ++i)
            {
                if (data[i + pos] != (byte)match_string[i])
                {
                    return false;
                }
            }
            index = pos + match_string.Length;
            return true;
        }

        private static bool MatchDigitUnderscore(byte[] data, ref int index)
        {
            //--- 2014/10/22 UPDATE marky
            //if (!char.IsDigit((char)data[index]) && data[index] != (byte)'_')
            if (!char.IsLetterOrDigit((char)data[index]) && data[index] != (byte)'_')
            //---
            {
                return false;
            }
            while (index < data.Length)
            {
                //--- 2014/10/22 UPDATE marky
                //if (char.IsDigit((char)data[index]) || data[index] == (byte)'_')
                if (char.IsLetterOrDigit((char)data[index]) || data[index] == (byte)'_')
                //---
                {
                    ++index;
                }
                else
                {
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// Opera から user_session を取得。エラーが起こった場合、例外を投げずに空文字を返す
        /// 動くかどうか未確認
        /// </summary>
        /// <returns>user_session</returns>
        public static string GetUserSessionFromOpera()
        {
            try
            {
                //string cookie_filename = System.Environment.GetEnvironmentVariable("APPDATA") + @"\Opera\Opera\cookies4.dat";
                //2019/01/28 UPDATE marky ChromeエンジンOperaに対応
                string cookie_filename = System.Environment.GetEnvironmentVariable("APPDATA") + @"\Opera Software\Opera Stable\Cookies";

                if (cookie_filename == "" || !File.Exists(cookie_filename))
                {
                    //2022/08/07 ADD marky
                    cookie_filename = System.Environment.GetEnvironmentVariable("APPDATA") + @"\Opera Software\Opera Stable\Network\Cookies";
                    if (cookie_filename == "" || !File.Exists(cookie_filename))
                    {
                        return "";
                    }
                }
                FileStream fs = new FileStream(cookie_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                //for (int i = 0; i < data.Length; ++i)
                //{
                //    int pos = i;
                //    if (!MatchString(data, "user_session_", ref pos))
                //    {
                //        continue;
                //    }
                //    if (!MatchDigitUnderscore(data, ref pos))
                //    {
                //        i = pos - 1;
                //        continue;
                //    }
                //    string user_session = "";
                //    for (int k = i; k < pos; ++k)
                //    {
                //        user_session += (char)data[k];
                //    }
                //    return user_session;
                //}

                ////2019/01/28 UPDATE marky ChromeエンジンOperaに対応
                //for (int i = 0; i < data.Length; ++i)
                //{
                //    int pos = i;
                //    if (MatchString(data, "user_session/", ref pos))   //name="user_session",value="",path="/"を探す（2回ヒットする）
                //    {
                //        pos = pos + 16;                         //pathから16ﾊﾞｲﾄ後にencrypted_valueが始まるぽい
                //        //encrypted_valueは[1 0 0]で始まるぽい
                //        if (((long)data[pos] == 1) && ((long)data[pos + 1] == 0) && ((long)data[pos + 2] == 0))
                //        {
                //            Byte[] encryptedValue = new Byte[310];  //encrypted_valueは310ﾊﾞｲﾄぽい
                //            for (int k = 0; k < 310; ++k)
                //            {
                //                encryptedValue[k] = data[pos + k];
                //            }
                //            string user_session = "";
                //            var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedValue, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                //            var plainText = Encoding.ASCII.GetString(decodedData);
                //            user_session = plainText;
                //            return user_session;
                //        }
                //        else
                //        {
                //            i = pos;
                //            continue;
                //        }
                //    }
                //}

                //2022/08/07 UPDATE marky AES暗号化に対応
                string local_state = System.Environment.GetEnvironmentVariable("APPDATA") + @"\Opera Software\Opera Stable\Local State";
                string user_session = GetDecryptData(data, local_state);
                return user_session;
            }
            catch (Exception) { }
            return "";
        }

        /// <summary>
        /// Chrome から user_session を取得。エラーが起こった場合、例外を投げずに空文字を返す
        /// </summary>
        /// <returns>user_session</returns>
        public static string GetUserSessionFromChrome()
        {
            string cand_user_session = "";
            //long cand_expire_time = 0;

            try
            {
                string cookie_filename = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Google\Chrome\User Data\Default\Cookies";

                if (cookie_filename == "" || !File.Exists(cookie_filename))
                {
                    //2022/08/07 ADD marky
                    cookie_filename = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Google\Chrome\User Data\Default\Network\Cookies";
                    if (cookie_filename == "" || !File.Exists(cookie_filename))
                    {
                        return "";
                    }
                }

                FileStream fs = new FileStream(cookie_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                //for (int i = 0; i < data.Length; ++i)
                //{
                //    int pos = i;
                //    if (!MatchString(data, "user_session_", ref pos))
                //    {
                //        continue;
                //    }
                //    if (!MatchDigitUnderscore(data, ref pos))
                //    {
                //        i = pos - 1;
                //        continue;
                //    }
                //    string user_session = "";
                //    for (int k = i; k < pos; ++k)
                //    {
                //        user_session += (char)data[k];
                //    }
                //    if (!MatchString(data, "/", ref pos))
                //    {
                //        i = pos - 1;
                //        continue;
                //    }
                //    if (pos + 8 > data.Length)
                //    {
                //        i = pos - 1;
                //        continue;
                //    }
                //    // クッキーの有効時刻（64ビットunixtime？）を取得
                //    long expire_time = ((long)data[pos] << 56) | ((long)data[pos + 1] << 48) |
                //        ((long)data[pos + 2] << 40) | ((long)data[pos + 3] << 32) |
                //        ((long)data[pos + 4] << 24) | ((long)data[pos + 5] << 16) |
                //        ((long)data[pos + 6] << 8) | (long)data[pos + 7];

                //    if (expire_time > cand_expire_time) // クッキーの有効時刻が最も遅いものを採用
                //    {
                //        cand_expire_time = expire_time;
                //        cand_user_session = user_session;
                //    }
                //    i = pos - 1;
                //}

                ////2019/01/28 UPDATE marky クッキー値の暗号化に対応
                // for (int i = 0; i < data.Length; ++i)
                // {
                //     int pos = i;
                //     if (MatchString(data, "user_session/", ref pos))   //name="user_session",value="",path="/"を探す（2回ヒットする）
                //     {
                //         pos = pos + 16;                         //pathから16ﾊﾞｲﾄ後にencrypted_valueが始まるぽい
                //         encrypted_valueは[1 0 0]で始まるぽい ->DPAPIで暗号化されたcookie
                //         if (((long)data[pos] == 1) && ((long)data[pos + 1] == 0) && ((long)data[pos + 2] == 0))
                //         {
                //             Byte[] encryptedValue = new Byte[310];  //encrypted_valueは310ﾊﾞｲﾄぽい
                //             for (int k = 0; k < 310; ++k)
                //             {
                //                 encryptedValue[k] = data[pos + k];
                //             }

                //             var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedValue, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                //             var plainText = Encoding.ASCII.GetString(decodedData);
                //             cand_user_session = plainText;

                //             break;
                //         }
                //         else
                //         {
                //             i = pos;
                //             continue;
                //         }
                //     }
                // }

                //2022/08/07 UPDATE marky AES暗号化に対応
                string local_state = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Google\Chrome\User Data\Local State";
                cand_user_session = GetDecryptData(data, local_state);
            }
            catch (Exception) { }
            return cand_user_session;
        }

        /// <summary>
        /// 2022/08/07 ADD marky
        /// Edge から user_session を取得。エラーが起こった場合、例外を投げずに空文字を返す
        /// </summary>
        /// <returns>user_session</returns>
        public static string GetUserSessionFromEdge()
        {
            string cand_user_session = "";

            try
            {
                string cookie_filename = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Edge\User Data\Default\Cookies";

                if (cookie_filename == "" || !File.Exists(cookie_filename))
                {
                    //2022/08/07 ADD marky
                    cookie_filename = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Edge\User Data\Default\Network\Cookies";
                    if (cookie_filename == "" || !File.Exists(cookie_filename))
                    {
                        return "";
                    }
                }

                FileStream fs = new FileStream(cookie_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                string local_state = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Edge\User Data\Local State";
                cand_user_session = GetDecryptData(data, local_state);
            }
            catch (Exception) { }
            return cand_user_session;
        }

        /// <summary>
        /// 2022/08/07 ADD marky
        /// encrypted_valueを復号化しuser_sessionを取得。
        /// <param name="data">cookie data</param>
        /// <param name="local_state">local_stateファイルパス</param>
        /// </summary>
        /// <returns>user_session</returns>
        private static string GetDecryptData(byte[] data, string local_state)
        {
            string plainText = "";

            string str = IJFile.ReadVer2(local_state, IJFile.EncodingPriority.Auto);
            int start = str.IndexOf("encrypted_key\":\"") + 16;   //"os_crypt":{"encrypted_key":"XXXXXX"}
            if (start < 16)
            {
                return "";
            }
            int end = str.IndexOf('"', start);
            if (end < 0)
            {
                return "";
            }
            string key = str.Substring(start, end - start);

            for (int i = 0; i < data.Length; ++i)
            {
                int pos = i;
                if (MatchString(data, "user_session", ref pos))   //name="user_session",value="",encrypted_value=[],path="/"を探す（4回ヒットする）
                {
                    //encrypted_valueは[v 1 0]で始まるぽい ->AESで暗号化されたcookie
                    if (((long)data[pos] == 118) && ((long)data[pos + 1] == 49) && ((long)data[pos + 2] == 48))
                    {
                        //pos = pos + 15;                       //'v10'+nonce 12bytesを除いた部分
                        int len = 0;
                        for (int k = 15; k < 150; ++k)
                        {
                            //path="/"を探す
                            if (((long)data[pos + k] == 47) && ((long)data[pos + k + 1] == 0) && ((long)data[pos + k + 2] == 48))
                            {
                                len = k;
                                break;
                            }
                        }
                        if (len == 0)
                        {
                            return "";
                        }
                        Byte[] encryptedValue = new Byte[len];  //encrypted_valueは116 or 117ﾊﾞｲﾄぽい
                        for (int k = 0; k < len; ++k)
                        {
                            encryptedValue[k] = data[pos + k];
                        }

                        var decodedKey = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(key).Skip(5).ToArray(), null, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                        plainText = _decryptWithKey(encryptedValue, decodedKey, 3);

                        break;
                    }
                    else
                    {
                        i = pos;
                        continue;
                    }
                }
            }
            return plainText;
        }
        /// <summary>
        /// 2022/08/07 ADD marky
        /// AES-GCMを復号化した文字列を返す
        /// <param name="message">暗号化文字列</param>
        /// <param name="key">復号キー</param>
        /// <param name="nonSecretPayloadLength">非暗号化データ長？</param>
        /// </summary>
        /// <returns>plainText</returns>
        private static string _decryptWithKey(byte[] message, byte[] key, int nonSecretPayloadLength)
        {
            const int KEY_BIT_SIZE = 256;
            const int MAC_BIT_SIZE = 128;
            const int NONCE_BIT_SIZE = 96;

            if (key == null || key.Length != KEY_BIT_SIZE / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KEY_BIT_SIZE), "key");
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message required!", "message");

            using (var cipherStream = new MemoryStream(message))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);
                var nonce = cipherReader.ReadBytes(NONCE_BIT_SIZE / 8);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), MAC_BIT_SIZE, nonce);
                cipher.Init(false, parameters);
                var cipherText = cipherReader.ReadBytes(message.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);
                }
                catch (InvalidCipherTextException)
                {
                    return null;
                }
                return Encoding.Default.GetString(plainText);
            }
        }

        ///// <summary>
        ///// IE7 から user_session を取得。エラーが起こった場合、例外を投げずに空文字を返す 2022/08/07 DEL marky
        ///// </summary>
        ///// <returns>user_session</returns>
        //public static string GetUserSessionFromIECookieFile()
        //{
        //    string user_session = "";

        //    string profile_dir = System.Environment.GetEnvironmentVariable("USERPROFILE");
        //    user_session = GetUserSessionFromDirectory(profile_dir + "\\AppData\\Roaming\\Microsoft\\Windows\\Cookies\\Low\\");
        //    if (user_session == "")
        //    {
        //        user_session = GetUserSessionFromDirectory(profile_dir + "\\AppData\\Roaming\\Microsoft\\Windows\\Cookies\\");
        //    }
        //    if (user_session == "")
        //    {
        //        user_session = GetUserSessionFromDirectory(profile_dir + "\\Cookies\\");
        //    }
        //    return user_session;
        //}

        //private static string GetUserSessionFromDirectory(string dir_name)
        //{
        //    string user_session = "";
        //    if (Directory.Exists(dir_name))
        //    {
        //        try
        //        {
        //            string[] files = Directory.GetFiles(dir_name);

        //            for (int i = 0; i < files.Length; ++i)
        //            {
        //                string filename = Path.GetFileName(files[i]);
        //                if (filename.IndexOf("nicovideo") >= 0 && filename.IndexOf("www") < 0)
        //                {
        //                    user_session = CutUserSession(File.ReadAllText(files[i], Encoding.GetEncoding(932)));

        //                    // user_sessionが見つかった場合は検索終了
        //                    if (!string.IsNullOrEmpty(user_session))
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception) { }
        //    }
        //    return user_session;
        //}

        ///// <summary>
        ///// 文字列から user_session_ で始まる文字列を切り出して返す。数字とアンダーバー以外の文字で切れる。 2022/08/07 DEL marky
        ///// </summary>
        ///// <param name="str">切り出す対象文字列</param>
        ///// <returns>user_session 文字列。見つからなければ空文字を返す</returns>
        //private static string CutUserSession(string str)
        //{
        //    int start = str.IndexOf("user_session_");
        //    if (start >= 0)
        //    {
        //        int index = start + "user_session_".Length;
        //        //--- 2014/10/22 UPDATE marky
        //        //while (index < str.Length && ('0' <= str[index] && str[index] <= '9' || str[index] == '_'))
        //        while (index < str.Length && ('0' <= str[index] && str[index] <= 'z' || str[index] == '_'))
        //        //---
        //        {
        //            ++index;
        //        }
        //        return str.Substring(start, index - start);
        //    }
        //    return "";
        //}

        /// <summary>
        /// DateTime を Unix 時間に変換
        /// </summary>
        /// <param name="datetime">変換する DateTime</param>
        /// <returns>Unix 時間の文字列</returns>
        public static string DateToUnix(DateTime datetime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 9, 0, 0);
            TimeSpan span = datetime - origin;
            return ((int)span.TotalSeconds).ToString();
        }

        // dir_name 内を再帰的に調べて filename を含む最初のファイル名を返す
        private static string SearchFile(string filename, string dir_name)
        {
            try
            {
                if (!dir_name.EndsWith("\\"))
                {
                    dir_name += "\\";
                }
                string[] files = Directory.GetFiles(dir_name);
                for (int i = 0; i < files.Length; ++i)
                {
                    if (Path.GetFileName(files[i]).IndexOf(filename) >= 0)
                    {
                        return files[i];
                    }
                }
                string[] dirs = Directory.GetDirectories(dir_name);
                for (int i = 0; i < dirs.Length; ++i)
                {
                    string ret = SearchFile(filename, dirs[i]);
                    if (ret != "")
                    {
                        return ret;
                    }
                }
            }
            catch (UnauthorizedAccessException) { } // 無視
            catch (IOException) { } // 無視
            return "";
        }
    }

    public class NiconicoLoginException : Exception
    {
        public NiconicoLoginException()
            : base("ログインされていません。")
        {

        }
    }

    public class NiconicoAccessFailedException : Exception
    {
        public NiconicoAccessFailedException()
            : base("ファイル情報の取得に失敗しました。ログインされていないかもしれません。")
        {

        }

        public NiconicoAccessFailedException(string message)
            : base(message)
        {

        }
    }

    public class NiconicoAccessDeniedException : Exception
    {

    }

    public class NiconicoAddingMylistFailedException : Exception
    {
        public NiconicoAddingMylistFailedException() { }

        public NiconicoAddingMylistFailedException(string message)
            : base(message)
        {

        }
    }

    public class NiconicoAddingMylistExistException : NiconicoAddingMylistFailedException
    {
        public NiconicoAddingMylistExistException() { }

        public NiconicoAddingMylistExistException(string message)
            : base(message)
        {

        }
    }

    public class NiconicoFormatException : Exception
    {
        public NiconicoFormatException()
            : base("HTMLの解析に失敗しました。")
        {

        }
        //2019/06/26 ADD marky
        public NiconicoFormatException(string message)
            : base(message)
        {

        }
    }

    //2018-09-14 ADD marky 検索API v2
    //[DataContract]
    //class sessionAPI
    //{
    //    [DataMember]
    //    public string recipe_id = "";

    //    [DataMember]
    //    public string player_id = "";

    //    [DataMember]
    //    public  List<string> videos = null;

    //    [DataMember]
    //    public List<string> audios = null;

    //    [DataMember]
    //    public List<string> movies = null;

    //    [DataMember]
    //    public List<string> protocols = null;

    //    [DataMember]
    //    public auth_typesC auth_types = null;

    //    [DataContract]
    //    public class auth_typesC
    //    {
    //        [DataMember]
    //        public string http = "";
    //    }

    //    [DataMember]
    //    public string service_user_id = "";

    //    [DataMember]
    //    public string token = "";

    //    [DataMember]
    //    public string signature = "";

    //    [DataMember]
    //    public string content_id = "";

    //    [DataMember]
    //    public long heartbeat_lifetime = 0;

    //    [DataMember]
    //    public long content_key_timeout = 0;

    //    [DataMember]
    //    public float priority = 0;

    //    [DataMember]
    //    public List<urlC> urls = null;

    //    [DataContract]
    //    public class urlC
    //    {
    //        [DataMember]
    //        public string url = "";

    //        [DataMember]
    //        public Boolean is_well_known_port = false;

    //        [DataMember]
    //        public Boolean is_ssl = false;
    //    }
    //}
    // 2021/03/16 Update marky  sessionフォーマット変更
    [DataContract]
    class sessionAPI
    {
        [DataMember]
        public string recipeId = "";

        [DataMember]
        public string playerId = "";

        [DataMember]
        public List<string> videos = null;

        [DataMember]
        public List<string> audios = null;

        [DataMember]
        public List<string> movies = null;

        [DataMember]
        public List<string> protocols = null;

        [DataMember]
        public auth_typesC authTypes = null;

        [DataContract]
        public class auth_typesC
        {
            [DataMember]
            public string http = "";
            public string hls = "";
        }

        [DataMember]
        public string serviceUserId = "";

        [DataMember]
        public string token = "";

        [DataMember]
        public string signature = "";

        [DataMember]
        public string contentId = "";

        [DataMember]
        public long heartbeatLifetime = 0;

        [DataMember]
        public long contentKeyTimeout = 0;

        [DataMember]
        public float priority = 0;

        [DataMember]
        public List<string> transferPresets = null;

        [DataMember]
        public List<urlC> urls = null;

        [DataContract]
        public class urlC
        {
            [DataMember]
            public string url = "";

            [DataMember]
            public Boolean isWellKnownPort = false;

            [DataMember]
            public Boolean isSsl = false;
        }
    }

    // 2019/06/26 ADD marky ジャンルor人気のタグ ランキング過去ログ
    [DataContract]
    class GenreTagLogList
    {
        [DataMember]
        public string id = "";

        [DataMember]
        public string title = "";

        [DataMember]
        public string registeredAt = "";

        [DataMember]
        public CountC count = null;

        [DataContract]
        public class CountC
        {
            [DataMember]
            public int view = 0;

            [DataMember]
            public int comment = 0;

            [DataMember]
            public int mylist = 0;

            [DataMember]
            public string like = "";    // 2021/07/30 ADD marky
        }

        [DataMember]
        public ThumbnailC thumbnail = null;

        [DataContract]
        public class ThumbnailC
        {
            [DataMember]
            public string url = "";

            [DataMember]
            public string middleUrl = "";

            [DataMember]
            public string largeUrl = "";
        }
    }

}
