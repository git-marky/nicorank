// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace IJLib
{
    public class IJUtil
    {
        private static Random random = new Random();

        // 1 から size までのランダムな順列を生成する
        public static int[] GetRandomPermutation(int size)
        {
            int[] ret = new int[size];
            bool[] check = new bool[size];
            int count = 0;
            while (count < size)
            {
                int n = random.Next(1, size + 1);
                if (!check[n - 1]) //その数字がまだ使われてなかったら 
                {
                    ret[count] = n;
                    ++count;
                    check[n - 1] = true;
                }
            }
            return ret;
        }

        public static void SleepRandomTime(int lower_milisec, int upper_milisec)
        {
            if (upper_milisec <= 0)
            {
                return;
            }
            else if (lower_milisec <= 0)
            {
                lower_milisec = 1;
            }
            if (lower_milisec < upper_milisec)
            {
                Thread.Sleep(random.Next(lower_milisec, upper_milisec));
            }
            else if (lower_milisec > upper_milisec)
            {
                Thread.Sleep(random.Next(lower_milisec, upper_milisec));
            }
            else
            {
                Thread.Sleep(lower_milisec);
            }
        }

        [DllImport("wininet.dll")]
        private extern static
            bool InternetGetCookie(string lpszUrl, string lpszCookieName,
                                   StringBuilder lpCookieData, ref uint lpdwSize);

        public static string GetCookieFromIE(string url)
        {
            const int max_cookie_size = 4096;
            StringBuilder cookie_str = new StringBuilder(new String(' ', max_cookie_size), max_cookie_size);

            uint size = (uint)max_cookie_size;

            InternetGetCookie(url, null, cookie_str, ref size);

            return cookie_str.ToString().Replace(";", ",");
        }
    }

    public class IJStringUtil
    {
        public static string GetStringBetweenTag(ref int start_index, string tagName, string str)
        {
            int start;

            while (true)
            {
                start = str.IndexOf("<" + tagName, start_index);
                if (start < 0)
                {
                    return "";
                }
                if (str[start + tagName.Length + 1] == ' ' ||
                    str[start + tagName.Length + 1] == '>') // tagName の後ろが空白か閉じかっこならそれが目的のタグ
                {
                    break;
                }
                start_index = start + 1;
            }
            start = str.IndexOf('>', start) + 1;
            int end = str.IndexOf("</" + tagName + ">", start);
            start_index = end + 1;
            if (start == end)
            {
                return "";
            }
            else
            {
                return IJStringUtil.UnescapeHtml(str.Substring(start, end - start));
            }
        }

        public static string ToStringWithComma(int value)
        {
            if (value < 1000)
            {
                return value.ToString();
            }
            else
            {
                return value.ToString("#,##0#");
            }
        }

        // str のフォーマットが間違っているときは def の値で代替
        public static int ToNumberWithDef(string str, int def)
        {
            int ret;
            if (!int.TryParse(str, out ret))
            {
                ret = def;
            }
            return ret;
        }

        // str のフォーマットが間違っているときは def の値で代替
        public static double ToDoubleWithDef(string str, double def)
        {
            double ret;
            if (!double.TryParse(str, out ret))
            {
                ret = def;
            }
            return ret;
        }

        // カンマ付き文字列を数字に変換
        public static int ToIntFromCommaValue(string str)
        {
            return int.Parse(str.Replace(",", ""));
        }

        // カンマ付き文字列を数字に変換
        public static int ToIntFromCommaValueWithDef(string str, int def)
        {
            int ret;
            if (!int.TryParse(str.Replace(",", ""), out ret))
            {
                ret = def;
            }
            return ret;
        }

        public static string DateToUnix(DateTime datetime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 9, 0, 0);
            TimeSpan span = datetime - origin;
            return ((int)span.TotalSeconds).ToString();
        }

        public static DateTime UnixToDate(string unix_date)
        {
            return UnixToDate(int.Parse(unix_date));
        }

        public static DateTime UnixToDate(int unix_date)
        {
            DateTime ret = new DateTime(1970, 1, 1, 9, 0, 0);
            ret = ret.AddSeconds(unix_date);
            return ret;
        }

        public static string[] SplitWithCRLF(string str)
        {
            string[] splitter = { "\r\n" };
            return str.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitWithCRLFWithEmptyLines(string str)
        {
            string[] splitter = { "\r\n" };
            return str.Split(splitter, StringSplitOptions.None);
        }
        
        public static string[] SplitWithCRLForLF(string str)
        {
            string[] splitter;
            if (str.IndexOf("\r\n") >= 0)
            {
                string[] splitter_arg = { "\r\n" };
                splitter = splitter_arg;
            }
            else if (str.IndexOf("\r") >= 0)
            {
                string[] splitter_arg = { "\r" };
                splitter = splitter_arg;
            }
            else
            {
                string[] splitter_arg = { "\n" };
                splitter = splitter_arg;
            }
            return str.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitWithCRLFAndEraseComment(string str)
        {
            string[] lines = SplitWithCRLF(str);
            List<string> ret_list = new List<string>();
            for (int i = 0; i < lines.Length; ++i)
            {
                if (!lines[i].StartsWith("#") && !lines[i].StartsWith("//"))
                {
                    ret_list.Add(lines[i]);
                }
            }
            return ret_list.ToArray();
        }

        public static bool IsAlnum(int c)
        {
            return ('0' <= c && c <= '9' || 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z');
        }

        private static System.Globalization.CompareInfo compare_info = System.Globalization.CultureInfo.CurrentCulture.CompareInfo;

        public static bool CompareString(string string1, string string2)
        {
            return compare_info.Compare(string1, string2,
                System.Globalization.CompareOptions.IgnoreCase |
                System.Globalization.CompareOptions.IgnoreKanaType |
                System.Globalization.CompareOptions.IgnoreWidth) == 0;
        }

        public static int IndexOfIgnoreCase(string source, string value)
        {
            return compare_info.IndexOf(source, value, System.Globalization.CompareOptions.IgnoreCase |
                System.Globalization.CompareOptions.IgnoreKanaType |
                System.Globalization.CompareOptions.IgnoreWidth);
        }

        public static string UnescapeHtml(string str)
        {
            return str.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").
                    Replace("&amp;", "&").Replace("&#039;", "'").Replace("〜", "～");
        }

        public static string EscapeForConfig(string text)
        {
            return text.Replace("__CR__", "__\\CR__").Replace("\r", "__CR__").Replace("__LF__", "__\\LF__").Replace("\n", "__LF__")
                .Replace("__TAB__", "__\\TAB__").Replace("\t", "__TAB__");
        }

        public static string UnescapeForConfig(string text)
        {
            return text.Replace("__TAB__", "\t").Replace("__\\TAB__", "__TAB__").Replace("__LF__", "\n").Replace("__\\LF__", "__LF__")
                .Replace("__CR__", "\r").Replace("__\\CR__", "__CR__");
        }

        // "30-50" のような文字列を解析する
        public static void ParseDlInterval(string dl_interval, ref double interval_min, ref double interval_max)
        {
            if (string.IsNullOrEmpty(dl_interval))
            {
                return;
            }

            int mid_index = dl_interval.IndexOf('-');
            if (mid_index < 0)
            {
                mid_index = dl_interval.IndexOf('～');
            }
            if (mid_index < 0)
            {
                interval_min = interval_max = double.Parse(dl_interval);
            }
            else
            {
                interval_min = double.Parse(dl_interval.Substring(0, mid_index));
                interval_max = double.Parse(dl_interval.Substring(mid_index + 1));
                if (interval_max < interval_min)
                {
                    double temp = interval_min;
                    interval_min = interval_max;
                    interval_max = temp;
                }
            }
        }

        public static string EncryptString(string str, string key)
        {
            MemoryStream ms = new MemoryStream();

            ICryptoTransform crypto_transform = GetDES(key).CreateEncryptor();
            CryptoStream crypto_stream = new CryptoStream(ms, crypto_transform, CryptoStreamMode.Write);

            byte[] data = Encoding.UTF8.GetBytes(str);
            crypto_stream.Write(data, 0, data.Length);
            crypto_stream.FlushFinalBlock();
            string crypted_data = System.Convert.ToBase64String(ms.ToArray());
            crypto_stream.Close();
            ms.Close();

            return crypted_data;
        }

        public static string DecryptString(string str, string key)
        {
            byte[] data = System.Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(data);
            ICryptoTransform crypto_transform = GetDES(key).CreateDecryptor();
            CryptoStream crypto_stream = new CryptoStream(ms, crypto_transform, CryptoStreamMode.Read);

            StreamReader reader = new StreamReader(crypto_stream, Encoding.UTF8);

            string decrypted_data = reader.ReadToEnd();

            reader.Close();
            crypto_stream.Close();
            ms.Close();

            return decrypted_data;
        }

        private static DESCryptoServiceProvider GetDES(string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] data = Encoding.UTF8.GetBytes(key);

            des.Key = GetNewArray(data, des.Key.Length);
            des.IV = GetNewArray(data, des.IV.Length);

            return des;
        }

        private static byte[] GetNewArray(byte[] data, int size)
        {
            byte[] ret = new byte[size];
            if (data.Length <= size)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    ret[i] = data[i];
                }
            }
            else
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    ret[i % size] ^= data[i];
                }
            }
            return ret;
        }
    }

    public class IJFile
    {
        public enum EncodingPriority { Auto, UTF8, ShiftJIS }
        private static Random random = new Random();

        // 将来的に Read と差し替える予定
        public static string ReadVer2(string path, EncodingPriority priority)
        {
            byte[] data = File.ReadAllBytes(path);

            Encoding shift_jis = Encoding.GetEncoding(932, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            Encoding shift_jis_no_error = Encoding.GetEncoding(932);
            Encoding utf8 = new UTF8Encoding(true, true);
            Encoding utf8_no_error = new UTF8Encoding(true, false);

            if (priority == EncodingPriority.Auto)
            {
                byte[] head = new byte[Math.Min(4096, data.Length)]; // 先頭 4096 バイトから推定

                Array.Copy(data, head, head.Length);
                priority = (IsUTF8(head) ? EncodingPriority.UTF8 : EncodingPriority.ShiftJIS);
            }
            string ret_str = "";

            if (priority == EncodingPriority.ShiftJIS) // Shift_JIS 優先
            {
                try
                {
                    // まず Shift_JIS として読み込んでみる
                    ret_str = shift_jis.GetString(data);
                }
                catch (DecoderFallbackException)
                {
                    // 失敗したら UTF-8 として読み込む
                    if (IsUTF8BomPresent(data))
                    {
                        ret_str = utf8_no_error.GetString(data, 3, data.Length - 3);
                    }
                    else
                    {
                        ret_str = utf8_no_error.GetString(data);
                    }
                }
            }
            else
            {
                try
                {
                    // まず UTF-8 として読み込んでみる
                    if (IsUTF8BomPresent(data))
                    {
                        ret_str = utf8.GetString(data, 3, data.Length - 3);
                    }
                    else
                    {
                        ret_str = utf8.GetString(data);
                    }
                }
                catch (DecoderFallbackException)
                {
                    // 失敗したら Shift_JIS として読み込む
                    ret_str = shift_jis_no_error.GetString(data);
                }
            }
            return ret_str;
        }

        // UTF-8 かどうかを判定する。
        // UTF-8 の BOM が先頭にある場合は true を返す。
        // ASCII になってる文字を除外して、それ以外の文字で UTF-8 の3バイト表現（[0xe0-0xef][0x80-0xbf][0x80-0xbf]）に
        // なっているものを数える。割合が半数以上なら true を返す。
        public static bool IsUTF8(byte[] data)
        {
            int utf8_count = 0;
            int multibyte_count = 0;

            // UTF8 の BOM がある場合は true を返却する
            if (IsUTF8BomPresent(data))
            {
                return true;
            }

            for (int i = 0; i < data.Length; ++i)
            {
                if (0x00 <= data[i] && data[i] <= 0x7e) // ASCII 文字
                {
                    continue;
                }
                ++multibyte_count;
                if (0xe0 <= data[i] && data[i] <= 0xef) // 3バイト表現になっているか
                {
                    if (i + 3 < data.Length && (0x80 <= data[i + 1] && data[i + 1] <= 0xbf)
                        && (0x80 <= data[i + 2] && data[i + 2] <= 0xbf))
                    {
                        // さらに次の文字がASCIIか3バイト表現の先頭なら
                        if ((0x20 <= data[i + 3] && data[i + 3] <= 0x7e) || (0xe0 <= data[i + 3] && data[i + 3] <= 0xef))
                        {
                            i += 2;
                            multibyte_count += 2;
                            utf8_count += 3;
                        }
                    }
                }
            }
            // ascii以外の文字で、UTF8の3バイト表現になっているバイトが半分以上あるならUTF8とみなす
            if (utf8_count >= multibyte_count / 2) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// UTF-8のBOMが先頭についているかを判定する。
        /// </summary>
        /// <param name="data">ファイルのバイトデータ</param>
        /// <returns>
        /// データの先頭にUTF-8のバイトオーダーマークがついている場合はtrue、
        /// その他の場合はfalse。
        /// </returns>
        private static bool IsUTF8BomPresent(byte[] data)
        {
            // 3バイト未満では判定不能
            if (data.Length < 3)
            {
                return false;
            }

            // 先頭がEF BB BFかどうか判定
            if (data[0] != 0xef)
            {
                return false;
            }
            if (data[1] != 0xbb)
            {
                return false;
            }
            if (data[2] != 0xbf)
            {
                return false;
            }

            return true;
        }

        // 将来的に Write と差し替える予定
        public static void WriteVer2(string filename, string contents, EncodingPriority priority)
        {
            if (priority == EncodingPriority.ShiftJIS)
            {
                File.WriteAllText(filename, contents, Encoding.GetEncoding(932));
            }
            else
            {
                // UTF8 BOM付きで書き込む
                Encoding utf8 = new UTF8Encoding(true, true);
                File.WriteAllText(filename, contents, utf8);
            }
        }

        public static string Read(string filename)
        {
            StreamReader sr = new StreamReader(filename, Encoding.GetEncoding(932));
            string s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

        public static string ReadEUC(string filename)
        {
            StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("euc-jp"));
            string s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

        public static string ReadUTF8(string filename)
        {
            StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("UTF-8"));
            string s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

        public static string ReadFirstLine(string filename, Encoding encoding)
        {
            using (Stream input = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (TextReader reader = new StreamReader(input, encoding))
            {
                return reader.ReadLine();
            }
        }

        public static string ReadFirstLineUTF8(string filename)
        {
            return ReadFirstLine(filename, Encoding.UTF8);
        }

        public static void Write(string filename, string str)
        {
            CreateDirectoryFromFilename(filename);
            StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding(932));
            sw.Write(str);
            sw.Close();
        }

        public static void WriteEUC(string filename, string str)
        {
            CreateDirectoryFromFilename(filename);
            StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding("euc-jp"));
            sw.Write(str);
            sw.Close();
        }

        public static void WriteUTF8(string filename, string str)
        {
            CreateDirectoryFromFilename(filename);
            StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding("UTF-8"));
            sw.Write(str);
            sw.Close();
        }

        public static void WriteAppend(string filename, string str)
        {
            CreateDirectoryFromFilename(filename);
            StreamWriter sw = new StreamWriter(filename, true, Encoding.GetEncoding(932));
            sw.Write(str);
            sw.Close();
        }

        public static string GetTemporaryFileName(string dir_name, string extension)
        {
            string prefix = "temp_";
            int try_count = 0;
            const int try_max = 100;

            if (!dir_name.EndsWith("\\"))
            {
                dir_name += "\\";
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            
            string filename = "";
            do
            {
                ++try_count;
                filename = dir_name + prefix + ((uint)random.Next()).ToString() + extension;
            } while (File.Exists(filename) && try_count < try_max);

            return filename;
        }

        // 引数で指定したディレクトリ名に数字を付加して返す。
        // 返り値は存在しないディレクトリ名になる
        public static string GetNoExistDirName(string dir_name)
        {
            const int try_count = 1000;
            string ret;
            if (dir_name.EndsWith("\\"))
            {
                dir_name = dir_name.TrimEnd('\\');
            }
            for (int i = 1; i <= try_count; ++i)
            {
                ret = dir_name + "_" + i.ToString() + "\\";
                if (!Directory.Exists(ret))
                {
                    return ret;
                }
            }
            // ここに来ることは想定していない
            string parent_dir = Path.GetDirectoryName(dir_name);
            string[] dirs = Directory.GetDirectories(parent_dir);
            return dir_name + "_" + dirs.Length.ToString() + "\\";
        }

        // 引数で指定したパスのディレクトリが存在しなければ作成する
        public static void CreateDirectoryFromFilename(string path)
        {
            string dir_name = Path.GetDirectoryName(path);
            if (dir_name != "")
            {
                Directory.CreateDirectory(dir_name);
            }
        }

        public static string SearchFile(string filename, string dir_name)
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
            return "";
        }

        public static string GetAbsolutePath(string path)
        {
            if (path.Length >= 3 && ('A' <= path[0] && path[0] <= 'Z' || 'a' <= path[0] && path[0] <= 'z') && path[1] == ':' && path[2] == '\\')
            {// 絶対パスならそのまま返す
                return path;
            }
            else
            {
                return Directory.GetCurrentDirectory() + '\\' + path;
            }
        }

        public static string GetAbsoluteDir(string dir)
        {
            string absolute_dir = GetAbsolutePath(dir);
            if (absolute_dir != "" && !absolute_dir.EndsWith("\\"))
            {
                absolute_dir += "\\";
            }
            return absolute_dir;
        }
    }

    public static class IJLog
    {
        private static string filename_ = "errorlog.txt";
        private static bool is_logging_ = false;

        public static void SetLogging(bool is_logging)
        {
            is_logging_ = is_logging;
        }

        public static void Write(string str)
        {
            if (is_logging_)
            {
                IJFile.WriteAppend(filename_, str);
            }
        }

        public static void Writeln(string str)
        {
            Write(str + "\r\n");
        }
    }

    public class IJFilePack
    {
        private string filename_;
        private bool is_exist_;
        private bool is_confirm_exist_;
        private string temp_filename_;

        public IJFilePack(string filename)
        {
            filename_ = filename;
            temp_filename_ = "";
            is_confirm_exist_ = true;
        }

        public IJFilePack(string filename, bool is_exist)
        {
            filename_ = filename;
            temp_filename_ = "";
            is_confirm_exist_ = false;
            is_exist_ = is_exist;
        }

        public string GetFilename()
        {
            if (temp_filename_ == "")
            {
                if (is_confirm_exist_)
                {
                    is_exist_ = File.Exists(filename_);
                    is_confirm_exist_ = false;
                }
                if (is_exist_)
                {
                    temp_filename_ = IJFile.GetTemporaryFileName(Path.GetDirectoryName(filename_),
                        Path.GetExtension(filename_));
                }
                else
                {
                    temp_filename_ = "dummy";
                }
            }
            return (is_exist_ ? temp_filename_ : filename_);
        }

        public void Move()
        {
            if (is_exist_)
            {
                File.Delete(filename_);
                File.Move(temp_filename_, filename_);
                is_exist_ = false; // GetFilename と Move の再呼び出しを防ぐ
                filename_ = "";
            }
        }
    }

    public class IJProcess
    {
        public static Process RunProcess(string exec_name, string argument, bool is_window_show)
        {
            Debug.WriteLine(exec_name + " " + argument);
            ProcessStartInfo p_info = new ProcessStartInfo();

            p_info.FileName = exec_name;
            p_info.CreateNoWindow = !is_window_show;
            p_info.UseShellExecute = is_window_show;
            p_info.Arguments = argument;

            Process p = Process.Start(p_info);
            return p;
        }

        public static Process RunProcess(string exec_name, string argument, bool is_window_show, string working_dir)
        {
            Debug.WriteLine(exec_name + " " + argument);
            ProcessStartInfo p_info = new ProcessStartInfo();

            p_info.FileName = exec_name;
            p_info.WorkingDirectory = working_dir;
            p_info.CreateNoWindow = !is_window_show;
            p_info.UseShellExecute = is_window_show;
            p_info.Arguments = argument;

            Process p = Process.Start(p_info);
            return p;
        }

        public static int RunProcessAndWaitForExit(string exec_name, string argument, bool is_window_show)
        {
            Process p = RunProcess(exec_name, argument, is_window_show);
            p.WaitForExit();
            return p.ExitCode;
        }

        public static int RunProcessAndWaitForExit(string exec_name, string argument, bool is_window_show, string working_dir)
        {
            Process p = RunProcess(exec_name, argument, is_window_show, working_dir);
            p.WaitForExit();
            return p.ExitCode;
        }

        public static int RunProcessAndWaitForExit(string exec_name, string argument, bool is_window_show, ProcessRunningEventDelegate dlg)
        {
            Process p = RunProcess(exec_name, argument, is_window_show);
            while (!p.HasExited)
            {
                if (dlg != null)
                {
                    try
                    {
                        dlg();
                    }
                    catch (Exception e)
                    {
                        p.Kill();
                        throw e;
                    }
                }
                Thread.Sleep(500);
            }
            return p.ExitCode;
        }

        public delegate void ProcessRunningEventDelegate();

        public static int RunProcessAndWaitForExitAndGetErr(string exec_name, string argument, bool is_window_show,
            out string standard_error)
        {
            Debug.WriteLine(exec_name + " " + argument);
            ProcessStartInfo p_info = new ProcessStartInfo();
            p_info.FileName = exec_name;
            p_info.CreateNoWindow = !is_window_show;
            p_info.Arguments = argument;
            p_info.RedirectStandardInput = false;
            p_info.RedirectStandardOutput = false;
            p_info.RedirectStandardError = true;
            p_info.UseShellExecute = false;

            Process p = Process.Start(p_info);

            standard_error = p.StandardError.ReadToEnd();
            
            p.WaitForExit();
            return p.ExitCode;
        }

        public delegate void ProcessRunningEventStringDelegate(string informing_text);

        public static int RunProcessAndWaitForExitAndGetOutput(string exec_name, string argument, bool is_window_show,
            string working_directory, ProcessRunningEventStringDelegate output_dlg, ProcessRunningEventStringDelegate outputerr_dlg)
        {
            Debug.WriteLine(exec_name + " " + argument);
            ProcessStartInfo p_info = new ProcessStartInfo();
            p_info.FileName = exec_name;
            if (working_directory != null)
            {
                p_info.WorkingDirectory = working_directory;
            }
            p_info.CreateNoWindow = !is_window_show;
            p_info.Arguments = argument;
            p_info.RedirectStandardInput = false;
            p_info.RedirectStandardOutput = (output_dlg != null);
            p_info.RedirectStandardError = (outputerr_dlg != null);
            p_info.UseShellExecute = false;

            Process p = Process.Start(p_info);

            if (output_dlg != null)
            {
                Thread thread = new Thread(
                    new ParameterizedThreadStart(ThreadLoop));
                thread.IsBackground = true;
                thread.Start(new ReaderDelegatePair(p.StandardOutput, output_dlg));
            }
            if (outputerr_dlg != null)
            {
                Thread thread = new Thread(
                    new ParameterizedThreadStart(ThreadLoop));
                thread.IsBackground = true;
                thread.Start(new ReaderDelegatePair(p.StandardError, outputerr_dlg));
            }

            p.WaitForExit();
            return p.ExitCode;
        }

        private static void ThreadLoop(object argument)
        {
            ReaderDelegatePair pair = (ReaderDelegatePair)argument;

            while (!pair.reader.EndOfStream)
            {
                string str = pair.reader.ReadLine();
                pair.dlg(str);
            }
        }

        public static bool FindWindow(string window_title)
        {
            Process[] process = Process.GetProcesses();
            for (int i = 0; i < process.Length; ++i)
            {
                if (process[i].MainWindowTitle.IndexOf(window_title) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    class ReaderDelegatePair
    {
        public StreamReader reader;
        public IJProcess.ProcessRunningEventStringDelegate dlg;

        public ReaderDelegatePair(StreamReader reader_a, IJProcess.ProcessRunningEventStringDelegate dlg_a)
        {
            reader = reader_a;
            dlg = dlg_a;
        }
    }

    public class IJGraphics
    {
        public static void CopyImage(string dest_filename, string src_filename, ImageFormat format)
        {
            Bitmap src_bmp = new Bitmap(src_filename);
            Bitmap dest_bmp = new Bitmap(src_bmp.Width, src_bmp.Height);
            Graphics graphics = Graphics.FromImage(dest_bmp);
            graphics.DrawImage(src_bmp, 0, 0, src_bmp.Width, src_bmp.Height);
            dest_bmp.Save(dest_filename, format);
            graphics.Dispose();
            dest_bmp.Dispose();
            src_bmp.Dispose();
        }

        public static void CopyImage(string dest_filename, string src_filename, int dest_width, int dest_height, ImageFormat format)
        {
            Bitmap src_bmp = new Bitmap(src_filename);
            Bitmap dest_bmp = new Bitmap(dest_width, dest_height);
            Graphics graphics = Graphics.FromImage(dest_bmp);
            graphics.DrawImage(src_bmp, 0, 0, dest_width, dest_height);
            dest_bmp.Save(dest_filename, format);
            graphics.Dispose();
            dest_bmp.Dispose();
            src_bmp.Dispose();
        }

        public static void SaveToJpgFile(Bitmap image, string filename, long quality)
        {
            ImageCodecInfo codec_info = null;
            ImageCodecInfo[] codec_info_array = ImageCodecInfo.GetImageDecoders();

            for (int i = 0; i < codec_info_array.Length; ++i)
            {
                if (codec_info_array[i].FormatID == ImageFormat.Jpeg.Guid)
                {
                    codec_info = codec_info_array[i];
                    break;
                }
            }
            if (codec_info == null)
            {
                throw new ArgumentException("JPEG コーデックが見つかりませんでした。");
            }

            EncoderParameters encoder_parameters = new EncoderParameters(1);

            EncoderParameter encoder_parameter =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            encoder_parameters.Param[0] = encoder_parameter;

            image.Save(filename, codec_info, encoder_parameters);
        }
    }

    public class CancelObject
    {
        private static Random random_ = new Random();
        private bool is_canceling_ = false;

        public virtual void Cancel()
        {
            is_canceling_ = true;
        }

        public virtual void ClearCanceling()
        {
            is_canceling_ = false;
        }

        public bool IsCanceling()
        {
            return is_canceling_;
        }

        public void CheckCancel()
        {
            if (is_canceling_)
            {
                throw new MyCancelException();
            }
        }

        public void Wait(int millisec)
        {
            while (millisec > 0)
            {
                CheckCancel();
                if (millisec >= 100)
                {
                    System.Threading.Thread.Sleep(100);
                    millisec -= 100;
                }
                else
                {
                    System.Threading.Thread.Sleep(millisec);
                    break;
                }
            }
        }

        public void Wait(int lower_millisec, int upper_millisec)
        {
            if (upper_millisec <= 0)
            {
                return;
            }
            else if (lower_millisec <= 0)
            {
                lower_millisec = 1;
            }

            if (lower_millisec < upper_millisec)
            {
                Wait(random_.Next(lower_millisec, upper_millisec));
            }
            else if (lower_millisec > upper_millisec)
            {
                Wait(random_.Next(upper_millisec, lower_millisec));
            }
            else
            {
                Wait(lower_millisec);
            }
        }
    }

    public class MyCancelException : Exception
    {

    }

    public interface MessageOut
    {
        void Write(string text);
        void WriteLine(string text);
    }
}
