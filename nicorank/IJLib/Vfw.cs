// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace IJLib.Vfw
{
    public static class VfwApi
    {
        public static readonly uint streamtypeVIDEO = mmioFOURCC('v', 'i', 'd', 's');
        public static readonly uint streamtypeAUDIO = mmioFOURCC('a', 'u', 'd', 's');

        public const int OF_READ = 0;
        public const int OF_WRITE = 1;
        public const int OF_CREATE = 4096;
        public const int OF_SHARE_DENY_NONE = 64;
        public const int OF_SHARE_DENY_WRITE = 32;

        public static uint mmioFOURCC(char c0, char c1, char c2, char c3)
        {
            return (uint)((byte)c0 | ((byte)c1 << 8) | ((byte)c2 << 16) | ((byte)c3 << 24));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RECT
        {
            public uint left;
            public uint top;
            public uint right;
            public uint bottom;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AVISTREAMINFO
        {
            public int fccType;
            public int fccHandler;
            public int dwFlags;
            public int dwCaps;
            public short wPriority;
            public short wLanguage;
            public int dwScale;
            public int dwRate;
            public int dwStart;
            public int dwLength;
            public int dwInitialFrames;
            public int dwSuggestedBufferSize;
            public int dwQuality;
            public int dwSampleSize;
            public RECT rcFrame;
            public int dwEditCount;
            public int dwFormatChangeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public ushort[] szName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PCMWAVEFORMAT
        {
            public short wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;
        }

        [DllImport("avifil32.dll")]
        public extern static void AVIFileInit();

        [DllImport("avifil32.dll")]
        public extern static void AVIFileExit();

        [DllImport("avifil32.dll")]
        public extern static int AVIFileOpen(ref IntPtr ppfile, string szFile, uint mode, IntPtr pclsidHandler);

        [DllImport("avifil32.dll")]
        public extern static int AVIFileRelease(IntPtr pfile);

        [DllImport("avifil32.dll")]
        public extern static int AVIFileGetStream(IntPtr pfile, ref IntPtr ppavi, uint fccType, uint lParam);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamRelease(IntPtr pavi);

        [DllImport("avifil32.dll")]
        public extern static IntPtr AVIStreamGetFrameOpen(IntPtr pavi, IntPtr lpbiWanted);

        [DllImport("avifil32.dll")]
        public extern static IntPtr AVIStreamGetFrame(IntPtr pgf, int lPos);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamGetFrameClose(IntPtr pget);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamStart(IntPtr pavi);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamLength(IntPtr pavi);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamInfo(IntPtr pavi, ref AVISTREAMINFO psi, int lSize);

        [DllImport("avifil32.dll")]
        public extern static int AVIFileCreateStream(IntPtr pfile, ref IntPtr ppavi, ref AVISTREAMINFO psi);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamSetFormat(IntPtr pavi, int lPos, ref BITMAPINFOHEADER lpFormat, int cbFormat);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamSetFormat(IntPtr pavi, int lPos, ref PCMWAVEFORMAT lpFormat, int cbFormat);

        [DllImport("avifil32.dll")]
        public extern static int AVIStreamWrite(IntPtr pavi, int lStart, int lSamples, IntPtr lpBuffer, int cbBuffer, int dwFlags,
            IntPtr plSampWritten, IntPtr plBytesWritten);
    }

    public abstract class VfwAvi
    {
        private static bool is_initialized_ = false;

        protected static void Initialize()
        {
            if (!is_initialized_)
            {
                VfwApi.AVIFileInit();
                is_initialized_ = true;
            }
        }

        protected static void DeInitialize()
        {
            if (is_initialized_)
            {
                VfwApi.AVIFileExit();
                is_initialized_ = false;
            }
        }
    }

    public class VfwAviReader : VfwAvi
    {
        private IntPtr pfile_ = IntPtr.Zero;
        private IntPtr pavi_ = IntPtr.Zero;
        private IntPtr pgf_ = IntPtr.Zero;
        private int frame_start_;
        private int frame_length_;
        private int rate_;
        private int scale_;
        private int width_;
        private int height_;
        private ushort bit_count_;
        private bool is_flip_ = true;

        public int FrameLength
        {
            get { return frame_length_; }
        }

        public int Rate
        {
            get { return rate_; }
        }

        public int Scale
        {
            get { return scale_; }
        }

        public VfwAviReader(string filename)
        {
            try
            {
                Open(filename);
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Open(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }
            Initialize();
            int hr = VfwApi.AVIFileOpen(ref pfile_, filename, VfwApi.OF_READ | VfwApi.OF_SHARE_DENY_NONE, IntPtr.Zero);
            if (hr != 0)
            {
                throw new VfwException("AVIFileOpen", hr);
            }
            hr = VfwApi.AVIFileGetStream(pfile_, ref pavi_, 0, 0);
            if (hr != 0)
            {
                throw new VfwException("AVIFileGetStream", hr);
            }
            frame_start_ = VfwApi.AVIStreamStart(pavi_);
            frame_length_ = VfwApi.AVIStreamLength(pavi_);

            VfwApi.AVISTREAMINFO psi = new VfwApi.AVISTREAMINFO();
            hr = VfwApi.AVIStreamInfo(pavi_, ref psi, Marshal.SizeOf(new VfwApi.AVISTREAMINFO()));
            if (hr != 0)
            {
                throw new VfwException("AVIStreamInfo", hr);
            }
            rate_ = psi.dwRate;
            scale_ = psi.dwScale;

            pgf_ = VfwApi.AVIStreamGetFrameOpen(pavi_, IntPtr.Zero);
            if (pgf_ == IntPtr.Zero)
            {
                throw new VfwException("AVIStreamGetFrameOpen", 0);
            }
            GetBitmapHeader();
        }

        public void Close()
        {
            if (pgf_ != IntPtr.Zero)
            {
                int hr = VfwApi.AVIStreamGetFrameClose(pgf_);
                if (hr != 0)
                {
                    throw new VfwException("AVIStreamGetFrameClose", hr);
                }
                pgf_ = IntPtr.Zero;
            }
            if (pavi_ != IntPtr.Zero)
            {
                VfwApi.AVIStreamRelease(pavi_);
                pavi_ = IntPtr.Zero;
            }
            if (pfile_ != IntPtr.Zero)
            {
                VfwApi.AVIFileRelease(pfile_);
                pfile_ = IntPtr.Zero;
            }
            DeInitialize();
        }

        public void GetBitmapHeader()
        {
            IntPtr p = VfwApi.AVIStreamGetFrame(pgf_, 0);
            if (p == IntPtr.Zero)
            {
                throw new VfwException("先頭のフレーム取得に失敗しました。");
            }
            byte[] header = new byte[40];
            Marshal.Copy(p, header, 0, header.Length);
            MemoryStream ms = new MemoryStream(header);
            BinaryReader reader = new BinaryReader(ms);
            reader.ReadInt32();
            width_ = reader.ReadInt32();
            height_ = reader.ReadInt32();
            if (height_ < 0)
            {
                is_flip_ = false;
                height_ = -height_;
            }
            reader.ReadInt16();
            bit_count_ = reader.ReadUInt16();
            if (bit_count_ != 24 && bit_count_ != 32)
            {
                throw new VfwException("24ビットと32ビットカラー以外には対応していません。");
            }
            ms.Close();
        }

        public Bitmap GetBitmap(int frame_number)
        {
            IntPtr p = VfwApi.AVIStreamGetFrame(pgf_, frame_start_ + frame_number);
            if (p == IntPtr.Zero)
            {
                throw new VfwException("AVIStreamGetFrame", 0);
            }
            p = new IntPtr(p.ToInt32() + 40);
            int stride = (width_ * bit_count_ / 8 + 3) & ~3;
            byte[] buffer = new byte[stride * height_ + 54];
            Marshal.Copy(p, buffer, 0, buffer.Length);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Bitmap bitmap = new Bitmap(width_, height_, stride, BitCountToPixelFormat(bit_count_), handle.AddrOfPinnedObject());
            handle.Free();
            if (is_flip_)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return bitmap;
        }

        private static PixelFormat BitCountToPixelFormat(int bit_count)
        {
            switch (bit_count)
            {
                case 24:
                    return PixelFormat.Format24bppRgb;
                case 32:
                    return PixelFormat.Format32bppArgb;
                default:
                    return PixelFormat.DontCare;
            }
        }
    }

    public class VfwAviWriter : VfwAvi
    {
        private IntPtr pfile_ = IntPtr.Zero;
        private IntPtr pavi_ = IntPtr.Zero;
        private IntPtr pavi_audio_ = IntPtr.Zero;
        private int width_;
        private int height_;
        private int stride_;

        private bool is_first = true;
        private int rate_;
        private int scale_;
        private PixelFormat pixel_format_;
        private int current_frame_ = 0;

        public VfwAviWriter(string filename, int rate, int scale)
        {
            Initialize();
            int hr = VfwApi.AVIFileOpen(ref pfile_, filename, VfwApi.OF_CREATE | VfwApi.OF_SHARE_DENY_WRITE, IntPtr.Zero);
            if (hr != 0)
            {
                throw new VfwException("AVIFileOpen", hr);
            }
            rate_ = rate;
            scale_ = scale;
        }

        private void CreateVideoStream(int width, int height, int stride, int rate, int scale)
        {
            VfwApi.AVISTREAMINFO sinfo = new VfwApi.AVISTREAMINFO();
            sinfo.fccType = (int)VfwApi.streamtypeVIDEO;
            sinfo.fccHandler = (int)VfwApi.mmioFOURCC('D', 'I', 'B', ' ');
            sinfo.dwFlags = 0;
            sinfo.dwCaps = 0;
            sinfo.wPriority = 0;
            sinfo.wLanguage = 0;
            sinfo.dwScale = scale;
            sinfo.dwRate = rate;
            sinfo.dwStart = 0;
            sinfo.dwLength = 0;
            sinfo.dwInitialFrames = 0;
            sinfo.dwSuggestedBufferSize = height * stride;
            sinfo.dwQuality = -1;
            sinfo.dwSampleSize = 0;
            sinfo.rcFrame = new VfwApi.RECT();
            sinfo.rcFrame.top = 0;
            sinfo.rcFrame.left = 0;
            sinfo.rcFrame.bottom = (uint)height;
            sinfo.rcFrame.right = (uint)width;
            sinfo.dwEditCount = 0;
            sinfo.dwFormatChangeCount = 0;
            sinfo.szName = new ushort[64];

            int hr = VfwApi.AVIFileCreateStream(pfile_, ref pavi_, ref sinfo);
            if (hr != 0)
            {
                throw new VfwException("AVIFileCreateStream", hr);
            }
        }

        private void CreateAudioStream(int sample, int length, int sample_size)
        {
            VfwApi.AVISTREAMINFO sinfo = new VfwApi.AVISTREAMINFO();
            sinfo.fccType = (int)VfwApi.streamtypeAUDIO;
            sinfo.fccHandler = 0;
            sinfo.dwFlags = 0;
            sinfo.dwCaps = 0;
            sinfo.wPriority = 0;
            sinfo.wLanguage = 0;
            sinfo.dwScale = 1;
            sinfo.dwRate = sample;
            sinfo.dwStart = 0;
            sinfo.dwLength = length;
            sinfo.dwInitialFrames = 0;
            sinfo.dwSuggestedBufferSize = 0;
            sinfo.dwQuality = 0;
            sinfo.dwSampleSize = sample_size;
            sinfo.rcFrame = new VfwApi.RECT();
            sinfo.rcFrame.top = 0;
            sinfo.rcFrame.left = 0;
            sinfo.rcFrame.bottom = 0;
            sinfo.rcFrame.right = 0;
            sinfo.dwEditCount = 0;
            sinfo.dwFormatChangeCount = 0;
            sinfo.szName = new ushort[64];

            int hr = VfwApi.AVIFileCreateStream(pfile_, ref pavi_audio_, ref sinfo);
            if (hr != 0)
            {
                throw new VfwException("AVIFileCreateStream", hr);
            }
        }

        private void SetVideoFormat(int width, int height, int stride, int bit_count)
        {
            VfwApi.BITMAPINFOHEADER binfo = new VfwApi.BITMAPINFOHEADER();
            binfo.biSize = Marshal.SizeOf(binfo);
            binfo.biWidth = width;
            binfo.biHeight = height;
            binfo.biPlanes = 1;
            binfo.biBitCount = (short)bit_count;
            binfo.biCompression = 0;
            binfo.biSizeImage = height * stride;
            binfo.biXPelsPerMeter = 0;
            binfo.biYPelsPerMeter = 0;
            binfo.biClrUsed = 0;
            binfo.biClrImportant = 0;

            int hr = VfwApi.AVIStreamSetFormat(pavi_, 0, ref binfo, binfo.biSize);
            if (hr != 0)
            {
                throw new VfwException("AVIStreamSetFormat", hr);
            }
        }

        // 引数に与えた bitmap は上下反転される
        public void AddFrame(Bitmap bitmap)
        {
            if (is_first)
            {
                int bit_count = PixelFormatToBitCount(bitmap.PixelFormat);
                width_ = bitmap.Width;
                height_ = bitmap.Height;
                pixel_format_ = bitmap.PixelFormat;
                stride_ = (width_ * bit_count / 8 + 3) & ~3;
                CreateVideoStream(width_, height_, stride_, rate_, scale_);
                SetVideoFormat(width_, height_, stride_, bit_count);
                is_first = false;
            }
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            BitmapData bmp_data = bitmap.LockBits(new Rectangle(0, 0, width_, height_), ImageLockMode.ReadOnly, pixel_format_);
            int hr = VfwApi.AVIStreamWrite(pavi_, current_frame_, 1, bmp_data.Scan0, stride_ * height_, 16, IntPtr.Zero, IntPtr.Zero);
            if (hr != 0)
            {
                throw new VfwException("AVIStreamWrite", hr);
            }
            bitmap.UnlockBits(bmp_data);
            ++current_frame_;
        }

        public void AddWave(string wave_filename)
        {
            byte[] wave_data = File.ReadAllBytes(wave_filename);
            MemoryStream header = new MemoryStream();
            header.Write(wave_data, 0, 36);
            header.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(header);
            reader.ReadBytes(20);

            IntPtr wave_ptr = Marshal.AllocHGlobal(wave_data.Length);
            IntPtr wave_data_ptr = new IntPtr(wave_ptr.ToInt32() + 44);
            try
            {
                Marshal.Copy(wave_data, 0, wave_ptr, wave_data.Length);

                VfwApi.PCMWAVEFORMAT pwformat = new VfwApi.PCMWAVEFORMAT();
                pwformat.wFormatTag = reader.ReadInt16();
                pwformat.nChannels = reader.ReadInt16();
                pwformat.nSamplesPerSec = reader.ReadInt32();
                pwformat.nAvgBytesPerSec = reader.ReadInt32();
                pwformat.nBlockAlign = reader.ReadInt16();
                pwformat.wBitsPerSample = reader.ReadInt16();

                int sample_size = pwformat.nChannels * pwformat.wBitsPerSample / 8;

                CreateAudioStream(pwformat.nSamplesPerSec, (wave_data.Length - 44) / sample_size, sample_size);

                int hr = VfwApi.AVIStreamSetFormat(pavi_audio_, 0, ref pwformat, Marshal.SizeOf(pwformat));
                if (hr != 0)
                {
                    throw new VfwException("AVIStreamSetFormat", hr);
                }

                hr = VfwApi.AVIStreamWrite(pavi_audio_, 0, wave_data.Length - 44, wave_data_ptr,
                    wave_data.Length - 44, 16, IntPtr.Zero, IntPtr.Zero);
                if (hr != 0)
                {
                    throw new VfwException("AVIStreamWrite", hr);
                }

                VfwApi.AVIStreamRelease(pavi_audio_);
                pavi_audio_ = IntPtr.Zero;
            }
            finally
            {
                Marshal.FreeHGlobal(wave_ptr);
            }
        }

        public void Close()
        {
            if (pavi_audio_ != IntPtr.Zero)
            {
                VfwApi.AVIStreamRelease(pavi_audio_);
                pavi_audio_ = IntPtr.Zero;
            }
            if (pavi_ != IntPtr.Zero)
            {
                VfwApi.AVIStreamRelease(pavi_);
                pavi_ = IntPtr.Zero;
            }
            if (pfile_ != IntPtr.Zero)
            {
                VfwApi.AVIFileRelease(pfile_);
                pfile_ = IntPtr.Zero;
            }
            DeInitialize();
        }

        private int PixelFormatToBitCount(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppArgb:
                    return 32;
                default:
                    return 0;
            }
        }
    }

    public class VfwException : Exception
    {
        public VfwException()
            : base()
        {

        }

        public VfwException(string message)
            : base(message)
        {

        }

        public VfwException(string message, Exception inner)
            : base(message, inner)
        {

        }

        public VfwException(string method_name, int error_code)
            : base(method_name + " の呼び出しに失敗しました：" + error_code)
        {

        }
    }
}