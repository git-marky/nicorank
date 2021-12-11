// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using IJLib;
using IJLib.Expression;

namespace NicoTools
{
    /// <summary>
    /// ランクファイルのカスタマイズのためのクラス。
    /// 入力用と出力用を分けて設定することができる。
    /// </summary>
    public class RankFileCustomFormat
    {
        private CustomFormatElementSet input_set_ = new CustomFormatElementSet(); // 入力用
        private CustomFormatElementSet output_set_ = new CustomFormatElementSet(); // 出力用
        private bool is_using_custom_format_; // カスタムフォーマットを実際に使うかどうか

        public RankFileCustomFormat()
        {
            is_using_custom_format_ = false;
        }

        public RankFileCustomFormat(string input_format, string output_format)
        {
            input_set_.PreParse(input_format);
            output_set_.PreParse(output_format);
            is_using_custom_format_ = true;
        }

        public string GetInputSeparator()
        {
            return (is_using_custom_format_ ? input_set_.GetSeparator() : "\t");
        }

        /// <summary>
        /// カスタムフォーマットを使用して、ランクファイルの一行を Video クラスに変換
        /// 入力用設定が使われる
        /// </summary>
        /// <param name="line">ランクファイルの一行</param>
        /// <returns>解析結果（Video クラス）</returns>
        public Video GetVideo(string line)
        {
            return input_set_.GetVideo(line);
        }

        /// <summary>
        /// カスタムフォーマットを使用して、Video クラスを文字列（ランクファイルの一行）に変換
        /// </summary>
        /// <param name="video">変換する Video クラス</param>
        /// <param name="line_number">行の番号（出力情報として必要）</param>
        /// <returns>変換結果の文字列</returns>
        public string VideoToString(Video video, int line_number)
        {
            return output_set_.VideoToString(video, line_number);
        }

        public bool IsUsingCustomFormat()
        {
            return is_using_custom_format_;
        }
    }

    class CustomFormatElementSet
    {
        string separator_;
        List<CustomFormatElement> element_list_ = new List<CustomFormatElement>();

        public string GetSeparator()
        {
            return separator_;
        }

        public void PreParse(string str)
        {
            if (str == "")
            {
                return;
            }
            string[] line = IJStringUtil.SplitWithCRLFAndEraseComment(str);
            ParseLine(line[0]);
            for (int i = 1; i < line.Length; ++i)
            {
                element_list_.Add(new CustomFormatElement(line[i]));
            }
        }

        private void ParseLine(string line)
        {
            if (!line.StartsWith("separator"))
            {
                throw new FormatException("1行目は\"separator=\"で始まる必要があります。");
            }
            int cp = "separator".Length;
            while (cp < line.Length && (line[cp] == ' ' || line[cp] == '\t'))
            {
                ++cp;
            }
            if (cp >= line.Length || line[cp] != '=')
            {
                throw new FormatException("1行目は\"separator=\"で始まる必要があります。");
            }
            int start = line.IndexOf('"', cp);
            if (start < 0)
            {
                throw new FormatException("'\"' は省略できません。");
            }
            ++start;
            int end = line.IndexOf('"', start);
            if (end < 0)
            {
                throw new FormatException("'\"' の最後が見つかりません。");
            }
            separator_ = line.Substring(start, end - start).Replace("\\t", "\t");
        }

        public Video GetVideo(string str)
        {
            Video video = new Video();
            string[] line = str.Split(new string[]{separator_}, StringSplitOptions.None);
            for (int i = 0; i < line.Length; ++i)
            {
                if (i < element_list_.Count)
                {
                    element_list_[i].Substitute(line[i], video);
                }
                else
                {
                    // 工事中
                }
            }
            return video;
        }

        public string VideoToString(Video video, int line_number)
        {
            StringBuilder buff = new StringBuilder();
            for (int i = 0; i < element_list_.Count; ++i)
            {
                element_list_[i].VideoToString(video, buff, line_number);
                if (i < element_list_.Count - 1)
                {
                    buff.Append(separator_);
                }
            }
            return buff.ToString();
        }
    }

    class CustomFormatElement
    {
        private List<CustomFormatElementBlock> block_list_ = new List<CustomFormatElementBlock>();

        public CustomFormatElement(string str)
        {
            Parse(str);
        }

