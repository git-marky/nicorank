// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using IJLib;
using NicoTools;
using IJLib.SWF2;

namespace nicorank
{
    /// <summary>
    /// FFmpeg を実行するためのクラス。
    /// 引数を保持する。
    /// </summary>
    public class FFmpeg
    {
        public double start_time = -1.0;
        public double duration = -1.0;
        public bool is_adjust = false;
        public string pix_fmt = "";

        public int changing_width = -1;
        public int changing_height = -1;
        public bool is_fix_aspect = false;

        public bool is_framerate_change;
        public string frame_rate = "";

        public bool is_window_show;

        private string video_codec_ = "";
        private string audio_codec_ = "";
        private string other_audio_option_ = "";
        private FFmpegAppPath app_path_;
        private string info_filename_;
        private FFmpegVideoInfo video_info_ = null;
        private IJProcess.ProcessRunningEventDelegate delegate_ = null;

        public FFmpeg(FFmpegAppPath app_path, string info_filename)
        {
            app_path_ = app_path;
            info_filename_ = info_filename;
        }

        public void SetCodecAviRawVideo()
        {
            video_codec_ = "rawvideo";
        }

        public void SetCodecHuffyuv()
        {
            video_codec_ = "huffyuv";
        }

        public void SetCodecVideoNone()
        {
            video_codec_ = "";
        }

        public void SetCodecWave()
        {
            audio_codec_ = "pcm_s16le";
            other_audio_option_ = "-ar 44100 -ac 2 ";
        }

        public void SetCodecMp3()
        {
            audio_codec_ = "libmp3lame";
            other_audio_option_ = "-ab 64k ";
        }

        public void SetCodecAudioNone()
        {
            audio_codec_ = "";
            other_audio_option_ = "";
        }

        public void InvestigateVideoInfo()
        {
            if (video_info_ == null)
            {
                video_info_ = GetVideoInfo(info_filename_);
            }
        }

        // padding 補正後の幅を取得
        public int GetWidth()
        {
            InvestigateVideoInfo();
            if (!is_fix_aspect || changing_width <= 0 || changing_height <= 0 || video_info_.width == 0 || video_info_.height == 0)
            {
                return changing_width;
            }
            else if (changing_width * video_info_.height > changing_height * video_info_.width) // 左右にパディングする必要あり
            {
                int width = changing_height * video_info_.width / video_info_.height;
                return (width % 2 == 1) ? width - 1 : width;
            }
            else
            {
                return changing_width;
            }

        }

        // padding 補正後の高さを取得
        public int GetHeight()
        {
            InvestigateVideoInfo();
            if (!is_fix_aspect || changing_width <= 0 || changing_height <= 0 || video_info_.width == 0 || video_info_.height == 0)
            {
                return changing_height;
            }
            else if (changing_width * video_info_.height < changing_height * video_info_.width) // 上下にパディングする必要あり
            {
                int height = changing_width * video_info_.height / video_info_.width;
                return (height % 2 == 1) ? height - 1 : height;
            }
            else
            {
                return changing_height;
            }
        }

        private int GetPaddingWidth()
        {
            return (changing_width - GetWidth()) / 2;
        }

        public int GetPaddingLeft()
        {
            int width = GetPaddingWidth();
            return (width % 2 == 1) ? width - 1 : width;
        }

        public int GetPaddingRight()
        {
            int width = GetPaddingWidth();
            return (width % 2 == 1) ? width + 1 : width;
        }

        private int GetPaddingHeight()
        {
            return (changing_height - GetHeight()) / 2;
        }

        public int GetPaddingTop()
        {
            int height = GetPaddingHeight();
            return (height % 2 == 1) ? height - 1 : height;
        }

        public int GetPaddingBottom()
        {
            int height = GetPaddingHeight();
            return (height % 2 == 1) ? height + 1 : height;
        }

        public string GetPaddingOption()
        {
            int pad_left = GetPaddingLeft();
            int pad_top = GetPaddingTop();
            int pad_right = GetPaddingRight();
            int pad_bottom = GetPaddingBottom();

            string pad_option = "";
            if (pad_left > 0)
            {
                pad_option += "-padleft " + pad_left.ToString() + " ";
            }
            if (pad_top > 0)
            {
                pad_option += "-padtop " + pad_top.ToString() + " ";
            }
            if (pad_right > 0)
            {
                pad_option += "-padright " + pad_right.ToString() + " ";
            }
            if (pad_bottom > 0)
            {
                pad_option += "-padbottom " + pad_bottom.ToString() + " ";
            }
            if (pix_fmt == "bgr24" && pad_option != "")
            {
                throw new FFmpegNotSupportPaddingException();
            }
            return pad_option;
        }

        public string GetOption()
        {
            string cut_start_option = "";
            string duration_option = "";
            string framerate_option = "";
            string size_option = "";
            string padding_option = "";
            string video_option = "";
            string audio_option = "";
            string pix_fmt_option = "";

            if (start_time >= 0.0)
            {
                cut_start_option = "-ss " + GetAdjustingStartTime().ToString("0.000") + " ";
            }
            if (duration > 0.0)
            {
                duration_option = "-t " + duration.ToString("0.000") + " ";
            }
            if (video_codec_ != "")
            {
                if (is_framerate_change && frame_rate != "")
                {
                    framerate_option = "-r " + frame_rate + " ";
                }
                if (changing_width > 0 && changing_height > 0)
                {
                    size_option = "-s " + GetWidth() + "x" + GetHeight() + " ";
                }
                if (pix_fmt != "")
                {
                    pix_fmt_option = "-pix_fmt " + pix_fmt + " ";
                }
                if (is_fix_aspect)
                {
                    padding_option = GetPaddingOption();
                }
            }
            if (video_codec_ != "")
            {
                video_option = "-vcodec " + video_codec_ + " ";
            }
            else
            {
                video_option = "-vn ";
            }
            if (audio_codec_ != "")
            {
                audio_option = "-acodec " + audio_codec_ + " " + other_audio_option_;
            }
            else
            {
                audio_option = "-an ";
            }
            return size_option + padding_option +
                cut_start_option + duration_option + pix_fmt_option +
                framerate_option + video_option + audio_option;
        }

        /// <summary>
        /// AVI 動画（rawvideo、WAV 音声付き）に変換する 
        /// </summary>
        public void TranslateToAviWithWav(string src_filename, string dest_filename)
        {
            SetCodecAviRawVideo();
            SetCodecWave();
            RunFFmpeg(src_filename, dest_filename);
        }

        /// <summary>
        /// 音声なし AVI 動画（rawvideo）に変換する
        /// </summary>
        public void TranslateToAviWithoutAudio(string src_filename, string dest_filename)
        {
            SetCodecAviRawVideo();
            SetCodecAudioNone();
            RunFFmpeg(src_filename, dest_filename);
        }

        /// <summary>
        /// AVI 動画（huffyuv、WAV 音声付き）に変換する 
        /// </summary>
        public void TranslateToHuffyuvWithWav(string src_filename, string dest_filename)
        {
            SetCodecHuffyuv();
            SetCodecWave();
            RunFFmpeg(src_filename, dest_filename);
        }

        /// <summary>
        /// 音声なし AVI 動画（huffyuv）に変換する
        /// </summary>
        public void TranslateToHuffyuvWithoutAudio(string src_filename, string dest_filename)
        {
            SetCodecHuffyuv();
            SetCodecAudioNone();
            RunFFmpeg(src_filename, dest_filename);
        }

