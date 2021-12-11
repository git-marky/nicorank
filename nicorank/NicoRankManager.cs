// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using IJLib;
using NicoTools;
using System.ComponentModel;

namespace nicorank
{
    public class NicoRankManager
    {
        private NicoNetworkManager nico_download_;
        private VideoConverter video_translater_;
        private NicoListManager nico_list_manager_;
        private NicoMylist nico_mylist_;
        private NicoTagManager tag_manager_;
        private NicoCommentManager comment_manager_;

        private CancelObject cancel_object_ = new CancelObject();
        private NicoNetwork niconico_network_ = new NicoNetwork();

        private NicoPathManager path_mgr_ = new NicoPathManager();
        private ArrayList param_;
        private bool is_running_ = false; // 実行中かどうかを表す

        private NicoRankManagerMsgReceiver msg_receiver_;

        public delegate void ThreadStarterDelegate();

        // 非同期スレッド処理終了時に呼び出されるデリゲート
        public delegate void ThreadCompletedDelegate(AsyncCompletedEventArgs e);

        public NicoRankManager(NicoRankManagerMsgReceiver msg_receiver)
        {
            msg_receiver_ = msg_receiver;
            nico_download_ = new NicoNetworkManager(niconico_network_, msg_receiver_, cancel_object_);
            video_translater_ = new VideoConverter(msg_receiver_, cancel_object_);
            nico_list_manager_ = new NicoListManager(msg_receiver_);
            nico_mylist_ = new NicoMylist(niconico_network_, msg_receiver_, cancel_object_);
            tag_manager_ = new NicoTagManager(niconico_network_, msg_receiver_, cancel_object_);
            comment_manager_ = new NicoCommentManager(niconico_network_, msg_receiver_, cancel_object_);

            nico_download_.SetDelegateSetDonwloadInfo(msg_receiver_.SetDownloadInfo);
        }

        public NicoNetwork GetNicoNetwork()
        {
            return niconico_network_;
        }

        public CancelObject GetCancelObject()
        {
            return cancel_object_;
        }

        public NicoPathManager GetPathMgr()
        {
            return path_mgr_;
        }

        public void SetParam(ArrayList param)
        {
            param_ = param;
        }

        private void Error(Exception e)
        {
            msg_receiver_.Write("エラー：" + e.Message + "\r\n");
            IJLog.Writeln("エラー\r\n---メッセージ\r\n" +
                e.Message + "\r\n---ソース\r\n" + e.Source + "\r\n---スタックトレース\r\n" +
                e.StackTrace + "\r\n---ターゲット\r\n" + e.TargetSite + "\r\n---文字列\r\n" +
                e.ToString());
        }

        private ThreadStarterDelegate current_delegate_;
        private ThreadCompletedDelegate current_completed_delegate;

        public void StartNewThread(ThreadStarterDelegate ts_delegate, ThreadCompletedDelegate completed_delegate, params object[] param_array)
        {
            if (is_running_)
            {
                msg_receiver_.Write("別の処理を実行中です。\r\n");
            }
            else
            {
                is_running_ = true;
                current_delegate_ = ts_delegate;
                current_completed_delegate = completed_delegate;
                param_ = new ArrayList(param_array);
                System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(StartNewThreadInner));
                t.IsBackground = true;
                t.Priority = System.Threading.ThreadPriority.BelowNormal;

                t.Start();
            }
        }

        private void StartNewThreadInner()
        {
            try
            {
                current_delegate_();

                // 非同期処理終了(エラー：NO、中止：NO)
                if (current_completed_delegate != null)
                {
                    AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(null, false, null);
                    current_completed_delegate(arg);
                }
            }
            catch (System.Threading.ThreadAbortException e)
            {
                // 非同期処理終了(エラー：YES、中止：YES)
                if (current_completed_delegate != null)
                {
                    AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(e, true, null);
                    current_completed_delegate(arg);
                }
                msg_receiver_.Write("処理は中止されました。ファイルの処理中に中止された場合はファイルが壊れている可能性があります。\r\n");
            }
            catch (MyCancelException)
            {
                // 非同期処理終了(エラー：NO、中止：YES)
                if (current_completed_delegate != null)
                {
                    AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(null, true, null);
                    current_completed_delegate(arg);
                }
            }
            catch (Exception e)
            {
                // 非同期処理終了(エラー：YES、中止：NO)
                if (current_completed_delegate != null)
                {
                    AsyncCompletedEventArgs arg = new AsyncCompletedEventArgs(e, false, null);
                    current_completed_delegate(arg);
                }
                Error(e);
            }
            finally
            {
                is_running_ = false;
                if (cancel_object_.IsCanceling())
                {
                    msg_receiver_.Write("処理はキャンセルされました。\r\n");
                    cancel_object_.ClearCanceling();
                }
            }
        }

