using System;
using System.Collections.Generic;
using System.Text;
using NicoTools;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class FilterParserTest
    {
        private FilterParser parser_;

        [SetUp]
        public void Setup()
        {
            parser_ = new FilterParser();
        }
        
        [Test]
        public void CommentTest()
        {
            string expression;
            Video video = new Video();
            video.title = "test";
            IVideoFilter filter;

            #region (1) # comment

            expression = "#comment\r\ntitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-1");

            expression = "#comment\ntitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-2");

            expression = "#comment\rtitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-3");

            expression = "title:test\r\n#comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-4");

            expression = "title:test\n#comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-5");

            expression = "title:test\r#comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-6");

            expression = "#comment1\r\ntitle:test\n#comment2";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-7");

            expression = "#\r\ntitle:test1\r\n#\r\nor title:test\r\n#";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_1-8");

            #endregion

            #region (2) // comment

            expression = "//comment\r\ntitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-1");

            expression = "// comment\ntitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-2");

            expression = "//comment\rtitle:test";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-3");

            expression = "title:test\r\n//comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-4");

            expression = "title:test\n// comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-5");

            expression = "title:test\r//comment";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-6");

            expression = "//comment1\r\ntitle:test\n//comment2";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-7");


            expression = "//\r\ntitle:test1\r\n//\r\nor title:test\r\n//";
            filter = parser_.Parse(expression);
            Assert.That(filter.IsThrough(video), Is.True, "CommentTest_2-8");
            #endregion
        }

        [Test]
        public void ParseVideoIdFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) plain

            expression = "id:sm1234";
            filter = parser_.Parse(expression);

            video.video_id = "sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_1-1");

            video.video_id = "sm4321";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdFilterTest_1-2");

            video.video_id = "sm12345";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdFilterTest_1-3");

            #endregion

            #region (2) regex

            expression = "id@r:nm\\d+";
            filter = parser_.Parse(expression);

            video.video_id = "nm12345";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_2-1");

            video.video_id = "co12345";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdFilterTest_2-2");

            #endregion

            #region (3) ignore case

            expression = "id@i:sm1234";
            filter = parser_.Parse(expression);

            video.video_id = "sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_3-1");

            video.video_id = "Sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_3-2");

            #endregion

            #region (4) case sensitive

            expression = "id@:sm1234";
            filter = parser_.Parse(expression);

            video.video_id = "sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_4-1");

            video.video_id = "Sm1234";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdFilterTest_4-2");

            #endregion

            #region (5) match all

            expression = "id@a:sm1234";
            filter = parser_.Parse(expression);

            video.video_id = "sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_5-1");

            video.video_id = "sm12345";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdFilterTest_5-2");

            #endregion

            #region (6) match partial

            expression = "id@:sm1234";
            filter = parser_.Parse(expression);

            video.video_id = "sm1234";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_6-1");

            video.video_id = "ssm12345";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdFilterTest_6-2");

            #endregion

        }

        [Test]
        public void ParseVideoTitleFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) plain

            expression = "title:テスト";
            filter = parser_.Parse(expression);

            video.title = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_1-1");

            video.title = "試験";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_1-2");

            video.title = "試験テスト試験";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_1-3");

            video.title = "ﾃｽﾄ";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_1-4");

            #endregion

            #region (2) regex

            expression = "title@r:\"^(?:テスト|試験)$\"";
            filter = parser_.Parse(expression);

            video.title = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_2-1");

            video.title = "試験";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_2-2");

            video.title = "ニコニコ";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_2-3");

            #endregion

            #region (3) ignore case

            expression = "title@i:abCdeア";
            filter = parser_.Parse(expression);

            video.title = "abCdeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_3-1");

            video.title = "abcDeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_3-2");

            video.title = "abCdeｱ";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_3-3");

            #endregion

            #region (4) case sensitive

            expression = "title@:abCdeア";
            filter = parser_.Parse(expression);

            video.title = "abCdeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_4-1");

            video.title = "abcDeア";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_4-2");

            video.title = "abCdeｱ";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_4-3");

            #endregion

            #region (5) match all

            expression = "title@a:テスト";
            filter = parser_.Parse(expression);

            video.title = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_5-1");

            video.title = "試験テスト";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_5-2");

            #endregion

            #region (6) match partial

            expression = "title@:テスト";
            filter = parser_.Parse(expression);

            video.title = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_6-1");

            video.title = "試験テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_6-2");

            #endregion

            #region (7) full width white space

            expression = "title:テスト　ビデオ";
            filter = parser_.Parse(expression);

            video.title = "テスト";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_7-1");

            video.title = "ビデオ";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleFilterTest_7-2");

            video.title = "テスト　ビデオ";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleFilterTest_7-3");

            #endregion

        }

        [Test]
        public void ParseVideoDescriptionFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) plain

            expression = "description:テスト";
            filter = parser_.Parse(expression);

            video.description = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_1-1");

            video.description = "試験";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoDescriptionFilterTest_1-2");

            video.description = "試験テスト試験";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_1-3");

            video.description = "ﾃｽﾄ";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_1-4");

            #endregion

            #region (2) regex

            expression = "description@r:\"^(?:テスト|試験)$\"";
            filter = parser_.Parse(expression);

            video.description = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_2-1");

            video.description = "試験";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_2-2");

            video.description = "ニコニコ";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoDescriptionFilterTest_2-3");

            #endregion

            #region (3) ignore case

            expression = "description@i:abCdeア";
            filter = parser_.Parse(expression);

            video.description = "abCdeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_3-1");

            video.description = "abcDeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_3-2");

            video.description = "abCdeｱ";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_3-3");

            #endregion

            #region (4) case sensitive

            expression = "description@:abCdeア";
            filter = parser_.Parse(expression);

            video.description = "abCdeア";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_4-1");

            video.description = "abcDeア";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoDescriptionFilterTest_4-2");

            video.description = "abCdeｱ";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoDescriptionFilterTest_4-3");

            #endregion

            #region (5) match all

            expression = "description@a:テスト";
            filter = parser_.Parse(expression);

            video.description = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_5-1");

            video.description = "試験テスト";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoDescriptionFilterTest_5-2");

            #endregion

            #region (6) match partial

            expression = "description@:テスト";
            filter = parser_.Parse(expression);

            video.description = "テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_6-1");

            video.description = "試験テスト";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoDescriptionFilterTest_6-2");

            #endregion
        }

        [Test]
        public void ParseVideoSubmitDateFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            TimeSpan one_tick = new TimeSpan(1);

            #region (1) yyyy-MM-ddTHH:mm:dd,yyyy-MM-ddTHH:mm:dd

            expression = "submit:2009-01-01T00:00:00,2009-01-03T00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_1-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_1-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_1-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_1-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_1-5");

            #endregion

            #region (2) yyyy-MM-ddTHH:mm:dd,

            expression = "submit:2009-01-01T00:00:00,";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_2-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_2-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_2-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_2-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_2-5");

            #endregion

            #region (3) ,yyyy-MM-ddTHH:mm:dd

            expression = "submit:,2009-01-03T00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_3-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_3-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_3-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_3-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_3-5");

            #endregion

            #region (4) yyyy-MM-ddTHH:mm:dd,yyyy-MM-ddTHH:mm:dd (<max>,<min>)

            expression = "submit:2009-01-03T00:00:00,2009-01-01T00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_4-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_4-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_4-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_4-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_4-5");

            #endregion

            #region (5) yyyy/MM/dd HH:mm:dd, yyyy/MM/dd HH:mm:dd

            expression = "submit:\"2009/01/01 00:00:00, 2009/01/03 00:00:00\"";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_5-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_5-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_5-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_5-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_5-5");

            #endregion

            #region (6) yyyy/MM/dd HH:mm:dd,

            expression = "submit:\"2009/01/01 00:00:00,\"";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_6-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_6-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_6-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_6-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_6-5");

            #endregion

            #region (7) , yyyy/MM/dd HH:mm:dd

            expression = "submit:\", 2009/01/03 00:00:00\"";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_7-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_7-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_7-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_7-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_7-5");

            #endregion

            #region (8) yyyy/MM/dd HH:mm:dd, yyyy/MM/dd HH:mm:dd (<max>,<min>)

            expression = "submit:\"2009/01/03 00:00:00, 2009/01/01 00:00:00\"";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_8-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_8-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_8-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_8-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_8-5");

            #endregion

            #region (9) yyyy/MM/dd HH:mm:dd, yyyy/MM/dd HH:mm:dd without quote

            expression = "submit:2009/01/01 00:00:00, 2009/01/03 00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_9-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_9-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_9-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_9-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_9-5");

            #endregion

            #region (10) yyyy/MM/dd HH:mm:dd, without quote

            expression = "submit:2009/01/01 00:00:00,";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_10-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_10-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_10-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_10-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_10-5");

            #endregion

            #region (11) , yyyy/MM/dd HH:mm:dd without quote

            expression = "submit:, 2009/01/03 00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_11-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_11-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_11-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_11-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_11-5");

            #endregion

            #region (12) yyyy/MM/dd HH:mm:dd, yyyy/MM/dd HH:mm:dd (<max>,<min>) without quote

            expression = "submit:2009/01/03 00:00:00, 2009/01/01 00:00:00";
            filter = parser_.Parse(expression);

            video.submit_date = new DateTime(2009, 1, 2);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_12-1");

            video.submit_date = new DateTime(2009, 1, 1);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_12-2");

            video.submit_date = new DateTime(2009, 1, 3);
            Assert.That(filter.IsThrough(video), Is.True, "ParseSubmitDateFilter_12-3");

            video.submit_date = new DateTime(2009, 1, 1) - one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_12-4");

            video.submit_date = new DateTime(2009, 1, 3) + one_tick;
            Assert.That(filter.IsThrough(video), Is.False, "ParseSubmitDateFilter_12-5");

            #endregion

        }

        [Test]
        public void ParseVideoViewFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) N,N

            expression = "view:100,300";
            filter = parser_.Parse(expression);

            video.point.view = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_1-1");

            video.point.view = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_1-2");

            video.point.view = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_1-3");

            video.point.view = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_1-4");

            video.point.view = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_1-5");

            #endregion

            #region (2) N,

            expression = "view:100,";
            filter = parser_.Parse(expression);

            video.point.view = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_2-1");

            video.point.view = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_2-2");

            video.point.view = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_2-3");

            video.point.view = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_2-4");

            video.point.view = 301;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_2-5");

            #endregion

            #region (3) ,N

            expression = "view:,300";
            filter = parser_.Parse(expression);

            video.point.view = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_3-1");

            video.point.view = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_3-2");

            video.point.view = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_3-3");

            video.point.view = 99;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_3-4");

            video.point.view = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_3-5");

            #endregion

            #region (4) N,N (<max>,<min>)

            expression = "view:300,100";
            filter = parser_.Parse(expression);

            video.point.view = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_4-1");

            video.point.view = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_4-2");

            video.point.view = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoViewFilterTest_4-3");

            video.point.view = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_4-4");

            video.point.view = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoViewFilterTest_4-5");

            #endregion
        }

        [Test]
        public void ParseVideoCommentFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) N,N

            expression = "comment:100,300";
            filter = parser_.Parse(expression);

            video.point.res = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_1-1");

            video.point.res = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_1-2");

            video.point.res = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_1-3");

            video.point.res = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_1-4");

            video.point.res = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_1-5");

            #endregion

            #region (2) N,

            expression = "comment:100,";
            filter = parser_.Parse(expression);

            video.point.res = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_2-1");

            video.point.res = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_2-2");

            video.point.res = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_2-3");

            video.point.res = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_2-4");

            video.point.res = 301;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_2-5");

            #endregion

            #region (3) ,N

            expression = "comment:,300";
            filter = parser_.Parse(expression);

            video.point.res = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_3-1");

            video.point.res = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_3-2");

            video.point.res = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_3-3");

            video.point.res = 99;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_3-4");

            video.point.res = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_3-5");

            #endregion

            #region (4) N,N (<max>,<min>)

            expression = "comment:300,100";
            filter = parser_.Parse(expression);

            video.point.res = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_4-1");

            video.point.res = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_4-2");

            video.point.res = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoCommentFilterTest_4-3");

            video.point.res = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_4-4");

            video.point.res = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoCommentFilterTest_4-5");

            #endregion
        }

        [Test]
        public void ParseVideoMylistFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) N,N

            expression = "mylist:100,300";
            filter = parser_.Parse(expression);

            video.point.mylist = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_1-1");

            video.point.mylist = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_1-2");

            video.point.mylist = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_1-3");

            video.point.mylist = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_1-4");

            video.point.mylist = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_1-5");

            #endregion

            #region (2) N,

            expression = "mylist:100,";
            filter = parser_.Parse(expression);

            video.point.mylist = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_2-1");

            video.point.mylist = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_2-2");

            video.point.mylist = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_2-3");

            video.point.mylist = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_2-4");

            video.point.mylist = 301;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_2-5");

            #endregion

            #region (3) ,N

            expression = "mylist:,300";
            filter = parser_.Parse(expression);

            video.point.mylist = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_3-1");

            video.point.mylist = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_3-2");

            video.point.mylist = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_3-3");

            video.point.mylist = 99;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_3-4");

            video.point.mylist = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_3-5");

            #endregion

            #region (4) N,N (<max>,<min>)

            expression = "mylist:300,100";
            filter = parser_.Parse(expression);

            video.point.mylist = 200;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_4-1");

            video.point.mylist = 100;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_4-2");

            video.point.mylist = 300;
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoMylistFilterTest_4-3");

            video.point.mylist = 99;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_4-4");

            video.point.mylist = 301;
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoMylistFilterTest_4-5");

            #endregion
        }

        [Test]
        public void ParseVideoPNameFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) plain

            expression = "pname:test";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_1-1");

            video.pname = "text";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoPNameFilterTest_1-2");

            video.pname = "testx";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoPNameFilterTest_1-3");

            #endregion

            #region (2) regex

            expression = "pname@r:te..";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_2-1");

            video.pname = "xest";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoPNameFilterTest_2-2");

            #endregion

            #region (3) ignore case

            expression = "pname@i:test";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_3-1");

            video.pname = "Test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_3-2");

            #endregion

            #region (4) case sensitive

            expression = "pname@:test";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_4-1");

            video.pname = "Test";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoPNameFilterTest_4-2");

            #endregion

            #region (5) match all

            expression = "pname@a:test";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_5-1");

            video.pname = "xtestx";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoPNameFilterTest_5-2");

            #endregion

            #region (6) match partial

            expression = "pname@:test";
            filter = parser_.Parse(expression);

            video.pname = "test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_6-1");

            video.pname = "xtestx";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoPNameFilterTest_6-2");

            #endregion
        }

        [Test]
        public void ParseVideoLengthFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) mm:ss,mm:ss

            expression = "length:30:00,90:00";
            filter = parser_.Parse(expression);

            video.length = "60:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_1-1");

            video.length = "30:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_1-2");

            video.length = "90:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_1-3");

            video.length = "29:59";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_1-4");

            video.length = "90:01";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_1-5");

            #endregion

            #region (2) mm:ss,

            expression = "length:30:00,";
            filter = parser_.Parse(expression);

            video.length = "60:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_2-1");

            video.length = "30:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_2-2");

            video.length = "90:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_2-3");

            video.length = "29:59";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_2-4");

            video.length = "90:01";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_2-5");

            #endregion

            #region (3) ,mm:ss

            expression = "length:,90:00";
            filter = parser_.Parse(expression);

            video.length = "60:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_3-1");

            video.length = "30:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_3-2");

            video.length = "90:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_3-3");

            video.length = "29:59";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_3-4");

            video.length = "90:01";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_3-5");

            #endregion

            #region (4) mm:ss,mm:ss (<max>,<min>)

            expression = "length:90:00,30:00";
            filter = parser_.Parse(expression);

            video.length = "60:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_4-1");

            video.length = "30:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_4-2");

            video.length = "90:00";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoLengthFilterTest_4-3");

            video.length = "29:59";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_4-4");

            video.length = "90:01";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoLengthFilterTest_4-5");

            #endregion
        }

        [Test]
        public void ParseVideoTagFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video;

            #region (1) plain

            expression = "tag:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("テスト", false);
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_1-1");

            video = new Video();
            video.tag_set.Add("テスト", false);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_1-2");

            video = new Video();
            video.tag_set.Add("テストtestテスト", false);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_1-3");

            video = new Video();
            video.tag_set.Add("Test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_1-4");

            #endregion

            #region (2) regex

            expression = "tag@r:te[sx]t";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_2-1");

            video = new Video();
            video.tag_set.Add("text", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_2-2");

            video = new Video();
            video.tag_set.Add("te0t", false);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_2-3");

            #endregion

            #region (3) ignore case

            expression = "tag@i:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_3-1");

            video = new Video();
            video.tag_set.Add("Test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_3-2");

            #endregion

            #region (4) match all

            expression = "tag@a:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_4-1");

            video = new Video();
            video.tag_set.Add("testx", false);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_4-2");

            #endregion

            #region (5) match partial

            expression = "tag@:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_5-1");

            video = new Video();
            video.tag_set.Add("xtestx", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_5-2");

            #endregion

            #region (6) locked, unlocked

            expression = "ltag:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", true);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_6-1");

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_6-2");

            expression = "utag:test";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("test", true);
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTagFilterTest_6-3");

            video = new Video();
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTagFilterTest_6-4");

            #endregion
        }

        [Test]
        public void ParseVideoIdListFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region and list

            expression = "idlist:(\r\nsm0001\r\n)";
            filter = parser_.Parse(expression);

            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_1-1");

            video.video_id = "Sm0001";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_1-2");

            #endregion

            #region or list

            expression = "idlist:{\r\nsm0001\r\nsm0002\r\n}";
            filter = parser_.Parse(expression);
            
            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_2-1");
            
            video.video_id = "sm0002";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_2-2");

            video.video_id = "sm0003";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_2-3");

            video.video_id = "Sm0001";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_2-4");

            video.video_id = "sm00011";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_2-5");

            #endregion

            #region and list ignore case

            expression = "idlist@ia:(\r\nsm0001\r\n)";
            filter = parser_.Parse(expression);

            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_3-1");

            video.video_id = "Sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_3-2");

            #endregion

            #region or list ignore case

            expression = "idlist@ia:{\r\nsm0001\r\nsM0002\r\n}";
            filter = parser_.Parse(expression);

            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_4-1");

            video.video_id = "sm0002";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_4-2");

            video.video_id = "sm0003";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_4-3");

            video.video_id = "Sm0002";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_4-4");

            #endregion

            #region and list match partial

            expression = "idlist@:(\nsm0001\n)\n";
            filter = parser_.Parse(expression);

            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_5-1");

            video.video_id = "sm00011";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_5-2");

            #endregion

            #region or list match partial

            expression = "idlist@:{\nsm0001\nsm0002\n}\n";
            filter = parser_.Parse(expression);

            video.video_id = "sm00011";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_6-1");

            video.video_id = "sm00021";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_6-2");

            #endregion

            #region and list regex

            expression = "idlist@r:(\rsm\\d3\rsm2[34]\r)\r";
            filter = parser_.Parse(expression);

            video.video_id = "sm23";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_7-1");

            video.video_id = "sm13";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_7-2");

            video.video_id = "sm24";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoIdListFilterTest_7-3");

            #endregion

            #region or list regex

            expression = "idlist@r:{\r\nsm\\d3\r\nsm2[34]\r\n}";
            filter = parser_.Parse(expression);

            video.video_id = "sm23";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_8-1");

            video.video_id = "sm13";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_8-2");

            video.video_id = "sm24";

            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoIdListFilterTest_8-3");

            #endregion
        }

        [Test]
        public void ParseVideoTitleListFilterTest()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region (1) and list

            expression = "titlelist:(\ntest\nテスト\n)";
            filter = parser_.Parse(expression);

            video.title = "テスト試験test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleListFilterTest_1-1");

            video.title = "テスト試験text";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleListFilterTest_1-2");
            
            #endregion

            #region (2) or list

            expression = "titlelist:{\ntest\nテスト\n}";
            filter = parser_.Parse(expression);

            video.title = "テスト試験test";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleListFilterTest_2-1");

            video.title = "テスト試験text";
            Assert.That(filter.IsThrough(video), Is.True, "ParseVideoTitleListFilterTest_2-2");

            video.title = "テス試験text";
            Assert.That(filter.IsThrough(video), Is.False, "ParseVideoTitleListFilterTest_2-3");

            #endregion
        }

        [Test]
        public void ParseComplexFilterTest_1()
        {
            string expression;
            IVideoFilter filter;
            Video video;

            expression = "-(tag:音楽 and tag:東方)";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("東方", false);

            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_1-1");

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("VOCALOID", false);

            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-2");

            video = new Video();
            video.tag_set.Add("ゲーム", false);
            video.tag_set.Add("東方", false);

            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-3");


            // (音楽 -VOCALOID) or ボカロオリジナルを歌ってみた or VOCALOIDと歌ってみた
            expression = "(tag:音楽 and -tag:VOCALOID) or tag:ボカロオリジナルを歌ってみた or tag:VOCALOIDと歌ってみた";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("VOCALOID", false);
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_1-4");

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("test", false);
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-5");

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("VOCALOID", false);
            video.tag_set.Add("ボカロオリジナルを歌ってみた", false);
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-6");

            video = new Video();
            video.tag_set.Add("音楽", false);
            video.tag_set.Add("VOCALOID", false);
            video.tag_set.Add("VOCALOIDと歌ってみた", false);
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-7");

            video = new Video();
            video.tag_set.Add("VOCALOID", false);
            video.tag_set.Add("VOCALOIDと歌ってみた", false);
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_1-8");
        }

        [Test]
        public void ParseComplexFilterTest_2()
        {
            string expression;
            IVideoFilter filter;
            Video video = new Video();

            #region long expression

            expression = @"
-idlist:{
sm0001
nm0002
sm0003
nm0004
sm0005
nm0006
sm0007
nm0008
sm0009
nm0010
sm0011
nm0012
sm0013
nm0014
sm0015
nm0016
sm0017
nm0018
sm0019
nm0020
sm0021
nm0022
sm0023
nm0024
sm0025
nm0026
sm0027
nm0028
sm0029
nm0030
sm0031
nm0032
sm0033
nm0034
sm0035
nm0036
sm0037
nm0038
sm0039
nm0040
sm0041
nm0042
sm0043
nm0044
sm0045
nm0046
sm0047
nm0048
sm0049
nm0050
sm0051
nm0052
sm0053
nm0054
sm0055
nm0056
sm0057
nm0058
sm0059
nm0060
sm0061
nm0062
sm0063
nm0064
sm0065
nm0066
sm0067
nm0068
sm0069
nm0070
sm0071
nm0072
sm0073
nm0074
sm0075
nm0076
sm0077
nm0078
sm0079
nm0080
sm0081
nm0082
sm0083
nm0084
sm0085
nm0086
sm0087
nm0088
sm0089
nm0090
sm0091
nm0092
sm0093
nm0094
sm0095
nm0096
sm0097
nm0098
sm0099
nm0100
}
";

            #endregion
            filter = parser_.Parse(expression);

            #region sm videos

            video.video_id = "sm0001";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0002";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0003";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0004";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0005";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0006";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0007";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0008";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0009";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0010";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0011";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0012";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0013";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0014";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0015";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0016";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0017";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0018";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0019";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0020";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0021";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0022";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0023";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0024";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0025";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0026";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0027";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0028";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0029";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0030";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0031";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0032";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0033";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0034";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0035";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0036";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0037";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0038";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0039";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0040";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0041";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0042";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0043";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0044";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0045";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0046";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0047";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0048";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0049";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0050";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0051";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0052";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0053";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0054";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0055";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0056";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0057";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0058";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0059";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0060";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0061";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0062";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0063";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0064";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0065";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0066";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0067";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0068";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0069";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0070";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0071";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0072";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0073";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0074";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0075";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0076";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0077";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0078";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0079";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0080";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0081";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0082";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0083";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0084";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0085";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0086";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0087";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0088";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0089";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0090";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0091";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0092";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0093";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0094";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0095";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0096";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0097";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0098";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "sm0099";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "sm0100";
            Assert.That(filter.IsThrough(video), Is.True);

            #endregion

            #region nm videos

            video.video_id = "nm0001";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0002";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0003";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0004";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0005";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0006";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0007";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0008";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0009";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0010";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0011";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0012";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0013";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0014";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0015";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0016";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0017";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0018";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0019";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0020";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0021";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0022";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0023";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0024";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0025";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0026";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0027";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0028";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0029";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0030";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0031";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0032";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0033";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0034";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0035";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0036";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0037";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0038";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0039";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0040";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0041";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0042";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0043";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0044";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0045";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0046";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0047";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0048";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0049";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0050";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0051";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0052";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0053";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0054";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0055";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0056";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0057";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0058";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0059";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0060";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0061";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0062";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0063";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0064";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0065";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0066";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0067";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0068";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0069";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0070";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0071";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0072";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0073";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0074";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0075";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0076";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0077";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0078";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0079";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0080";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0081";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0082";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0083";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0084";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0085";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0086";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0087";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0088";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0089";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0090";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0091";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0092";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0093";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0094";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0095";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0096";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0097";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0098";
            Assert.That(filter.IsThrough(video), Is.False);
            video.video_id = "nm0099";
            Assert.That(filter.IsThrough(video), Is.True);
            video.video_id = "nm0100";
            Assert.That(filter.IsThrough(video), Is.False);

            #endregion
        }

        [Test]
        public void ParseComplexFilterTest_3()
        {
            string expression;
            IVideoFilter filter;
            Video video;

            expression = "-idlist:{\r\nsm1\r\nsm2\r\nsm3\r\n}\r\n\r\nand\r\n\r\ntaglist:{\r\ntag1\r\ntag2\r\ntag3\r\n}";
            filter = parser_.Parse(expression);

            video = new Video();
            video.video_id = "sm3";
            video.tag_set.Add("tag4", false);
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-1");

            video = new Video();
            video.video_id = "sm4";
            video.tag_set.Add("tag4", false);
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-2");

            video = new Video();
            video.video_id = "sm3";
            video.tag_set.Add("tag3", false);
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-3");

            video = new Video();
            video.video_id = "sm4";
            video.tag_set.Add("tag3", false);
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_3-4");

            expression = "taglist:(\r\ntag1\r\ntag2\r\n)\r\n and titlelist:(\r\ntitle1\r\ntitle2\r\n)";
            filter = parser_.Parse(expression);

            video = new Video();
            video.tag_set.Add("tag1", false);
            video.tag_set.Add("tag3", false);
            video.title = "title1 title3";
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-5");

            video = new Video();
            video.tag_set.Add("tag1", false);
            video.tag_set.Add("tag2", false);
            video.title = "title1 title3";
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-5");

            video = new Video();
            video.tag_set.Add("tag1", false);
            video.tag_set.Add("tag3", false);
            video.title = "title1 title2";
            Assert.That(filter.IsThrough(video), Is.False, "ComplexFilterTest_3-5");

            video = new Video();
            video.tag_set.Add("tag1", false);
            video.tag_set.Add("tag2", false);
            video.title = "title1 title2";
            Assert.That(filter.IsThrough(video), Is.True, "ComplexFilterTest_3-5");
        }
    }
}
