using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using NicoTools;
using IJLib;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class NicoListManagerTest
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
        public void ParseRankingTest1() 
        {
            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];

            TestUtility.EnsureLogin(network_);

            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);

            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ParseRankingTest1-1");

            DownloadKind kind = new DownloadKind();
            kind.SetDuration(false, false, true, false, false);
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

            NicoNetwork.NetworkWaitDelegate onWait = delegate(string message, int current, int total)
            {
                TestUtility.Message("({0}/{1}){2}", current, total, message);
                TestUtility.Wait();
            };

            TestUtility.Message("Running ParseRankingTest1.");
            network_.DownloadRanking(temp_directory.FullName, kind, onWait);

            NicoListManager.ParseRankingKind parse_ranking_kind;
            List<Video> video_list;

            try
            {
                parse_ranking_kind = NicoListManager.ParseRankingKind.TotalPoint;
                video_list = NicoListManager.ParseRanking(temp_directory.FullName, DateTime.Now, parse_ranking_kind);
                Assert.Fail("ParseRankingTest1-2");
            }
            catch (InvalidOperationException e)
            {
                TestUtility.Message(e.Message);
            }

            parse_ranking_kind = NicoListManager.ParseRankingKind.TermPoint;
            video_list = NicoListManager.ParseRanking(temp_directory.FullName, DateTime.Now, parse_ranking_kind);
            Assert.That(video_list.Count, Is.GreaterThan(0), "ParseRankingTest1-3");
        }

        [Test]
        public void ParseMyVideoHtmlTest()
        {
            string id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];

            TestUtility.EnsureLogin(network_);

            string html = network_.GetMyVideoPage();
            Assert.That(html, Text.Contains(id), "ParseMyVideoHtmlTest1");

            List<Video> videos = NicoListManager.ParseMyVideoHtml(html);
            Assert.That(videos.Exists(delegate(Video video) {
                return string.Equals(video.video_id, id, StringComparison.Ordinal);
            }), Is.True, "ParseMyVideoHtmlTest2");
        }

        [Test]
        public void ParsePointRssTest1()
        {
            string mylist_id = TestUtility.TestData[TestUtility.KEY_MYLIST_ID];
            string video_id = TestUtility.TestData[TestUtility.KEY_VIDEO_ID];
            string video_title = TestUtility.TestData[TestUtility.KEY_VIDEO_TITLE];
            string video_description = TestUtility.TestData[TestUtility.KEY_VIDEO_DESCRIPTION];
            DateTime video_submit_date = DateTime.Parse(TestUtility.TestData[TestUtility.KEY_VIDEO_SUBMIT_DATE]);
            string video_length = TestUtility.TestData[TestUtility.KEY_VIDEO_LENGTH];

            TestUtility.EnsureLogin(network_);

            string rss = network_.GetMylistHtml(mylist_id, true);
            List<Video> video_list = new List<Video>();
            NicoListManager.ParsePointRss(rss, DateTime.Now, video_list, false, true);
            Assert.That(video_list.Count, Is.GreaterThanOrEqualTo(1), "ParsePointRssTest1-1");
            Video video = video_list[0];
            Assert.That(video.title, Is.EqualTo(video_title), "ParsePointRssTest1-x");
            Assert.That(video.video_id, Is.EqualTo(video_id), "ParsePointRssTest1-2");
            Assert.That(video.description, Is.EqualTo(video_description), "ParsePointRssTest1-4");
            Assert.That(video.submit_date, Is.EqualTo(video_submit_date), "ParsePointRssTest1-5");
            Assert.That(video.length, Is.EqualTo(video_length), "ParsePointRssTest1-6");
        }

        [Test]
        public void ParsePointRssTest2()
        {
            int wait_milliseconds = int.Parse(TestUtility.TestData[TestUtility.KEY_ACCESS_WAIT_MILLISECONDS]);
            DirectoryInfo temp_dir = new DirectoryInfo(TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY]);
            string video_title = "【初音ミク】みくみくにしてあげる♪【してやんよ】";

            Assert.That(TestUtility.InitDirectory(temp_dir), Is.True, "parsePointRssTest2-1");

            TestUtility.EnsureLogin(network_);

            DownloadKind download_kind = new DownloadKind();
            download_kind.SetFormatRss();
            download_kind.SetTarget(true, false, false);
            download_kind.SetDuration(true, false, false, false, false);
            download_kind.CategoryList = new List<CategoryItem>();
            CategoryItem category_item = new CategoryItem();
            category_item.id = "music";
            category_item.name = "音楽";
            category_item.page = new int[] { 3, 1, 1, 0 };
            category_item.short_name = "mus";
            download_kind.CategoryList.Add(category_item);

            network_.DownloadRanking(temp_dir.FullName, download_kind, wait_milliseconds);
            FileInfo rss_file = Array.Find(temp_dir.GetFiles(), delegate(FileInfo fi) {
                return fi.Name.StartsWith("tot_mus_vie_1_");
            });

            Assert.That(rss_file, Is.Not.Null, "ParsePointRssTest2-2");

            string rss = IJFile.ReadVer2(rss_file.FullName, IJFile.EncodingPriority.Auto);
            List<Video> video_list = new List<Video>();
            NicoListManager.ParsePointRss(rss, DateTime.Now, video_list, true, false);

            Assert.That(video_list.Count, Is.GreaterThanOrEqualTo(1), "ParsePointRssTest2-3");
            Assert.That(video_list[0].title, Is.EqualTo(video_title), "ParsePointRssTest2-4");
        }
    }
}