        /// <summary>
        /// WAV に変換する
        /// </summary>
        public void TranslateToWav(string src_filename, string dest_filename)
        {
            SetCodecVideoNone();
            SetCodecWave();
            RunFFmpeg(src_filename, dest_filename);
        }

        /// <summary>
        /// MP3 に変換する
        /// </summary>
        public void TranslateToMp3(string src_filename, string dest_filename)
        {
            SetCodecVideoNone();
            SetCodecMp3();
            RunFFmpeg(src_filename, dest_filename);
        }

        private void RunFFmpeg(string before_filename, string after_filename)
        {
            string argument = "-i \"" + before_filename + "\" " + GetOption() + "\"" + after_filename + "\"";

            try
            {
                int ret = IJProcess.RunProcessAndWaitForExit(
                   app_path_.ffmpeg_path, argument,
                   is_window_show, delegate_);
                if (ret != 0)
                {
                    throw new FFmpegFailedException("FFmpeg の実行に失敗しました。");
                }
            }
            catch (Exception)
            {
                try
                {
                    File.Delete(after_filename);
                }
                catch (Exception) { }
                throw;
            }
        }

        /// <summary>
        /// 動画から PNG 画像を抜き出す。1秒ごとに1枚抜き出す。
        /// </summary>
        /// <param name="src_filename"></param>
        /// <param name="dest_filename"></param>
        public void MakePng(string src_filename, string dest_filename)
        {
            string size_option = "";
            if (changing_width > 0 && changing_height > 0)
            {
                size_option = "-s " + GetWidth() + "x" + GetHeight() + " ";
            }
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(dest_filename));
            IJProcess.RunProcessAndWaitForExit(app_path_.ffmpeg_path,
                "-i \"" + src_filename + "\" " + size_option + "-an -r 1 -vcodec png \"" + dest_filename + "\"",
                is_window_show, delegate_);
        }

        /// <summary>
        /// 動画の情報を得るために FFmpeg を実行し、結果の情報を文字列で取得する。
        /// </summary>
        public string GetFFmpegOutput(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FFmpegFailedException(filename + " が存在しません。");
            }
            string error_str;
            IJProcess.RunProcessAndWaitForExitAndGetErr(app_path_.ffmpeg_path,
                "-i \"" + filename + "\"", is_window_show, out error_str);

