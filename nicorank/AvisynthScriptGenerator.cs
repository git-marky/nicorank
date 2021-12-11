// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Text;
using IJLib;
using IJLib.Expression;

namespace nicorank
{
    /// <summary>
    /// 「手順ファイル」から AviSynth のためのスクリプトを生成するためのクラス
    /// </summary>
    public class AvisynthScriptGenerator
    {
        /// <summary>
        /// 「手順ファイル」から AviSynth 用のスクリプトを生成する。
        /// </summary>
        /// <param name="tejun_path">「手順ファイル」のファイル名</param>
        /// <param name="rank_file_path">「ランクファイル」のファイル名</param>
        /// <returns></returns>
        public string GenerateScriptWithPath(string tejun_path, string rank_file_path)
        {
            string tejun_str = IJFile.Read(tejun_path);
            string rank_file_str = IJFile.Read(rank_file_path);
            return GenerateScript(tejun_str, rank_file_str);
        }

        /// <summary>
        /// 「手順ファイル」から AviSynth 用のスクリプトを生成する。
        /// </summary>
        /// <param name="tejun_str">「手順ファイル」の中身</param>
        /// <param name="rank_file_str">「ランクファイル」の中身</param>
        /// <returns>生成されたスクリプト</returns>
        public string GenerateScript(string tejun_str, string rank_file_str)
        {
            Environment environment = new Environment();
            environment.SetRankFile(rank_file_str);
            Setting setting;
            List<Element> element_list = Parse(tejun_str, out setting);

            element_list = ExpandRepeater(element_list, environment);
            element_list = MakeGroup(element_list);

            return GenSc2(element_list, environment, setting);
        }

        private static bool IsCommentLine(string line)
        {
            return line.StartsWith("#") || line.StartsWith("//");
        }

