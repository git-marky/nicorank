// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using IJLib.Vfw;

namespace IJLib.SWF2
{
    public class SWFFile
    {
        private MemoryStream data_stream_;
        private BinaryReader data_reader_;
        private string magic_;
        private byte version_;
        private uint file_length_;
        private SWFRect frame_size_;
        private ushort frame_rate_;
        private ushort frame_count_;
        private bool is_prepared_picture_ = false;
        private List<Tags> tag_list_ = new List<Tags>();
        private List<int> frame_position_list_ = new List<int>();
        private List<Picture> picture_list_ = new List<Picture>();
        private List<Shape> shape_list_ = new List<Shape>();
        private Cache cache_ = new Cache();

        public delegate void RunningDelegate(int proceeding_value);

        public enum ObjectType
        {
            ShowFrame = 1,
            DefineBitsJPEG = 6,
            JPEGTables = 8,
            SoundStreamHead = 18,
            SoundStreamBlock = 19,
            DefineBitsJPEG2 = 21,
            PlaceObject2 = 26,
            DefineShape3 = 32,
            DefineBitsJPEG3 = 35,
            DefineBitsLossLess2 = 36,
            DefineUnknown = 100
        };

        public void LoadFromFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            Parse(fs);
            fs.Close();
        }

        public void Parse(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            try
            {
                magic_ = new string(reader.ReadChars(3));
                version_ = reader.ReadByte();
                file_length_ = reader.ReadUInt32();
            }
            catch (Exception)
            {
                throw new Exception("NMMファイルではありません。");
            }

            data_stream_ = new MemoryStream((int)stream.Length);

            Stream zis = null;
            try
            {
                if (magic_ == "CWS")
                {
                    zis = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(stream);
                }
                else if (magic_ == "FWS")
                {
                    zis = stream;
                }
                else
                {
                    throw new Exception("NMMファイルではありません。");
                }
                int s;
                byte[] b = new byte[1024];
                while ((s = zis.Read(b, 0, b.Length)) > 0)
                {
                    data_stream_.Write(b, 0, s);
                }
            }
            finally
            {
                if (magic_ == "CWS" && zis != null)
                {
                    zis.Close();
                }
            }
            data_stream_.Seek(0, SeekOrigin.Begin);
            data_reader_ = new BinaryReader(data_stream_);

            frame_size_.Parse(data_reader_.BaseStream);
            frame_rate_ = data_reader_.ReadUInt16();
            frame_count_ = data_reader_.ReadUInt16();

            while (data_reader_.BaseStream.Position < data_reader_.BaseStream.Length)
            {
                Tags tags = new Tags();
                ushort v = data_reader_.ReadUInt16();
                if (v == 0)
                {
                    break;
                }
                tags.tag = (byte)(v >> 6);
                tags.size = (uint)v & 0x3F;
                if (tags.size == 63)
                {
                    tags.size = data_reader_.ReadUInt32();
                }
                tags.position = (int)data_reader_.BaseStream.Position;
                data_reader_.BaseStream.Seek(tags.size, SeekOrigin.Current);
                tag_list_.Add(tags);
            }
        }

        private void PreparePicture()
        {
            frame_position_list_.Add(0); // 番兵

            for (int i = 0; i < tag_list_.Count; ++i)
            {
                switch (tag_list_[i].tag)
                {
                    case (int)ObjectType.DefineBitsJPEG:
                    case (int)ObjectType.DefineBitsJPEG2:
                    case (int)ObjectType.DefineBitsJPEG3:
                    case (int)ObjectType.DefineBitsLossLess2:
                        Picture picture = new Picture(tag_list_[i], data_reader_, (int)tag_list_[i].size);
                        picture_list_.Add(picture);
                        break;
                    case (int)ObjectType.DefineShape3:
                        Shape shape = new Shape(tag_list_[i], data_reader_, (int)tag_list_[i].size);
                        shape.SetPicture(FindPicture(shape.ImageId));
                        shape_list_.Add(shape);
                        break;
                    case (int)ObjectType.ShowFrame:
                        frame_position_list_.Add(i);
                        break;
                }
            }
        }