            //IJFile.WriteAppend("ffmpeglog.txt", "\r\n\r\nffmpeglog getflvtime start\r\n" +
            //    output + "\r\n" + error_str + "\r\n");
            return error_str;
        }

        /// <summary>
        /// 動画の情報を得るために FFmpeg を実行し、結果を FFmpegVideoInfo として取得する。
        /// </summary>
        public FFmpegVideoInfo GetVideoInfo(string filename)
        {
            FFmpegVideoInfo info = new FFmpegVideoInfo();
            string output = GetFFmpegOutput(filename);
            int index = output.IndexOf("\nInput") + 3;
            index = output.IndexOf("\n", index) + 1;
            int start = output.IndexOf("Duration", index);
            if (start < 0)
            {
                throw new FFmpegFailedException("FFmpeg が正しく実行されていません。");
            }
            start = output.IndexOf(':', start) + 1;
            int end = output.IndexOf(':', start);
            int hour = int.Parse(output.Substring(start, end - start));
            start = end + 1;
            end = output.IndexOf(':', start);
            int minute = int.Parse(output.Substring(start, end - start));
            start = end + 1;
            end = output.IndexOf('.', start);
            int sec = int.Parse(output.Substring(start, end - start));
            start = end + 1;
            end = output.IndexOf(',', start);
            int decisec = int.Parse(output.Substring(start, end - start));
            info.video_time = (double)(hour * 36000 + minute * 600 + sec * 10 + decisec) / 10.0;

            index = output.IndexOf("Video:", end);
            if (index >= 0)
            {
                index = output.IndexOf(',', index) + 1;
                index = output.IndexOf(',', index) + 2;
                end = index;
                while (end < output.Length && '0' <= output[end] && output[end] <= '9')
                {
                    ++end;
                }
                info.width = int.Parse(output.Substring(index, end - index));
                index = end + 1;
                end = index;
                while (end < output.Length && '0' <= output[end] && output[end] <= '9')
                {
                    ++end;
                }
                info.height = int.Parse(output.Substring(index, end - index));
            }

            return info;
        }

        /// <summary>
        /// FFmpeg で動画の時間を取得してから、調整時間を取得
        /// </summary>
        /// <returns></returns>
        public double GetAdjustingStartTime()
        {
            if (is_adjust)
            {
                InvestigateVideoInfo();
                return GetAdjustingStartTime(video_info_.video_time);
            }
            else
            {
                return start_time;
            }
        }

        /// <summary>
        /// 引数で動画の時間を指定して、調整時間を取得
        /// </summary>
        /// <param name="video_time"></param>
        /// <returns></returns>
        public double GetAdjustingStartTime(double video_time)
        {
            if (is_adjust)
            {
                if (video_time > start_time + duration)
                {
                    return start_time;
                }
                else if (video_time > duration)
                {
                    // 始まる時間を10秒単位にする
                    return (double)((int)((video_time - duration) / 10.0) * 10);
                }
                else
                {
                    return 0.0;
                }
            }
            else
            {
                return start_time;
            }
        }

        /// <summary>
        /// 映像と音声を合成する。
        /// </summary>
        /// <param name="avi_filename">映像ファイル名</param>
        /// <param name="wav_filename">音声ファイル名</param>
        /// <param name="after_filename">出力ファイル名</param>
        public void ComposeAviWav(string avi_filename, string wav_filename, string after_filename)
        {
            string argument = "-i \"" + avi_filename + "\" -i \"" + wav_filename + "\" -vcodec copy -acodec copy -sameq \"" + after_filename + "\"";
            IJProcess.RunProcessAndWaitForExit(app_path_.ffmpeg_path, argument, is_window_show);
        }

        /// <summary>
        /// 音声（WAV）にノーマライズとフェードをかけるため、WAVEFLT2 を実行する。
        /// </summary>
        /// <param name="src_filename">入力ファイル名</param>
        /// <param name="dest_filename">出力ファイル名</param>
        /// <param name="fadein">フェードインをかける秒数</param>
        /// <param name="fadeout">フェードアウトをかける秒数</param>
        public void NormalizeFadeWav(string src_filename, string dest_filename, double fadein, double fadeout)
        {
            string temp_filename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_filename), "wav");
            NormalizeWav(src_filename, temp_filename);
            try
            {
                FadeWav(temp_filename, dest_filename, fadein, fadeout);
            }
            finally
            {
                try
                {
                    File.Delete(temp_filename);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 音声（WAV）にノーマライズとフェードをかけるため、WAVEFLT2 を実行する。
        /// 入力ファイルは上書きされる。
        /// </summary>
        /// <param name="filename">入出力ファイル名</param>
        /// <param name="fadein">フェードインをかける秒数</param>
        /// <param name="fadeout">フェードアウトをかける秒数</param>
        public void NormalizeFadeWav(string filename, double fadein, double fadeout)
        {
            string temp_filename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(filename), "wav");
            NormalizeFadeWav(filename, temp_filename, fadein, fadeout);
            if (File.Exists(temp_filename))
            {
                File.Delete(filename);
                File.Move(temp_filename, filename);
            }
            else
            {
                throw new FFmpegFailedException("ノーマライズフェードに失敗しました。");
            }
        }

        /// <summary>
        /// 音声（WAV）にノーマライズをかけるため、WAVEFLT2 を実行する。
        /// </summary>
        /// <param name="src_filename">入力ファイル名</param>
        /// <param name="dest_filename">出力ファイル名</param>
        public void NormalizeWav(string before_filename, string after_filename)
        {
            try
            {
                int ret = IJProcess.RunProcessAndWaitForExit(
                    app_path_.wavflv_path,
                    "-normal 0 \"" + before_filename + "\" \"" + after_filename + "\"",
                    is_window_show);
                if (ret != 0)
                {
                    throw new FFmpegFailedException("ノーマライズに失敗しました。");
                }
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(after_filename);
                }
                catch (Exception) { }
                throw e;
            }
        }

        /// <summary>
        /// 音声（WAV）にノーマライズをかけるため、WAVEFLT2 を実行する。
        /// 入力ファイルは上書きされる。
        /// </summary>
        /// <param name="filename">入出力ファイル名</param>
        public void NormalizeWav(string filename)
        {
            string temp_filename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(filename), "wav");
            NormalizeWav(filename, temp_filename);
            if (File.Exists(temp_filename))
            {
                File.Delete(filename);
                File.Move(temp_filename, filename);
            }
            else
            {
                throw new FFmpegFailedException("ノーマライズに失敗しました。");
            }
        }

        /// <summary>
        /// 音声（WAV）にフェードをかけるため、WAVEFLT2 を実行する。
        /// </summary>
        /// <param name="src_filename">入力ファイル名</param>
        /// <param name="dest_filename">出力ファイル名</param>
        /// <param name="fadein">フェードインをかける秒数</param>
        /// <param name="fadeout">フェードアウトをかける秒数</param>
        public void FadeWav(string src_filename, string dest_filename, double fadein, double fadeout)
        {
            string fadein_option = "";
            string fadeout_option = "";
            if (fadein > 0.0)
            {
                fadein_option = "-fin " + fadein.ToString("0.000") + " ";
            }
            if (fadeout > 0.0)
            {
                fadeout_option = "-fout " + fadeout.ToString("0.000") + " ";
            }
            try
            {
                int ret = IJProcess.RunProcessAndWaitForExit(
                    app_path_.wavflv_path,
                    fadein_option + fadeout_option + "\"" +
                    src_filename + "\" \"" + dest_filename + "\"",
                    is_window_show);
                if (ret != 0)
                {
                    throw new FFmpegFailedException("フェードに失敗しました。");
                }
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(dest_filename);
                }
                catch (Exception) { }
                throw e;
            }
        }

        /// <summary>
        /// 音声（WAV）にフェードをかけるため、WAVEFLT2 を実行する。
        /// 入力ファイルは上書きされる。
        /// </summary>
        /// <param name="filename">入出力ファイル名</param>
        /// <param name="fadein">フェードインをかける秒数</param>
        /// <param name="fadeout">フェードアウトをかける秒数</param>
        public void FadeWav(string filename, double fadein, double fadeout)
        {
            string temp_filename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(filename), "wav");
            FadeWav(filename, temp_filename, fadein, fadeout);
            if (File.Exists(temp_filename))
            {
                File.Delete(filename);
                File.Move(temp_filename, filename);
            }
            else
            {
                throw new FFmpegFailedException("フェードに失敗しました。");
            }
        }
    }

    /// <summary>
    /// 動画変換のためのクラス
    /// </summary>
    public class VideoConverter
    {
        public enum TransKind { FlvToAvi, FlvToWav, FlvToMp3, FlvToPng };

        private CancelObject cancel_object_;
        private MessageOut msgout_;

        public delegate void StringDelegate(string str);

        private StringDelegate InformMencWatching;

        public VideoConverter(MessageOut msgout, CancelObject cancel_object)
        {
            msgout_ = msgout;
            cancel_object_ = cancel_object;
        }

        public void SetDelegateInformMencWatching(StringDelegate dlg)
        {
            InformMencWatching = dlg;
        }

        public void TranslateVideo(TranslatingOption trans_option)
        {
            if (!File.Exists(trans_option.app_path.ffmpeg_path))
            {
                msgout_.Write("FFmpeg のパスの設定が正しくありません。\r\n");
                return;
            }
            if (trans_option.cut_list_name != "" && !File.Exists(trans_option.cut_list_name))
            {
                msgout_.Write("カットリストファイルが存在しません。\r\n");
                return;
            }
            if (trans_option.IsNeedWavFilter() && !File.Exists(trans_option.app_path.wavflv_path))
            {
                msgout_.Write("WAVEFLT2 のパスの設定が正しくありません。\r\n" +
                    "ノーマライズやフェード機能を使うには WAVEFLT2 が必要です。\r\n" +
                    "WAVEFLT2 については bin フォルダ内の waveflt2 フォルダ内の情報をご覧ください。\r\n");
                return;
            }
            if (trans_option.changing_width >= 0 || trans_option.changing_height >= 0)
            {
                if (trans_option.changing_width % 2 != 0 || trans_option.changing_height % 2 != 0)
                {
                    msgout_.Write("動画のサイズ指定は偶数にする必要があります。\r\n");
                    return;
                }
            }
            List<string> src_file_list;
            List<string> dest_file_list;

            MakeTransList(trans_option, out src_file_list, out dest_file_list);

            string dir_name = (trans_option.trans_file_kind == TranslatingOption.TransFileKind.File ?
                Path.GetDirectoryName(trans_option.trans_after_file_or_dir) : trans_option.trans_after_file_or_dir);
            if (!Directory.Exists(dir_name))
            {
                Directory.CreateDirectory(dir_name);
            }
            CutList cut_list = new CutList(trans_option.cut_list_name, trans_option.cut_start, trans_option.cut_end);
            //bool is_using_cut_list = cut_list_.Load(trans_option.cut_list_name);
            for (int i = 0; i < src_file_list.Count; ++i)
            {
                //CutInfo cut_info = (is_using_cut_list ?
                //    cut_list_.Get(src_file_list[i]) :
                //    new CutInfo(src_file_list[i], trans_option.cut_start, trans_option.cut_end));
                TranslateAll(src_file_list[i], dest_file_list[i], trans_option,
                    cut_list.Get(src_file_list[i]), msgout_, RunningNMMToAvi);
                cancel_object_.CheckCancel();
            }
            msgout_.Write("すべての処理が終了しました。\r\n");
        }

        public static void MakeTransList(TranslatingOption trans_option, out List<string> src_file_list, out List<string> dest_file_list)
        {
            src_file_list = new List<string>();
            dest_file_list = new List<string>();
            switch (trans_option.trans_file_kind)
            {
                case TranslatingOption.TransFileKind.RankFile:
                    RankFile rank_file = trans_option.iooption.GetRankFile();
                    for (int i = 0; i < rank_file.Count; ++i)
                    {
                        if (trans_option.is_only_sm && !rank_file[i].StartsWith("sm"))
                        {
                            continue;
                        }
                        if (trans_option.is_only_nm && !rank_file[i].StartsWith("nm"))
                        {
                            continue;
                        }
                        if (File.Exists(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir) + rank_file[i] + ".swf"))
                        {
                            src_file_list.Add(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir) + rank_file[i] + ".swf");
                        }
                        else if (File.Exists(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir) + rank_file[i] + ".mp4"))
                        {
                            src_file_list.Add(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir) + rank_file[i] + ".mp4");
                        }
                        else
                        {
                            src_file_list.Add(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir) + rank_file[i] + ".flv");
                        }
                        dest_file_list.Add(IJFile.GetAbsoluteDir(trans_option.trans_after_file_or_dir) + rank_file[i] + ".avi");
                    }
                    break;
                case TranslatingOption.TransFileKind.Directory:
                    string[] files = Directory.GetFiles(IJFile.GetAbsoluteDir(trans_option.trans_before_file_or_dir));
                    src_file_list.AddRange(files);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        dest_file_list.Add(IJFile.GetAbsoluteDir(trans_option.trans_after_file_or_dir) + Path.GetFileName(files[i]));
                    }
                    break;
                case TranslatingOption.TransFileKind.File:
                    src_file_list.Add(trans_option.trans_before_file_or_dir);
                    dest_file_list.Add(trans_option.trans_after_file_or_dir);
                    break;
            }
        }

        public void RunningNMMToAvi(int proceeding_value)
        {
            cancel_object_.CheckCancel();
        }

        public void EncodeByMencoder(string menc_path, string input_path, string output_path, bool is_window_show, bool is_overwrite)
        {
            if (System.IO.File.Exists(output_path))
            {
                if (is_overwrite)
                {
                    System.IO.File.Delete(output_path);
                }
                else
                {
                    msgout_.Write(Path.GetFileName(output_path) + " は存在します。\r\n");
                    return;
                }
            }
            msgout_.Write("mencoder によるエンコードを開始します…\r\n");

            IJProcess.RunProcessAndWaitForExitAndGetOutput(menc_path,
                "-of lavf -lavfopts format=flv -ovc vfw -xvfwopts codec=vp6vfw.dll:compdata=1stpass.mcf -oac mp3lame -lameopts abr:br=96 -vf flip,scale=512:384 -sws 9 -af resample=44100 -o \"" +
                output_path + "\" \"" + input_path + "\"",
                is_window_show, Path.GetDirectoryName(menc_path), MencWatchingLoop, null);

            msgout_.Write("1st pass が終了しました。\r\n");

            IJProcess.RunProcessAndWaitForExitAndGetOutput(menc_path,
                "-of lavf -lavfopts format=flv -ovc vfw -xvfwopts codec=vp6vfw.dll:compdata=2ndpass.mcf -oac mp3lame -lameopts abr:br=96 -vf flip,scale=512:384 -sws 9 -af resample=44100 -o \"" +
                output_path + "\" \"" + input_path + "\"",
                is_window_show, Path.GetDirectoryName(menc_path), MencWatchingLoop, null);

            msgout_.Write("エンコードが終了しました。\r\n");
        }

        private void MencWatchingLoop(string str)
        {
            if (InformMencWatching != null)
            {
                InformMencWatching(str);
            }
        }

        public void FFmpegExec(string argument, string ffmpeg_path)
        {
            string o_err_str;
            IJProcess.RunProcessAndWaitForExitAndGetErr(ffmpeg_path,
                argument, false, out o_err_str);
            msgout_.Write(o_err_str);
        }

        /// <summary>
        /// TranslatingOption で指定したとおりに動画の変換を行う。
        /// </summary>
        /// <param name="src_filename">入力ファイル名</param>
        /// <param name="dest_filename">出力ファイル名（変換対象によって、拡張子は適切なものに自動的に置換される）</param>
        /// <param name="translating_option">変換のオプション</param>
        /// <param name="cut_info">動画を切る時間</param>
        /// <param name="informer">情報出力オブジェクト</param>
        /// <param name="dlg">実行最中に呼び出されるコールバック</param>
        private static void TranslateAll(string src_filename, string dest_filename, TranslatingOption translating_option, CutInfo cut_info,
            MessageOut msgout, SWFFile.RunningDelegate dlg)
        {
            msgout.Write(Path.GetFileName(src_filename) + " の変換を開始します。\r\n");

            FFmpeg ffmpeg = GetFFmpegInstance(translating_option, cut_info, src_filename);

            if (File.Exists(src_filename))
            {
                bool is_swf = (NicoUtil.JudgeFileType(src_filename) == NicoUtil.FileType.Swf);

                string dest_avi_filename = Path.ChangeExtension(dest_filename, "avi");
                string dest_wav_filename = Path.ChangeExtension(dest_filename, "wav");
                string dest_mp3_filename = Path.ChangeExtension(dest_filename, "mp3");
                string dest_png_filename = Path.ChangeExtension(dest_filename, "png");

                // Wav変換先が存在するかどうか
                bool is_dest_wav_exist = File.Exists(dest_wav_filename);

                // WAV ファイルの出力が必要かどうか
                bool is_need_wav = (translating_option.is_flv_to_wav && (translating_option.is_overwrite || !is_dest_wav_exist));

                // WAV ファイルが一時的に必要かどうか
                bool is_need_temp_wav = !is_need_wav &&
                    ((translating_option.is_flv_to_avi && translating_option.is_avi_include_audio && translating_option.IsNeedWavFilter()) ||
                    (translating_option.is_flv_to_avi && translating_option.is_avi_include_audio && is_swf) ||
                    (translating_option.is_flv_to_mp3 && translating_option.IsNeedWavFilter())) &&
                    CheckFileIsNeedWav(dest_avi_filename, dest_wav_filename, dest_mp3_filename, translating_option);

                // WAV ファイルが正しく作られたかどうか
                bool is_success_made_wav = false;

                string wav_filename = (is_need_wav ? dest_wav_filename :
                    (is_need_temp_wav ? IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_wav_filename), "wav") : ""));

                SWFFile swf = null;
                string swf_mp3_tempfilename = "";

                if (is_swf)
                {
                    try
                    {
                        ProceedNMMFile(src_filename, dest_mp3_filename, translating_option, msgout, is_need_temp_wav, ref swf, ref swf_mp3_tempfilename);
                        // ここで調整を行っておく
                        ffmpeg.start_time = cut_info.GetAdjustingStartTime(swf.Length);
                        ffmpeg.duration = cut_info.GetDuration();
                        ffmpeg.is_adjust = false;
                    }
                    catch (Exception e)
                    {
                        msgout.Write("エラー：" + e.Message + "\r\n");
                        return;
                    }
                }

                try
                {
                    if (is_need_temp_wav || (is_need_wav && CheckFile(dest_wav_filename, translating_option, msgout)))
                    {
                        is_success_made_wav = ProceedWav(src_filename, ffmpeg, translating_option, msgout, is_swf, dest_wav_filename, is_need_temp_wav, is_success_made_wav, wav_filename, swf_mp3_tempfilename);
                    }

                    if (translating_option.is_flv_to_avi && CheckFile(dest_avi_filename, translating_option, msgout))
                    {
                        ProceedAvi(src_filename, ffmpeg, translating_option, msgout, dlg, is_swf, is_need_wav, is_need_temp_wav, is_success_made_wav, wav_filename, swf, dest_avi_filename);
                    }

                    if (translating_option.is_flv_to_mp3 && CheckFile(dest_mp3_filename, translating_option, msgout))
                    {
                        swf_mp3_tempfilename = ProceedMp3(src_filename, ffmpeg, translating_option, msgout, is_swf, dest_mp3_filename, wav_filename, swf_mp3_tempfilename);
                    }
                    //if (translating_option.is_flv_to_detail)
                    //{
                    //    string argument = translating_option.trans_detail_option.Replace("%%video_id%%", video_id).Replace("%i", video_id);
                    //    argument = argument.Replace("%fi", translating_option.flv_dl_dir + video_id);
                    //    argument = argument.Replace("%fo", translating_option.trans_after_dir + video_id);
                    //    ffmpeg.RunFFmpeg(argument);
                    //}
                }
                catch (Exception e)
                {
                    msgout.Write(e.Message + "\r\n");
                    return;
                }
                finally
                {
                    DeleteTempfile(is_need_temp_wav, is_success_made_wav, wav_filename, swf_mp3_tempfilename);
                }
                if (translating_option.is_flv_to_png && CheckFile(dest_png_filename, translating_option, msgout))
                {
                    if (File.Exists(dest_png_filename))
                    {
                        File.Delete(dest_png_filename);
                    }
                    try
                    {
                        TranslateToPng(src_filename, dest_png_filename, is_swf, swf, ffmpeg, msgout);
                    }
                    catch (Exception e)
                    {
                        msgout.Write("画像の抜き出しに失敗しました。\r\n" + e.Message + "\r\n");
                        return;
                    }
                }
                msgout.Write(Path.GetFileName(src_filename) + " の変換を終了します。\r\n");
            }
            else
            {
                msgout.Write(Path.GetFileName(src_filename) + " は存在しません。\r\n");
            }
        }

        private static FFmpeg GetFFmpegInstance(TranslatingOption option, CutInfo cut_info, string src_filename)
        {
            FFmpeg ffmpeg = new FFmpeg(option.app_path, src_filename);

            ffmpeg.start_time = cut_info.GetStartTime();
            ffmpeg.duration = cut_info.GetDuration();
            ffmpeg.is_adjust = cut_info.IsAdjust();
            switch (option.trans_avi_kind)
            {
                case TransDetailOption.TransAviKind.Normal:
                case TransDetailOption.TransAviKind.Huffyuv:
                    ffmpeg.pix_fmt = "";
                    break;
                case TransDetailOption.TransAviKind.Bgr24Flip:
                case TransDetailOption.TransAviKind.Bgr24:
                    ffmpeg.pix_fmt = "bgr24";
                    break;
                case TransDetailOption.TransAviKind.Yuv420p:
                    ffmpeg.pix_fmt = "yuv420p";
                    break;
            }

            ffmpeg.changing_width = option.changing_width;
            ffmpeg.changing_height = option.changing_height;
            ffmpeg.is_fix_aspect = option.is_fix_aspect;
            ffmpeg.is_framerate_change = option.is_framerate_change;
            ffmpeg.frame_rate = option.frame_rate;

            ffmpeg.is_window_show = option.is_window_show;

            return ffmpeg;
        }

        private static void DeleteTempfile(bool is_need_temp_wav, bool is_success_made_wav, string wav_filename,
            string swf_mp3_tempfilename)
        {
            if (is_need_temp_wav && is_success_made_wav)
            {
                try
                {
                    File.Delete(wav_filename);
                }
                catch (Exception) { }
            }
            if (swf_mp3_tempfilename != "")
            {
                try
                {
                    File.Delete(swf_mp3_tempfilename);
                }
                catch (Exception) { }
            }
        }

        private static string ProceedMp3(string src_filename, FFmpeg ffmpeg, TranslatingOption translating_option,
            MessageOut informer, bool is_swf, string dest_mp3_filename, string wav_filename, string swf_mp3_tempfilename)
        {
            try
            {
                IJFilePack filepack = new IJFilePack(dest_mp3_filename);
                if (is_swf && !translating_option.IsNeedWavFilter() && !translating_option.IsCut())
                {
                    File.Move(swf_mp3_tempfilename, filepack.GetFilename());
                    swf_mp3_tempfilename = "";
                }
                else
                {
                    if (translating_option.IsNeedWavFilter())
                    {
                        // すでにカットされているので再カットしない
                        double temp_start_time = ffmpeg.start_time;
                        double temp_duration = ffmpeg.duration;
                        ffmpeg.start_time = -1.0;
                        ffmpeg.duration = -1.0;
                        ffmpeg.TranslateToMp3(wav_filename, filepack.GetFilename());
                        ffmpeg.start_time = temp_start_time;
                        ffmpeg.duration = temp_duration;
                    }
                    else
                    {
                        ffmpeg.TranslateToMp3(src_filename, filepack.GetFilename());
                    }
                }
                filepack.Move();
            }
            catch (Exception)
            {
                informer.Write("MP3ファイルの作成に失敗しました。\r\n");
                throw;
            }
            return swf_mp3_tempfilename;
        }

        private static void ProceedAvi(string src_filename, FFmpeg ffmpeg, TranslatingOption translating_option,
            MessageOut informer, SWFFile.RunningDelegate dlg, bool is_swf, bool is_need_wav, bool is_need_temp_wav,
            bool is_success_made_wav, string wav_filename, SWFFile swf, string dest_avi_filename)
        {
            if (!translating_option.is_avi_include_audio || !(is_need_wav || is_need_temp_wav) || is_success_made_wav)
            {
                try
                {
                    IJFilePack filepack = new IJFilePack(dest_avi_filename);
                    if (is_swf)
                    {
                        double video_time = swf.Length;
                        double cutting_start_time = ffmpeg.GetAdjustingStartTime(video_time);
                        double cutting_end_time = cutting_start_time + ffmpeg.duration;
                        swf.SaveAvi((translating_option.is_avi_include_audio ? wav_filename : ""), filepack.GetFilename(),
                            cutting_start_time, cutting_end_time, translating_option.changing_width, translating_option.changing_height, false, dlg);
                    }
                    else
                    {
                        TranslateToAvi(src_filename, ((is_need_wav || is_need_temp_wav) ? wav_filename : ""),
                            filepack.GetFilename(), ffmpeg, translating_option);
                    }
                    filepack.Move();
                    // 上下を反転する（FFmpeg の仕様回避）
                    if (!is_swf && translating_option.trans_avi_kind == TransDetailOption.TransAviKind.Bgr24Flip)
                    {
                        IJLib.IJAVI.IJAviManager avi_manager = new IJLib.IJAVI.IJAviManager(dest_avi_filename, true);
                        try
                        {
                            avi_manager.FlipAndWriteAll();
                        }
                        finally
                        {
                            avi_manager.Close();
                        }
                    }
                }
                catch (Exception)
                {
                    informer.Write("AVIファイルの作成に失敗しました。\r\n");
                    if (is_swf)
                    {
                        informer.Write("NMM 動画ではない（swf の）可能性があります。swf には対応していません。\r\n");
                    }
                    throw;
                }
            }
        }

        private static bool ProceedWav(string src_filename, FFmpeg ffmpeg, TranslatingOption translating_option,
            MessageOut informer, bool is_nmm, string dest_wav_filename, bool is_need_temp_wav,
            bool is_success_made_wav, string wav_filename, string swf_mp3_tempfilename)
        {
            if (is_need_temp_wav)
            {
                // 存在しますというメッセージを表示するが、変換は行う
                CheckFile(dest_wav_filename, translating_option, informer);
            }
            try
            {
                IJFilePack filepack = new IJFilePack(wav_filename);
                TranslateToWav((is_nmm ? swf_mp3_tempfilename : src_filename), filepack.GetFilename(), ffmpeg, translating_option);
                filepack.Move();
                is_success_made_wav = true;
            }
            catch (Exception)
            {
                informer.Write("WAVファイルの作成に失敗しました。\r\n");
                throw;
            }
            return is_success_made_wav;
        }

        private static void ProceedNMMFile(string src_filename, string dest_mp3_filename, TranslatingOption translating_option, MessageOut informer, bool is_need_temp_wav, ref SWFFile swf, ref string swf_mp3_tempfilename)
        {
            try
            {
                swf = new SWFFile();
                swf.LoadFromFile(src_filename);
                
            }
            catch (Exception)
            {
                informer.Write("NMMファイル読み込みに失敗しました。\r\n");
                throw;
            }
            if (translating_option.is_flv_to_wav || is_need_temp_wav || translating_option.is_flv_to_mp3)
            {
                swf_mp3_tempfilename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_mp3_filename), "mp3");
                try
                {
                    swf.SaveMp3(swf_mp3_tempfilename);
                }
                catch (Exception)
                {
                    informer.Write("NMMファイルからMP3の抜き出しに失敗しました。\r\n");
                    throw;
                }
            }
        }

        public static bool CheckFileIsNeedWav(string dest_avi_filename, string dest_wav_filename, string dest_mp3_filename, TranslatingOption translating_option)
        {
            if (!translating_option.is_overwrite)
            {
                if (translating_option.is_flv_to_avi)
                {
                    if (!File.Exists(dest_avi_filename))
                    {
                        return true;
                    }
                }
                if (translating_option.is_flv_to_wav)
                {
                    if (!File.Exists(dest_wav_filename))
                    {
                        return true;
                    }
                }
                if (translating_option.is_flv_to_mp3)
                {
                    if (!File.Exists(dest_mp3_filename))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        // wav_filename が空なら（必要な場合）一時ファイルが作られる
        // wav_filename はエフェクトをかける必要がある場合以外使われない
        public static void TranslateToAvi(string src_filename, string wav_filename, string dest_filename, FFmpeg ffmpeg, TranslatingOption translating_option)
        {
            if (translating_option.is_avi_include_audio)
            {
                if (translating_option.IsNeedWavFilter())
                {
                    string temp_wavfilename = (wav_filename != "" ? wav_filename : IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_filename), "wav"));
                    if (wav_filename == "")
                    {
                        TranslateToWav(src_filename, temp_wavfilename, ffmpeg, translating_option);
                    }
                    string temp_avifilename = IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_filename), "avi");
                    try
                    {
                        ffmpeg.TranslateToAviWithoutAudio(src_filename, temp_avifilename);
                        try
                        {
                            ffmpeg.ComposeAviWav(temp_avifilename, temp_wavfilename, dest_filename);
                        }
                        finally
                        {
                            try
                            {
                                File.Delete(temp_avifilename);
                            }
                            catch (Exception) { }
                        }
                    }
                    finally
                    {
                        if (wav_filename == "")
                        {
                            try
                            {
                                File.Delete(temp_wavfilename);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                else
                {
                    if (translating_option.trans_avi_kind == TransDetailOption.TransAviKind.Huffyuv)
                    {
                        ffmpeg.TranslateToHuffyuvWithWav(src_filename, dest_filename);
                    }
                    else
                    {
                        ffmpeg.TranslateToAviWithWav(src_filename, dest_filename);
                    }
                }
            }
            else
            {
                if (translating_option.trans_avi_kind == TransDetailOption.TransAviKind.Huffyuv)
                {
                    ffmpeg.TranslateToHuffyuvWithoutAudio(src_filename, dest_filename);
                }
                else
                {
                    ffmpeg.TranslateToAviWithoutAudio(src_filename, dest_filename);
                }
            }
        }

        // FFmpeg を実行して wav に変換
        // src_filename を FFmpeg の入力に与える。必要なら normalize、Fade を掛けたあと、 dest_filename に出力。
        // src_filename が存在すること、dest_filename が存在しないことは呼び出し前にチェックする必要がある。
        // エラー処理は厳密に行われる。エラーが起こったときは作りかけの dest_filename は削除され、例外が投げられる。
        public static void TranslateToWav(string src_filename, string dest_filename, FFmpeg ffmpeg, TranslatingOption translating_option)
        {
            string temp_wavfilename = (translating_option.IsNeedWavFilter() ?
                IJFile.GetTemporaryFileName(Path.GetDirectoryName(dest_filename), "wav") : dest_filename);

            ffmpeg.TranslateToWav(src_filename, temp_wavfilename);
            if (translating_option.IsNeedWavFilter())
            {
                try
                {
                    if (translating_option.is_normalize && translating_option.IsFade())
                    {
                        ffmpeg.NormalizeFadeWav(temp_wavfilename, dest_filename, translating_option.fadein, translating_option.fadeout);
                    }
                    else if (translating_option.is_normalize)
                    {
                        ffmpeg.NormalizeWav(temp_wavfilename, dest_filename);
                    }
                    else if (translating_option.IsFade())
                    {
                        ffmpeg.FadeWav(temp_wavfilename, dest_filename, translating_option.fadein, translating_option.fadeout);
                    }
                }
                finally
                {
                    try
                    {
                        File.Delete(temp_wavfilename);
                    }
                    catch (Exception) { }
                }
            }
        }

        public static void TranslateToPng(string src_filename, string dest_filename, bool is_nmm, SWFFile swf, FFmpeg ffmpeg, MessageOut informer)
        {
            informer.Write(Path.GetFileName(src_filename) + " から画像を抜き出し中…\r\n");
            string dest_png_dir = GetDestPngDir(dest_filename);
            string dest_png_file = GetDestPngFilename(dest_filename);
            if (is_nmm)
            {
                string dest_jpeg_dir = dest_png_dir + "jpegtmp";
                Directory.CreateDirectory(dest_jpeg_dir);
                swf.SaveJPG(dest_jpeg_dir);
                string[] file = Directory.GetFiles(dest_jpeg_dir);
                for (int i = 0; i < file.Length; ++i)
                {
                    if (ffmpeg.changing_width > 0 && ffmpeg.changing_height > 0)
                    {
                        IJGraphics.CopyImage(dest_png_file.Replace("%4d", (i + 1).ToString("0000")),
                            file[i], ffmpeg.changing_width, ffmpeg.changing_height,
                            System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        IJGraphics.CopyImage(dest_png_file.Replace("%4d", (i + 1).ToString("0000")),
                            file[i],
                            System.Drawing.Imaging.ImageFormat.Png);
                    }
                    File.Delete(file[i]);
                }
                Directory.Delete(dest_jpeg_dir);
                if (file.Length == 0)
                {
                    informer.Write("画像は存在しません。\r\n");
                    return;
                }
            }
            else
            {
                ffmpeg.MakePng(src_filename, dest_png_file);
            }
            ChoosePic(dest_png_dir, dest_filename);
            informer.Write(Path.GetFileName(src_filename) + " から画像を抜き出しました。\r\n");
        }

        private static void ChoosePic(string src_dir, string dest_filename)
        {
            string filename = "";
            string[] files = Directory.GetFiles(src_dir);

            if (files.Length <= 0)
            {
                throw new FileNotFoundException();
            }

            if (files.Length >= 70)
            {
                FileInfo info = new FileInfo(files[70 - 1]);
                if (info.Length >= 15 * 1024)
                {
                    filename = files[70 - 1];
                }
            }

            if (filename == "")
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    FileInfo info = new FileInfo(files[i]);
                    if (info.Length >= 15 * 1024)
                    {
                        filename = files[i];
                        break;
                    }
                }
            }

            if (filename == "")
            {
                filename = files[0];
            }

            File.Copy(filename, dest_filename);
        }

        private static string GetDestPngFilename(string dest_filename)
        {
            return Path.GetDirectoryName(dest_filename) + "\\" +
                Path.GetFileNameWithoutExtension(dest_filename) + "\\" +
                Path.GetFileNameWithoutExtension(dest_filename) + "_%4d.png";
        }

        private static string GetDestPngDir(string dest_filename)
        {
            return Path.GetDirectoryName(dest_filename) + "\\" +
                Path.GetFileNameWithoutExtension(dest_filename) + "\\";
        }

        private static bool CheckFile(string dest_filename, TranslatingOption translating_option, MessageOut informer)
        {
            if (!translating_option.is_overwrite && File.Exists(dest_filename))
            {
                informer.Write(Path.GetFileName(dest_filename) + " は存在します。\r\n");
                return false;
            }
            return true;
        }

        public void DrawRankPic(InputOutputOption iooption, string layout_file_path, string rank_pic_dir)
        {
            if (!System.IO.File.Exists(layout_file_path))
            {
                msgout_.Write("レイアウトファイルが存在しません。");
                return;
            }
            msgout_.Write("画像を生成中…\r\n");
            string layout_text = IJFile.Read(layout_file_path);

            string rank_data = iooption.GetRawText();

            if (!System.IO.Directory.Exists(rank_pic_dir))
            {
                System.IO.Directory.CreateDirectory(rank_pic_dir);
            }

            Layout layout = new Layout(layout_text, Path.GetDirectoryName(layout_file_path));
            layout.SaveAllToFile(new LayoutData(rank_data), rank_pic_dir, msgout_);
            msgout_.Write("画像を生成しました。\r\n");
        }

        public void MakeScript(string script_input_path, string avisynth_script_path, InputOutputOption iooption)
        {
            if (!File.Exists(script_input_path))
            {
                throw new FileNotFoundException(Path.GetFileName(script_input_path) + " が見つかりません。");
            }
            AvisynthScriptGenerator generator = new AvisynthScriptGenerator();
            string rank_file_text = "";
            try
            {
                rank_file_text = iooption.GetRawText();
            }
            catch (FileNotFoundException) { } // ランクファイルがなければ空テキストにする

            string script = generator.GenerateScript(IJFile.Read(script_input_path), rank_file_text);
            IJFile.Write(avisynth_script_path, script);
            msgout_.Write("スクリプトを作成しました。\r\n");
        }

        public void MakeAviFromScript(string avisynth_script_path, string avi_from_script_path, string ffmpeg_path)
        {
            if (!File.Exists(ffmpeg_path))
            {
                msgout_.Write("FFmpeg のパスが正しく設定されていません。\r\n");
                return;
            }
            if (!File.Exists(avisynth_script_path))
            {
                msgout_.Write("Avisynth スクリプトが存在しません。\r\n");
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(avi_from_script_path));
            msgout_.Write("AVI の作成を開始します…\r\n");
            int ret = IJProcess.RunProcessAndWaitForExit(
                   ffmpeg_path, "-i \"" + avisynth_script_path +
                   "\" -vcodec rawvideo -acodec pcm_s16le -y \"" + avi_from_script_path + "\"",
                   false, Path.GetDirectoryName(ffmpeg_path));
            if (ret == 0)
            {
                msgout_.Write("AVI を作成しました。\r\n");
            }
            else
            {
                msgout_.Write("AVI の作成に失敗しました。\r\n");
            }
        }
    }

    /// <summary>
    /// FFmpeg、WAVEFLT2 の場所（ファイルパス）を表すクラス
    /// </summary>
    public class FFmpegAppPath
    {
        public string ffmpeg_path;
        public string wavflv_path;

        public FFmpegAppPath(string ffmpeg_path, string wavflv_path)
        {
            this.ffmpeg_path = ffmpeg_path;
            this.wavflv_path = wavflv_path;
        }

        public FFmpegAppPath(NicoPathManager path_mgr)
        {
            ffmpeg_path = path_mgr.GetFFMpegPath();
            wavflv_path = path_mgr.GetWFltPath();
        }
    }

    /// <summary>
    /// 変換オプションを表すクラス
    /// </summary>
    public class TranslatingOption
    {
        // ファイルをどこから探すか。
        // 左から順にランクファイル、フォルダ内、ファイル直接指定
        public enum TransFileKind { RankFile, Directory, File }; 

        public TransFileKind trans_file_kind; // ファイルをどこから探すか
        public bool is_flv_to_avi;            // 動画→AVI 変換を行うか
        public bool is_avi_include_audio;     // AVI 変換時に音声を含めるか
        public TransDetailOption.TransAviKind trans_avi_kind;
                                              // AVI 変換の種類（色空間の種類、huffyuvを使うかどうかなど）

        public bool is_flv_to_wav;            // 動画→WAV 変換を行うか
        public bool is_flv_to_mp3;            // 動画→MP3 変換を行うか
        public bool is_flv_to_png;            // 動画→PNG 抜き出しを行うか
        public bool is_flv_to_detail;         // オプションを指定した詳細変換を行うか

        public double fadein;                 // フェードインをかける秒数（負の値だとかけない）
        public double fadeout;                // フェードアウトをかける秒数（負の値だとかけない）
        public bool is_normalize;             // ノーマライズをかけるか

        public int changing_width;            // 動画、画像の変換後の横幅（0以下だとサイズを変えない）
        public int changing_height;           // 動画、画像の変換後の高さ（0以下だとサイズを変えない）
        public bool is_fix_aspect;            // 動画、画像変換時にアスペクト比を固定するか

        public bool is_framerate_change;      // フレームレートを変換するか
        public string frame_rate;             // フレームレート（文字列で表す。3000/1001 などの文字列も有効）

        public double cut_start;              // 動画切り取り開始位置（負の値だと切り取らず、最初からになる）
        public double cut_end;                // 動画切り取り終了位置（負の値だと切り取らず、最後までになる）
        public string cut_list_name;          // カットリストファイル名

        public bool is_only_sm;               // sm で始まるIDのみ処理
        public bool is_only_nm;               // nm で始まるIDのみ処理

        public bool is_window_show;           // FFmpeg 実行時にウィンドウを表示するか
        public bool is_overwrite;             // ファイルの上書きをするか

        public string trans_detail_option;    // 詳細変換時のオプション

        public string trans_before_file_or_dir; // 変換元のファイル（フォルダ）名
        public string trans_after_file_or_dir;  // 変換先のファイル（フォルダ）名
        public FFmpegAppPath app_path;        // 外部アプリのパス

        public InputOutputOption iooption;    // ランクファイルの入力元

        public bool IsFade()
        {
            return fadein > 0.0 || fadeout > 0.0;
        }

        public bool IsNeedWavFilter()
        {
            return is_normalize || IsFade();
        }

        public bool IsCut()
        {
            return cut_start >= 0.0 && cut_end >= 0.0;
        }
    }

    public class FFmpegVideoInfo
    {
        public double video_time;
        public int width;
        public int height;
    }

    public class Mencoder
    {
        public static void EncodeFlvWithPath(string mencoder_path, string input_avi_path, string output_flv_path, bool is_window_show)
        {
            IJProcess.RunProcessAndWaitForExit(mencoder_path, "-of lavf -lavfopts format=flv -ovc vfw -xvfwopts codec=vp6vfw.dll:compdata=1stpass.mcf -oac mp3lame -lameopts abr:br=96 -vf flip,scale=512:384 -sws 9 -af resample=44100 -o \"" + output_flv_path + "\" \"" + input_avi_path + "\"", is_window_show, Path.GetDirectoryName(mencoder_path));
            IJProcess.RunProcessAndWaitForExit(mencoder_path, "-of lavf -lavfopts format=flv -ovc vfw -xvfwopts codec=vp6vfw.dll:compdata=2ndpass.mcf -oac mp3lame -lameopts abr:br=96 -vf flip,scale=512:384 -sws 9 -af resample=44100 -o \"" + output_flv_path + "\" \"" + input_avi_path + "\"", is_window_show, Path.GetDirectoryName(mencoder_path));
        }
    }

    public class CutInfo
    {
        private string filename_;
        private string start_time_str_;
        private string end_time_str_;
        private string duration_str_;
        private string adjustment_str_;

        public CutInfo(string line)
        {
            string[] ar = line.Split('\t');
            filename_ = (ar.Length >= 1 ? ar[0] : "");
            start_time_str_ = (ar.Length >= 2 ? ar[1] : "");
            end_time_str_ = (ar.Length >= 3 ? ar[2] : "");
            duration_str_ = (ar.Length >= 4 ? ar[3] : "");
            adjustment_str_ = (ar.Length >= 5 ? ar[4] : "");
        }

        public CutInfo(string filename, double start_time, double end_time)
        {
            filename_ = filename;
            start_time_str_ = start_time.ToString();
            end_time_str_ = end_time.ToString();
            duration_str_ = "";
            adjustment_str_ = "";
        }

        public CutInfo(string filename, string start_time_str, string end_time_str)
        {
            filename_ = filename;
            start_time_str_ = start_time_str;
            end_time_str_ = end_time_str;
            duration_str_ = "";
            adjustment_str_ = "";
        }

        public CutInfo(string filename, string start_time_str, string end_time_str, string duration_str,
            string adjustment_str)
        {
            filename_ = filename;
            start_time_str_ = start_time_str;
            end_time_str_ = end_time_str;
            duration_str_ = duration_str;
            adjustment_str_ = adjustment_str;
        }

        // base_cut_info の各項目について、空欄でなければその要素を、
        // 空欄ならば default_cut_info の要素をコピーした新しい CutInfo を作成する。
        public CutInfo(CutInfo base_cut_info, CutInfo default_cut_info)
        {
            filename_ = base_cut_info.filename_;

            if (base_cut_info.ExistsStartTime())
            {
                start_time_str_ = base_cut_info.start_time_str_;
            }
            else
            {
                start_time_str_ = default_cut_info.start_time_str_;
            }
            if (base_cut_info.ExistsDuration())
            {
                duration_str_ = base_cut_info.duration_str_;
                end_time_str_ = base_cut_info.end_time_str_;
            }
            else
            {
                duration_str_ = default_cut_info.duration_str_;
                end_time_str_ = default_cut_info.end_time_str_;
            }
            if (base_cut_info.adjustment_str_ != "")
            {
                adjustment_str_ = base_cut_info.adjustment_str_;
            }
            else
            {
                adjustment_str_ = default_cut_info.adjustment_str_;
            }
        }

        public string GetFilename()
        {
            return filename_;
        }

        public double GetStartTime()
        {
            return ParseTimeStr(start_time_str_);
        }

        public double GetDuration()
        {
            if (end_time_str_ != "")
            {
                return ParseTimeStr(end_time_str_) - ParseTimeStr(start_time_str_);
            }
            else
            {
                return ParseTimeStr(duration_str_);
            }
        }

        public double GetAdjustingStartTime(double video_time)
        {
            if (adjustment_str_.ToLower() == "on")
            {
                double start_time = GetStartTime();
                double duration = GetDuration();
                if (video_time > start_time + duration)
                {
                    return start_time;
                }
                else if (video_time > duration)
                {
                    // 始まる時間を10秒単位にする
                    return (double)((int)((video_time - duration) / 10.0) * 10);
                }
                else
                {
                    return 0.0;
                }
            }
            else
            {
                return GetStartTime();
            }
        }

        public bool ExistsStartTime()
        {
            return start_time_str_ != "";
        }

        public bool ExistsDuration()
        {
            return !(duration_str_ == "" && end_time_str_ == "");
        }

        public bool IsAdjust()
        {
            return adjustment_str_.ToLower() == "on";
        }

        public bool IsDefault()
        {
            return filename_.ToLower() == "default";
        }

        public bool IsCut()
        {
            return GetStartTime() >= 0.0;
        }

        public void SetDefaultValue(CutInfo default_info)
        {
            if (start_time_str_ == "")
            {
                start_time_str_ = default_info.start_time_str_;
            }
            if (end_time_str_ == "" && duration_str_ == "")
            {
                end_time_str_ = default_info.end_time_str_;
                duration_str_ = default_info.duration_str_;
            }
            if (adjustment_str_ != "")
            {
                adjustment_str_ = default_info.adjustment_str_;
            }
        }

        public override string ToString()
        {
            return filename_ + "\t" + start_time_str_ + "\t" + end_time_str_ + "\t" + duration_str_ + "\t" + adjustment_str_;
        }

        private static double ParseTimeStr(string str)
        {
            int index = str.IndexOf(':');
            double sec = 0;
            if (index >= 0)
            {
                int first = int.Parse(str.Substring(0, index));
                int next = index + 1;
                index = str.IndexOf(':', next);
                if (index >= 0)
                {
                    int second = int.Parse(str.Substring(next, index - next));
                    sec = (first * 60.0 + second) * 60.0;
                    next = index + 1;
                }
                else
                {
                    sec = first * 60.0;
                }
                sec += double.Parse(str.Substring(next));
                return sec;
            }
            else
            {
                return double.Parse(str);
            }
        }
    }

    public class CutList
    {
        private List<CutInfo> cut_info_list_ = new List<CutInfo>();
        private CutInfo default_info_;

        public CutList()
        {
            SetDefault();
        }

        public CutList(string filename, double default_start, double default_end)
        {
            if (filename != "")
            {
                Parse(filename);
            }
            else
            {
                default_info_ = new CutInfo("default", default_start, default_end);
                cut_info_list_.Add(default_info_);
            }
        }

        private void SetDefault()
        {
            default_info_ = new CutInfo("default", "60", "80", "", "on");
            cut_info_list_.Add(default_info_);
        }

        // filename が存在することは事前に確認する必要がある
        public void Parse(string filename)
        {
            cut_info_list_.Clear();
            default_info_ = null;

            string[] lines = IJStringUtil.SplitWithCRLFAndEraseComment(IJFile.Read(filename));
            for (int i = 0; i < lines.Length; ++i)
            {
                CutInfo cut_info = new CutInfo(lines[i]);

                if (cut_info.IsDefault())
                {
                    default_info_ = cut_info;
                }
                cut_info_list_.Add(cut_info);
            }
            if (default_info_ == null)
            {
                SetDefault();
            }
            for (int i = 0; i < cut_info_list_.Count; ++i)
            {
                cut_info_list_[i].SetDefaultValue(default_info_);
            }
        }

        public CutInfo Get(string filename)
        {
            // まずは完全一致検索
            for (int i = 0; i < cut_info_list_.Count; ++i)
            {
                if (cut_info_list_[i].GetFilename() == filename)
                {
                    return new CutInfo(cut_info_list_[i], default_info_);
                }
            }
            // 次にファイル名一致検索
            string removing_dir_filename = Path.GetFileName(filename);
            for (int i = 0; i < cut_info_list_.Count; ++i)
            {
                if (cut_info_list_[i].GetFilename() == removing_dir_filename)
                {
                    return new CutInfo(cut_info_list_[i], default_info_);
                }
            }
            // 最後に拡張子を抜いたファイル名一致検索
            string removing_extension_filename = Path.GetFileNameWithoutExtension(filename);
            for (int i = 0; i < cut_info_list_.Count; ++i)
            {
                if (cut_info_list_[i].GetFilename() == removing_extension_filename)
                {
                    return new CutInfo(cut_info_list_[i], default_info_);
                }
            }
            // それでも見つからなければデフォルトを返す
            return default_info_;
        }
    }

    public class FFmpegFailedException : Exception
    {
        public FFmpegFailedException(string message)
            : base(message)
        {

        }
    }

    public class FFmpegNotSupportPaddingException : Exception
    {
        public FFmpegNotSupportPaddingException()
            : base("bgr24 色空間ではアスペクト比自動調整に対応していません。")
        {

        }
    }
}
