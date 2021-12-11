using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NicoTools;
using IJLib;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class NicoUtilTest
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
        public void GetVideoTest1()
        {
            TestUtility.Message("Running GetVideoTest1");

            TestUtility.EnsureLogin(network_);

            Video video = NicoUtil.GetVideo(network_, "so5558738", cancel_object_, msgout_);

            TestUtility.Message(video.description);
            TestUtility.Message(video.length);
            TestUtility.Message(video.submit_date.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            TestUtility.Message(video.tag_set.GetDisplayingTag());
            TestUtility.Message(video.title);
            TestUtility.Message(video.video_id);
            
            Assert.That(video.description, Text.Contains("初音ミクがこんなことを言ってくれたら嬉しいなぁという妄想を形にしてみました。"), "GetVideoTest1-1");
            Assert.That(video.length, Is.EqualTo("0:02"), "GetVideoTesta1-2");
            Assert.That(video.submit_date, Is.EqualTo(new DateTime(2008, 12, 15, 20, 11, 39)), "GetVideoTest1-3");
            Assert.That(video.tag_set.Count, Is.GreaterThan(0), "GetVideoTest1-4");
            Assert.That(video.title, Is.EqualTo("初音ミクからひとこと「オハヨ」"), "GetVideoTest1-5");
            Assert.That(video.video_id, Is.EqualTo("so5558738"), "GetVideoTest1-6");
        }
    }
}