        private Picture FindPicture(int id)
        {
            for (int i = 0; i < picture_list_.Count; ++i)
            {
                if (picture_list_[i].Id == id)
                {
                    return picture_list_[i];
                }
            }
            for (int i = 0; i < shape_list_.Count; ++i)
            {
                if (shape_list_[i].Id == id)
                {
                    return shape_list_[i].Picture;
                }
            }
            return null;
        }

        // 長さを秒で返す
        public double Length
        {
            get { return (double)frame_count_ / ((double)frame_rate_ / 256.0); }
        }

        public int FrameCount
        {
            get { return frame_count_; }
        }

        public Bitmap GetFrame(int position, int width, int height)
        {
            if (!is_prepared_picture_)
            {
                PreparePicture();
                is_prepared_picture_ = true;
            }
            
            CacheItem cache_item = new CacheItem();
            int prev = frame_position_list_[position];
            int current = frame_position_list_[position + 1];

            for (int i = prev; i < current; ++i)
            {
                if ((int)tag_list_[i].tag == (int)ObjectType.PlaceObject2)
                {
                    PlaceObject place = new PlaceObject(tag_list_[i], data_reader_, (int)tag_list_[i].size);
                    cache_item.AddCommand(FindPicture(place.ObjectId), place.ObjectId, place.X / 20, place.Y / 20, place.Alpha);
                }
            }

            int src_width = (frame_size_.x_max - frame_size_.x_min) / 20;
            int src_height = (frame_size_.y_max - frame_size_.y_min) / 20;

            Bitmap bitmap = cache_.GetBitmap(cache_item, (width > 0 ? width : src_width), 
                (height > 0 ? height : src_height), src_width, src_height);
            return (Bitmap)bitmap.Clone();
        }

