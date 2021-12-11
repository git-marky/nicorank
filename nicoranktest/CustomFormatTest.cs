using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NicoTools;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class CustomFormatTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetInputSeparatorTest()
        {
            RankFileCustomFormat format = new RankFileCustomFormat();
            string separator = format.GetInputSeparator();
            Assert.That(separator, Is.EqualTo("\t"), "GetInputSeparatorTest1");

            string input_format = "separator=\"\\t\"\r\n<video_id/>";
            string output_format = input_format;
            format = new RankFileCustomFormat(input_format, output_format);
            separator = format.GetInputSeparator();
            Assert.That(separator, Is.EqualTo("\t"), "GetInputSeparatorTest2");

            input_format = "separator=\",\"\r\n<video_id/>";
            output_format = input_format;
            format = new RankFileCustomFormat(input_format, output_format);
            separator = format.GetInputSeparator();
            Assert.That(separator, Is.EqualTo(","), "GetInputSeparatorTest3");

            input_format = "separator=\"/\"\r\n<video_id/>";
            output_format = input_format;
            format = new RankFileCustomFormat(input_format, output_format);
            separator = format.GetInputSeparator();
            Assert.That(separator, Is.EqualTo("/"), "GetInputSeparatorTest4");

            input_format = "separator=\",\"\r\n<video_id/>";
            output_format = "separator=\"\\t\"\r\n<video_id/>";
            format = new RankFileCustomFormat(input_format, output_format);
            separator = format.GetInputSeparator();
            Assert.That(separator, Is.EqualTo(","), "GetInputSeparatorTest5");
        }

        [Test]
        public void GetVideoTest1()
        {
            string input_format = "separator=\"\\t\"\r\n<video_id/>";
            string output_format = input_format;
            RankFileCustomFormat format = new RankFileCustomFormat(input_format, output_format);
            string video_id = "sm12345";
            Video video = format.GetVideo(video_id);
            Assert.That(video.video_id, Is.EqualTo(video_id), "GetVideoTest1-1");
        }
    }
}
