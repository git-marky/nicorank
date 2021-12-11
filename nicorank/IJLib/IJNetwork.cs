// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//


// HttpWebRequest と HttpWebResponse のラッパークラス

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace IJLib
{
    public class IJNetwork
    {
        public delegate void DownloadingEventDelegate(ref bool is_cancel, long current_size, long file_size);

        private string referer_ = "";
        private WebProxy proxy_ = null;
        private string content_type_;
        private string user_agent_ = null;
        private List<string> custom_header_ = new List<string>();
        private CookieContainer container_ = new CookieContainer();
        private DownloadingEventDelegate delegate_ = null;
        private bool is_set_max_age_zero_ = false;

        public IJNetwork()
        {
            SetDefaultContentType();
        }

        public string UserAgent
        {
            get { return user_agent_; }
            set { user_agent_ = value; }
        }

        public void SetReferer(string referer)
        {
            referer_ = referer;
        }

        public void SetContentType(string content_type)
        {
            content_type_ = content_type;
        }

        public void SetDefaultContentType()
        {
            content_type_ = "application/x-www-form-urlencoded";
        }

        public void SetContentTypeJSON()
        {
            content_type_ = "application/json";
        }

        public void SetProxy(string proxy_url)
        {
            proxy_ = new WebProxy(proxy_url);
        }

        public void SetMaxAgeZero()
        {
            is_set_max_age_zero_ = true;
        }

        public void Reset()
        {
            referer_ = "";
            proxy_ = null;
            custom_header_.Clear();
            SetDefaultContentType();
            is_set_max_age_zero_ = false;
        }

        // header はコロン区切りの文字列
        public void AddCustomHeader(string header)
        {
            custom_header_.Add(header);
        }

        public void SetDownloadingEventDelegate(DownloadingEventDelegate dlg)
        {
            delegate_ = dlg;
        }

        public CookieCollection GetCookie(string uri)
        {
            return container_.GetCookies(new Uri(uri));
        }

        public void SetCookie(string uri, string cookie_str)
        {
            container_.SetCookies(new Uri(uri), cookie_str);
        }

        public void ClearCookie()
        {
            container_ = new CookieContainer();
        }

        public string GetAndReadFromWeb(string uri)
        {
            return ReadFromWebInner(uri, false, null, Encoding.GetEncoding(932));
        }

        public string GetAndReadFromWebUTF8(string uri)
        {
            return ReadFromWebInner(uri, false, null, Encoding.GetEncoding("UTF-8"));
        }

        public string PostAndReadFromWeb(string uri, string post_data)
        {
            byte[] post_data_byte = Encoding.ASCII.GetBytes(post_data);
            return PostAndReadFromWeb(uri, post_data_byte);
        }

        public string PostAndReadFromWeb(string uri, byte[] post_data)
        {
            return ReadFromWebInner(uri, true, post_data, Encoding.GetEncoding(932));
        }

        public string PostAndReadFromWebUTF8(string uri, string post_data)
        {
            byte[] post_data_byte = Encoding.UTF8.GetBytes(post_data);
            return PostAndReadFromWebUTF8(uri, post_data_byte);
        }

        public string PostAndReadFromWebUTF8(string uri, byte[] post_data)
        {
            return ReadFromWebInner(uri, true, post_data, Encoding.GetEncoding("UTF-8"));
        }

        public void GetAndSaveToFile(string uri, string filename)
        {
            ReadAndSaveInner(uri, false, null, filename);
        }

        public void PostAndSaveToFile(string uri, string post_data, string filename)
        {
            byte[] post_data_byte = Encoding.ASCII.GetBytes(post_data);
            PostAndSaveToFile(uri, post_data_byte, filename);
        }

        public void PostAndSaveToFile(string uri, byte[] post_data, string filename)
        {
            ReadAndSaveInner(uri, true, post_data, filename);
        }

        /// <summary>
        /// uri のページにアクセスして、返ってきたHTTPヘッダーのLocation: を取得
        /// </summary>
        public string GetLocationHeader(string uri)
        {
            HttpWebResponse response = GetResponse(uri, false, null, false);
            try
            {
                return response.Headers[HttpResponseHeader.Location];
            }
            finally
            {
                response.Close();
            }
        }

        private string ReadFromWebInner(string uri, bool is_post, byte[] post_data, Encoding encoding)
        {
            string ret;
            HttpWebResponse response = GetResponse(uri, is_post, post_data, true);
            try
            {
                Stream stream = response.GetResponseStream();
                try
                {
                    StreamReader stream_reader = new StreamReader(stream, encoding);
                    try
                    {
                        ret = stream_reader.ReadToEnd();
                    }
                    finally
                    {
                        stream_reader.Close();
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
            finally
            {
                response.Close();
            }
            return ret;
        }

        private void ReadAndSaveInner(string uri, bool is_post, byte[] post_data, string filename)
        {
            HttpWebResponse response = GetResponse(uri, is_post, post_data, true);
            try
            {
                Stream stream = response.GetResponseStream();
                try
                {
                    MemoryStream mstream = new MemoryStream();
                    long count = 0;
                    long file_size = response.ContentLength;
                    bool is_cancel = false;
                    int s;
                    byte[] buff = new byte[4096 * 4];
                    while ((s = stream.Read(buff, 0, buff.Length)) > 0)
                    {
                        mstream.Write(buff, 0, s);
                        count += s;
                        if (delegate_ != null)
                        {
                            delegate_(ref is_cancel, count, file_size);
                            if (is_cancel)
                            {
                                delegate_(ref is_cancel, count, file_size);
                            }
                        }
                    }
                    FileStream fstream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    try
                    {
                        mstream.WriteTo(fstream);
                    }
                    finally
                    {
                        fstream.Close();
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
            finally
            {
                response.Close();
            }
        }

        private HttpWebResponse GetResponse(string uri, bool is_post, byte[] post_data, bool allow_auto_redirect)
        {
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);

            if (is_post)
            {
                request.Method = "POST";
                request.ContentLength = post_data.Length;
            }
            else
            {
                request.Method = "GET";
            }

            request.ContentType = content_type_;
            if (content_type_ == "application/json")
            {
                request.Accept = "application/json";
            }
            request.Timeout = 15 * 1000;
            request.CookieContainer = container_;
            if (user_agent_ != null)
            {
                request.UserAgent = user_agent_;
            }
            if (referer_ != "")
            {
                request.Referer = referer_;
            }
            if (proxy_ != null)
            {
                request.Proxy = proxy_;
            }
            for (int i = 0; i < custom_header_.Count; ++i)
            {
                request.Headers.Add(custom_header_[i]);
            }
            if (!allow_auto_redirect)
            {
                request.AllowAutoRedirect = false;
            }
            if (is_set_max_age_zero_)
            {
                request.Headers["Cache-Control"] = "max-age=0";
            }

            if (is_post && post_data != null && post_data.Length > 0)
            {
                Stream rs = request.GetRequestStream();
                try
                {
                    rs.Write(post_data, 0, post_data.Length);
                }
                finally
                {
                    rs.Close();
                }
            }
            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// application/x-www-form-urlencoded 形式の文字列を作成する
        /// </summary>
        /// <param name="key_val_array"></param>
        /// <returns>
        /// { key1, value1, key2, value2, ... } の形式で引数を渡すと、key1'=val1'&key2'=val2'&...
        /// (key1'はkey1をURLエンコードしたもの)の形式の文字列を返却する。
        /// </returns>
        public static string ConstructPostData(params string[] key_val_array)
        {
            if (key_val_array == null)
            {
                return string.Empty;
            }

            int item_count;
            if (key_val_array.Length % 2 == 0)
            {
                item_count = key_val_array.Length / 2;
            }
            else
            {
                item_count = (key_val_array.Length + 1) / 2;
            }

            string[] escaped_key_val_array = new string[item_count];

            for (int i = 0; i < key_val_array.Length; i += 2)
            {
                string raw_key = key_val_array[i] ?? string.Empty;
                string raw_val;
                if (i + 1 < key_val_array.Length)
                {
                    raw_val = key_val_array[i + 1] ?? string.Empty;
                }
                else
                {
                    raw_val = string.Empty;
                }

                string key = Uri.EscapeDataString(raw_key);
                string val = Uri.EscapeDataString(raw_val);
                escaped_key_val_array[i / 2] = string.Format("{0}={1}", key, val);
            }

            return string.Join("&", escaped_key_val_array);
        }
    }
}
