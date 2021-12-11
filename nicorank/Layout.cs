// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using IJLib;
using IJLib.Vfw;
using NicoTools;

namespace nicorank
{
    /// <summary>
    /// レイアウトを表すクラス
    /// </summary>
    public class Layout
    {
        private List<LayoutElement> element_list_ = new List<LayoutElement>();
        private LayoutVideoManager video_manager_ = new LayoutVideoManager();

        private IntVal line_number_val_ = new IntVal(1);
        private IntVal img_width_val_ = new IntVal(512);
        private IntVal img_height_val_ = new IntVal(384);
        private IntVal range_start_val_ = new IntVal(1);
        private IntVal range_end_val_ = new IntVal(int.MaxValue);
        private StringVal filename_val_ = new StringVal("%d.*");
        private StringVal filetype_val_ = new StringVal("png");
        private StringVal background_val_ = new StringVal("white");
        private StringVal base_dir_relative_val_ = new StringVal("auto");

        private string layout_dir_;

        public Layout(string layout_text, string layout_dir)
        {
            layout_dir_ = layout_dir;
            Parse(layout_text);
        }

        public int LineNumber
        {
            get { return line_number_val_.GetRawInt(); }
        }
        
        public int Width
        {
            get { return img_width_val_.GetRawInt(); }
        }

        public int Height
        {
            get { return img_height_val_.GetRawInt(); }
        }

        public void Parse(string layout_text)
        {
            // 行番号が必要なので StringSplitOptions.None を指定している
            string[] lines = layout_text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i] == "" || lines[i].StartsWith("#") || lines[i].StartsWith("//"))
                {
                    continue;
                }

                LayoutLine line = new LayoutLine(lines[i]);