        private static List<Element> Parse(string tejun_str, out Setting setting)
        {
            string[] lines = tejun_str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            setting = new Setting();
            List<string> comment_list = new List<string>();
            List<Element> element_list = new List<Element>();

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i] == "")
                {
                    continue;
                }
                if (IsCommentLine(lines[i]))
                {
                    comment_list.Add(lines[i]);
                    continue;
                }
                if (lines[i].StartsWith("framenumber", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    try
                    {
                        setting.frame_number = double.Parse(array[1]);
                    }
                    catch (FormatException)
                    {
                        throw new FormatException("framenumber の後ろにはタブ文字と数字が必要です。");
                    }
                    continue;
                }
                if (lines[i].StartsWith("FlipVertical", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    if (array.Length >= 2 && array[1] == "on")
                    {
                        setting.flip_vertical = true;
                    }
                    continue;
                }
                if (lines[i].StartsWith("imcolorspace", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    if (array.Length >= 2)
                    {
                        setting.imcolorspace = array[1];
                    }
                    continue;
                }
                if (lines[i].StartsWith("colorspace", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    if (array.Length >= 2)
                    {
                        setting.colorspace = array[1];
                    }
                    continue;
                }
                if (lines[i].StartsWith("ConvertFpsZone", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    if (array.Length >= 2)
                    {
                        setting.zone = int.Parse(array[1]);
                    }
                    continue;
                }
                if (lines[i].StartsWith("LayerLevel", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] array = lines[i].Split('\t');
                    if (array.Length >= 2)
                    {
                        setting.layer_level = array[1];
                    }
                    continue;
                }
                Element element;

                if (lines[i].StartsWith("繰り返し\t"))
                {
                    element = new ElementRepeater(lines[i], i + 1);
                }
                else
                {
                    element = new Element(lines[i], i + 1);
                }
                element.AddCommentRange(comment_list);
                comment_list.Clear();
                element_list.Add(element);
            }
            if (comment_list.Count > 0)
            {
                Element element = new Element(Element.Kind.Comment);
                element.AddCommentRange(comment_list);
                element_list.Add(element);
            }
            return element_list;
        }

        private static List<Element> ExpandRepeater(List<Element> element_list, Environment environment)
        {
            List<Element> expanded_list = new List<Element>();

            for (int i = 0; i < element_list.Count; ++i)
            {
                if (element_list[i].GetKind() == Element.Kind.Repeater)
                {
                    ElementRepeater repeater = (ElementRepeater)element_list[i];
                    int times = repeater.GetTimes(environment);
                    expanded_list.Add(repeater.GetCommentElement());
                    if (repeater.IsExistIndex(environment))
                    {
                        int start = repeater.GetStart(environment);
                        int end = repeater.GetEnd(environment);
                        int step = (repeater.IsReverse(environment) ? -1 : 1);
                        for (int j = start; Math.Min(start, end) <= j && j <= Math.Max(start, end); j += step)
                        {
                            for (int k = 0; k < times; ++k)
                            {
                                Element element = element_list[i + k + 1].GetClone();
                                element.SetRepeaterLineNumber(repeater.GetLineNumber());
                                element.SetIteratingNumber(j);
                                expanded_list.Add(element);
                            }
                        }
                    }
                    i += times;
                }
                else
                {
                    expanded_list.Add(element_list[i]);
                }
            }
            return expanded_list;
        }

        private static List<Element> MakeGroup(List<Element> element_list)
        {
            List<Element> grouping_list = new List<Element>();

            for (int i = 0; i < element_list.Count; ++i)
            {
                int j;
                for (j = i + 1; j < element_list.Count; ++j)
                {
                    if (!element_list[j].IsContinue())
                    {
                        break;
                    }
                }
                if (j > i + 1) // Group を作る
                {
                    ElementGroup group = new ElementGroup();
                    group.AddElementRange(element_list.GetRange(i, j - i));
                    grouping_list.Add(group);
                    i = j - 1;
                }
                else // Group を作らない
                {
                    grouping_list.Add(element_list[i]);
                }
            }
            return grouping_list;
        }

        private string GenSc2(List<Element> element_list, Environment environment, Setting setting)
        {
            int video_num = 0;
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < element_list.Count; ++i)
            {
                switch (element_list[i].GetKind())
                {
                    case Element.Kind.Image:
                    case Element.Kind.Video:
                    case Element.Kind.VideoWithAudio:
                    case Element.Kind.FrameVideo:
                    case Element.Kind.Group:
                        ++video_num;
                        buff.Append(element_list[i].GenerateScript(environment, setting, video_num));
                        break;
                    case Element.Kind.Comment:
                        buff.Append(element_list[i].GenerateScript(environment, setting, video_num));
                        break;
                }
            }
            for (int i = 1; i <= video_num; ++i)
            {
                if (i == 1)
                {
                    buff.Append("result = video1\r\n");
                }
                else
                {
                    buff.Append("result = result ++ video" + i.ToString() + "\r\n");
                }
            }
            buff.Append("return result.ConvertTo" + setting.colorspace + "()");
            if (setting.flip_vertical)
            {
                buff.Append(".FlipVertical()");
            }
            buff.Append("\r\n");
            return buff.ToString();
        }

        private static bool IsInteger(string str)
        {
            if (str == "")
            {
                return false;
            }
            int dummy;
            return int.TryParse(str, out dummy);
        }

        /// <summary>
        /// 変数を保持するための「環境」クラス。
        /// #記法の置換もこのクラスが行う
        /// </summary>
        private class Environment
        {
            private Dictionary<string, int> dictionary_ = new Dictionary<string, int>();
            string[][] matrix_;

            public void Register(string key, int value)
            {
                if (dictionary_.ContainsKey(key))
                {
                    dictionary_[key] = value;
                }
                else
                {
                    dictionary_.Add(key, value);
                }
            }

            public int EvaluateInt(string str)
            {
                return Expression.Evaluate(Replace(str)).int_value;
            }

            public string Replace(string str)
            {
                if (str == "")
                {
                    return "";
                }
                Dictionary<string, int>.Enumerator etor = dictionary_.GetEnumerator();
                // これだと短い文字が先に置き換えられると誤り。要修正
                while (etor.MoveNext())
                {
                    str = str.Replace(etor.Current.Key, etor.Current.Value.ToString());
                }
                return ReplaceSharp(str);
            }

            public void SetRankFile(string rank_file_str)
            {
                string[] rank_file_lines = new string[0];
                if (rank_file_str != "")
                {
                    rank_file_lines = IJStringUtil.SplitWithCRLF(rank_file_str);
                }
                Register("end", rank_file_lines.Length);
                matrix_ = new string[rank_file_lines.Length][];
                for (int i = 0; i < matrix_.Length; ++i)
                {
                    matrix_[i] = rank_file_lines[i].Split('\t');
                }
            }

            public string ReplaceSharp(string str)
            {
                int start = 0;
                while ((start = str.IndexOf("#{")) >= 0)
                {
                    int comma = str.IndexOf(':', start);
                    int end_par = str.IndexOf('}', comma);
                    int row = Expression.Evaluate(str.Substring(start + 2, comma - start - 2)).int_value - 1;
                    int col = Expression.Evaluate(str.Substring(comma + 1, end_par - comma - 1)).int_value - 1;
                    string rep_str = "";
                    if (row < matrix_.Length && col < matrix_[row].Length)
                    {
                        rep_str = matrix_[row][col];
                    }
                    str = str.Substring(0, start) + rep_str + str.Substring(end_par + 1);
                }
                return str;
            }
        }

        private class Setting
        {
            public double frame_number = 24.0;
            public bool flip_vertical = false;
            public string imcolorspace = "RGB32";
            public string colorspace = "RGB24";
            public int zone = -1;
            public string layer_level = "256";
        }

        private class Element
        {
            public enum Kind { Repeater, Group, Video, VideoWithAudio, Image, FrameVideo, Comment };
            protected Kind kind_;
            protected string line_;
            protected int line_number_;
            protected int iterating_number_ = -100;
            protected int repeater_line_number_;
            protected List<string> comment_list_ = new List<string>();

            string video_name_ = "";
            string frame_name_ = "";
            string audio_name_ = "";
            string length_ = "";
            string video_suffix_ = "";
            string frame_suffix_ = "";
            string audio_suffix_ = "";
            string total_suffix_ = "";

            public Element(Kind kind)
            {
                kind_ = kind;
            }

            public Element(string line, int line_number)
            {
                line_ = line;
                string[] ar = line_.Split('\t');
                line_number_ = line_number;
                switch (ar[0])
                {
                    case "音声付き動画":
                        kind_ = Kind.Video;
                        video_name_ = (ar.Length >= 2 ? ar[1] : "");
                        length_ = (ar.Length >= 3 ? ar[2] : "");
                        video_suffix_ = (ar.Length >= 4 ? ar[3] : "");
                        audio_suffix_ = (ar.Length >= 5 ? ar[4] : "");
                        total_suffix_ = (ar.Length >= 6 ? ar[5] : "");
                        break;
                    case "音声別指定動画":
                        kind_ = Kind.VideoWithAudio;
                        video_name_ = (ar.Length >= 2 ? ar[1] : "");
                        audio_name_ = (ar.Length >= 3 ? ar[2] : "");
                        length_ = (ar.Length >= 4 ? ar[3] : "");
                        video_suffix_ = (ar.Length >= 5 ? ar[4] : "");
                        audio_suffix_ = (ar.Length >= 6 ? ar[5] : "");
                        total_suffix_ = (ar.Length >= 7 ? ar[6] : "");
                        break;
                    case "静止画":
                        kind_ = Kind.Image;
                        video_name_ = (ar.Length >= 2 ? ar[1] : "");
                        audio_name_ = (ar.Length >= 3 ? ar[2] : "");
                        length_ = (ar.Length >= 4 ? ar[3] : "");
                        video_suffix_ = (ar.Length >= 5 ? ar[4] : "");
                        audio_suffix_ = (ar.Length >= 6 ? ar[5] : "");
                        total_suffix_ = (ar.Length >= 7 ? ar[6] : "");
                        break;
                    case "フレーム付動画":
                        kind_ = Kind.FrameVideo;
                        frame_name_ = (ar.Length >= 2 ? ar[1] : "");
                        video_name_ = (ar.Length >= 3 ? ar[2] : "");
                        audio_name_ = (ar.Length >= 4 ? ar[3] : "");
                        length_ = (ar.Length >= 5 ? ar[4] : "");
                        frame_suffix_ = (ar.Length >= 6 ? ar[5] : "");
                        video_suffix_ = (ar.Length >= 7 ? ar[6] : "");
                        audio_suffix_ = (ar.Length >= 8 ? ar[7] : "");
                        total_suffix_ = (ar.Length >= 9 ? ar[8] : "");
                        break;
                    default:
                        throw new FormatException("認識できない要素です。");
                }
            }

            public Element GetClone()
            {
                Element element = (Element)this.MemberwiseClone();
                element.comment_list_ = new List<string>(this.comment_list_);
                return element;
            }

            public Kind GetKind()
            {
                return kind_;
            }

            public bool IsContinue()
            {
                return IsContinueVideo() || IsContinueAudio();
            }

            public bool IsContinueVideo()
            {
                return video_name_ == "continue";
            }

            public bool IsContinueAudio()
            {
                return audio_name_ == "continue";
            }

            public void SetLineNumber(int line_number)
            {
                line_number_ = line_number;
            }

            public int GetLineNumber()
            {
                return line_number_;
            }

            public void SetRepeaterLineNumber(int repeater_line_number)
            {
                repeater_line_number_ = repeater_line_number;
            }

            public int GetRepeaterLineNumber()
            {
                return repeater_line_number_;
            }

            public void SetIteratingNumber(int iterating_number)
            {
                iterating_number_ = iterating_number;
            }

            public void AddComment(string comment)
            {
                comment_list_.Add(comment);
            }

            public void AddCommentRange(IEnumerable<string> collection)
            {
                comment_list_.AddRange(collection);
            }

            public Element GetCommentElement()
            {
                Element element = new Element(Kind.Comment);
                element.AddCommentRange(comment_list_);
                return element;
            }

            public string GetVideoLine(int group_num, int video_num, Environment environment, Setting setting)
            {
                environment.Register("$_", iterating_number_);
                // 第4引数は false で正しい？
                switch (kind_) {
                    case Kind.Video:
                        return GetVideoLine(group_num, video_num, environment.Replace(video_name_), false,
                            setting, environment.Replace(length_), environment.Replace(video_suffix_));
                    case Kind.Image:
                        return GetImageLine(group_num, video_num, setting, environment.Replace(video_name_),
                            environment.Replace(length_), environment.Replace(video_suffix_));
                    case Kind.FrameVideo:
                        return GetVideoLine(group_num, video_num, environment.Replace(video_name_), false, setting,
                                            environment.Replace(length_), environment.Replace(video_suffix_)) +
                               GetFrameLine(group_num, video_num, setting, environment.Replace(frame_name_)) +
                               GetLayedLine(group_num, video_num, setting, environment.Replace(frame_suffix_));
                }
                return "";
            }

            public string GetAudioLine(int group_num, int audio_num, Environment environment, Setting setting)
            {
                environment.Register("$_", iterating_number_);
                return GetAudioLine(group_num, audio_num, setting, environment.Replace(audio_name_),
                    environment.Replace(length_), environment.Replace(audio_suffix_));
            }

            public virtual string GenerateScript(Environment environment, Setting setting, int group_num)
            {
                environment.Register("$_", iterating_number_);
                string v_name = environment.Replace(video_name_);
                string a_name = environment.Replace(audio_name_);
                string f_name = environment.Replace(frame_name_);
                string len = environment.Replace(length_);
                string v_suffix = environment.Replace(video_suffix_);
                string f_suffix = environment.Replace(frame_suffix_);
                string a_suffix = environment.Replace(audio_suffix_);
                string t_suffix = environment.Replace(total_suffix_);

                StringBuilder buff = new StringBuilder();
                for (int i = 0; i < comment_list_.Count; ++i)
                {
                    buff.Append(comment_list_[i]);
                    buff.Append("\r\n");
                }
                switch (kind_)
                {
                    case Kind.Video:
                        buff.Append(GetVideoLine(group_num, 1, v_name, true, setting, len, v_suffix));
                        buff.Append(GetGroupingVideo(group_num, 1, 0, setting, len, t_suffix));
                        break;
                    case Kind.VideoWithAudio:
                        buff.Append(GetAudioLine(group_num, 1, setting, a_name, len, a_suffix));
                        buff.Append(GetVideoLine(group_num, 1, v_name, false, setting, len, v_suffix));
                        buff.Append(GetGroupingVideo(group_num, 1, 1, setting, len, t_suffix));
                        break;
                    case Kind.Image:
                        buff.Append(GetImageLine(group_num, 1, setting, v_name, len, v_suffix));
                        buff.Append(GetAudioLine(group_num, 1, setting, a_name, len, a_suffix));
                        buff.Append(GetGroupingVideo(group_num, 1, 1, setting, len, t_suffix));
                        break;
                    case Kind.FrameVideo:
                        buff.Append(GetAudioLine(group_num, 1, setting, a_name, len, a_suffix));
                        buff.Append(GetVideoLine(group_num, 1, v_name, false, setting, len, v_suffix));
                        buff.Append(GetFrameLine(group_num, 1, setting, f_name));
                        buff.Append(GetLayedLine(group_num, 1, setting, f_suffix));
                        buff.Append(GetGroupingVideo(group_num, 1, 1, setting, len, t_suffix));
                        break;
                }
                return buff.ToString();
            }

            protected static string GetVideoLine(int group_num, int video_num, string avi_filename, bool use_audio,
                Setting setting, string length, string suffix)
            {
                StringBuilder buff = new StringBuilder();
                buff.Append("video" + group_num + "_" + video_num + " = AVISource(\""
                        + avi_filename + "\"");
                if (!use_audio)
                {
                    buff.Append(", audio=false");
                }
                buff.Append(")");
                if (setting.imcolorspace != "")
                {
                    buff.Append(".ConvertTo" + setting.imcolorspace + "()");
                }
                buff.Append(".ConvertFPS(" + setting.frame_number.ToString("0.000"));
                if (setting.zone >= 0)
                {
                    buff.Append(", zone=" + setting.zone.ToString());
                }
                buff.Append(")");
                if (length != "" && length != "audio")
                {
                    buff.Append(".Trim(0, " + ((int)(double.Parse(length) * setting.frame_number)).ToString() + ")");
                }
                buff.Append(suffix + "\r\n");
                return buff.ToString();
            }

            protected static string GetAudioLine(int group_num, int video_num, Setting setting, string audio_filename, string length, string suffix)
            {
                StringBuilder buff = new StringBuilder();
                buff.Append("audio" + group_num + "_" + video_num);
                if (audio_filename != "")
                {
                    buff.Append(" = WAVSource(\"" + audio_filename + "\")" + suffix + "\r\n");
                    return buff.ToString();
                }
                else
                {
                    if (length != "" && length != "audio")
                    {
                        buff.Append(" = " + GetBlankClip(setting.frame_number, double.Parse(length)) + suffix + "\r\n");
                        return buff.ToString();
                    }
                    else
                    {
                        throw new FormatException("音声ファイルまたは音声の長さを指定してください。");
                    }
                }
            }

            protected static string GetBlankClip(double frame_number, double length)
            {
                return "BlankClip(length=" +
                            ((int)(frame_number * length)).ToString() + ", width=1, height=1, fps=" +
                            frame_number.ToString("0.000") + ", color=$000000, audio_rate=44100, stereo=true)";
            }

            protected static string GetImageLine(int group_num, int video_num, Setting setting, string image_filename, string length, string suffix)
            {
                StringBuilder buff = new StringBuilder();
                buff.Append("video" + group_num + "_" + video_num + " = ImageSource(\"" + image_filename + "\", 0, ");
                if (length == "" || length == "audio")
                {
                    buff.Append(GetAudioLength(setting.frame_number, "audio" + group_num + "_" + video_num));
                }
                else
                {
                    buff.Append(((int)(setting.frame_number * double.Parse(length))).ToString());
                }
                buff.Append(", " + setting.frame_number.ToString("0.000") + ", pixel_type=\"RGB32\")");
                if (setting.imcolorspace != "RGB32")
                {
                    buff.Append(".ConvertTo" + setting.imcolorspace + "()");
                }
                buff.Append(suffix + "\r\n");
                return buff.ToString();
            }

            protected static string GetFrameLine(int group_num, int video_num, Setting setting, string frame_filename)
            {
                string line = "frame" + group_num + "_" + video_num + " = ImageSource(\"" + frame_filename +
                    "\", 0, video" + group_num + "_" + video_num + ".FrameCount(), " + setting.frame_number.ToString("0.000") +
                    ", pixel_type=\"RGB32\")";
                if (setting.imcolorspace != "RGB32")
                {
                    line += ".ConvertTo" + setting.imcolorspace + "()";
                }
                line += "\r\n";
                return line;
            }

            protected static string GetLayer(int group_num, int video_num, int x, int y, string suffix, Setting setting)
            {
                string line = "Layer(frame" + group_num + "_" + video_num +
                    ", video" + group_num + "_" + video_num + ", \"add\", x=" + x + ", y=" + y;
                if (setting.layer_level.ToLowerInvariant() != "none")
                {
                    line += ", level=" + setting.layer_level;
                }
                line += ")" + suffix;
                return line;
            }

            protected static string GetLayedLine(int group_num, int video_num, Setting setting, string suffix)
            {
                return "video" + group_num + "_" + video_num + " = " + GetLayer(group_num, video_num, 0, 0, suffix, setting) + "\r\n";
            }

            protected static string GetGroupingVideo(int group_num, int video_max_num, int audio_max_num, Setting setting, string length, string suffix)
            {
                if (video_max_num == 1 && audio_max_num == 0)
                {
                    return "video" + group_num + " = video" + group_num + "_1" + suffix + "\r\n";
                }
                StringBuilder buff = new StringBuilder();

                buff.Append("video" + group_num + " = AudioDub(");

                for (int i = 1; i <= video_max_num; ++i)
                {
                    if (i > 1)
                    {
                        buff.Append(" + ");
                    }
                    buff.Append("video" + group_num + "_" + i);
                }
                buff.Append(", ");

                for (int i = 1; i <= audio_max_num; ++i)
                {
                    if (i > 1)
                    {
                        buff.Append(" + ");
                    }
                    buff.Append("audio" + group_num + "_" + i);
                }
                buff.Append(")");

                if (length != "")
                {
                    if (length == "audio")
                    {
                        buff.Append(".Trim(0, " + GetAudioLength(setting.frame_number, "audio" + group_num + "_" + audio_max_num) + ")");
                    }
                    else
                    {
                        buff.Append(".Trim(0, " + ((int)(double.Parse(length) * setting.frame_number)).ToString() + ")");
                    }
                }
                buff.Append(suffix);
                buff.Append("\r\n");

                return buff.ToString();
            }

            protected static string GetAudioLength(double frame_number, string audio_name)
            {
                return "int(" + frame_number.ToString("0.000") + " * " + audio_name + ".AudioLength() / " + audio_name + ".Audiorate())";
            }
        }

        private class ElementRepeater : Element
        {
            string[] ar_;
            int start_ = -1; // -1 は未解析なことを表す
            int end_ = -1;
            bool reverse_ = false;

            public int GetStart(Environment environment)
            {
                if (start_ < 0)
                {
                    Analyze(environment);
                }
                return start_;
            }

            public int GetEnd(Environment environment)
            {
                if (start_ < 0)
                {
                    Analyze(environment);
                }
                return end_;
            }

            public bool IsReverse(Environment environment)
            {
                if (start_ < 0)
                {
                    Analyze(environment);
                }
                return reverse_;
            }

            public bool IsExistIndex(Environment environment)
            {
                if (start_ < 0)
                {
                    Analyze(environment);
                }
                return (reverse_ ? start_ >= end_ : start_ <= end_);
            }

            public ElementRepeater(string line, int line_number) : base(Kind.Repeater)
            {
                ar_ = line.Split('\t');
                line_number_ = line_number;
            }

            public int GetTimes(Environment environment)
            {
                return environment.EvaluateInt(ar_[1]);
            }

            private void Analyze(Environment environment)
            {
                start_ = environment.EvaluateInt(ar_[2]);
                end_ = environment.EvaluateInt(ar_[3]);
                if (ar_.Length >= 5 && ar_[4] == "asc")
                {
                    reverse_ = false;
                }
                else if (ar_.Length >= 5 && ar_[4] == "desc")
                {
                    reverse_ = true;
                }
                else
                {
                    reverse_ = (start_ > end_);
                }
            }
        }

        private class ElementGroup : Element
        {
            private List<Element> element_list_ = new List<Element>();

            public ElementGroup()
                : base(Kind.Group)
            {

            }

            public void AddElement(Element element)
            {
                element_list_.Add(element);
            }

            public void AddElementRange(IEnumerable<Element> collection)
            {
                element_list_.AddRange(collection);
            }

            public override string GenerateScript(Environment environment, Setting setting, int group_num)
            {
                int video_num = 0;
                int audio_num = 0;
                StringBuilder buff = new StringBuilder();
                for (int i = 0; i < element_list_.Count; ++i)
                {
                    if (!element_list_[i].IsContinueVideo())
                    {
                        ++video_num;
                        buff.Append(element_list_[i].GetVideoLine(group_num, video_num, environment, setting));
                    }
                    if (!element_list_[i].IsContinueAudio())
                    {
                        ++audio_num;
                        buff.Append(element_list_[i].GetAudioLine(group_num, audio_num, environment, setting));
                    }
                }
                buff.Append(GetGroupingVideo(group_num, video_num, audio_num, setting, "", ""));
                return buff.ToString();
            }
        }
    }
}
