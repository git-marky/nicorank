using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NicoTools;
using NUnit.Framework.SyntaxHelpers;
using IJLib;
using WatiN.Core;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace nicoranktest
{
    [TestFixture]
    public class NicoNetworkTest
    {

        NicoNetwork network_;

        [SetUp]
        public void Setup()
        {
            network_ = new NicoNetwork();
        }

        [Test]
        public void LoginNiconicoTest()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            string ng_mail = TestUtility.TestData[TestUtility.KEY_NG_MAIL];
            string ng_pass = TestUtility.TestData[TestUtility.KEY_NG_PASS];

            TestUtility.Wait();
            TestUtility.Message("Running LoginNiconicoTest1.");
            bool result = network_.LoginNiconico(ok_mail, ok_pass);
            Assert.That(result, Is.True, "LoginNiconicoTest1");

            TestUtility.Wait();
            TestUtility.Message("Running LoginNiconicoTest2.");
            result = network_.LoginNiconico(ok_mail, ng_pass);
            Assert.That(result, Is.False, "LoginNiconicoTest2");

            TestUtility.Wait();
            TestUtility.Message("Running LoginNiconicoTest3.");
            result = network_.LoginNiconico(ng_mail, ok_pass);
            Assert.That(result, Is.False, "LoginNiconicoTest3");

            TestUtility.Wait();
            TestUtility.Message("Running LoginNiconicoTest4.");
            result = network_.LoginNiconico(ng_mail, ng_pass);
            Assert.That(result, Is.False, "LoginNiconicoTest4");
        }

        [Test]
        public void CheckAndLoginNiconicoTest()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            string ng_pass = TestUtility.TestData[TestUtility.KEY_NG_PASS];

            network_.ClearCookie();
            network_.SetCookieKind(NicoNetwork.CookieKind.None);
            TestUtility.Wait();
            TestUtility.Message("Running CheckAndLoginNiconicoTest1.");
            bool result = network_.CheckAndLoginNiconico(ok_mail, ok_pass);
            Assert.That(result, Is.True, "CheckAndLoginNiconicoTest1");

            network_.ClearCookie();
            TestUtility.Wait();
            TestUtility.Message("Running CheckAndLoginNiconicoTest2.");
            result = network_.CheckAndLoginNiconico(ok_mail, ng_pass);
            Assert.That(result, Is.False, "CheckAndLoginNiconicoTest2");
            TestUtility.Wait();
            TestUtility.Message("Running CheckAndLoginNiconicoTest3.");
            result = network_.CheckAndLoginNiconico(ok_mail, ok_pass);
            Assert.That(result, Is.True, "CheckAndLoginNiconicoTest3");
        }

        [Test]
        public void IsLoginNiconicoTest()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            string ng_pass = TestUtility.TestData[TestUtility.KEY_NG_PASS];

            network_.SetCookieKind(NicoNetwork.CookieKind.None);
            network_.ClearCookie();
            TestUtility.Wait();
            TestUtility.Message("Running IsLoginNiconicoTest1.");
            bool result = network_.IsLoginNiconico();
            Assert.That(result, Is.False, "IsLoginNiconicoTest1");
            TestUtility.Wait();
            TestUtility.Message("Running IsLoginNiconicoTest2.");
            bool result2 = network_.LoginNiconico(ok_mail, ok_pass);
            Assert.That(result2, Is.True, "IsLoginNiconicoTest2-1");
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "IsLoginNiconicoTest2-2");

            network_.ClearCookie();
            TestUtility.Wait();
            TestUtility.Message("Running IsLoginNiconicoTest3.");
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.False, "IsLoginNiconicoTest3");
        }

        [Test]
        public void ReloadCookieTest01IE()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            bool result;

            TestUtility.Message("Running ReloadCookieTestIE.");
            using (Browser browser = new IE("about:blank"))
            {
                network_.SetCookieKind(NicoNetwork.CookieKind.None);
                network_.ClearCookie();

                TestUtility.BrowserLogout(browser);

                result = network_.IsLoginNiconico();
                Assert.That(result, Is.False, "ReloadCookieTestIE-1");

                TestUtility.BrowserLogin(browser);
            }

            network_.ReloadCookie(NicoNetwork.CookieKind.IE);
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "ReloadCookieTestIE-2");
        }

        [Test]
        public void ReloadCookieTest02Firefox()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            bool result;

            TestUtility.Message("Running ReloadCookieTestFirefox.");
            using (Browser browser = new FireFox())
            {
                network_.SetCookieKind(NicoNetwork.CookieKind.None);
                network_.ClearCookie();

                TestUtility.BrowserLogout(browser);

                result = network_.IsLoginNiconico();
                Assert.That(result, Is.False, "ReloadCookieTestFireFox-1");

                TestUtility.BrowserLogin(browser);
            }

            network_.ReloadCookie(NicoNetwork.CookieKind.Firefox3);
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "ReloadCookieTestFirefox-2");
        }

        // Firefox Manual mode. Use if automation is problematic.
        //[Test]
        //public void ReloadCookieTest02Firefox()
        //{
        //    string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
        //    string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
        //    string browser = TestUtility.TestData[TestUtility.KEY_BROWSER_FIREFOX];
        //    string firefox_profile_name = TestUtility.TestData[TestUtility.KEY_FIREFOX_PROFILE_NAME];
        //    bool result;

        //    TestUtility.Message("Running ReloadCookieTestFirefox.");
        //    network_.SetCookieKind(NicoNetwork.CookieKind.None);
        //    network_.ClearCookie();
        //    result = network_.IsLoginNiconico();
        //    Assert.That(result, Is.False, "ReloadCookieTestFirefox-1");

        //    string option;
        //    if (string.IsNullOrEmpty(firefox_profile_name))
        //    {
        //        option = null;
        //    }
        //    else
        //    {
        //        option = string.Format("-p \"{0}\"", firefox_profile_name);
        //    }
        //    TestUtility.BrowserLoginManual(browser, option, "Firefox");

        //    network_.ReloadCookie(NicoNetwork.CookieKind.Firefox3);
        //    result = network_.IsLoginNiconico();
        //    Assert.That(result, Is.True, "ReloadCookieTestFirefox-2");
        //}

        [Test]
        public void ReloadCookieTest03Opera()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            string browser = TestUtility.TestData[TestUtility.KEY_BROWSER_OPERA];
            string opera_settings_file_path = TestUtility.TestData[TestUtility.KEY_OPERA_SETTINGS_FILE_PATH];
            bool result;

            TestUtility.Message("Running ReloadCookieTestOpera.");
            network_.SetCookieKind(NicoNetwork.CookieKind.None);
            network_.ClearCookie();
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.False, "ReloadCookieTestOpera-1");

            string option;
            if (string.IsNullOrEmpty(opera_settings_file_path))
            {
                option = null;
            }
            else
            {
                option = string.Format("/Settings \"{0}\"", opera_settings_file_path);
            }
            TestUtility.BrowserLoginManual(browser, option, "Opera");

            network_.ReloadCookie(NicoNetwork.CookieKind.Opera);
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "ReloadCookieTestOpera-2");
        }

        // WatiN support for Chrome is still work in progress.
        //[Test]
        //[RunUnitTest]
        //public void ReloadCookieTest04Chrome()
        //{
        //    string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
        //    string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
        //    bool result;

        //    TestUtility.Message("Running ReloadCookieTestChrome.");
        //    using (Browser browser = new Chrome())
        //    {
        //        network_.SetCookieKind(NicoNetwork.CookieKind.None);
        //        network_.ClearCookie();

        //        TestUtility.BrowserLogout(browser);

        //        result = network_.IsLoginNiconico();
        //        Assert.That(result, Is.False, "ReloadCookieTestChrome-1");

        //        TestUtility.BrowserLogin(browser);
        //    }

        //    network_.ReloadCookie(NicoNetwork.CookieKind.Chrome);
        //    result = network_.IsLoginNiconico();
        //    Assert.That(result, Is.True, "ReloadCookieTestChrome-2");
        //}

        [Test]
        public void ReloadCookieTest04Chrome()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];
            string browser = TestUtility.TestData[TestUtility.KEY_BROWSER_CHROME];
            string chrome_profile_dir = TestUtility.TestData[TestUtility.KEY_CHROME_PROFILE_DIR];
            bool result;

            TestUtility.Message("Running ReloadCookieTestChrome.");
            network_.SetCookieKind(NicoNetwork.CookieKind.None);
            network_.ClearCookie();
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.False, "ReloadCookieTestChrome-1");

            string option;
            if (string.IsNullOrEmpty(chrome_profile_dir))
            {
                option = null;
            }
            else
            {
                option = string.Format("--user-data-dir=\"{0}\"", chrome_profile_dir);
            }
            TestUtility.BrowserLoginManual(browser, option, "Chrome");

            network_.ReloadCookie(NicoNetwork.CookieKind.Chrome);
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "ReloadCookieTestChrome-2");
        }


        [Test]
        public void ResetUserSessionTest()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];

            TestUtility.Wait();
            TestUtility.Message("Running GetResetUserSessionTest1");
            bool result = network_.LoginNiconico(ok_mail, ok_pass);
            Assert.That(result, Is.True, "GetResetUserSessionTest1");
            string sid1 = network_.GetUserSession();
            TestUtility.Message(sid1);

            TestUtility.Wait();
            TestUtility.Message("Running GetResetUserSessionTest2");
            result = network_.LoginNiconico(ok_mail, ok_pass);
            Assert.That(result, Is.True, "GetResetUserSessionTest2");
            string sid2 = network_.GetUserSession();
            TestUtility.Message(sid2);

            network_.ResetUserSession(sid1);
            TestUtility.Wait();
            TestUtility.Message("Running GetResetUserSessionTest3");
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.False, "GetResetUserSessionTest3");

            network_.ResetUserSession(sid2);
            TestUtility.Wait();
            TestUtility.Message("Running GetResetUserSessionTest4");
            result = network_.IsLoginNiconico();
            Assert.That(result, Is.True, "GetResetUserSessionTest4");
        }

        [Test]
        public void DownloadRankingTest()
        {
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];

            TestUtility.EnsureLogin(network_);

            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);

            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "DownloadRankingTest1");

            DownloadKind kind = new DownloadKind();
            CategoryItem categoryItem = new CategoryItem();
            categoryItem.id = "music";
            categoryItem.short_name = "mus";
            categoryItem.name = "音楽";
            categoryItem.page = new int[] { 3, 1, 1, 1, 0 };
            List<CategoryItem> categoryList = new List<CategoryItem>();
            categoryList.Add(categoryItem);
            kind.CategoryList = categoryList;
            kind.SetTarget(true, true, true);
            kind.SetDuration(false, false, true, false, false);
            kind.SetFormat(DownloadKind.FormatKind.Rss);

            NicoNetwork.NetworkWaitDelegate onWait = delegate(string message, int current, int total)
            {
                TestUtility.Message("({0}/{1}){2}", current, total, message);
                TestUtility.Wait();
            };

            TestUtility.Message("Running DownloadRankingTest2.");
            network_.DownloadRanking(temp_directory.FullName, kind, onWait);

            List<string> name_list = null;
            List<string> filename_list = new List<string>();
            kind.GetRankingNameList(ref name_list, ref filename_list);
            string[] downloaded_files = Directory.GetFiles(temp_directory.FullName, "*", SearchOption.AllDirectories);
            int i = 0;
            foreach (string filename in filename_list)
            {
                i++;
                bool result = Array.Exists<string>(downloaded_files, delegate(string downloaded_file)
                {
                    return Path.GetFileName(downloaded_file).StartsWith(filename);
                });
                Assert.That(result, Is.True, string.Format("DownloadRankingTest2-{0}", i));
            }
            i = 0;
            foreach (string downloaded_file in downloaded_files)
            {
                i++;
                bool result = filename_list.Exists(delegate(string filename)
                {
                    return Path.GetFileName(downloaded_file).StartsWith(filename);
                });
                Assert.That(result, Is.True, string.Format("DownloadRankingTest3-{0}", i));
            }

            i = 0;
            foreach (string downloaded_file in downloaded_files)
            {
                i++;
                TestUtility.IsValidXml(downloaded_file);
            }
        }

        [Test]
        public void DownloadAndSaveFlvTest()
        {
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            string local_file = TestUtility.TestData[TestUtility.KEY_VIDEO_LOCAL];
            string local_file_nm = TestUtility.TestData[TestUtility.KEY_VIDEO_LOCAL_NM];
            string local_file_swf = TestUtility.TestData[TestUtility.KEY_VIDEO_LOCAL_SWF];
            string local_file_so = TestUtility.TestData[TestUtility.KEY_VIDEO_LOCAL_SO];
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string id_nm = TestUtility.TestData[TestUtility.KEY_VIDEO_ID_NM];
            string id_swf = TestUtility.TestData[TestUtility.KEY_VIDEO_ID_SWF];
            string id_so = TestUtility.TestData[TestUtility.KEY_VIDEO_ID_SO];

            TestUtility.EnsureLogin(network_);

            DirectoryInfo tempDirectory = new DirectoryInfo(temp_directory_path);

            Assert.That(TestUtility.InitDirectory(tempDirectory), Is.True, "DownloadAndSaveFlvTest1");

            IJNetwork.DownloadingEventDelegate onDownloading = delegate(ref bool is_cancel, long current_size, long file_size)
            {
                TestUtility.Message("{0} of {1} ({2}%)", current_size, file_size, (current_size * 100f) / file_size);
            };

            // flv
            string download_file = Path.Combine(tempDirectory.FullName, Path.GetFileName(local_file));
            TestUtility.Wait(5);
            TestUtility.Message("Running DownloadAndSaveFlv2");
            TestUtility.Message(id);
            network_.DownloadAndSaveFlv(id, download_file, onDownloading);
            Assert.That(File.Exists(download_file), Is.True, "DownloadAndSaveFlv2-1");
            Assert.That(TestUtility.FileEquals(local_file, download_file), Is.True, "DownloadAndSaveFlv2-2");

            // nm
            download_file = Path.Combine(tempDirectory.FullName, Path.GetFileName(local_file_nm));
            TestUtility.Wait(5);
            TestUtility.Message("Running DownloadAndSaveFlv3");
            TestUtility.Message(id_nm);
            network_.DownloadAndSaveFlv(id_nm, download_file, onDownloading);
            Assert.That(File.Exists(download_file), Is.True, "DownloadAndSaveFlv3-1");
            Assert.That(TestUtility.FileEquals(local_file_nm, download_file), Is.True, "DownloadAndSaveFlv3-2");

            // swf
            download_file = Path.Combine(tempDirectory.FullName, Path.GetFileName(local_file_swf));
            TestUtility.Wait(5);
            TestUtility.Message("Running DownloadAndSaveFlv4");
            TestUtility.Message(id_swf);
            network_.DownloadAndSaveFlv(id_swf, download_file, onDownloading);
            Assert.That(File.Exists(download_file), Is.True, "DownloadAndSaveFlv4-1");
            Assert.That(TestUtility.FileEquals(local_file_swf, download_file), Is.True, "DownloadAndSaveFlv4-2");

            // so
            download_file = Path.Combine(tempDirectory.FullName, Path.GetFileName(local_file_so));
            TestUtility.Wait(5);
            TestUtility.Message("Running DownloadAndSaveFlv5");
            TestUtility.Message(id_so);
            network_.DownloadAndSaveFlv(id_so, download_file, onDownloading);
            Assert.That(File.Exists(download_file), Is.True, "DownloadAndSaveFlv5-1");
            Assert.That(TestUtility.FileEquals(local_file_so, download_file), Is.True, "DownloadAndSaveFlv5-2");
        }

        [Test]
        public void GetVideoPageTest()
        {
            TestUtility.EnsureLogin(network_);

            string str = network_.GetVideoPage("sm9");
            Assert.That(str, Text.Contains("新・豪血寺一族 -煩悩解放 - レッツゴー！陰陽師"));
        }

        [Test]
        public void GetVideoInfoTest()
        {
            TestUtility.EnsureLogin(network_);

            string str = network_.GetVideoInfo("sm9");
            Assert.That(str, Text.Contains("url="), "GetVideoInfoTest1");
            TestUtility.Message(str);
        }

        [Test]
        public void GetThumbInfoTest()
        {
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string str = network_.GetThumbInfo(video_id);
            Assert.That(str, Text.Contains("<nicovideo_thumb_response status=\"ok\">"), "GetThumbInfoTest1");
            Assert.That(str, Text.Contains(string.Format("<video_id>{0}</video_id>", video_id)), "GetThumbInfoTest2");
        }

        [Test]
        public void GetExtThumbTest()
        {

            string str = network_.GetExtThumb("sm9");
            Assert.That(str, Text.Contains("新・豪血寺一族 -煩悩解放 - レッツゴー！陰陽師‐ニコニコ動画"), "GetExtThumbTest");
        }

        [Test]
        public void SaveThumbnailWithVideoIdTest()
        {
            string thumbnail_local = TestUtility.TestData[TestUtility.KEY_THUMBNAIL_LOCAL];
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            string id = Path.GetFileNameWithoutExtension(thumbnail_local);
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "SaveThumbnailWithVideoIdTest1");
            string file = Path.Combine(temp_directory.FullName, Path.GetFileName(thumbnail_local));
            network_.SaveThumbnailWithVideoId(id, file);
            Assert.That(TestUtility.FileEquals(thumbnail_local, file), Is.True, "SaveThumbnailWithVideoIdTest2");
        }

        [Test]
        public void SaveThumbnailTest()
        {
            string thumbnail_local = TestUtility.TestData[TestUtility.KEY_THUMBNAIL_LOCAL];
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            string id = Path.GetFileNameWithoutExtension(thumbnail_local);
            string str = network_.GetThumbInfo(id);
            Match m = Regex.Match(str, "<thumbnail_url>(?<url>.+?)</thumbnail_url>");
            Assert.That(m.Success, Is.True, "SaveThumbnailTest1");
            string url = m.Groups["url"].Value;
            DirectoryInfo tempDirectory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(tempDirectory), Is.True, "SaveThumbnailTest2");
            string file = Path.Combine(tempDirectory.FullName, Path.GetFileName(thumbnail_local));
            network_.SaveThumbnail(url, file);
            Assert.That(TestUtility.FileEquals(thumbnail_local, file), Is.True, "SaveThumbnail3");
        }

        [Test]
        public void GetSearchKeywordTest()
        {
            string search_keyword = TestUtility.TestData[TestUtility.KEY_SEARCH_KEYWORD];
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.EnsureLogin(network_);

            string str = network_.GetSearchKeyword(search_keyword, 1, NicoNetwork.SearchSortMethod.Mylist, NicoNetwork.SearchOrder.Asc);
            Assert.That(str, Text.Contains(id), "GetSearchKeywordTest1");
        }

        [Test]
        public void GetSearchTagTest()
        {
            string search_tag = TestUtility.TestData[TestUtility.KEY_SEARCH_TAG];
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.EnsureLogin(network_);

            string str = network_.GetSearchTag(search_tag, 1, NicoNetwork.SearchSortMethod.Mylist, NicoNetwork.SearchOrder.Asc);
            Assert.That(str, Text.Contains(id), "GetSearchTagTest1");
        }

        [Test]
        public void GetNewArrivalTest()
        {
            TestUtility.EnsureLogin(network_);

            string str = network_.GetNewArrival(1);
            Assert.That(str, Text.Contains("新着投稿順"), "GetNewArrivalTest1");
        }

        [Test]
        public void GetMylistAddPageTest()
        {
            TestUtility.EnsureLogin(network_);

            string video_id = "sm9";
            string html = network_.GetMylistAddPage(video_id);
            Assert.That(html, Text.Contains("この動画をマイリストに登録しますか？"), "GetMylistAddPageTest1");
            Assert.That(html, Text.Contains("<input type=\"hidden\" name=\"item_id\" value=\"1173108780\">"), "GetMylistAddPageTest2");
        }

        [Test]
        public void GetMylistHtmlTest()
        {
            string mylist_id = TestUtility.TestData[TestUtility.KEY_MYLIST_ID];
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.EnsureLogin(network_);

            string str = network_.GetMylistHtml(mylist_id, true);
            Assert.That(str, Text.Contains(id), "GetMylistHtmlTest1");

            str = network_.GetMylistHtml(mylist_id, false);
            Assert.That(str, Text.Contains(id), "GetMylistHtmlTest2");
        }

        [Test]
        public void GetMylistPageFromMyPageTest()
        {
            string mylist_id = TestUtility.TestData[TestUtility.KEY_MYLIST_ID];

            TestUtility.EnsureLogin(network_);

            string html = network_.GetMylistPageFromMyPage();
            Assert.That(html, Text.Contains(mylist_id), "GetMylistPageFromMyPageTest1");
        }

        [Test]
        public void MylistTest()
        {
            int wait_millisecond = int.Parse(TestUtility.TestData[TestUtility.KEY_ACCESS_WAIT_MILLISECONDS]);

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string video_id1 = "sm9";
            string video_id2 = "sm1097445";

            // {"id":(?<mylist_id>\d+),"status":"ok"}
            Regex mylist_id_regex = new Regex("{\"id\":(?<mylist_id>\\d+),\"status\":\"ok\"}");

            TestUtility.EnsureLogin(network_);

            string mylist_id1 = null;
            string mylist_id2 = null;
            string mylist_id3 = null;
            string token = null;

            bool test_succeeded = false;

            try
            {
                // ()
                // -> (mylist1())

                TestUtility.Message("Running MylistTest - Create new mylist");
                string mylist_title1 = "まいりすと_1_" + timestamp;
                string result = network_.MakeNewMylistGroup(mylist_title1);
                TestUtility.Message(result);
                Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTest1-1");
                mylist_id1 = mylist_id_regex.Match(result).Groups["mylist_id"].Value;
                string str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(mylist_title1), "MylistTest1-2");


                TestUtility.Wait(5);

                // (mylist1())
                // -> (mylist1() mylist2())

                TestUtility.Message("Running MylistTest - Create new mylist with token");
                string mylist_title2 = "マイリスト_2_" + timestamp;
                string my_mylist_page = network_.GetMylistPageFromMyPage();
                token = Regex.Match(my_mylist_page, "NicoAPI.token = \"([-0-9a-f]+)\";").Groups[1].Value;
                result = network_.MakeNewMylistGroup(mylist_title2, token);
                TestUtility.Message(result);
                Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTest2-1");
                mylist_id2 = mylist_id_regex.Match(result).Groups["mylist_id"].Value;
                str = network_.GetMylistHtml(mylist_id2, true);
                Assert.That(str, Text.Contains(mylist_title2), "MylistTest2-2");


                TestUtility.Wait(5);

                // (mylist1() mylist2())
                // -> (*mylist1() mylist2())

                TestUtility.Message("Running MylistTest - Update mylist information");
                bool mylist_visibility1 = false;
                mylist_title1 = mylist_title1 + "_mod";
                string mylist_description1 = Guid.NewGuid().ToString() + "テスト";
                int mylist_order1 = 16;
                int mylist_color1 = 3;
                result = network_.UpdateMylistGroup(mylist_id1, mylist_visibility1, mylist_title1, mylist_description1, mylist_order1, mylist_color1);
                TestUtility.Message(result);
                Assert.That(result, Is.EqualTo("{\"status\":\"ok\"}"), "MylistTest3-1");
                str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(mylist_description1), "MylistTest3-2");



                TestUtility.Wait(5);

                // (*mylist1() mylist2())
                // -> (*mylist1() *mylist2())

                TestUtility.Message("Running MylistTest - Update mylist information with token");
                bool mylist_visibility2 = false;
                mylist_title2 = mylist_title2 + "_mod";
                string mylist_description2 = Guid.NewGuid().ToString() + "テスト";
                int mylist_order2 = 15;
                int mylist_color2 = 2;
                result = network_.UpdateMylistGroup(mylist_id2, mylist_visibility2, mylist_title2, mylist_description2, mylist_order2, mylist_color2, token);
                TestUtility.Message(result);
                Assert.That(result, Is.EqualTo("{\"status\":\"ok\"}"), "MylistTest4-1");
                str = network_.GetMylistHtml(mylist_id2, true);
                Assert.That(str, Text.Contains(mylist_description2), "MylistTest4-2");

                TestUtility.Wait(5);

                // (*mylist1() *mylist2())
                // -> (*mylist1() *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Create mylist with mylist information");
                bool mylist_visibility3 = false;
                string mylist_title3 = "᧠᧡᧢᧣᧥᧥᧼_3_" + timestamp;
                string mylist_description3 = Guid.NewGuid().ToString() + "テスト";
                int mylist_order3 = 17;
                int mylist_color3 = 4;
                result = network_.MakeNewAndUpdateMylistGroup(mylist_visibility3, mylist_title3, mylist_description3, mylist_order3, mylist_color3, out mylist_id3);
                TestUtility.Message(result);
                Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTest5-1");
                str = network_.GetMylistHtml(mylist_id3, true);
                Assert.That(str, Text.Contains(mylist_description3), "MylistTest5-2");

                TestUtility.Wait(5);

                // (*mylist1() *mylist2() *mylist3())
                // -> (*mylist1(video1) *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Add video to mylist");
                result = network_.AddMylist(mylist_id1, video_id1);
                TestUtility.Message(result);
                Assert.That(result, Is.EqualTo("{\"status\":\"ok\"}"), "MylistTest6-1");
                str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(video_id1), "MylistTest6-2");

                TestUtility.Wait(5);

                // (*mylist1(video1) *mylist2() *mylist3())
                // -> (*mylist1(video1,video2) *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Add video to mylist with token");
                result = network_.AddMylist(mylist_id1, video_id2);
                TestUtility.Message(result);
                Assert.That(result, Is.EqualTo("{\"status\":\"ok\"}"), "MylistTest7-1");
                str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(video_id2), "MylistTest7-2");

                TestUtility.Wait(5);

                // (*mylist1(video1,video2) *mylist2() *mylist3())
                // -> (*mylist1(*video1,*video2) *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Update description of videos in mylist");
                List<string> video_id_list = new List<string>();
                video_id_list.Add(video_id1);
                video_id_list.Add(video_id2);
                List<string> description_list = new List<string>();
                string video_description1 = Guid.NewGuid().ToString();
                string video_description2 = Guid.NewGuid().ToString();
                description_list.Add(video_description1);
                description_list.Add(video_description2);
                network_.UpdateMylistDescription(mylist_id1, video_id_list, description_list, wait_millisecond);
                str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(video_description1), "MylistTest8-1");
                Assert.That(str, Text.Contains(video_description2), "MylistTest8-2");


                TestUtility.Wait(5);

                // (*mylist1(*video1,*video2) *mylist2() *mylist3())
                // -> (*mylist1(**video1,**video2) *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Update description of videos in mylist with delegate");
                description_list = new List<string>();
                video_description1 = Guid.NewGuid().ToString();
                video_description2 = Guid.NewGuid().ToString();
                description_list.Add(video_description1);
                description_list.Add(video_description2);
                NicoNetwork.NetworkWaitDelegate on_wait = delegate(string message, int current, int total)
                {
                    TestUtility.Message("{0}({1}/{2})", message, current, total);
                };
                network_.UpdateMylistDescription(mylist_id1, video_id_list, description_list, on_wait);
                str = network_.GetMylistHtml(mylist_id1, true);
                Assert.That(str, Text.Contains(video_description1), "MylistTest9-1");
                Assert.That(str, Text.Contains(video_description2), "MylistTest9-2");

                TestUtility.Wait(5);

                // (*mylist1(*video1,*video2) *mylist2() *mylist3())
                // -> (*mylist1(**video1,**video2) *mylist2() *mylist3())

                TestUtility.Message("Running MylistTest - Add same video.");
                try
                {
                    result = network_.AddMylist(mylist_id1, video_id1);
                    TestUtility.Message(result);
                    Assert.Fail("MylistTest10-1");
                }
                catch (NiconicoAddingMylistExistException e)
                {
                    TestUtility.Message(e.ToString());
                }

                TestUtility.Wait(5);

                TestUtility.Message("Running MylistTest - Fetch my mylist information.");
                List<MylistInfo> mylist_info_list = network_.GetMylistInfoListFromMypage();
                // mylist1, mylist2, mylist3, mylist_id
                Assert.That(mylist_info_list.Count, Is.GreaterThanOrEqualTo(4), "MylistTest10-1");
                MylistInfo mylist_info1 = mylist_info_list.Find(delegate(MylistInfo mi) { return mi.mylist_id == mylist_id1; });
                Assert.That(mylist_info1, Is.Not.Null, "MylistTest10-2");
                Assert.That(mylist_info1.description, Is.EqualTo(mylist_description1), "MylistTest10-3");
                Assert.That(mylist_info1.is_public, Is.EqualTo(mylist_visibility1), "MylistTest10-4");
                //TODO Assert.That(mylist_info1.number_of_item, Is.EqualTo(2), "MylistTest10-5");
                Assert.That(mylist_info1.title, Is.EqualTo(mylist_title1), "MylistTest10-6");
                MylistInfo mylist_info2 = mylist_info_list.Find(delegate(MylistInfo mi) { return mi.mylist_id == mylist_id2; });
                Assert.That(mylist_info2, Is.Not.Null, "MylistTest10-2");
                Assert.That(mylist_info2.description, Is.EqualTo(mylist_description2), "MylistTest10-7");
                Assert.That(mylist_info2.is_public, Is.EqualTo(mylist_visibility2), "MylistTest10-8");
                //TODO Assert.That(mylist_info2.number_of_item, Is.EqualTo(0), "MylistTest10-9");
                Assert.That(mylist_info2.title, Is.EqualTo(mylist_title2), "MylistTest10-10");
                MylistInfo mylist_info3 = mylist_info_list.Find(delegate(MylistInfo mi) { return mi.mylist_id == mylist_id3; });
                Assert.That(mylist_info3, Is.Not.Null, "MylistTest10-11");
                Assert.That(mylist_info3.description, Is.EqualTo(mylist_description3), "MylistTest10-12");
                Assert.That(mylist_info3.is_public, Is.EqualTo(mylist_visibility3), "MylistTest10-13");
                //TODO Assert.That(mylist_info3.number_of_item, Is.EqualTo(0), "MylistTest10-14");
                Assert.That(mylist_info3.title, Is.EqualTo(mylist_title3), "MylistTest10-15");


                // TODO Move video.
                // (*mylist1(**video1,**video2) *mylist2() *mylist3())
                // -> (*mylist1(**video2) *mylist2(**video1) *mylist3())

                // TODO Copy video.
                // (*mylist1(**video2) *mylist2(**video1) *mylist3())
                // -> (*mylist1(**video2) *mylist2(**video1, **video2) *mylist3())

                // TODO Delete video.
                // (*mylist1(**video2) *mylist2(**video1, **video2) *mylist3())
                // -> (*mylist1() *mylist2(**video1, **video2) *mylist3())

                test_succeeded = true;
            }
            finally
            {
                bool delete_succeeded = true;

                if (token == null)
                {
                    string my_mylist_page = network_.GetMylistPageFromMyPage();
                    token = Regex.Match(my_mylist_page, "NicoAPI.token = \"([-0-9a-f]+)\";").Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(mylist_id1))
                {
                    // (*mylist1() *mylist2(**video1, **video2) *mylist3())
                    // -> (*mylist2(**video1, **video2) *mylist3())
                    try
                    {
                        TestUtility.Wait(5);
                        TestUtility.Message("Running MylistTest - Delete mylist1");
                        string result = RemoveMylistGroup(network_, mylist_id1, token);
                        TestUtility.Message(result);
                        Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTestI-1");
                    }
                    catch (Exception e)
                    {
                        delete_succeeded = false;
                        TestUtility.Message(e.ToString());
                    }
                }

                if (!string.IsNullOrEmpty(mylist_id2))
                {
                    // (*mylist2(**video1, **video2) *mylist3())
                    // -> (*mylist3())
                    try
                    {
                        TestUtility.Wait(5);
                        TestUtility.Message("Running MylistTest - Delete mylist2");
                        string result = RemoveMylistGroup(network_, mylist_id2, token);
                        TestUtility.Message(result);
                        Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTestII-1");
                    }
                    catch (Exception e)
                    {
                        delete_succeeded = false;
                        TestUtility.Message(e.ToString());
                    }
                }

                if (!string.IsNullOrEmpty(mylist_id3))
                {
                    // (*mylist3())
                    // -> ()
                    try
                    {
                        TestUtility.Wait(5);
                        TestUtility.Message("Running MylistTest - Delete mylist3");
                        string result = RemoveMylistGroup(network_, mylist_id3, token);
                        TestUtility.Message(result);
                        Assert.That(result, Text.Contains("\"status\":\"ok\""), "MylistTestIII-1");
                    }
                    catch (Exception e)
                    {
                        delete_succeeded = false;
                        TestUtility.Message(e.ToString());
                    }
                }

                if (test_succeeded && !delete_succeeded)
                {
                    Assert.Fail("MylistTest delete");
                }
            }
        }

        private string RemoveMylistGroup(NicoNetwork network, string mylist_id, string token)
        {
            HttpWebRequest request = WebRequest.Create("http://www.nicovideo.jp/api/mylistgroup/delete") as HttpWebRequest;
            request.Method = "POST";
            string cookie_value = network.GetUserSession();
            Cookie cookie = new Cookie("user_session", cookie_value, "/", ".nicovideo.jp");
            CookieContainer cookie_container = new CookieContainer();
            cookie_container.Add(cookie);
            request.CookieContainer = cookie_container;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Headers.Add("x-requested-with", "XMLHttpRequest");

            string post_data = IJNetwork.ConstructPostData(
                "group_id", mylist_id,
                "token", token);

            using (Stream request_stream = request.GetRequestStream())
            using (TextWriter request_writer = new StreamWriter(request_stream, new UTF8Encoding()))
            {
                request_writer.Write(post_data);
            }

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream response_stream = response.GetResponseStream())
            using (TextReader response_reader = new StreamReader(response_stream, new UTF8Encoding()))
            {
                return response_reader.ReadToEnd();
            }
        }

        [Test]
        public void TagTest()
        {
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            List<string> tag_list = new List<string>();
            List<bool> is_lock_list = new List<bool>();

            string test_tag_lock = "タグ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string test_tag_unlock = "たぐ" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

            tag_list.Add(test_tag_lock);
            is_lock_list.Add(true);
            tag_list.Add(test_tag_unlock);
            is_lock_list.Add(false);

            TestUtility.EnsureLogin(network_);

            TestUtility.Message("Running TagTest - Add tags");
            network_.AddTag(tag_list, is_lock_list, id, delegate(string message, int current, int total)
            {
                TestUtility.Message("({0}/{1}){2}", current, total, message);
                TestUtility.Wait();
            });
            string html = network_.GetVideoPage(id);
            Assert.That(html, Text.Contains(test_tag_lock), "TagTest1-1");
            Assert.That(html, Text.Contains(test_tag_unlock), "TagTest1-2");
            TestUtility.Wait(30);
            network_.RemoveTag(test_tag_lock, id);
            html = network_.GetVideoPage(id);
            Assert.That(html, Text.Contains(test_tag_lock), "TagTest1-3");


            TestUtility.Message("Running TagTest - Unlock tag");
            TestUtility.Wait(30);
            network_.UnlockTag(test_tag_lock, id);
            html = network_.GetVideoPage(id);
            Assert.That(html, Text.Contains(test_tag_lock), "TagTest2-1");
            Assert.That(html, Text.Contains(test_tag_unlock), "TagTest2-2");

            TestUtility.Message("Running TagTest - Remove tags");
            TestUtility.Wait(30);
            network_.RemoveTag(test_tag_lock, id);
            TestUtility.Wait(30);
            html = network_.GetVideoPage(id);
            Assert.That(html, Text.DoesNotContain(test_tag_lock), "TagTest3-1");
            Assert.That(html, Text.Contains(test_tag_unlock), "TagTest3-2");
            TestUtility.Wait(30);
            network_.RemoveTag(test_tag_unlock, id);
            TestUtility.Wait(30);
            html = network_.GetVideoPage(id);
            Assert.That(html, Text.DoesNotContain(test_tag_lock), "TagTest3-3");
            Assert.That(html, Text.DoesNotContain(test_tag_unlock), "TagTest3-4");
        }

        [Test]
        public void GetTagEditHtmlTest()
        {
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string[] tags = TestUtility.TestData[TestUtility.KEY_SEARCH_TAG].Split(' ');

            TestUtility.EnsureLogin(network_);

            TestUtility.Message("Running GetTagEditHtmlTest");

            string str = network_.GetTagEditHtml(video_id);

            for (int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                Assert.That(str, Text.Contains(tag), string.Format("GetTagEditHtmlTest{0}", i));
            }
        }

        [Test]
        public void GetMyVideoPageTest()
        {
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.EnsureLogin(network_);

            string html = network_.GetMyVideoPage();
            Assert.That(html, Text.Contains(id), "GetMyVideoPageTest1");
        }

        [Test]
        public void CommentTest()
        {
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.Message("Running CommentTest.");

            TestUtility.EnsureLogin(network_);

            string comment = "コメント_" + Guid.NewGuid().ToString();
            int vpos = 0;

            TestUtility.Message("CommentTest - Posting a comment.");
            string result = network_.PostComment(video_id, comment, vpos);
            TestUtility.Message(result);

            TestUtility.Message("CommentTest - Getting comments.");
            result = network_.GetComment(video_id, -10);
            Assert.That(result, Text.Contains(comment), "CommentTest1");
        }
    }
}