        public void Parse(string str)
        {
            int cp = 0;
            while (cp < str.Length)
            {
                int start = cp;
                if (str[start] == '<')
                {
                    ++start;
                    int end = str.IndexOf('>', start);
                    if (end < 0)
                    {
                        throw new FormatException("'<' に対応する'>'が見つかりません。");
                    }
                    block_list_.Add(new CustomFormatElementBlock(str.Substring(start, end - start)));
                    cp = end + 1;
                }
                else
                {
                    int end = str.IndexOf('<', start);
                    if (end < 0)
                    {
                        end = str.Length;
                    }
                    block_list_.Add(new CustomFormatElementBlock(str.Substring(start, end - start), CustomFormatElementBlock.Kind.Literal));
                    cp = end;
                }
            }
        }

        public void Substitute(string elem, Video video)
        {
            int pos = 0;
            for (int i = 0; i < block_list_.Count; ++i)
            {
                if (block_list_[i].GetKind() == CustomFormatElementBlock.Kind.Literal)
                {
                    string text = block_list_[i].GetText();
                    int end = elem.IndexOf(text, pos);
                    if (end < 0)
                    {
                        throw new FormatException("書式が間違えています。");
                    }
                    pos += text.Length;
                }
                else
                {
                    if (i < block_list_.Count - 1)
                    {
                        if (block_list_[i + 1].GetKind() != CustomFormatElementBlock.Kind.Literal)
                        {
                            throw new FormatException("要素の区切りが判別できません。");
                        }
                        else
                        {
                            string next_text = block_list_[i + 1].GetText();
                            int next_pos = elem.IndexOf(next_text, pos);
                            string text = elem.Substring(pos, next_pos - pos);
                            block_list_[i].Substitute(video, text);
                            pos = next_pos;
                        }
                    }
                    else
                    {
                        string text = elem.Substring(pos);
                        block_list_[i].Substitute(video, text);
                    }
                }
            }
        }

        public void VideoToString(Video video, StringBuilder buff, int line_number)
        {
            for (int i = 0; i < block_list_.Count; ++i)
            {
                block_list_[i].VideoToString(video, buff, line_number);
            }
        }
    }

    class CustomFormatElementBlock
    {
        public enum Kind { Literal, Id, View, Res, Mylist, Title, Date, Description, LineNumber, Tag, ExtractTag, Expression, UserText, Special };
        public enum Rounding { Nearest, Floor, Ceil };
        private Kind kind_;
        private string text_;
        private int num_;
        private Object obj_;
        private int seido_ = 0;
        private string value_format_; // for expression
        private string arrange_; // for tag
        private bool is_comma_ = true;
        private Rounding rounding_ = Rounding.Floor;
        private bool is_nan_to_zero_ = false;

        public CustomFormatElementBlock(string str)
        {
            Parse(str);
        }

        public CustomFormatElementBlock(string str, Kind kind)
        {
            text_ = str;
            kind_ = kind;
        }

        public Kind GetKind()
        {
            return kind_;
        }

        public string GetText()
        {
            return text_;
        }

