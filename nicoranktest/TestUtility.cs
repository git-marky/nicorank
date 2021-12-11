using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using WatiN.Core.Interfaces;
using WatiN.Core;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using NicoTools;
using System.Diagnostics;

namespace nicoranktest
{
    public abstract class TestUtility
    {
        public static readonly string KEY_ACCESS_WAIT_MILLISECONDS = "access_wait_milliseconds";
        public static readonly string KEY_BROWSER_CHROME = "browser_chrome";
        public static readonly string KEY_BROWSER_FIREFOX = "browser_firefox";
        public static readonly string KEY_BROWSER_IE = "browser_ie";
        public static readonly string KEY_BROWSER_OPERA = "browser_opera";
        public static readonly string KEY_NG_MAIL = "ng_mail";
        public static readonly string KEY_NG_PASS = "ng_pass";
        public static readonly string KEY_OK_MAIL = "ok_mail";
        public static readonly string KEY_OK_PASS = "ok_pass";
        public static readonly string KEY_SEARCH_KEYWORD = "search_keyword";
        public static readonly string KEY_SEARCH_TAG = "search_tag";
        public static readonly string KEY_TEMP_DIRECTORY = "temp_directory";
        public static readonly string KEY_MYLIST_ID = "mylist_id";
        public static readonly string KEY_TAG_LOCK = "tag_lock";
        public static readonly string KEY_TAG_UNLOCK = "tag_unlock";
        public static readonly string KEY_VIDEO_ID = "video_id";
        public static readonly string KEY_VIDEO_TITLE = "video_title";
        public static readonly string KEY_VIDEO_DESCRIPTION = "video_description";
        public static readonly string KEY_VIDEO_SUBMIT_DATE = "video_submit_date";
        public static readonly string KEY_VIDEO_LENGTH = "video_length";
        public static readonly string KEY_VIDEO_ID_NM = "video_id_nm";
        public static readonly string KEY_VIDEO_ID_SO = "video_id_so";
        public static readonly string KEY_VIDEO_ID_SWF = "video_id_swf";
        public static readonly string KEY_THUMBNAIL_LOCAL = "thumbnail_local";
        public static readonly string KEY_VIDEO_LOCAL = "video_local";
        public static readonly string KEY_VIDEO_LOCAL_NM = "video_local_nm";
        public static readonly string KEY_VIDEO_LOCAL_SO = "video_local_so";
        public static readonly string KEY_VIDEO_LOCAL_SWF = "video_local_swf";
        public static readonly string KEY_CHROME_PROFILE_DIR = "chrome_profile_dir";
        public static readonly string KEY_FIREFOX_PROFILE_NAME = "firefox_profile_name";
        public static readonly string KEY_OPERA_SETTINGS_FILE_PATH = "opera_settings_file_path";

        private static Dictionary<string, string> test_data_;

        public static IDictionary<string, string> TestData
        {
            get { return test_data_; }
        }

        static TestUtility()
        {
            test_data_ = new Dictionary<string, string>();
            using (Stream input = File.Open("testdata.txt", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (TextReader reader = new StreamReader(input))
            {
                Regex line_regex = new Regex("^(?<name>[^=]+)=(?<value>.*)$");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match m = line_regex.Match(line);
                    if (m.Success)
                    {
                        string name = m.Groups["name"].Value;
                        string value = m.Groups["value"].Value;
                        test_data_.Add(name, value);
                    }
                }
            }
        }

        internal static void Wait()
        {
            int access_wait_milliseconds = int.Parse(test_data_[KEY_ACCESS_WAIT_MILLISECONDS]);
            Thread.Sleep(access_wait_milliseconds);
        }

        internal static void Wait(int wait_count)
        {
            int access_wait_milliseconds = int.Parse(test_data_[KEY_ACCESS_WAIT_MILLISECONDS]);
            for (int i = 0; i < wait_count; i++)
            {
                Thread.Sleep(access_wait_milliseconds);
            }
        }

        internal static void Message(string message)
        {
            Console.WriteLine(message);
        }

        internal static void Message(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        internal static void IsValidXml(string downloaded_file)
        {
            using (Stream input = File.Open(downloaded_file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (XmlReader reader = XmlReader.Create(input))
            {
                while (reader.Read()) ;
            }
        }


        internal static void BrowserLogout(Browser browser)
        {
            browser.GoTo("https://secure.nicovideo.jp/secure/logout");
        }

        internal static void BrowserLogin(Browser browser)
        {
            string ok_mail = test_data_["ok_mail"];
            string ok_pass = test_data_["ok_pass"];

            browser.GoTo("https://secure.nicovideo.jp/secure/login_form");
            TextField mail_field = browser.TextField(Find.ById("mail"));
            if (mail_field == null)
            {
                return;
            }
            TextField pass_field = browser.TextField(Find.ById("password"));
            if (pass_field == null)
            {
                return;
            }
            Image login_button = browser.Image(Find.ById("login_submit"));

            mail_field.Value = ok_mail;
            pass_field.Value = ok_pass;
            login_button.Click();
        }

        internal static void BrowserLoginManual(string browser, string option, string browser_name)
        {
            string login_url = "https://secure.nicovideo.jp/secure/login_form";
            string arg;
            if (string.IsNullOrEmpty(option))
            {
                arg = login_url;
            }
            else
            {
                arg = string.Format("{0} {1}", option, login_url);
            }
            using (Process proc = Process.Start(browser, arg))
            {
                System.Windows.Forms.MessageBox.Show(string.Format("{0} will open, log in to .nicovideo.jp, then click OK.", browser_name));
                if (!proc.HasExited)
                {
                    try
                    {
                        proc.CloseMainWindow();
                        proc.WaitForExit();
                    }
                    catch (InvalidOperationException e)
                    {
                        TestUtility.Message(string.Format("An Error occurred while attempting to close {0}.", browser_name));
                        TestUtility.Message(e.Message);
                        TestUtility.Message("This error can safely be ignored.");
                    }
                }
            }
        }

        internal static bool InitDirectory(DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                directory.Delete(true);
                directory.Refresh();
                if (directory.Exists)
                {
                    return false;
                }
            }
            directory.Create();
            directory.Refresh();
            if (!directory.Exists)
            {
                return false;
            }
            return true;
        }

        internal static bool FileEquals(string file1, string file2)
        {
            if (new FileInfo(file1).Length != new FileInfo(file2).Length)
            {
                return false;
            }

            using (Stream input1 = File.Open(file1, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream input2 = File.Open(file2, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream input3 = new BufferedStream(input1))
            using (Stream input4 = new BufferedStream(input2))
            {
                int b1;
                int b2;
                while ((b1 = input3.ReadByte()) != -1)
                {
                    b2 = input4.ReadByte();
                    if (b1 != b2)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static void EnsureLogin(NicoNetwork network)
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            bool login_result = false;
            if (!network.IsLoginNiconico())
            {
                login_result = network.LoginNiconico(ok_mail, ok_pass);
            }
            Assert.That(login_result, Is.True, "EnsureLogin");
        }
    }
}