                switch (line.GetData(0).ToLowerInvariant())
                {
                    case "linenumber":
                        line_number_val_ = new IntVal(line.GetData(1), i + 1, 2);
                        break;
                    case "outputsize":
                        img_width_val_ = new IntVal(line.GetData(1), i + 1, 2);
                        img_height_val_ = new IntVal(line.GetData(2), i + 1, 3);
                        break;
                    case "readrankrange":
                        range_start_val_ = new IntVal(line.GetData(1), i + 1, 2);
                        range_end_val_ = new IntVal(line.GetData(2), i + 1, 3);
                        break;
                    case "filename":
                        filename_val_ = new StringVal(line.GetData(1), i + 1, 2);
                        break;
                    case "filetype":
                        filetype_val_ = new StringVal(line.GetData(1), i + 1, 2);
                        break;
                    case "basedirrelative":
                        base_dir_relative_val_ = new StringVal(line.GetData(1), i + 1, 2);
                        break;
                    case "backgroundcolor":
                        background_val_ = new StringVal(line.GetData(1), i + 1, 2);
                        break;
                    default:
                        LayoutElement element = MakeNewElement(line, i + 1);
                        if (element != null)
                        {
                            element_list_.Add(element);
                        }
                        break;
                }
            }
        }

        private LayoutElement MakeNewElement(LayoutLine line, int line_number)
        {
            switch (line.GetData(0).ToLowerInvariant())
            {
                case "basedir":
                    return new LayoutElementBaseDir(line, line_number);
                case "antialias":
                    return new LayoutElementAntiAlias(line, line_number);
                case "テキスト":
                    return new LayoutElementNormalText(line, line_number);
                case "テキスト四角形":
                    return new LayoutElementTextRect(line, line_number);
                case "サイズ可変テキスト":
                    return new LayoutElementTextSizeFree(line, line_number);
                case "幅固定テキスト":
                    return new LayoutElementTextFixedWidth(line, line_number);
                case "幅固定横倍率可変テキスト":
                    return new LayoutElementFixedWidthFlexible(line, line_number);
                case "画像テキスト":
                    return new LayoutElementImageText(line, line_number);
                case "画像テキスト四角形":
                    return new LayoutElementImageTextRect(line, line_number);
                case "画像":
                    return new LayoutElementImage(line, line_number);
                case "vfw動画":
                    LayoutElementVfwFrame element = new LayoutElementVfwFrame(line, line_number);
                    video_manager_.Add(element);
                    return element;
                default:
                    return null;
            }
        }

        public void DrawPicture(Graphics graphics, LayoutData data)
        {
            InitializeForDrawingPicture(data);

            InitializeGraphics(graphics, data);

            for (int i = 0; i < element_list_.Count; ++i) // 描画
            {
                if (element_list_[i] is ILayoutEffect)
                {
                    ILayoutEffect effect = element_list_[i] as ILayoutEffect;
                    effect.DoEffect(graphics, data);
                }
                else
                {
                    element_list_[i].Draw(graphics, data);
                }
            }

            FinalizeForDrawingPicture();
        }

        private void InitializeForDrawingPicture(LayoutData data)
        {
            // エラーチェック
            for (int i = 0; i < element_list_.Count; ++i)
            {
                element_list_[i].CheckError(data);
            }

            string base_dir = "";
            for (int i = 0; i < element_list_.Count; ++i) // ファイルのオープン処理
            {
                if (element_list_[i] is Openable)
                {
                    Openable element = element_list_[i] as Openable;
                    switch (base_dir_relative_val_.GetString(data).ToLower())
                    {
                        case "auto":
                            element.Open(data, layout_dir_, base_dir);
                            break;
                        case "on":
                            element.Open(data, "", Path.Combine(layout_dir_, base_dir));
                            break;
                        default: // off
                            element.Open(data, "", base_dir);
                            break;
                    }
                }
                else if (element_list_[i] is LayoutElementBaseDir)
                {
                    LayoutElementBaseDir element_base_dir = element_list_[i] as LayoutElementBaseDir;
                    base_dir = element_base_dir.GetBaseDir(data);
                }
            }
        }

        private void FinalizeForDrawingPicture()
        {
            for (int i = 0; i < element_list_.Count; ++i) // ファイルのクローズ処理
            {
                if (element_list_[i] is Openable)
                {
                    Openable element = element_list_[i] as Openable;
                    element.Close();
                }
            }
        }

        public void SaveVideo(string output_filename, LayoutData data, MessageOut msgout)
        {
            InitializeForDrawingPicture(data);

            if (!video_manager_.IsOpenAtLeastOne())
            {
                msgout.WriteLine("AVIファイルが正しくオープンできませんでした。");
                msgout.WriteLine(output_filename + " は出力されません。");
                return;
            }

            VfwAviWriter avi_writer = new VfwAviWriter(output_filename, video_manager_.Rate, video_manager_.Scale);
            try
            {
                for (int i = 0; i < video_manager_.Length; ++i)
                {
                    using (Bitmap bitmap = new Bitmap(img_width_val_.GetInt(data), img_height_val_.GetInt(data), PixelFormat.Format24bppRgb))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            InitializeGraphics(graphics, data);
                            for (int j = 0; j < element_list_.Count; ++j) // 描画
                            {
                                if (element_list_[j] is ILayoutEffect)
                                {
                                    ILayoutEffect effect = element_list_[j] as ILayoutEffect;
                                    effect.DoEffect(graphics, data);
                                }
                                else
                                {
                                    element_list_[j].DrawFrame(graphics, data, i);
                                }
                            }
                        }
                        avi_writer.AddFrame(bitmap);
                    }
                }
            }
            finally
            {
                avi_writer.Close();
            }

            FinalizeForDrawingPicture();
        }

        /// <summary>
        /// レイアウトを元に、画像を1枚保存
        /// </summary>
        /// <param name="filename">保存画像ファイル名</param>
        /// <param name="splitted_data">ランクファイルの中身を2次元リストとして区切ったもの</param>
        public void SavePicture(string filename, LayoutData data)
        {
            using (Bitmap bitmap = new Bitmap(img_width_val_.GetInt(data), img_height_val_.GetInt(data)))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    DrawPicture(graphics, data);
                    bitmap.Save(filename, StringToFormat(filetype_val_.GetString(data)));
                }
            }
        }

        /// <summary>
        /// レイアウトを元に、動画または画像を出力
        /// </summary>
        /// <param name="tsv_data">ランクファイルの中身（単なるタブ区切りテキストでよい）</param>
        /// <param name="frame_dir">出力フォルダ</param>
        /// <param name="msgout">情報出力オブジェクト</param>
        public void SaveAllToFile(LayoutData data, string frame_dir, MessageOut msgout)
        {
            int file_no = 1;
            int line_number = line_number_val_.GetInt(data);
            for (int current_pos = range_start_val_.GetInt(data) - 1; current_pos < range_end_val_.GetInt(data) && current_pos < data.RowLength;
                current_pos += line_number)
            {
                LayoutData cut_data = data.Cut(current_pos, Math.Min(line_number, Math.Min(range_end_val_.GetInt(data), data.RowLength) - current_pos));
                string output_filename = GetFileName(cut_data, frame_dir, file_no, video_manager_.HasVideo());
                ++file_no;
                if (video_manager_.HasVideo())
                {
                    SaveVideo(output_filename, cut_data, msgout);
                }
                else
                {
                    SavePicture(output_filename, cut_data);
                }
            }
        }

        private string GetFileName(LayoutData data, string frame_dir, int file_no, bool has_video)
        {
            string output_filename = Path.Combine(frame_dir, filename_val_.GetString(data)); //.Replace("%d", count.ToString())
            // %d を count に置き換える。 %04d などの書式（桁揃え0埋め）も認める
            Regex regex = new Regex("%(0[1-9][0-9]*)?d");
            Match match = regex.Match(output_filename);
            if (match.Success)
            {
                string num = match.Groups[1].Value;
                if (num != "")
                {
                    output_filename = regex.Replace(output_filename, file_no.ToString(new string('0', int.Parse(num))));
                }
                else
                {
                    output_filename = regex.Replace(output_filename, file_no.ToString());
                }
            }
            // 拡張子が .* か無しの場合は自動で付加する
            if (output_filename.EndsWith(".*") || Path.GetExtension(output_filename) == "")
            {
                if (output_filename.EndsWith(".*"))
                {
                    output_filename = output_filename.Substring(0, output_filename.Length - 2);
                }
                if (has_video)
                {
                    output_filename = output_filename + ".avi";
                }
                else
                {
                    string extension = filetype_val_.GetString(data);
                    if (extension == "")
                    {
                        extension = "png";
                    }
                    output_filename = output_filename + "." + extension;
                }
            }
            return output_filename;
        }

        private void InitializeGraphics(Graphics graphics, LayoutData data)
        {
            if (background_val_.GetString(data).ToLower() != "transparent")
            {
                using (Brush brush = new SolidBrush(ColorTranslator.FromHtml(background_val_.GetString(data))))
                {
                    graphics.FillRectangle(brush, 0, 0, img_width_val_.GetInt(data), img_height_val_.GetInt(data));
                }
            }
        }

        private ImageFormat StringToFormat(string str)
        {
            switch (str.ToLower())
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "bitmap":
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                case "tif":
                case "tiff":
                    return ImageFormat.Tiff;
                default:
                    return ImageFormat.Png;
            }
        }
    }

    public class LayoutLine
    {
        private string[] data_;

        public LayoutLine(string data)
        {
            data_ = data.Split('\t');
        }

        public string GetData(int index)
        {
            if (index < data_.Length)
            {
                return data_[index];
            }
            else
            {
                return "";
            }
        }
    }

    public class LayoutData
    {
        private List<string[]> data_ = new List<string[]>();

        public int RowLength
        {
            get { return data_.Count; }
        }

        private LayoutData(LayoutData data, int index, int count)
        {
            foreach (string[] ar in data.data_.GetRange(index, count))
            {
                data_.Add((string[])ar.Clone());
            }
        }

        public LayoutData(string data)
        {
            string[] lines = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (i < lines.Length - 1 || lines[i] != "") // 最終行の末尾は除く
                {
                    data_.Add(lines[i].Split('\t'));
                }
            }
        }

        public string GetData(int row, int col)
        {
            if (row - 1 >= data_.Count)
            {
                return "";
            }
            else if (col - 1 >= data_[row - 1].Length)
            {
                return "";
            }
            else
            {
                return data_[row - 1][col - 1];
            }
        }

        public LayoutData Cut(int index, int count)
        {
            return new LayoutData(this, index, count);
        }

        public string GetReplacedString(string text)
        {
            return NicoUtil.GetReplacedString(text, data_);
        }
    }

    class LayoutVideoManager
    {
        List<LayoutElementVfwFrame> element_list_ = new List<LayoutElementVfwFrame>();

        public int Length
        {
            get
            {
                for (int i = 0; i < element_list_.Count; ++i)
                {
                    if (element_list_[i].IsOpen())
                    {
                        return element_list_[i].FrameLength;
                    }
                }
                return 0;
            }
        }

        public int Rate
        {
            get
            {
                for (int i = 0; i < element_list_.Count; ++i)
                {
                    if (element_list_[i].IsOpen())
                    {
                        return element_list_[i].Rate;
                    }
                }
                return 0;
            }
        }

        public int Scale
        {
            get
            {
                for (int i = 0; i < element_list_.Count; ++i)
                {
                    if (element_list_[i].IsOpen())
                    {
                        return element_list_[i].Scale;
                    }
                }
                return 0;
            }
        }

        public bool HasVideo()
        {
            return element_list_.Count > 0;
        }

        // 少なくとも1つの動画がオープンされているか
        public bool IsOpenAtLeastOne()
        {
            for (int i = 0; i < element_list_.Count; ++i)
            {
                if (element_list_[i].IsOpen())
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(LayoutElementVfwFrame element)
        {
            element_list_.Add(element);
        }
    }

    class Val
    {
        protected string val_string_;
        protected bool is_including_variable_;
        protected int line_number_;
        protected int column_;

        public string ValString
        {
            get { return val_string_; }
        }

        public bool IsIncludingVariable
        {
            get { return is_including_variable_; }
        }

        public Val(int line_number, int column)
        {
            line_number_ = line_number;
            column_ = column;
        }

        public string GetString(LayoutData data)
        {
            return is_including_variable_ ? data.GetReplacedString(val_string_) : val_string_;
        }

        public string GetRawString()
        {
            return val_string_;
        }

        public static bool IsIncludingVariableInText(string text)
        {
            // #記法は3桁まで対応（HTMLカラーコードを#記法とみなさないようにするため）
            return Regex.IsMatch(text, "#[0-9]{1,3}(:[0-9]+)?([^0-9a-fA-F]|$)");
        }
    }

    class IntVal : Val
    {
        private int val_;

        public int IVal
        {
            get { return val_; }
        }

        public IntVal(string str, int line_number, int column)
            : base(line_number, column)
        {
            val_string_ = str;
            is_including_variable_ = Val.IsIncludingVariableInText(str);
            if (!is_including_variable_)
            {
                if (!int.TryParse(val_string_, out val_))
                {
                    throw new LayoutValueException(val_string_, line_number, column);
                }
            }
        }

        public IntVal(string str, int default_val, int line_number, int column)
            : base(line_number, column)
        {
            val_string_ = str;
            is_including_variable_ = Val.IsIncludingVariableInText(str);
            if (!is_including_variable_)
            {
                if (!int.TryParse(val_string_, out val_))
                {
                    val_ = default_val;
                }
            }
        }

        public IntVal(int val)
            : base(-1, -1)
        {
            val_ = val;
            is_including_variable_ = false;
        }

        public int GetInt(LayoutData data)
        {
            if (is_including_variable_)
            {
                return int.Parse(GetString(data));
            }
            else
            {
                return val_;
            }
        }

        public int GetInt(LayoutData data, int default_val)
        {
            if (is_including_variable_)
            {
                string str = GetString(data);
                int val;
                if (!int.TryParse(str, out val))
                {
                    val = default_val;
                }
                return val;
            }
            else
            {
                return val_;
            }
        }

        public void CheckValue(LayoutData data, int column)
        {
            if (is_including_variable_)
            {
                int val;
                string str = GetString(data);
                if (!int.TryParse(GetString(data), out val))
                {
                    throw new LayoutFormatException(str, line_number_, column);
                }
            }
        }

        public void CheckLargerThan(LayoutData data, int border, int column)
        {
            int val;
            if (is_including_variable_)
            {
                string str = GetString(data);
                if (!int.TryParse(GetString(data), out val))
                {
                    throw new LayoutFormatException(str, line_number_, column);
                }
            }
            else
            {
                val = val_;
            }
            if (val <= border)
            {
                throw new LayoutValueLargerThanException(val, border, line_number_, column);
            }
        }

        public int GetRawInt()
        {
            return val_;
        }
    }

    class FloatVal : Val
    {
        private float val_;

        public float FVal
        {
            get { return val_; }
        }

        public FloatVal(string str, int line_number, int column)
            : base(line_number, column)
        {
            val_string_ = str;
            is_including_variable_ = Val.IsIncludingVariableInText(str);
            if (!is_including_variable_)
            {
                if (!float.TryParse(val_string_, out val_))
                {
                    throw new LayoutValueException(val_string_, line_number, column);
                }
            }
            line_number_ = line_number;
        }

        public FloatVal(int val)
            : base(-1, -1)
        {
            val_ = val;
            is_including_variable_ = false;
        }

        public float GetFloat(LayoutData data)
        {
            if (is_including_variable_)
            {
                return float.Parse(GetString(data));
            }
            else
            {
                return val_;
            }
        }

        public float GetInt(LayoutData data, float default_val)
        {
            if (is_including_variable_)
            {
                string str = GetString(data);
                float val;
                if (!float.TryParse(str, out val))
                {
                    val = default_val;
                }
                return val;
            }
            else
            {
                return val_;
            }
        }

        public void CheckValue(LayoutData data, int column)
        {
            if (is_including_variable_)
            {
                float val;
                string str = GetString(data);
                if (!float.TryParse(GetString(data), out val))
                {
                    throw new LayoutFormatException(str, line_number_, column);
                }
            }
        }

        public void CheckLargerThan(LayoutData data, float border, int column)
        {
            float val;
            if (is_including_variable_)
            {
                string str = GetString(data);
                if (!float.TryParse(GetString(data), out val))
                {
                    throw new LayoutFormatException(str, line_number_, column);
                }
            }
            else
            {
                val = val_;
            }
            if (val <= border)
            {
                throw new LayoutValueLargerThanException(val, border, line_number_, column);
            }
        }

        public float GetRawFloat()
        {
            return val_;
        }
    }

    class StringVal : Val
    {
        public StringVal(string str)
            : this(str, -1, -1)
        {
            // Nothing
        }

        public StringVal(string str, int line_number, int column)
            : base(line_number, column)
        {
            val_string_ = str;
            is_including_variable_ = Val.IsIncludingVariableInText(str);
        }
    }

    abstract class LayoutElement
    {
        protected int line_number_;

        public LayoutElement(int line_number)
        {
            line_number_ = line_number;
        }

        // 静止画を描画
        public abstract void Draw(Graphics graphics, LayoutData data);

        // 動画のフレームを描画（デフォルトは静止画の場合と同じ）
        public virtual void DrawFrame(Graphics graphics, LayoutData data, int frame_number)
        {
            Draw(graphics, data);
        }

        // フォーマットが正しいか調べて正しくない場合は例外を返す
        public abstract void CheckError(LayoutData data);

        protected static void DrawStringRight(Graphics g, string str, Font font, Brush brush, int right, int y)
        {
            SizeF size = g.MeasureString(str, font);
            g.DrawString(str, font, brush, right - size.Width, y);
        }

        protected static FontStyle GetFontStyle(string name)
        {
            switch (name.ToLower())
            {
                case "bold":
                    return FontStyle.Bold;
                case "italic":
                    return FontStyle.Italic;
                case "underline":
                    return FontStyle.Underline;
                case "strikeout":
                    return FontStyle.Strikeout;
                default:
                    return FontStyle.Regular;
            }
        }
    }

    abstract class LayoutElementTextBase : LayoutElement
    {
        protected StringVal text_;
        protected StringVal font_;
        protected IntVal font_size_;
        protected StringVal font_style_;
        protected StringVal font_color_;
        protected IntVal left_;
        protected IntVal top_;
        protected IntVal width_;
        protected StringVal alignment_hori_;

        public LayoutElementTextBase(LayoutLine line, int line_number) : base(line_number)
        {
            text_ = new StringVal(line.GetData(1), line_number, 2);
            font_ = new StringVal(line.GetData(2), line_number, 3);
            font_size_ = new IntVal(line.GetData(3), line_number, 4);
            font_style_ = new StringVal(line.GetData(4), line_number, 5);
            font_color_ = new StringVal(line.GetData(5), line_number, 6);
            left_ = new IntVal(line.GetData(6), line_number, 7);
            top_ = new IntVal(line.GetData(7), line_number, 8);
        }

        protected virtual void CheckErrorInner(LayoutData data)
        {
            font_size_.CheckLargerThan(data, 0, 4);
            try
            {
                ColorTranslator.FromHtml(font_color_.GetString(data));
            }
            catch (Exception)
            {
                throw new LayoutInvalidColorException(font_color_.GetString(data), line_number_, 6);
            }
            left_.CheckValue(data, 7);
            top_.CheckValue(data, 8);
        }
    }

    abstract class LayoutElementTextRectBase : LayoutElementTextBase
    {
        protected IntVal height_;
        protected StringVal alignment_vert_;

        public LayoutElementTextRectBase(LayoutLine line, int line_number)
            : base(line, line_number)
        {
            width_ = new IntVal(line.GetData(8), line_number, 9);
            height_ = new IntVal(line.GetData(9), line_number, 10);
            alignment_hori_ = new StringVal(line.GetData(10), line_number, 11);
            alignment_vert_ = new StringVal(line.GetData(11), line_number, 12);
        }

        protected void DrawTextRect(Graphics graphics, LayoutData data, int font_size)
        {
            FontStyle style = GetFontStyle(font_style_.GetString(data));
            Color color = ColorTranslator.FromHtml(font_color_.GetString(data));
            StringFormat format = new StringFormat();
            using (Font font = new Font(font_.GetString(data), font_size, style))
            using (SolidBrush brush = new SolidBrush(color))
            {
                switch (alignment_hori_.GetString(data).ToLower())
                {
                    case "left":
                        format.Alignment = StringAlignment.Near;
                        break;
                    case "center":
                        format.Alignment = StringAlignment.Center;
                        break;
                    case "right":
                        format.Alignment = StringAlignment.Far;
                        break;
                }
                switch (alignment_vert_.GetString(data).ToLower())
                {
                    case "top":
                        format.LineAlignment = StringAlignment.Near;
                        break;
                    case "center":
                        format.LineAlignment = StringAlignment.Center;
                        break;
                    case "bottom":
                        format.LineAlignment = StringAlignment.Far;
                        break;
                }
                Rectangle rect = new Rectangle(left_.GetInt(data), top_.GetInt(data),
                    width_.GetInt(data), height_.GetInt(data));
                graphics.DrawString(text_.GetString(data).Replace("\\n", "\r\n"), font, brush, rect, format);
            }
        }

        protected override void CheckErrorInner(LayoutData data)
        {
            width_.CheckLargerThan(data, 0, 9);
            height_.CheckLargerThan(data, 0, 10);
            base.CheckErrorInner(data);
        }
    }

    class LayoutElementBaseDir : LayoutElement
    {
        private StringVal base_dir_;

        public LayoutElementBaseDir(LayoutLine line, int line_number)
            : base(line_number)
        {
            base_dir_ = new StringVal(line.GetData(1), line_number, 2);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            // Nothing
        }

        public override void CheckError(LayoutData data)
        {
            // Nothing
        }

        public string GetBaseDir(LayoutData data)
        {
            return base_dir_.GetString(data);
        }
    }

    class LayoutElementAntiAlias : LayoutElement, ILayoutEffect
    {
        private StringVal anti_alias_val_;

        public LayoutElementAntiAlias(LayoutLine line, int line_number)
            : base(line_number)
        {
            anti_alias_val_ = new StringVal(line.GetData(1), line_number, 2);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            // Nothing
        }

        public override void CheckError(LayoutData data)
        {
            // Nothing
        }

        public void DoEffect(Graphics graphics, LayoutData data)
        {
            if (anti_alias_val_.GetString(data).ToLower() == "on")
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            else
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            }
        }
    }

    class LayoutElementNormalText : LayoutElementTextBase
    {
        public LayoutElementNormalText(LayoutLine line, int line_number)
            : base(line, line_number)
        {
            alignment_hori_ = new StringVal(line.GetData(8), line_number, 9);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            FontStyle style = GetFontStyle(font_style_.GetString(data));
            Color color = ColorTranslator.FromHtml(font_color_.GetString(data));
            using (Font font = new Font(font_.GetString(data), font_size_.GetInt(data), style))
            using (SolidBrush brush = new SolidBrush(color))
            {
                switch (alignment_hori_.GetString(data).ToLower())
                {
                    case "right":
                        LayoutElement.DrawStringRight(graphics, text_.GetString(data).Replace("\\n", "\r\n"), font, brush,
                            left_.GetInt(data), top_.GetInt(data));
                        break;
                    default: // "left" とその他
                        graphics.DrawString(text_.GetString(data).Replace("\\n", "\r\n"), font, brush, 
                            left_.GetInt(data), top_.GetInt(data));
                        break;
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            CheckErrorInner(data);
        }
    }

    class LayoutElementTextRect : LayoutElementTextRectBase
    {
        public LayoutElementTextRect(LayoutLine line, int line_number)
            : base(line, line_number)
        {

        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            base.DrawTextRect(graphics, data, font_size_.GetInt(data));
        }

        public override void CheckError(LayoutData data)
        {
            base.CheckErrorInner(data);
        }
    }

    class LayoutElementTextSizeFree : LayoutElementTextRectBase
    {
        private IntVal min_font_size_;
        private IntVal font_step_;

        private const int default_min_font_size_ = 10;
        private const int default_font_step_ = 2;

        public LayoutElementTextSizeFree(LayoutLine line, int line_number)
            : base(line, line_number)
        {
            min_font_size_ = new IntVal(line.GetData(12), default_min_font_size_, line_number);
            font_step_ = new IntVal(line.GetData(13), default_font_step_, line_number);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            int width = width_.GetInt(data);
            int font_size = font_size_.GetInt(data);
            int lower_size = min_font_size_.GetInt(data, default_min_font_size_);
            int step = font_step_.GetInt(data, default_font_step_);

            FontStyle style = GetFontStyle(font_style_.GetString(data));
            for (; font_size >= lower_size; font_size -= step)
            {
                using (Font font = new Font(font_.GetString(data), font_size, style))
                {
                    int w = (int)graphics.MeasureString(text_.GetString(data).Replace("\\n", "\r\n"), font).Width;
                    if (w < width)
                    {
                        break;
                    }
                }
            }
            base.DrawTextRect(graphics, data, font_size);
        }

        public override void CheckError(LayoutData data)
        {
            min_font_size_.CheckLargerThan(data, 0, 13);
            font_step_.CheckLargerThan(data, 0, 14);
            base.CheckErrorInner(data);
        }
    }

    class LayoutElementTextFixedWidth : LayoutElementTextBase
    {
        public LayoutElementTextFixedWidth(LayoutLine line, int line_number)
            : base(line, line_number)
        {
            width_ = new IntVal(line.GetData(8), line_number, 9);
            alignment_hori_ = new StringVal(line.GetData(9), line_number, 10);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            int font_size = font_size_.GetInt(data);
            float y = (float)top_.GetInt(data);

            FontStyle style = GetFontStyle(font_style_.GetString(data));

            using (Font font = new Font(font_.GetString(data), font_size, style))
            {
                Color color = ColorTranslator.FromHtml(font_color_.GetString(data));
                using (SolidBrush brush = new SolidBrush(color))
                {
                    float total_width = 0.0F;
                    float fixed_width = (float)width_.GetInt(data);
                    StringFormat format = new StringFormat();
                    CharacterRange[] cr = { new CharacterRange(0, 1) };
                    format.SetMeasurableCharacterRanges(cr);

                    string text = text_.GetString(data).Replace("\\n", "\r\n");

                    for (int i = 0; i < text.Length; ++i)
                    {
                        total_width += GetCharacterWidth(text.Substring(i, 1), graphics, font, format);
                    }
                    float ratio = (total_width >= fixed_width ? fixed_width / total_width : 1.0F);
                    float x = (float)left_.GetInt(data);
                    switch (alignment_hori_.GetString(data).ToLower())
                    {
                        case "center":
                            x -= ((total_width >= fixed_width ? fixed_width : total_width) / 2);
                            break;
                        case "right":
                            x -= (total_width >= fixed_width ? fixed_width : total_width);
                            break;
                    }
                    for (int i = 0; i < text.Length; ++i)
                    {
                        string t = text.Substring(i, 1);
                        graphics.DrawString(t, font, brush, x, y);
                        x += GetCharacterWidth(t, graphics, font, format) * ratio;
                    }
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            width_.CheckLargerThan(data, 0, 9);
            base.CheckErrorInner(data);
        }

        private static float GetCharacterWidth(string t, Graphics graphics, Font font, StringFormat format)
        {
            RectangleF layout_rect = new RectangleF(0, 0, 200, 200);
            bool is_half_space = false;
            if (t == " ") // スペースはそのままだと幅0になるので特別対応
            {
                t = "あ";
                is_half_space = true;
            }
            else if (t == "　")
            {
                t = "あ";
            }
            Region[] regions = graphics.MeasureCharacterRanges(t, font, layout_rect, format);
            float w = regions[0].GetBounds(graphics).Width;
            if (is_half_space)
            {
                w /= 2;
            }
            return w;
        }
    }

    class LayoutElementFixedWidthFlexible : LayoutElementTextBase
    {
        public LayoutElementFixedWidthFlexible(LayoutLine line, int line_number)
            : base(line, line_number)
        {
            width_ = new IntVal(line.GetData(8), line_number, 9);
            alignment_hori_ = new StringVal(line.GetData(9), line_number, 10);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            int font_size = font_size_.GetInt(data);
            float y = (float)top_.GetInt(data);

            FontStyle style = GetFontStyle(font_style_.GetString(data));

            using (Font font = new Font(font_.GetString(data), font_size, style))
            {
                Color color = ColorTranslator.FromHtml(font_color_.GetString(data));
                using (SolidBrush brush = new SolidBrush(color))
                {
                    string text = text_.GetString(data).Replace("\\n", "\r\n");

                    float fixed_width = (float)width_.GetInt(data);

                    float string_width = graphics.MeasureString(text, font).Width;
                    float x = (float)left_.GetInt(data);

                    switch (alignment_hori_.GetString(data).ToLower())
                    {
                        case "center":
                            x -= ((string_width >= fixed_width ? fixed_width : string_width) / 2);
                            break;
                        case "right":
                            x -= (string_width >= fixed_width ? fixed_width : string_width);
                            break;
                    }

                    if (string_width > fixed_width)
                    {
                        graphics.ScaleTransform(fixed_width / string_width, 1.0F);
                        x *= string_width / fixed_width;
                    }

                    graphics.DrawString(text, font, brush, x, y);

                    if (string_width > fixed_width)
                    {
                        graphics.ResetTransform();
                    }
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            width_.CheckLargerThan(data, 0, 9);
            base.CheckErrorInner(data);
        }
    }

    abstract class LayoutElementImageBase : LayoutElement, Openable
    {
        protected StringVal picture_file_name_;
        protected Bitmap bitmap_ = null;

        public LayoutElementImageBase(int line_number)
            : base(line_number)
        {
            // Nothing
        }

        public virtual void Open(LayoutData data, string base_dir1, string base_dir2)
        {
            string filename = Path.Combine(base_dir2, picture_file_name_.GetString(data));
            if (File.Exists(filename))
            {
                bitmap_ = new Bitmap(filename);
            }
            else
            {
                if (base_dir1 != "")
                {
                    filename = Path.Combine(base_dir1, filename);
                    if (File.Exists(filename))
                    {
                        bitmap_ = new Bitmap(filename);
                    }
                }
            }
        }

        public void Close()
        {
            if (bitmap_ != null)
            {
                bitmap_.Dispose();
                bitmap_ = null;
            }
        }
    }

    abstract class LayoutElementImageTextBase : LayoutElementImageBase
    {
        protected string info_text_ = "";

        public LayoutElementImageTextBase(int line_number)
            : base(line_number)
        {

        }

        public override void Open(LayoutData data, string base_dir1, string base_dir2)
        {
            string filename = Path.Combine(base_dir2, picture_file_name_.GetString(data));
            string pic_filename = Path.ChangeExtension(filename, ".png");
            string info_filename = Path.ChangeExtension(filename, ".txt");
            if (File.Exists(pic_filename) && File.Exists(info_filename))
            {
                bitmap_ = new Bitmap(pic_filename);
                info_text_ = IJFile.Read(info_filename);
            }
            else
            {
                if (base_dir1 != "")
                {
                    filename = Path.Combine(base_dir1, filename);
                    pic_filename = Path.ChangeExtension(filename, ".png");
                    info_filename = Path.ChangeExtension(filename, ".txt");
                    if (File.Exists(pic_filename) && File.Exists(info_filename))
                    {
                        bitmap_ = new Bitmap(pic_filename);
                        info_text_ = IJFile.Read(info_filename);
                    }
                }
            }
        }

        // 画像テキストを返す。返り値は Dispose する必要がある
        protected Bitmap GetImageText(string text)
        {
            string[] line = IJStringUtil.SplitWithCRLF(info_text_);
            string[][] info_matrix = new string[line.Length][];
            for (int i = 0; i < line.Length; ++i)
            {
                info_matrix[i] = line[i].Split('\t');
            }
            int ret_width = 0;
            int ret_height = 0;
            for (int i = 0; i < text.Length; ++i) // 画像の幅、高さを計算
            {
                string s = "";
                s += text[i];
                int index = SearchString(info_matrix, s);
                if (index >= 0)
                {
                    int width = int.Parse(info_matrix[index][3]);
                    int height = int.Parse(info_matrix[index][4]);
                    ret_width += width;
                    if (ret_height < height)
                    {
                        ret_height = height;
                    }
                }
            }
            Bitmap ret_image = new Bitmap(ret_width, ret_height);
            using (Graphics graphics = Graphics.FromImage(ret_image))
            {
                int x = 0;
                for (int i = 0; i < text.Length; ++i)
                {
                    string s = "";
                    s += text[i];
                    int index = SearchString(info_matrix, s);
                    if (index >= 0)
                    {
                        int sx = int.Parse(info_matrix[index][1]);
                        int sy = int.Parse(info_matrix[index][2]);
                        int width = int.Parse(info_matrix[index][3]);
                        int height = int.Parse(info_matrix[index][4]);
                        Rectangle src_rect = new Rectangle(sx, sy, width, height);
                        Rectangle dst_rect = new Rectangle(x, 0, width, height);
                        graphics.DrawImage(bitmap_, dst_rect, src_rect, GraphicsUnit.Pixel);
                        x += width;
                    }
                }
            }
            return ret_image;
        }

        private static int SearchString(string[][] info_matrix, string str)
        {
            for (int i = 0; i < info_matrix.Length; ++i)
            {
                if (info_matrix[i][0] == str)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    class LayoutElementImageText : LayoutElementImageTextBase
    {
        private StringVal text_;
        private FloatVal ratio_;
        private IntVal left_;
        private IntVal top_;
        private StringVal alignment_hori_;

        public LayoutElementImageText(LayoutLine line, int line_number)
            : base(line_number)
        {
            text_ = new StringVal(line.GetData(1), line_number, 2);
            picture_file_name_ = new StringVal(line.GetData(2), line_number, 3);
            ratio_ = new FloatVal(line.GetData(3), line_number, 4);
            left_ = new IntVal(line.GetData(4), line_number, 5);
            top_ = new IntVal(line.GetData(5), line_number, 6);
            alignment_hori_ = new StringVal(line.GetData(6), line_number, 7);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            if (bitmap_ != null)
            {
                float ratio = ratio_.GetFloat(data);
                Rectangle rect;
                using (Bitmap img = GetImageText(text_.GetString(data)))
                {
                    if (alignment_hori_.GetString(data).ToLower() == "right")
                    {
                        rect = new Rectangle(left_.GetInt(data) - img.Width, top_.GetInt(data),
                            (int)(img.Width * ratio), (int)(img.Height * ratio));
                    }
                    else
                    {
                        rect = new Rectangle(left_.GetInt(data), top_.GetInt(data),
                            (int)(img.Width * ratio), (int)(img.Height * ratio));
                    }
                    graphics.DrawImage(img, rect);
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            ratio_.CheckLargerThan(data, 0F, 4);
            left_.CheckValue(data, 5);
            top_.CheckValue(data, 6);
        }
    }

    class LayoutElementImageTextRect : LayoutElementImageTextBase
    {
        private StringVal text_;
        private FloatVal ratio_;
        private IntVal left_;
        private IntVal top_;
        private IntVal width_;
        private IntVal height_;
        private StringVal alignment_hori_;
        private StringVal alignment_vert_;

        public LayoutElementImageTextRect(LayoutLine line, int line_number)
            : base(line_number)
        {
            text_ = new StringVal(line.GetData(1), line_number, 2);
            picture_file_name_ = new StringVal(line.GetData(2), line_number, 3);
            ratio_ = new FloatVal(line.GetData(3), line_number, 4);
            left_ = new IntVal(line.GetData(4), line_number, 5);
            top_ = new IntVal(line.GetData(5), line_number, 6);
            width_ = new IntVal(line.GetData(6), line_number, 7);
            height_ = new IntVal(line.GetData(7), line_number, 8);
            alignment_hori_ = new StringVal(line.GetData(8), line_number, 9);
            alignment_vert_ = new StringVal(line.GetData(9), line_number, 10);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            if (bitmap_ != null)
            {
                using (Bitmap img = GetImageText(text_.GetString(data)))
                {
                    float ratio = ratio_.GetFloat(data);
                    Rectangle rect = new Rectangle(0, 0, (int)(img.Width * ratio), (int)(img.Height * ratio));
                    if (alignment_hori_.GetString(data).ToLower() == "right")
                    {
                        rect.X = left_.GetInt(data) + width_.GetInt(data) - rect.Width;
                    }
                    else if (alignment_hori_.GetString(data).ToLower() == "center")
                    {
                        rect.X = left_.GetInt(data) + (width_.GetInt(data) - rect.Width) / 2;
                    }
                    else
                    {
                        rect.X = left_.GetInt(data);
                    }
                    if (alignment_vert_.GetString(data).ToLower() == "bottom")
                    {
                        rect.Y = top_.GetInt(data) + height_.GetInt(data) - rect.Height;
                    }
                    else if (alignment_vert_.GetString(data).ToLower() == "center")
                    {
                        rect.Y = top_.GetInt(data) + (height_.GetInt(data) - rect.Height) / 2;
                    }
                    else
                    {
                        rect.Y = top_.GetInt(data);
                    }
                    graphics.DrawImage(img, rect);
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            ratio_.CheckLargerThan(data, 0F, 4);
            left_.CheckValue(data, 5);
            top_.CheckValue(data, 6);
            width_.CheckLargerThan(data, 0, 7);
            height_.CheckLargerThan(data, 0, 8);
        }
    }

    class LayoutElementImage : LayoutElementImageBase
    {
        private IntVal left_;
        private IntVal top_;
        private IntVal width_;
        private IntVal height_;

        public LayoutElementImage(LayoutLine line, int line_number)
            : base(line_number)
        {
            picture_file_name_ = new StringVal(line.GetData(1), line_number, 2);
            left_ = new IntVal(line.GetData(2), line_number, 3);
            top_ = new IntVal(line.GetData(3), line_number, 4);
            width_ = new IntVal(line.GetData(4), line_number, 5);
            height_ = new IntVal(line.GetData(5), line_number, 6);
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            if (bitmap_ != null)
            {
                int width = width_.GetInt(data);
                if (width <= 0)
                {
                    width = bitmap_.Width;
                }
                int height = height_.GetInt(data);
                if (height <= 0)
                {
                    height = bitmap_.Height;
                }
                graphics.DrawImage(bitmap_, left_.GetInt(data), top_.GetInt(data), width, height);
            }
        }

        public override void CheckError(LayoutData data)
        {
            left_.CheckValue(data, 3);
            top_.CheckValue(data, 4);
            width_.CheckLargerThan(data, 0, 5);
            height_.CheckLargerThan(data, 0, 6);
        }
    }

    class LayoutElementVfwFrame : LayoutElement, Openable
    {
        private StringVal filename_;
        private IntVal left_;
        private IntVal top_;
        private IntVal width_;
        private IntVal height_;

        private VfwAviReader avi_reader_ = null;

        public int FrameLength
        {
            get { return avi_reader_.FrameLength; }
        }

        public int Rate
        {
            get { return avi_reader_.Rate; }
        }

        public int Scale
        {
            get { return avi_reader_.Scale; }
        }

        public LayoutElementVfwFrame(LayoutLine line, int line_number)
            : base(line_number)
        {
            filename_ = new StringVal(line.GetData(1), line_number, 2);
            left_ = new IntVal(line.GetData(2), line_number, 3);
            top_ = new IntVal(line.GetData(3), line_number, 4);
            width_ = new IntVal(line.GetData(4), line_number, 5);
            height_ = new IntVal(line.GetData(5), line_number, 6);
        }

        public void Open(LayoutData data, string base_dir1, string base_dir2)
        {
            string filename = Path.Combine(base_dir2, filename_.GetString(data));
            if (File.Exists(filename))
            {
                avi_reader_ = new VfwAviReader(filename);
            }
            else
            {
                if (base_dir1 != "")
                {
                    filename = Path.Combine(base_dir1, filename);
                    if (File.Exists(filename))
                    {
                        avi_reader_ = new VfwAviReader(filename);
                    }
                }
            }
        }

        public void Close()
        {
            if (avi_reader_ != null)
            {
                avi_reader_.Close();
            }
        }

        public bool IsOpen()
        {
            return avi_reader_ != null;
        }

        public override void Draw(Graphics graphics, LayoutData data)
        {
            DrawFrame(graphics, data, 0);
        }

        public override void DrawFrame(Graphics graphics, LayoutData data, int frame_number)
        {
            if (avi_reader_ != null)
            {
                using (Bitmap bitmap = avi_reader_.GetBitmap(frame_number))
                {
                    int width = width_.GetInt(data);
                    int height = height_.GetInt(data);

                    if (width > 0 && height > 0)
                    {
                        graphics.DrawImage(bitmap, left_.GetInt(data), top_.GetInt(data), width, height);
                    }
                    else
                    {
                        graphics.DrawImage(bitmap, left_.GetInt(data), top_.GetInt(data));
                    }
                }
            }
        }

        public override void CheckError(LayoutData data)
        {
            left_.CheckValue(data, 3);
            top_.CheckValue(data, 4);
            width_.CheckLargerThan(data, 0, 5);
            height_.CheckLargerThan(data, 0, 6);
        }
    }

    interface Openable
    {
        void Open(LayoutData data, string base_dir1, string base_dir2);
        void Close();
    }

    interface ILayoutEffect
    {
        void DoEffect(Graphics graphics, LayoutData data);
    }

    public class LayoutFormatException : Exception
    {
        protected int line_number_;
        protected int column_;

        public LayoutFormatException(string message, int line_number, int column)
            : base(message)
        {
            line_number_ = line_number;
            column_ = column;
        }
    }

    public class LayoutInvalidColorException : LayoutFormatException
    {
        protected string format_str_;

        public LayoutInvalidColorException(string format_str, int line_number, int column)
            : base(line_number + "行目, " + column + "列目 '" + format_str + "' : 色の指定が正しくありません。", line_number, column)
        {
            format_str_ = format_str;
        }
    }

    public class LayoutValueException : LayoutFormatException
    {
        protected string format_str_;

        public LayoutValueException(string format_str, int line_number, int column)
            : base(line_number + "行目, " + column + "列目 '" + format_str + "' : 数字である必要があります。", line_number, column)
        {
            format_str_ = format_str;
        }
    }

    public class LayoutValueLargerThanException : LayoutFormatException
    {
        private int value_;
        private float float_val_;

        private int border_;
        private float float_border_;

        public LayoutValueLargerThanException(int val, int border, int line_number, int column)
            : base(line_number + "行目, " + column + "列目 '" + val + "' : " + border + "より大きい数字である必要があります。", line_number, column)
        {
            value_ = val;
            border_ = border;
        }

        public LayoutValueLargerThanException(float val, float border, int line_number, int column)
            : base(line_number + "行目, " + column + "列目 '" + val + "' : " + border + "より大きい数字である必要があります。", line_number, column)
        {
            float_val_ = val;
            float_border_ = border;
        }
    }
}