        public void Parse(string str)
        {
            int cp = 0;
            while (cp < str.Length && (str[cp] != ' ' && str[cp] != '\t' && str[cp] != '/'))
            {
                ++cp;
            }
            string element_name = str.Substring(0, cp);
            switch (element_name.ToLower())
            {
                case "video_id":
                    kind_ = Kind.Id;
                    return;
                case "view":
                    kind_ = Kind.View;
                    break;
                case "res":
                    kind_ = Kind.Res;
                    break;
                case "mylist":
                    kind_ = Kind.Mylist;
                    break;
                case "title":
                    kind_ = Kind.Title;
                    return;
                case "date":
                    kind_ = Kind.Date;
                    break;
                case "description":
                    kind_ = Kind.Description;
                    break;
                case "line_number":
                    kind_ = Kind.LineNumber;
                    break;
                case "tag":
                    kind_ = Kind.Tag;
                    break;
                case "extract_tag":
                    kind_ = Kind.ExtractTag;
                    break;
                case "expression":
                    kind_ = Kind.Expression;
                    break;
                case "text":
                    kind_ = Kind.UserText;
                    break;
                case "special":
                    kind_ = Kind.Special;
                    break;
                default: // 通常のテキストとして扱う
                    kind_ = Kind.Literal;
                    text_ = "<" + str + ">";
                    return;
            }
            if (str[cp] == '/')
            {
                return;
            }
            Dictionary<string, string> dictionary = ParseAttr(str, cp);
            if (kind_ == Kind.View || kind_ == Kind.Res || kind_ == Kind.Mylist)
            {
                if (dictionary.ContainsKey("comma") && dictionary["comma"] == "off")
                {
                    is_comma_ = false;
                }
            }
            else if (kind_ == Kind.Date)
            {
                if (dictionary.ContainsKey("format"))
                {
                    text_ = dictionary["format"];
                }
                else
                {
                    text_ = "";
                }
            }
            else if (kind_ == Kind.LineNumber)
            {
                if (dictionary.ContainsKey("offset"))
                {
                    num_ = int.Parse(dictionary["offset"]);
                }
                else
                {
                    num_ = 0;
                }
            }
            else if (kind_ == Kind.Tag)
            {
                if (dictionary.ContainsKey("separator"))
                {
                    text_ = dictionary["separator"];
                }
                else
                {
                    text_ = " ";
                }
                if (dictionary.ContainsKey("arrange"))
                {
                    arrange_ = dictionary["arrange"];
                }
            }
            else if (kind_ == Kind.ExtractTag)
            {
                if (dictionary.ContainsKey("match"))
                {
                    obj_ = new Regex(dictionary["match"]);
                }
                else
                {
                    obj_ = null;
                }
            }
            else if (kind_ == Kind.UserText)
            {
                if (dictionary.ContainsKey("number"))
                {
                    num_ = int.Parse(dictionary["number"]);
                }
                else
                {
                    throw new FormatException("text には number が必要です。");
                }
            }
            else if (kind_ == Kind.Special)
            {
                if (dictionary.ContainsKey("value"))
                {
                    text_ = dictionary["value"];
                }
            }
            else if (kind_ == Kind.Expression)
            {
                value_format_ = "0";
                if (dictionary.ContainsKey("val"))
                {
                    text_ = dictionary["val"];
                }
                else
                {
                    throw new FormatException("expression には val が必要です。");
                }
                if (dictionary.ContainsKey("comma") && dictionary["comma"] == "off")
                {
                    is_comma_ = false;
                }
                if (dictionary.ContainsKey("rounding"))
                {
                    if (dictionary["rounding"] == "nearest")
                    {
                        rounding_ = Rounding.Nearest;
                    }
                    else if (dictionary["rounding"] == "ceil")
                    {
                        rounding_ = Rounding.Ceil;
                    }
                }
                if (dictionary.ContainsKey("seido"))
                {
                    try
                    {
                        seido_ = int.Parse(dictionary["seido"]);
                    }
                    catch (Exception)
                    {
                        throw new FormatException("seido には 0 から 9 の値を指定してください。");
                    }
                    if (seido_ < 0 || seido_ >= 10)
                    {
                        throw new FormatException("seido には 0 から 9 の値を指定してください。");
                    }
                    if (seido_ > 0)
                    {
                        value_format_ = "0.";
                        for (int i = 0; i < seido_; ++i)
                        {
                            value_format_ += "0";
                        }
                    }
                }
                if (dictionary.ContainsKey("nantozero"))
                {
                    is_nan_to_zero_ = (dictionary["nantozero"] == "on");
                }
            }
        }

        public void Substitute(Video video, string str)
        {
            switch (kind_)
            {
                case Kind.Id:
                    video.video_id = str;
                    break;
                case Kind.View:
                    video.point.view = IJStringUtil.ToIntFromCommaValue(str);
                    break;
                case Kind.Res:
                    video.point.res = IJStringUtil.ToIntFromCommaValue(str);
                    break;
                case Kind.Mylist:
                    video.point.mylist = IJStringUtil.ToIntFromCommaValue(str);
                    break;
                case Kind.Title:
                    video.title = str;
                    break;
                case Kind.Date:
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (text_ != "") // 日時用の書式が存在するなら
                        {
                            video.submit_date = DateTime.ParseExact(str, text_, null);
                        }
                        else
                        {
                            video.submit_date = NicoUtil.StringToDate(str);
                        }
                    }
                    else // 空文字列の場合
                    {
                        video.submit_date = new DateTime();
                    }
                    break;
                case Kind.Description:
                    video.description = str;
                    break;
                case Kind.Tag: // text_ は separator
                    if (arrange_ == "daily")
                    {
                        video.tag_set.Parse(str);
                    }
                    else
                    {
                        video.tag_set.Parse(str, text_);
                    }
                    break;
                case Kind.UserText:
                    video.SetUserText(num_, str);
                    break;
            }
        }

