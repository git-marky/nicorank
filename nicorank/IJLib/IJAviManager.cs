// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace IJLib.IJAVI
{
    public class AviStream
    {
        private string fcc_type_ = "";
        private byte[] fcc_handler_ = new byte[4];
        private int scale_;
        private int rate_;
        private int start_;
        private int length_;
        public byte[] strf_;

        private IndexSet index_set_ = new IndexSet();

        public IndexSet IndexSet
        {
            get { return index_set_; }
        }

        public AviStream(BinaryReader br, uint list_size)
        {
            Parse(br, list_size);
        }

        public void Parse(BinaryReader br, uint list_size)
        {
            byte[] buff1 = new byte[1024];

            long start_pos = br.BaseStream.Position;
            while (br.BaseStream.Position - start_pos < list_size)
            {
                br.Read(buff1, 0, 4);
                if (Util.EqualsSubString(buff1, 0, 4, "LIST"))
                {
                    uint list_size2 = br.ReadUInt32();
                    br.BaseStream.Seek((long)list_size2, SeekOrigin.Current);
                }
                else // チャンクなら
                {
                    uint chunk_size = br.ReadUInt32();
                    if (Util.EqualsSubString(buff1, 0, 4, "strh"))
                    {
                        if (chunk_size > buff1.Length)
                        {
                            throw new AVIFormatException();
                        }
                        br.Read(buff1, 0, (int)chunk_size);
                        ParseStrh(buff1);
                    }
                    else if (Util.EqualsSubString(buff1, 0, 4, "strf"))
                    {
                        strf_ = new byte[chunk_size];
                        br.Read(strf_, 0, (int)chunk_size);
                    }
                    else if (Util.EqualsSubString(buff1, 0, 4, "indx")) // AVI 2.0
                    {
                        // 位置を保存しておく
                        long current_pos = br.BaseStream.Position;
                        index_set_.ParseSuperIndex(br);
                        // 位置を戻す
                        br.BaseStream.Seek(current_pos + chunk_size, SeekOrigin.Begin);
                    }
                    else
                    {
                        if (chunk_size % 4 != 0)
                        {
                            chunk_size += (4 - chunk_size % 4);
                        }
                        br.BaseStream.Seek((long)chunk_size, SeekOrigin.Current);
                    }
                }
            }
            if (br.BaseStream.Position < start_pos + list_size)
            {
                br.BaseStream.Seek(start_pos + list_size, SeekOrigin.Begin);
            }
        }

        private void ParseStrh(byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            BinaryReader br = new BinaryReader(ms);
            for (int i = 0; i < 4; ++i)
            {
                fcc_type_ += (char)br.ReadByte();
            }
            br.Read(fcc_handler_, 0, 4);
            br.BaseStream.Seek(12, SeekOrigin.Current);
            scale_ = (int)br.ReadUInt32();
            rate_ = (int)br.ReadUInt32();
            start_ = (int)br.ReadUInt32();
            length_ = (int)br.ReadUInt32();
        }

        public bool IsVideo()
        {
            return fcc_type_ == "vids";
        }

        public bool IsAudio()
        {
            return fcc_type_ == "auds";
        }

        public int GetLength()
        {
            return length_;
        }

        public double GetFrameRate()
        {
            return (double)rate_ / scale_;
        }

        public string GetFccHandlerString()
        {
            string ret = "";
            for (int i = 0; i < 4; ++i)
            {
                ret += (char)fcc_handler_[i];
            }
            return ret;
        }

        public bool IsWave()
        {
            return EqualsFccHandler(0, 0, 0, 0);
        }

        public bool EqualsFccHandler(byte b1, byte b2, byte b3, byte b4)
        {
            if (fcc_handler_[0] == b1 && fcc_handler_[1] == b2 && fcc_handler_[2] == b3 && fcc_handler_[3] == b4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] GetAllData(Stream stream)
        {
            return index_set_.GetAllData(stream);
        }
    }

    public struct IndexEntry
    {
        public string ckid;
        public uint flags;
        public long chunk_offset;
        public int chunk_length;
    }

    public struct SuperIndexEntry
    {
        public long offset;
        public uint size;
        public uint duration;
    }

    public class IndexSet
    {
        List<IndexEntry> index_list_ = new List<IndexEntry>();
        List<SuperIndexEntry> super_index_list_ = new List<SuperIndexEntry>();

        public int Count
        {
            get { return index_list_.Count; } 
        }

        public IndexEntry this[int index]
        {
            get { return index_list_[index]; }
        }

        public void Add(IndexEntry index_entry)
        {
            index_list_.Add(index_entry);
        }

        public void ParseSuperIndex(BinaryReader br)
        {
            br.ReadBytes(4);
            int len = br.ReadInt32();
            br.ReadBytes(16);
            for (int i = 0; i < len; ++i)
            {
                SuperIndexEntry entry = new SuperIndexEntry();
                entry.offset = br.ReadInt64();
                entry.size = br.ReadUInt32();
                entry.duration = br.ReadUInt32();
                super_index_list_.Add(entry);
            }
            for (int i = 0; i < super_index_list_.Count; ++i)
            {
                br.BaseStream.Seek(super_index_list_[i].offset, SeekOrigin.Begin);
                br.ReadBytes(12);
                int len2 = br.ReadInt32();
                br.ReadBytes(4);
                long offset = br.ReadInt64();
                br.ReadBytes(4);
                for (int j = 0; j < len2; ++j)
                {
                    IndexEntry entry = new IndexEntry();
                    entry.chunk_offset = br.ReadInt32() + offset;
                    entry.chunk_length = br.ReadInt32();
                    index_list_.Add(entry);
                }
            }
        }

        public void MakeIndex(BinaryReader br, int movi_start)
        {
            br.BaseStream.Seek(movi_start + 4, SeekOrigin.Begin);
            index_list_ = new List<IndexEntry>();
            byte[] buff1 = new byte[1024];

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                br.Read(buff1, 0, 4);
                if (Util.EqualsSubString(buff1, 2, 2, "dc")) // "??dc" なら
                {
                    uint size = br.ReadUInt32();

                    IndexEntry index = new IndexEntry();
                    index.chunk_length = (int)size;
                    index.chunk_offset = (int)(br.BaseStream.Position - movi_start - 8);
                    br.BaseStream.Seek(size, SeekOrigin.Current);
                    index_list_.Add(index);
                }
                else
                {
                    break;
                }
            }
        }

        public bool HasEntry()
        {
            return index_list_.Count > 0;
        }

        public bool HasSuperIndex()
        {
            return super_index_list_.Count > 0;
        }

        public byte[] GetAllData(Stream stream)
        {
            long sum = 0;
            for (int i = 0; i < index_list_.Count; ++i)
            {
                sum += index_list_[i].chunk_length;
            }
            byte[] data = new byte[sum];
            sum = 0;
            for (int i = 0; i < index_list_.Count; ++i)
            {
                stream.Seek(index_list_[i].chunk_offset, SeekOrigin.Begin);
                stream.Read(data, (int)sum, index_list_[i].chunk_length);
                sum += index_list_[i].chunk_length;
            }
            return data;
        }
    }

    public interface AviElement
    {

    }

    public class AviList : AviElement
    {
        public List<AviElement> child = new List<AviElement>();
        string fourCC_;

        public void SetFourCC(string fourCC)
        {
            fourCC_ = fourCC;
        }

        public void Add(AviElement element)
        {
            child.Add(element);
        }
    }

    public class Chunk : AviElement
    {
        string fourCC_;
        byte[] data_ = null;
        long size_;
        long position_ = -1;

        public Chunk(string fourCC, long size, byte[] data)
        {
            fourCC_ = fourCC;
            size_ = size;
            data_ = data;
        }

        public Chunk(string fourCC, long size, long position)
        {
            fourCC_ = fourCC;
            size_ = size;
            position_ = position;
        }

        public byte[] GetData(Stream stream)
        {
            if (data_ != null)
            {
                return data_;
            }
            else
            {
                stream.Seek(position_, SeekOrigin.Begin);
                byte[] b = new byte[size_];
                stream.Read(b, 0, (int)size_);
                return b;
            }
        }
    }

    public class IJAviManager
    {
        private FileStream file_stream_ = null;

        private int stream_number_ = 0;
        private List<AviStream> stream_list_ = new List<AviStream>();
        private int width_;
        private int height_;

        public IJAviManager()
        {
            // Nothing
        }

        public IJAviManager(string filename)
            : this(filename, false)
        {
            // Nothing
        }

        public IJAviManager(string filename, bool is_edit)
        {
            Open(filename, is_edit);
        }

        public void Open(string filename, bool is_edit)
        {
            if (is_edit)
            {
                file_stream_ = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                file_stream_ = new FileStream(filename, FileMode.Open, FileAccess.Read);
            }
            Parse(file_stream_);
        }

        public void Close()
        {
            if (file_stream_ != null)
            {
                file_stream_.Close();
                file_stream_ = null;
                stream_number_ = 0;
                stream_list_.Clear();
            }
        }

        public int GetWidth()
        {
            return width_;
        }

        public int GetHeight()
        {
            return height_;
        }

        public int GetLength()
        {
            return GetVideoStream().GetLength();
        }

        public AviStream GetVideoStream()
        {
            for (int i = 0; i < stream_list_.Count; ++i)
            {
                if (stream_list_[i].IsVideo())
                {
                    return stream_list_[i];
                }
            }
            return null;
        }

        public AviStream GetAudioStream()
        {
            for (int i = 0; i < stream_list_.Count; ++i)
            {
                if (stream_list_[i].IsAudio())
                {
                    return stream_list_[i];
                }
            }
            return null;
        }

        public byte[] GetWave()
        {
            AviStream audio_stream = GetAudioStream();
            return (audio_stream != null ? audio_stream.GetAllData(file_stream_) : null);
        }

        public void Parse(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            uint riff_size = ParseHeader(br);
            byte[] buff1 = new byte[1024];
            int movi_start = 0;
            AviStream video_stream = null;

            long start = br.BaseStream.Position;

            if (riff_size == 0)
            {
                riff_size = uint.MaxValue;
            }

            while (br.BaseStream.Position - start < riff_size && br.BaseStream.Position < br.BaseStream.Length)
            {
                br.Read(buff1, 0, 4);
                if (Util.EqualsSubString(buff1, 0, 4, "LIST"))
                {
                    long list_size = (long)br.ReadUInt32() - 4;
                    br.Read(buff1, 0, 4);
                    if (Util.EqualsSubString(buff1, 0, 4, "hdrl"))
                    {
                        ParseHdrl(br, (uint)list_size);
                    }
                    else if (Util.EqualsSubString(buff1, 0, 4, "movi"))
                    {
                        movi_start = (int)br.BaseStream.Position - 4;
                        if (list_size > 0)
                        {
                            br.BaseStream.Seek((long)list_size, SeekOrigin.Current);
                        }
                        else
                        {
                            br.BaseStream.Seek(0, SeekOrigin.End);
                            break;
                        }
                    }
                    else
                    {
                        // 無視
                        br.BaseStream.Seek((long)list_size, SeekOrigin.Current);
                    }
                }
                else // チャンクなら
                {
                    uint chunk_size = br.ReadUInt32();
                    
                    if (Util.EqualsSubString(buff1, 0, 4, "idx1"))
                    {
                        video_stream = GetVideoStream();
                        // AVI 2.0 の場合は解析する必要なし
                        if (video_stream != null && !video_stream.IndexSet.HasSuperIndex())
                        {
                            ParseIdx1(br, chunk_size, movi_start);
                        }
                    }
                    else
                    {
                        br.BaseStream.Seek((long)chunk_size, SeekOrigin.Current);
                    }
                }
            }

            video_stream = GetVideoStream();
            if (video_stream != null)
            {
                // IndexEntry が無いときは "??dc" を探してエントリーを作る
                if (!video_stream.IndexSet.HasEntry())
                {
                    video_stream.IndexSet.MakeIndex(br, movi_start);
                }
            }
        }

        private uint ParseHeader(BinaryReader br)
        {
            byte[] buff1 = new byte[4];
            br.Read(buff1, 0, 4);
            if (!Util.EqualsString(buff1, "RIFF"))
            {
                throw new AVIFormatException("AVI ファイルではありません。");
            }
            uint riff_size = br.ReadUInt32() - 4;
            br.Read(buff1, 0, 4);

            if (!Util.EqualsString(buff1, "AVI ") && !Util.EqualsString(buff1, "AVIX"))
            {
                throw new AVIFormatException("AVI ファイルではありません。");
            }
            return riff_size;
        }

        private void ParseHdrl(BinaryReader br, uint list_size)
        {
            byte[] buff1 = new byte[1024];

            long start = br.BaseStream.Position;
            while (br.BaseStream.Position - start < list_size)
            {
                br.Read(buff1, 0, 4);
                if (Util.EqualsSubString(buff1, 0, 4, "LIST"))
                {
                    uint list_size2 = br.ReadUInt32() - 4;
                    br.Read(buff1, 0, 4);
                    if (Util.EqualsSubString(buff1, 0, 4, "strl"))
                    {
                        stream_list_.Add(new AviStream(br, list_size2));
                    }
                    else
                    {
                        // 無視
                        br.BaseStream.Seek((long)list_size2, SeekOrigin.Current);
                    }
                }
                else // チャンクなら
                {
                    uint chunk_size = br.ReadUInt32();
                    if (Util.EqualsSubString(buff1, 0, 4, "avih"))
                    {
                        if (chunk_size > buff1.Length)
                        {
                            throw new Exception("フォーマットエラー");
                        }
                        br.Read(buff1, 0, (int)chunk_size);
                        ParseAvih(buff1);
                    }
                    else
                    {
                        br.BaseStream.Seek((long)chunk_size, SeekOrigin.Current);
                    }
                }
            }
        }

        private void ParseAvih(byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            ms.Seek(24, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(ms);
            stream_number_ = (int)br.ReadUInt32();
            br.BaseStream.Seek(4, SeekOrigin.Current);
            width_ = (int)br.ReadUInt32();
            height_ = (int)br.ReadUInt32();
        }

        private void ParseIdx1(BinaryReader br, uint chunk_size, int movi_start)
        {
            long start_pos = br.BaseStream.Position;
            AviStream video_stream = GetVideoStream();
            while (br.BaseStream.Position - start_pos < chunk_size)
            {
                IndexEntry index_entry = new IndexEntry();
                index_entry.ckid = "";
                for (int i = 0; i < 4; ++i)
                {
                    index_entry.ckid += (char)br.ReadByte();
                }
                index_entry.flags = br.ReadUInt32();
                index_entry.chunk_offset = movi_start + br.ReadInt32() + 8;
                index_entry.chunk_length = br.ReadInt32();
                int stream_num = (index_entry.ckid[0] - '0') * 10 + (index_entry.ckid[1] - '0');
                
                stream_list_[stream_num].IndexSet.Add(index_entry);
            }
            if (br.BaseStream.Position < start_pos + chunk_size)
            {
                br.BaseStream.Seek(start_pos + chunk_size, SeekOrigin.Begin);
            }
        }

        public Bitmap GetFrame(int frame_number)
        {
            AviStream video_stream = GetVideoStream();
            if (video_stream != null && frame_number < video_stream.IndexSet.Count)
            {
                IndexEntry index_entry = video_stream.IndexSet[frame_number];
                byte[] buff = new byte[index_entry.chunk_length];
                file_stream_.Seek(index_entry.chunk_offset, SeekOrigin.Begin);
                file_stream_.Read(buff, 0, index_entry.chunk_length);
                GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                Bitmap bitmap = new Bitmap(width_, height_, width_ * 3, PixelFormat.Format24bppRgb, handle.AddrOfPinnedObject());
                handle.Free();
                //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                return bitmap;
            }
            else
            {
                return new Bitmap(1, 1);
            }
        }

        // 全フレームを反転させてファイルに書き込む。ファイルOpen時に is_edit を true にする必要がある
        public void FlipAndWriteAll()
        {
            AviStream video_stream = GetVideoStream();
            if (video_stream != null)
            {
                // ヘッダの高さがマイナス値になっている場合はプラスにする
                //（このようにしないとVFWで読めない）
                byte[] buff = new byte[1024];
                file_stream_.Seek(0, SeekOrigin.Begin);
                file_stream_.Read(buff, 0, buff.Length);
                for (int i = 0; i < buff.Length - 20; ++i)
                {
                    if (buff[i] == 's' && buff[i + 1] == 't' && buff[i + 2] == 'r' && buff[i + 3] == 'f')
                    {
                        int height = buff[i + 16] + buff[i + 17] * 256 + buff[i + 18] * 256 * 256 + buff[i + 19] * 256 * 256 * 256;
                        if (height < 0) // 高さがマイナスの場合だけ修正する
                        {
                            file_stream_.Seek(i + 16, SeekOrigin.Begin);
                            BinaryWriter writer = new BinaryWriter(file_stream_);
                            writer.Write(-height);
                            writer.Flush();
                        }
                    }
                }

                // 各画像の上下を反転させる
                byte[] in_buff = new byte[video_stream.IndexSet[0].chunk_length];
                byte[] out_buff = new byte[video_stream.IndexSet[0].chunk_length];

                for (int i = 0; i < video_stream.GetLength(); ++i)
                {
                    IndexEntry index_entry = video_stream.IndexSet[i];

                    file_stream_.Seek(index_entry.chunk_offset, SeekOrigin.Begin);
                    file_stream_.Read(in_buff, 0, index_entry.chunk_length);

                    for (int j = 0; j < in_buff.Length; ++j)
                    {
                        out_buff[(height_ - j / (3 * width_) - 1) * 3 * width_ + j % (3 * width_)] = in_buff[j];
                    }
                    file_stream_.Seek(index_entry.chunk_offset, SeekOrigin.Begin);
                    file_stream_.Write(out_buff, 0, index_entry.chunk_length);
                }
            }
        }

        public void WriteRateAndScale(uint rate, uint scale)
        {
            file_stream_.Seek(0, SeekOrigin.Begin);
            byte[] header = new byte[4096];

            file_stream_.Read(header, 0, header.Length);

            for (int i = 0; i < header.Length - 3; ++i)
            {
                if (header[i] == (byte)'s' && header[i + 1] == (byte)'t' &&
                    header[i + 2] == (byte)'r' && header[i + 3] == (byte)'h')
                {
                    file_stream_.Seek(i + 28, SeekOrigin.Begin);

                    BinaryWriter writer = new BinaryWriter(file_stream_);
                    writer.Write(scale);
                    writer.Write(rate);
                    break;
                }
            }
        }
    }

    static class Util
    {
        public static bool EqualsString(byte[] b, string str)
        {
            return EqualsSubString(b, 0, b.Length, str);
        }

        public static bool EqualsSubString(byte[] b, int start, int count, string str)
        {
            if (count != str.Length)
            {
                return false;
            }
            for (int i = 0; i < count; ++i)
            {
                if ((char)b[i + start] != str[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AVIFormatException : Exception
    {
        public AVIFormatException()
        {

        }

        public AVIFormatException(string message)
            : base(message)
        {

        }
    }
}