        public void SaveAvi(string input_wav_filename, string output_avi_filename, double start_time, double end_time,
            int dest_width, int dest_height, bool is_compress, RunningDelegate dlg)
        {
            int f_rate = frame_rate_ / 256;

            VfwAviWriter avi_writer = new VfwAviWriter(output_avi_filename, frame_rate_ / 256, 1);

            bool is_success = false;
            try
            {
                int start_frame = (start_time >= 0.0 ? (int)(start_time * f_rate) : 0);
                int end_frame = (end_time >= 0.0 ? (int)(Math.Ceiling(end_time * f_rate)) : frame_count_) - 1;
                if (end_frame >= frame_count_)
                {
                    end_frame = frame_count_ - 1;
                }

                for (int i = start_frame; i <= end_frame; ++i)
                {
                    Bitmap bitmap = GetFrame(i, dest_width, dest_height);
                    avi_writer.AddFrame(bitmap);
                    bitmap.Dispose();
                }
                if (input_wav_filename != "")
                {
                    avi_writer.AddWave(input_wav_filename);
                }
                is_success = true;
            }
            finally
            {
                avi_writer.Close();
                if (!is_success)
                {
                    if (File.Exists(output_avi_filename))
                    {
                        try
                        {
                            File.Delete(output_avi_filename);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        public void SaveJPG(string dir)
        {
            if (!dir.EndsWith("\\"))
            {
                dir += "\\";
            }
            Directory.CreateDirectory(dir);
            int count = 1;
            byte[] jpeg_tables = null;

            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if ((int)tag_list_[i].tag == (int)ObjectType.DefineBitsJPEG || (int)tag_list_[i].tag == (int)ObjectType.DefineBitsJPEG2
                    || (int)tag_list_[i].tag == (int)ObjectType.DefineBitsJPEG3)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(dir + count.ToString("0000") + ".jpg", FileMode.Create, FileAccess.Write);
                        if ((int)tag_list_[i].tag == (int)ObjectType.DefineBitsJPEG3)
                        {
                            // 最初の2バイトはタグなので無視
                            data_reader_.BaseStream.Seek(tag_list_[i].position + 2, SeekOrigin.Begin);
                            int image_data_size = data_reader_.ReadInt32();

                            StreamUtil.Copy(data_stream_, fs, image_data_size);
                        }
                        else
                        {
                            const int header_size = 20;
                            byte[] pre_search = new byte[6];
                            data_stream_.Seek(tag_list_[i].position, SeekOrigin.Begin);
                            data_stream_.Read(pre_search, 0, pre_search.Length);
                            int offset = 2;
                            if (pre_search[3] == 0xD9 && pre_search[5] == 0xD8)
                            {
                                offset += 4;
                            }
                            data_stream_.Seek(tag_list_[i].position + offset, SeekOrigin.Begin);
                            StreamUtil.Copy(data_stream_, fs, header_size);
                            if (tag_list_[i].tag == 6 && jpeg_tables != null)
                            {
                                fs.Write(jpeg_tables, 2, jpeg_tables.Length - 4); // 最初と最後2バイトずつは除く
                            }
                            StreamUtil.Copy(data_stream_, fs, (int)tag_list_[i].size - header_size - offset);
                        }
                        ++count;
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
                else if ((int)tag_list_[i].tag == (int)ObjectType.JPEGTables)
                {
                    jpeg_tables = new byte[tag_list_[i].size];
                    data_stream_.Seek(tag_list_[i].position, SeekOrigin.Begin);
                    data_stream_.Read(jpeg_tables, 0, (int)tag_list_[i].size);
                }
            }
        }

        public void SaveBmp()
        {
            int count = 1;
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if ((int)tag_list_[i].tag == (int)ObjectType.DefineBitsLossLess2)
                {
                    Picture picture = new Picture(tag_list_[i], data_reader_, (int)tag_list_[i].size);
                    picture.SaveBitmap(count.ToString("0000") + ".bmp");
                    ++count;
                }
            }
        }

        public void SaveMp3(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            for (int i = 0; i < tag_list_.Count; ++i)
            {
                if ((int)tag_list_[i].tag == (int)ObjectType.SoundStreamBlock)
                {
                    const int offset = 4;
                    data_stream_.Seek(tag_list_[i].position + offset, SeekOrigin.Begin);
                    StreamUtil.Copy(data_stream_, fs, (int)tag_list_[i].size - offset);
                }
            }
            fs.Close();
        }

        public void Print()
        {
            StringBuilder buff = new StringBuilder();
            foreach (Tags tag in tag_list_)
            {
                buff.Append(tag.tag + ", " + tag.position + ", " + tag.size + "\r\n");
            }
            string str = buff.ToString();
            str += "";
        }

        public static void DrawImageWithAlpha(Graphics graphics, Bitmap bitmap, int x, int y, int alpha)
        {
            if (alpha >= 255)
            {
                graphics.DrawImage(bitmap, x, y);
            }
            else
            {
                System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                cm.Matrix00 = 1;
                cm.Matrix11 = 1;
                cm.Matrix22 = 1;
                cm.Matrix33 = (float)alpha / 255.0F;
                cm.Matrix44 = 1;

                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);

                graphics.DrawImage(bitmap, new Rectangle(x, y, bitmap.Width, bitmap.Height), 0, 0,
                    bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
            }
        }

        private class Picture
        {
            private Tags tags_;
            private int image_id_;
            private Bitmap image_ = null;
            private byte[] raw_data_;
            private bool is_loss_less_ = false;
            private int width_;
            private int height_;

            public Bitmap Image
            {
                get { return image_; }
            }

            public int Id
            {
                get { return image_id_; }
            }

            public bool IsLossLess
            {
                get { return is_loss_less_; }
            }

            public int Width
            {
                get { return width_; }
            }

            public int Height
            {
                get { return height_; }
            }

            public byte[] RawData
            {
                get { return raw_data_; }
            }

            public Picture(Tags tags, BinaryReader reader, int size)
            {
                tags_ = tags;
                Parse(tags, reader, size);
            }

            public void Parse(Tags tags, BinaryReader reader, int size)
            {
                reader.BaseStream.Seek(tags.position, SeekOrigin.Begin);
                image_id_ = (int)reader.ReadUInt16();

                if ((int)tags.tag == (int)ObjectType.DefineBitsLossLess2)
                {
                    is_loss_less_ = true;

                    byte format = reader.ReadByte();
                    width_ = reader.ReadUInt16();
                    height_ = reader.ReadUInt16();

                    MemoryStream ms = new MemoryStream();

                    StreamUtil.Copy(reader.BaseStream, ms, size - 7);
                    ms.Seek(0, SeekOrigin.Begin);

                    ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream zis =
                        new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms);

                    MemoryStream ms2 = new MemoryStream();

                    byte[] b = new byte[1024];
                    int s;
                    while ((s = zis.Read(b, 0, b.Length)) > 0)
                    {
                        ms2.Write(b, 0, s);
                    }
                    zis.Close();

                    ms2.Seek(0, SeekOrigin.Begin);
                    raw_data_ = new byte[ms2.Length];
                    ms2.Read(raw_data_, 0, raw_data_.Length);
                }
                else
                {
                    reader.BaseStream.Seek(tags.position, SeekOrigin.Begin);
                    image_id_ = (int)reader.ReadUInt16();
                    MemoryStream ms = new MemoryStream();
                    StreamUtil.Copy(reader.BaseStream, ms, size - 2);
                    image_ = new Bitmap(ms);
                }
            }

            public void SaveBitmap(string filename)
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                WriteBitmapHeader(fs, (uint)width_, (uint)height_, (uint)raw_data_.Length);
                fs.Write(raw_data_, 0, raw_data_.Length);
                fs.Close();
            }

            private void WriteBitmapHeader(Stream stream, uint width, uint height, uint data_length)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((byte)'B');
                writer.Write((byte)'M');
                writer.Write((uint)(54 + data_length));
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write(54);
                writer.Write(40);
                writer.Write(width);
                writer.Write(height);
                writer.Write((ushort)1);
                writer.Write((ushort)32);
                writer.Write(0);
                writer.Write(0x86C0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
            }
        }

        private class Shape
        {
            private Tags tags_;
            private int shape_id_;
            private int picture_ref_;
            private Picture picture_;

            private SWFRect rect_;
            private SWFShapeWithStyle shape_with_style_;

            public int ImageId
            {
                get { return picture_ref_; }
            }

            public int Id
            {
                get { return shape_id_; }
            }

            public Picture Picture
            {
                get { return picture_; }
            }

            public Shape(Tags tags, BinaryReader reader, int size)
            {
                tags_ = tags;
                reader.BaseStream.Seek(tags.position, SeekOrigin.Begin);
                shape_id_ = (int)reader.ReadUInt16();
                Parse(reader.BaseStream);

                if (shape_with_style_ != null)
                {
                    SWFFillStyle fill_style = shape_with_style_.GetFillStyle();
                    if (fill_style.IsBitmap())
                    {
                        picture_ref_ = fill_style.GetBitmapRef();
                    }
                }
            }

            public void SetPicture(Picture picture)
            {
                picture_ = picture;
            }

            public void Parse(Stream stream)
            {
                rect_ = new SWFRect();
                rect_.Parse(stream);
                shape_with_style_ = new SWFShapeWithStyle();
                shape_with_style_.Parse(stream);
            }
        }

        private class PlaceObject
        {
            private Tags tags_;
            private ushort depth_;
            private ushort object_id_ref_ = 0xFFFF;
            private SWFMatrix matrix_ = null;
            private SWFColorTransform color_trans_ = null;

            public int ObjectId
            {
                get { return object_id_ref_; }
            }

            public SWFMatrix Matrix
            {
                get { return matrix_; }
            }

            public SWFColorTransform ColorTransform
            {
                get { return color_trans_; }
            }

            public int X
            {
                get { return matrix_.translate_x; }
            }

            public int Y
            {
                get { return matrix_.translate_y; }
            }

            public int Alpha
            {
                get { return ((color_trans_ != null) ? color_trans_.GetColorRedMult() : 255); }
            }

            public PlaceObject(Tags tags, BinaryReader reader, int size)
            {
                tags_ = tags;
                reader.BaseStream.Seek(tags.position, SeekOrigin.Begin);
                Parse(reader);
            }

            public void Parse(BinaryReader reader)
            {
                byte b = (byte)reader.ReadByte();
                depth_ = reader.ReadUInt16();
                if ((b & 0xE1) != 0)
                {
                    throw new Exception("error");
                }
                if ((b & 0x02) != 0)
                {
                    object_id_ref_ = reader.ReadUInt16();
                }
                if ((b & 0x04) != 0)
                {
                    matrix_ = new SWFMatrix();
                    matrix_.Parse(reader.BaseStream);
                }
                if ((b & 0x08) != 0)
                {
                    color_trans_ = new SWFColorTransform();
                    color_trans_.Parse(reader.BaseStream, true);
                }
            }
        }

        private class Cache
        {
            private List<CacheItem> cache_item_list_ = new List<CacheItem>();
            private List<Bitmap> bitmap_list_ = new List<Bitmap>();

            public Bitmap GetBitmap(CacheItem cache_item, int dest_width, int dest_height, int src_width, int src_height)
            {
                for (int i = 0; i < cache_item_list_.Count; ++i)
                {
                    if (cache_item_list_[i].Equals(cache_item))
                    {
                        return bitmap_list_[i];
                    }
                }
                cache_item_list_.Add(cache_item);
                Bitmap bitmap = cache_item.MakeBitmap(dest_width, dest_height, src_width, src_height);
                bitmap_list_.Add(bitmap);
                return bitmap;
            }
        }

        private class CacheItem
        {
            private List<Command> command_list_ = new List<Command>();

            public void AddCommand(Picture picture, int id, int x, int y, int alpha)
            {
                Command command = new Command(picture, id, x, y, alpha);
                command_list_.Add(command);
            }

            public override bool Equals(object obj)
            {
                CacheItem cache_item = (CacheItem)obj;
                if (command_list_.Count != cache_item.command_list_.Count)
                {
                    return false;
                }
                for (int i = 0; i < command_list_.Count; ++i)
                {
                    if (!command_list_[i].Equals(cache_item.command_list_[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public Bitmap MakeBitmap(int dest_width, int dest_height, int src_width, int src_height)
            {
                Bitmap bitmap = new Bitmap(dest_width, dest_height, PixelFormat.Format24bppRgb);
                using (Bitmap temp_bitmap = new Bitmap(src_width, src_height))
                {
                    using (Graphics graphics = Graphics.FromImage(temp_bitmap))
                    {
                        for (int i = 0; i < command_list_.Count; ++i)
                        {
                            if (command_list_[i].picture.IsLossLess)
                            {
                                Drawing.CopyImage(temp_bitmap, command_list_[i].picture.RawData, command_list_[i].x, command_list_[i].y,
                                    command_list_[i].picture.Width, command_list_[i].picture.Height, command_list_[i].alpha);
                            }
                            else
                            {
                                DrawImageWithAlpha(graphics, command_list_[i].picture.Image, command_list_[i].x,
                                    command_list_[i].y, command_list_[i].alpha);
                            }
                        }
                    }
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.DrawImage(temp_bitmap, new Rectangle(0, 0, dest_width, dest_height));
                    }
                }
                return bitmap;
            }

            private class Command
            {
                public int id;
                public Picture picture;
                public int x;
                public int y;
                public int alpha;

                public Command(Picture picture, int id, int x, int y, int alpha)
                {
                    this.picture = picture;
                    this.id = id;
                    this.x = x;
                    this.y = y;
                    this.alpha = alpha;
                }

                public override bool Equals(object obj)
                {
                    Command com = (Command)obj;
                    return id == com.id && x == com.x && y == com.y && alpha == com.alpha;
                }

                public override int GetHashCode()
                {
 	                 return base.GetHashCode();
                }
            }
        }
    }

    class Tags
    {
        public byte tag;
        public int position;
        public uint size;
    }

    struct SWFRect
    {
        public int size;
        public int x_min;
        public int x_max;
        public int y_min;
        public int y_max;

        public void Parse(Stream stream)
        {
            uint b = (byte)stream.ReadByte();
            size = (int)((b >> 3) & 0x1F);
            int rem = 3;
            b &= 0x7;
            x_min = (int)StreamUtil.GetBit(stream, ref rem, ref b, size);
            x_max = (int)StreamUtil.GetBit(stream, ref rem, ref b, size);
            y_min = (int)StreamUtil.GetBit(stream, ref rem, ref b, size);
            y_max = (int)StreamUtil.GetBit(stream, ref rem, ref b, size);
        }
    }

    public class SWFMatrix
    {
        public short scale_x = 1 << 8;
        public short scale_y = 1 << 8;

        public short rotate_skew0 = 0;
        public short rotate_skew1 = 0;

        public short translate_x = 0;
        public short translate_y = 0;


        public void Parse(Stream stream)
        {
            uint b = 0;
            int rem = 0;
            byte c = (byte)StreamUtil.GetBit(stream, ref rem, ref b, 1);
            if (c != 0)
            {
                int scale_bits = (int)StreamUtil.GetBit(stream, ref rem, ref b, 5);
                scale_x = (short)StreamUtil.GetBit(stream, ref rem, ref b, scale_bits);
                scale_y = (short)StreamUtil.GetBit(stream, ref rem, ref b, scale_bits);
            }
            c = (byte)StreamUtil.GetBit(stream, ref rem, ref b, 1);
            if (c != 0)
            {
                int rotate_bits = (int)StreamUtil.GetBit(stream, ref rem, ref b, 5);
                rotate_skew0 = (short)StreamUtil.GetBit(stream, ref rem, ref b, rotate_bits);
                rotate_skew1 = (short)StreamUtil.GetBit(stream, ref rem, ref b, rotate_bits);
            }
            int translate_bits = (int)StreamUtil.GetBit(stream, ref rem, ref b, 5);
            translate_x = (short)StreamUtil.GetBit(stream, ref rem, ref b, translate_bits);
            translate_y = (short)StreamUtil.GetBit(stream, ref rem, ref b, translate_bits);
        }

        public override string ToString()
        {
            return "(" + scale_x + ", " + scale_y + ", " + rotate_skew0 + ", " +
                rotate_skew1 + ", " + translate_x + ", " + translate_y + ")";
        }
    }

    public class SWFColorTransform
    {
        public bool color_has_add;
        public bool color_has_mult;
        public byte color_bits;

        public short color_red_mult;
        public short color_green_mult;
        public short color_blue_mult;
        public short color_alpha_mult;

        public short color_red_add;
        public short color_green_add;
        public short color_blue_add;
        public short color_alpha_add;

        public short GetColorRedMult()
        {
            return (color_has_mult) ? color_red_mult : (short)255;
        }

        public short GetColorGreenMult()
        {
            return (color_has_mult) ? color_green_mult : (short)255;
        }

        public short GetColorBlueMult()
        {
            return (color_has_mult) ? color_blue_mult : (short)255;
        }

        public short GetColorAlphaMult()
        {
            return (color_has_mult) ? color_alpha_mult : (short)255;
        }

        public void Parse(Stream stream, bool is_place_object2)
        {
            uint b = 0;
            int rem = 0;
            byte c = (byte)StreamUtil.GetBit(stream, ref rem, ref b, 1);
            color_has_add = (c != 0);
            c = (byte)StreamUtil.GetBit(stream, ref rem, ref b, 1);
            color_has_mult = (c != 0);
            color_bits = (byte)StreamUtil.GetBit(stream, ref rem, ref b, 4);

            if (color_has_mult)
            {
                color_red_mult = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                color_green_mult = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                color_blue_mult = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                if (is_place_object2)
                {
                    color_alpha_mult = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                }
            }
            if (color_has_add)
            {
                color_red_add = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                color_green_add = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                color_blue_add = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                if (is_place_object2)
                {
                    color_alpha_add = (short)StreamUtil.GetBit(stream, ref rem, ref b, (int)color_bits);
                }
            }
        }

        public override string ToString()
        {
            string ret = "[";
            if (color_has_add)
            {
                ret += "add, " + color_red_add + ", " + color_green_add + ", " + color_blue_add + ", " + color_alpha_add;
            }
            if (color_has_mult)
            {
                if (color_has_add)
                {
                    ret += ", ";
                }
                ret += "mult, " + color_red_mult + ", " + color_green_mult + ", " + color_blue_mult + ", " + color_alpha_mult;
            }
            ret += "]";
            return ret;
        }
    }

    public class SWFShapeWithStyle
    {
        private SWFStyles styles = new SWFStyles();
        // SWFShapeRecord の実装まだ

        public void Parse(Stream stream)
        {
            styles.Parse(stream);
        }

        public SWFFillStyle GetFillStyle()
        {
            return styles.GetFillStyle();
        }
    }

    public class SWFStyles
    {
        private List<SWFFillStyle> fill_style_list = new List<SWFFillStyle>();
        private List<SWFLineStyle> line_style_list = new List<SWFLineStyle>();
        private byte fill_bits_count;
        private byte line_bits_count;

        public void Parse(Stream stream)
        {
            int count = stream.ReadByte();
            BinaryReader reader = new BinaryReader(stream);
            if (count == 255)
            {
                count = (int)reader.ReadUInt16();
            }
            for (int i = 0; i < count; ++i)
            {
                SWFFillStyle fill_style = new SWFFillStyle();
                fill_style.Parse(stream);
                fill_style_list.Add(fill_style);
            }
            count = stream.ReadByte();
            if (count == 255)
            {
                count = (int)reader.ReadUInt16();
            }
            for (int i = 0; i < count; ++i)
            {
                SWFLineStyle line_style = new SWFLineStyle();
                line_style.Parse(stream);
                line_style_list.Add(line_style);
            }
            byte c = (byte)stream.ReadByte();
            fill_bits_count = (byte)(c >> 4);
            line_bits_count = (byte)(c & 0x0F);
        }

        public SWFFillStyle GetFillStyle()
        {
            return fill_style_list[0];
        }
    }

    public class SWFFillStyle
    {
        private byte type;
        private ushort bitmap_ref;
        private SWFMatrix bitmap_matrix;

        public void Parse(Stream stream)
        {
            type = (byte)stream.ReadByte();
            if (type == 0x40 || type == 0x41 || type == 0x42 || type == 0x43)
            {
                BinaryReader reader = new BinaryReader(stream);
                bitmap_matrix = new SWFMatrix();
                bitmap_ref = reader.ReadUInt16();
                bitmap_matrix.Parse(stream);
            }
            else
            {
                throw new Exception("認識できない FillStyle です。");
            }
        }

        public bool IsBitmap()
        {
            return (type == 0x40 || type == 0x41 || type == 0x42 || type == 0x43);
        }

        public int GetBitmapRef()
        {
            return bitmap_ref;
        }
    }

    public class SWFLineStyle
    {
        private ushort width;
        private uint rgba;

        public void Parse(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            width = reader.ReadUInt16();
            rgba = reader.ReadUInt32();
        }
    }

    static class Drawing
    {
        public static void CopyImage(Bitmap bmp, byte[] src_pic, int x, int y, int width, int height, int alpha)
        {
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    if (0 <= x + j && x + j < bmp.Width && 0 <= y + i && y + i < bmp.Height)
                    {
                        Color c = bmp.GetPixel(x + j, y + i);
                        int a = src_pic[4 * (i * width + j) + 0];
                        int r = src_pic[4 * (i * width + j) + 1];
                        int g = src_pic[4 * (i * width + j) + 2];
                        int b = src_pic[4 * (i * width + j) + 3];

                        int n = (0xFF << 24);
                        n |= (((c.R * (65536 - alpha * a) + r * alpha * a) / 65536) << 16);
                        n |= (((c.G * (65536 - alpha * a) + g * alpha * a) / 65536) << 8);
                        n |= ((c.B * (65536 - alpha * a) + b * alpha * a) / 65536);

                        bmp.SetPixel(x + j, y + i, Color.FromArgb(n));
                    }
                }
            }
        }
    }

    static class StreamUtil
    {
        public static uint GetBit(Stream stream, ref int rem, ref uint b, int s)
        {
            if (s <= 0)
            {
                return 0;
            }
            while (rem < s)
            {
                b <<= 8;
                b |= (byte)stream.ReadByte();
                rem += 8;
            }
            rem -= s;
            uint ret = (uint)(b >> rem);
            b &= (uint)((1u << rem) - 1);
            return ret;
        }

        public static int Copy(Stream src_stream, Stream dest_stream, int num)
        {
            byte[] b = new byte[1024];
            int remain = num;
            while (remain >= 0)
            {
                int c = src_stream.Read(b, 0, Math.Min(b.Length, remain));
                if (c <= 0)
                {
                    return num - remain;
                }
                dest_stream.Write(b, 0, c);
                remain -= c;
            }
            return num;
        }
    }
}