        public void VideoToString(Video video, StringBuilder buff, int line_number)
        {
            switch (kind_)
            {
                case Kind.Literal:
                    buff.Append(text_);
                    break;
                case Kind.Id:
                    buff.Append(video.video_id);
                    break;
                case Kind.View:
                    buff.Append(is_comma_ ? IJStringUtil.ToStringWithComma(video.point.view) : video.point.view.ToString());
                    break;
                case Kind.Res:
                    buff.Append(is_comma_ ? IJStringUtil.ToStringWithComma(video.point.res) : video.point.res.ToString());
                    break;
                case Kind.Mylist:
                    buff.Append(is_comma_ ? IJStringUtil.ToStringWithComma(video.point.mylist) : video.point.mylist.ToString());
                    break;
                case Kind.Title:
                    if (video.GetStatus() == Video.Status.DELETED)
                    {
                        buff.Append("DELETED");
                    }
                    else
                    {
                        buff.Append(video.title);
                    }
                    break;
                case Kind.Date:
                    if (text_ != "") // 日時用の書式が存在するなら
                    {
                        buff.Append(video.submit_date.ToString(text_));
                    }
                    else
                    {
                        buff.Append(NicoUtil.DateToString(video.submit_date));
                    }
                    break;
                case Kind.Description:
                    buff.Append(video.description);
                    break;
                case Kind.LineNumber: // num_ は offset
                    buff.Append((line_number + num_).ToString());
                    break;
                case Kind.Tag: // text_ は separator
                    if (arrange_ == "daily")
                    {
                        buff.Append(video.tag_set.GetDisplayingTag());
                    }
                    else
                    {
                        buff.Append(video.tag_set.ToStringWithSplitter(text_));
                    }
                    break;
                case Kind.ExtractTag:
                    if (obj_ != null)
                    {
                        buff.Append(video.tag_set.ExtractTag((Regex)obj_));
                    }
                    break;
                case Kind.Expression:
                    buff.Append(DoCalc(text_, video));
                    break;
                case Kind.UserText:
                    buff.Append(video.GetUserText(num_));
                    break;
                case Kind.Special:
                    break;
            }
        }

        private string DoCalc(string exp, Video video)
        {
            exp = exp.Replace("view", video.point.view.ToString("0.0")).
                Replace("res", video.point.res.ToString("0.0")).
                Replace("mylist", video.point.mylist.ToString("0.0"));
            double val = Expression.Evaluate(exp).GetDouble();
            if (is_nan_to_zero_ && (double.IsNaN(val) || double.IsInfinity(val)))
            {
                val = 0.0;
            }
            if (rounding_ == Rounding.Floor)
            {
                int keta = (int)Math.Pow(10, seido_);
                val = Math.Floor(val * keta) / keta;
            }
            else if (rounding_ == Rounding.Ceil)
            {
                int keta = (int)Math.Pow(10, seido_);
                val = Math.Ceiling(val * keta) / keta;
            }
            string format;
            if (is_comma_)
            {
                if (value_format_ == "0")
                {
                    format = "#,##0";
                }
                else
                {
                    format = value_format_.Replace("0.", "#,##0.");
                }
            }
            else
            {
                format = value_format_;
            }
            return val.ToString(format);
        }

        private static Dictionary<string, string> ParseAttr(string str, int start_pos)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            int pos = start_pos;
            while (pos < str.Length)
            {
                while (pos < str.Length && (str[pos] == ' ' || str[pos] == '\t'))
                {
                    ++pos;
                }
                if (pos >= str.Length)
                {
                    break;
                }
                int start = pos;
                while (pos < str.Length && (str[pos] != ' ' && str[pos] != '\t' && str[pos] != '='))
                {
                    ++pos;
                }
                string name = str.Substring(start, pos - start);
                while (pos < str.Length && (str[pos] == ' ' || str[pos] == '\t'))
                {
                    ++pos;
                }
                if (pos >= str.Length - 1 || str[pos] != '=')
                {
                    dic.Add(name, "");
                    continue;
                }
                ++pos;
                while (pos < str.Length && (str[pos] == ' ' || str[pos] == '\t'))
                {
                    ++pos;
                }
                string attr = "";
                if (str[pos] == '"')
                {
                    ++pos;
                    int end = str.IndexOf('"', pos);
                    if (end < 0)
                    {
                        throw new FormatException("'\"' の最後が見つかりません。");
                    }
                    attr = str.Substring(pos, end - pos);
                    pos = end + 1;
                }
                else
                {
                    int s2 = pos;
                    while (pos < str.Length && (str[pos] != ' ' && str[pos] != '\t'))
                    {
                        ++pos;
                    }
                    attr = str.Substring(s2, pos - s2);
                }
                dic.Add(name, attr);
            }
            return dic;
        }
    }
}
