using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NicoTools;
using IJLib;
using NUnit.Framework.SyntaxHelpers;
using System.IO;

namespace nicoranktest
{
    [TestFixture]
    public class NicoNetworkManagerTest
    {
        NicoNetwork network_;
        TestMessageOut msgout_;
        CancelObject cancel_object_;
        NicoNetworkManager network_manager_;

        [SetUp]
        public void Setup()
        {
            network_ = new NicoNetwork();
            msgout_ = new TestMessageOut();
            cancel_object_ = new CancelObject();
            network_manager_ = new NicoNetworkManager(network_, msgout_, cancel_object_);
            NicoTools.NicoNetworkManager.StringDelegate string_delegate = delegate(string str) 
            {
                Console.WriteLine(str);
            };
            network_manager_.SetDelegateSetDonwloadInfo(string_delegate);
        }

        [Test]
        public void CheckLoginTest()
        {
            string ok_mail = TestUtility.TestData[TestUtility.KEY_OK_MAIL];
            string ok_pass = TestUtility.TestData[TestUtility.KEY_OK_PASS];

            TestUtility.Message("Running CheckLoginTest");
            network_.SetCookieKind(NicoNetwork.CookieKind.None);
            network_.ClearCookie();

            msgout_.OnWriteLine = delegate(string str)
            {
                Assert.Fail("CheckLoginTest1");
            };

            msgout_.OnWrite = delegate(string str)
            {
                Assert.That(str, Is.EqualTo("ログインされていません。\r\n"), "CheckLoginTest2");
            };

            network_manager_.CheckLogin();

            network_.LoginNiconico(ok_mail, ok_pass);

            msgout_.OnWrite = delegate(string str)
            {
                Assert.That(str, Is.EqualTo("ログインされています。\r\n"), "CheckLoginTest3");
            };

            network_manager_.CheckLogin();
        }

        [Test]
        public void DownloadRankingTest()
        {
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];

            TestUtility.EnsureLogin(network_);

            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);

            DownloadKind kind = new DownloadKind();
            kind.SetDuration(true, false, false, false, false);
            CategoryItem categoryItem = new CategoryItem();
            categoryItem.id = "music";
            categoryItem.short_name = "mus";
            categoryItem.name = "音楽";
            categoryItem.page = new int[] { 3, 1, 1, 1, 0 };
            List<CategoryItem> categoryList = new List<CategoryItem>();
            categoryList.Add(categoryItem);
            kind.CategoryList = categoryList;
            kind.SetTarget(true, true, true);
            kind.SetFormat(DownloadKind.FormatKind.Html);

            bool completed = false;

            msgout_.OnWrite = delegate(string str)
            {
                if (str == "すべてのランキングのDLが完了しました。\r\n")
                {
                    completed = true;
                }
            };

            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "DownloadRankingTest1");

            TestUtility.Message("Running DownloadRankingTest - Download HTML");
            network_manager_.DownloadRanking(kind, temp_directory_path);

            Assert.That(completed, Is.True, "DownloadRankingTest2-1");
            int html_count = Directory.GetFiles(temp_directory_path, "*.html", SearchOption.AllDirectories).Length;
            Assert.That(html_count, Is.EqualTo(9), "DownloadRankingTest2-2");


            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "DownloadRankingTest3");
            kind.SetFormatRss();

            TestUtility.Message("Running DownloadRankingTest - Download RSS");
            network_manager_.DownloadRanking(kind, temp_directory_path);

            Assert.That(completed, Is.True, "DownloadRankingTest4-1");
            html_count = Directory.GetFiles(temp_directory_path, "*.xml", SearchOption.AllDirectories).Length;
            Assert.That(html_count, Is.EqualTo(9), "DownloadRankingTest4-2");
        }

        [Test]
        public void GetDetailInfoTest1()
        {
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string video_title = TestUtility.TestData[TestUtility.KEY_VIDEO_TITLE];

            TestUtility.EnsureLogin(network_);

            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "GetDetailInfoTest1-1");

            string rank_file_path = Path.Combine(temp_directory_path, "rank.txt");
            string rank_file_text = string.Format("{0}\r\n{1}\r\n", "sm9", video_id);
            IJFile.WriteVer2(rank_file_path, rank_file_text, IJFile.EncodingPriority.UTF8);

            RankFileCustomFormat custom_format = new RankFileCustomFormat();
            InputOutputOption iooption = new InputOutputOption(rank_file_path, rank_file_path, custom_format);

            RankingMethod ranking_method = new RankingMethod(HoseiKind.Nothing, SortKind.Nothing, 0);

            List<string> id_list = new List<string>(new string[] {
                video_id,
                "sm1097445"
            });

            network_manager_.GetDetailInfo(id_list, iooption, true, ranking_method, "1");

            RankFile rank_file = new RankFile(rank_file_path, custom_format);
            List<Video> video_list = rank_file.GetVideoList();

            Assert.That(video_list.Count, Is.EqualTo(3), "GetDetailInfoTest1-2");
            Assert.That(video_list[1].title, Is.EqualTo(video_title), "GetDetailInfoTest1-3");
        }

        [Test]
        public void ParseSearchTest()
        {
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string video_title = TestUtility.TestData[TestUtility.KEY_VIDEO_TITLE];
            string search_tag = TestUtility.TestData[TestUtility.KEY_SEARCH_TAG];

            TestUtility.EnsureLogin(network_);

            string html = network_.GetSearchTag(search_tag, 1, NicoNetwork.SearchSortMethod.Mylist, NicoNetwork.SearchOrder.Asc);

            List<Video> video_list = NicoNetworkManager.ParseSearch(html, -1);

            Assert.That(video_list.Exists(delegate(Video v) { return v.video_id == video_id; }), Is.True, "ParseSearchTest1");
        }
    }
}