        public void Cancel()
        {
            if (is_running_)
            {
                msg_receiver_.Write("キャンセル処理中です…\r\n");
                cancel_object_.Cancel();
            }
        }

        public void StartNewThreadNotCatch(ThreadStarterDelegate ts_delegate, params object[] param_array)
        {
            current_delegate_ = ts_delegate;
            param_ = new ArrayList(param_array);
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(current_delegate_));
            t.IsBackground = true;
            t.Start();
        }

        public void CheckLogin()
        {
            msg_receiver_.Write("ログインチェック中です…\r\n");
            try
            {
                if (niconico_network_.IsLoginNiconico())
                {
                    msg_receiver_.Write("ログインされています。\r\n");
                }
                else
                {
                    msg_receiver_.Write("ログインされていません。\r\n");
                }
            }
            catch (NiconicoFormatException)
            {
                msg_receiver_.Write("ログインに成功したか確認できません。ニコニコ動画が仕様変更された可能性があります。\r\n");
            }
        }

        public void LoginNiconico()
        {
            msg_receiver_.Write("ログインしています…\r\n");
            try
            {
                if (niconico_network_.LoginNiconico((string)param_[0], (string)param_[1]))
                {
                    msg_receiver_.Write("ログインに成功しました。\r\n");
                }
                else
                {
                    msg_receiver_.Write("ログインに失敗しました。\r\n");
                }
            }
            catch (NiconicoFormatException)
            {
                msg_receiver_.Write("ログインに成功したか確認できません。ニコニコ動画が仕様変更された可能性があります。\r\n");
            }
        }

        public void ReloadCookie(NicoNetwork.CookieKind cookie_kind, string profile_dir)
        {
            niconico_network_.ProfileDir = profile_dir;
            niconico_network_.ReloadCookie(cookie_kind);
        }

        public void SetCookieKind(NicoNetwork.CookieKind cookie_kind)
        {
            niconico_network_.SetCookieKind(cookie_kind);
        }

        public bool LoginNiconico(string user, string password)
        {
            return niconico_network_.LoginNiconico(user, password);
        }

        public string GetUserSession()
        {
            return niconico_network_.GetUserSession();
        }

        public void ResetUserSession(string user_session)
        {
            niconico_network_.ResetUserSession(user_session);
        }

        public void SetNoCache(bool is_no_cache)
        {
            niconico_network_.NoCache = is_no_cache;
        }

        // スレッドを使うため引数なし関数を用意する

        public void DownloadRanking()
        {
            string rank_dl_dir = nico_download_.DownloadRanking((DownloadKind)param_[0], path_mgr_.GetRankDlDir());
            msg_receiver_.UpdateSavedRankDir(rank_dl_dir);
        }

        public void DownloadNicoChart()
        {
            string rank_dl_dir = nico_download_.DownloadNicoChart(path_mgr_.GetRankDlDir(), (DateTime)param_[0], (DateTime)param_[1]);
            msg_receiver_.UpdateSavedRankNicoChartDir(rank_dl_dir);
        }

        public void TranslateVideo()
        {
            video_translater_.TranslateVideo((TranslatingOption)param_[0]);
        }

        public void AnalyzeRanking()
        {
            nico_list_manager_.AnalyzeRanking((InputOutputOption)param_[0], (RankingMethod)param_[1], (NicoListManager.ParseRankingKind)param_[2],
                path_mgr_.GetSavedRankDir());
        }

        public void AnalyzeRankingNicoChart()
        {
            nico_list_manager_.AnalyzeRankingNicoChart((InputOutputOption)param_[0], (RankingMethod)param_[1], path_mgr_.GetSavedRankNicoChartDir(),
                (DateTime)param_[2], (DateTime)param_[3]);
        }

        public void MakeListAndWriteBySearchTag()
        {
            nico_download_.MakeListAndWriteBySearchTag((InputOutputOption)param_[0], (SearchingTagOption)param_[1], (RankingMethod)param_[2]);
        }

        public void MylistSearch()
        {
            nico_download_.MylistSearch((string)param_[0], (InputOutputOption)param_[1], (RankingMethod)param_[2]);
        }

        public void UpdateRankFileByMylist()
        {
            nico_download_.UpdateRankFileByMylist((string)param_[0], (InputOutputOption)param_[1], (RankingMethod)param_[2]);
        }

        public void GetNewArrival()
        {
            nico_download_.GetNewArrival((int)param_[0], (int)param_[1], (InputOutputOption)param_[2], (RankingMethod)param_[3]);
        }

        public void MakeDiff()
        {
            nico_list_manager_.MakeDiff(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1], (DateTime)param_[2]);
        }

        public void MakeDiffB()
        {
            nico_list_manager_.MakeDiffB(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void MakeDup()
        {
            nico_list_manager_.MakeDup(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void MergeRankFileA()
        {
            nico_list_manager_.MergeRankFileA(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void MergeRankFileB()
        {
            nico_list_manager_.MergeRankFileB(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void UpdatePoint()
        {
            nico_list_manager_.UpdatePoint(path_mgr_.GetDiff1Path(), path_mgr_.GetDiff2Path(), (InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void SortRankFile()
        {
            nico_list_manager_.SortRankFile((InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void CalculateSum()
        {
            nico_list_manager_.CalculateSum((InputOutputOption)param_[0], (RankingMethod)param_[1]);
        }

        public void DownloadFlv()
        {
            nico_download_.DownloadFlv((InputOutputOption)param_[0], (string)param_[1], path_mgr_.GetFlvDlDir(), (bool)param_[2]);
        }

        public void RenameFlvInDirectory()
        {
            nico_download_.RenameFlvInDirectory(path_mgr_.GetRenameDir());
        }

        public void DownloadThumbnail()
        {
            nico_download_.DownloadThumbnail((InputOutputOption)param_[0], path_mgr_.GetThumbnailDir());
        }

        public void UpdateDetailInfo()
        {
            nico_download_.UpdateDetailInfo((InputOutputOption)param_[0], (UpdateRankKind)param_[1], (RankingMethod)param_[2], (string)param_[3]);
        }

        public void UpdateUserId()
        {
            string str = nico_download_.UpdateUserId((List<string>)param_[0], (List<string>)param_[1], (string)param_[2]);
            msg_receiver_.SetMakeUserIdOutput(str);
        }

        public void GetDetailInfo()
        {
            nico_download_.GetDetailInfo((List<string>)param_[0], (InputOutputOption)param_[1], (bool)param_[2], (RankingMethod)param_[3], (string)param_[4]);
        }

        public void DrawRankPic()
        {
            video_translater_.DrawRankPic((InputOutputOption)param_[0], path_mgr_.GetLayoutPath(),
                path_mgr_.GetRankPicDir());
        }

        public void MakeScript()
        {
            video_translater_.MakeScript(path_mgr_.GetScriptInputPath(), path_mgr_.GetAviSynthScriptPath(), (InputOutputOption)param_[0]);
        }

        public void MakeAviFromScript()
        {
            video_translater_.MakeAviFromScript(path_mgr_.GetAviSynthScriptPath(), path_mgr_.GetAviFromScriptPath(), path_mgr_.GetFFMpegPath());
        }

        public void MakeScriptAndAvi()
        {
            MakeScript();
            MakeAviFromScript();
        }

        public void EncodeByMencoder()
        {
            video_translater_.EncodeByMencoder(path_mgr_.GetMencPath(), path_mgr_.GetDTransBeforePath(), path_mgr_.GetDTransAfterPath(), (bool)param_[0], (bool)param_[1]);
        }

        public void MakeNewMylistGroup()
        {
            string mylist_id = nico_mylist_.MakeNewMylistGroup((bool)param_[0], (string)param_[1], (string)param_[2], (int)param_[3], (int)param_[4]);
            msg_receiver_.UpdateMylistId(mylist_id);
        }

        public void UpdateMylistGroup()
        {
            nico_mylist_.UpdateMylistGroup((string)param_[0], (bool)param_[1], (string)param_[2], (string)param_[3], (int)param_[4], (int)param_[5]);
        }

        public void AddMylist()
        {
            nico_mylist_.AddMylist((InputOutputOption)param_[0], (string)param_[1]);
        }

        public void AddMultipleMylist()
        {
            nico_mylist_.AddMultipleMylist((InputOutputOption)param_[0], (List<string>)param_[1], (List<int>)param_[2]);
        }

        public void UpdateMylistDescription()
        {
            nico_mylist_.UpdateMylistDescription((string)param_[0], (string)param_[1], (InputOutputOption)param_[2]);
        }

        public void PreviewMylistDescription()
        {
            nico_mylist_.PreviewMylistDescription((string)param_[0], path_mgr_.GetInputRankFilePath());
        }

        public void GetMyMylistList()
        {
            msg_receiver_.Write(nico_mylist_.GetMyMylistList());
        }

        public void AddTags()
        {
            tag_manager_.AddTags((List<string>)param_[0], (List<bool>)param_[1], (string)param_[2]);
        }

        public void PostComment()
        {
            comment_manager_.PostComment((string)param_[0], (string)param_[1], (double)param_[2]);
        }

        public void FFmpegExec()
        {
            video_translater_.FFmpegExec((string)param_[0], path_mgr_.GetFFMpegPath());
        }

        public void GetDataFromNicoApi()
        {
            nico_download_.GetDataFromNicoApi();
        }
    }

    public interface NicoRankManagerMsgReceiver : MessageOut
    {
        void UpdateSavedRankDir(string saved_rank_dir);
        void UpdateSavedRankNicoChartDir(string saved_rank_dir);
        void UpdateMylistId(string id);
        void SetDownloadInfo(string info_text);
        void SetMakeUserIdOutput(string str);
    }
}
