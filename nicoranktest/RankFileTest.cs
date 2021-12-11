using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NicoTools;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class RankFileTest
    {
        [Test]
        public void AddOrOverwriteTest1()
        {
            RankFileCustomFormat custom_format = new RankFileCustomFormat();
            List<Video> video_list = new List<Video>();
            RankFile rank_file = new RankFile(video_list, custom_format);

            string new_video_id = "sm12345";
            string new_video_title = "new_video";
            Video new_video = new Video();
            new_video.video_id = new_video_id;
            new_video.title = new_video_title;
            
            bool overwritten = rank_file.AddOrOverwrite(new_video);
            
            List<Video> rank_video_list = rank_file.GetVideoList();
            Assert.That(overwritten, Is.False, "AddOrOverwriteTest1-1");
            Assert.That(rank_video_list.Count, Is.EqualTo(1), "AddOrOverwriteTest1-2");
            Assert.That(rank_video_list[0].title, Is.EqualTo(new_video_title), "AddOrOverwriteTest1-3");
        }

        [Test]
        public void AddOrOverwriteTest2()
        {
            RankFileCustomFormat custom_format = new RankFileCustomFormat();
            List<Video> video_list = new List<Video>();
            Video video = new Video();
            video.video_id = "sm11111";
            video.title = "video1";
            video_list.Add(video);
            RankFile rank_file = new RankFile(video_list, custom_format);

            string new_video_id = "sm12345";
            string new_video_title = "new_video";
            Video new_video = new Video();
            new_video.video_id = new_video_id;
            new_video.title = new_video_title;

            bool overwritten = rank_file.AddOrOverwrite(new_video);

            List<Video> rank_video_list = rank_file.GetVideoList();
            Assert.That(overwritten, Is.False, "AddOrOverwriteTest2-1");
            Assert.That(rank_video_list.Count, Is.EqualTo(2), "AddOrOverwriteTest2-2");
            Assert.That(rank_video_list[0].title, Is.EqualTo(video.title), "AddOrOverwriteTest2-3");
            Assert.That(rank_video_list[1].title, Is.EqualTo(new_video_title), "AddOrOverwriteTest2-3");
        }

        [Test]
        public void AddOrOverwriteTest3()
        {
            RankFileCustomFormat custom_format = new RankFileCustomFormat();
            List<Video> video_list = new List<Video>();
            Video video1 = new Video();
            video1.video_id = "sm12345";
            video1.title = "video1";
            video_list.Add(video1);
            RankFile rank_file = new RankFile(video_list, custom_format);

            string new_video_id = "sm12345";
            string new_video_title = "new_video";
            Video new_video = new Video();
            new_video.video_id = new_video_id;
            new_video.title = new_video_title;

            bool overwritten = rank_file.AddOrOverwrite(new_video);

            List<Video> rank_video_list = rank_file.GetVideoList();
            Assert.That(overwritten, Is.True, "AddOrOverwriteTest3-1");
            Assert.That(rank_video_list.Count, Is.EqualTo(1), "AddOrOverwriteTest3-2");
            Assert.That(rank_video_list[0].title, Is.EqualTo(new_video_title), "AddOrOverwriteTest3-3");
        }

        [Test]
        public void AddOrOverwriteTest4()
        {
            RankFileCustomFormat custom_format = new RankFileCustomFormat();
            List<Video> video_list = new List<Video>();
            Video video1 = new Video();
            video1.video_id = "sm11111";
            video1.title = "video1";
            video_list.Add(video1);
            Video video2 = new Video();
            video2.video_id = "sm12345";
            video2.title = "video2";
            video_list.Add(video2);
            Video video3 = new Video();
            video3.video_id = "sm12345";
            video3.title = "video3";
            video_list.Add(video3);
            RankFile rank_file = new RankFile(video_list, custom_format);

            string new_video_id = "sm12345";
            string new_video_title = "new_video";
            Video new_video = new Video();
            new_video.video_id = new_video_id;
            new_video.title = new_video_title;

            bool overwritten = rank_file.AddOrOverwrite(new_video);

            List<Video> rank_video_list = rank_file.GetVideoList();
            Assert.That(overwritten, Is.True, "AddOrOverwriteTest4-1");
            Assert.That(rank_video_list.Count, Is.EqualTo(2), "AddOrOverwriteTest4-2");
            Assert.That(rank_video_list[0].title, Is.EqualTo(video1.title), "AddOrOverwriteTest4-3");
            Assert.That(rank_video_list[1].title, Is.EqualTo(new_video_title), "AddOrOverwriteTest4-4");
        }
    }
}
