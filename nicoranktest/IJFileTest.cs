using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using NUnit.Framework.SyntaxHelpers;
using IJLib;

namespace nicoranktest
{
    [TestFixture]
    public partial class IJFileTest
    {
        private Encoding shift_jis_;
        private Encoding utf8_;
        private Encoding utf8n_;


        [SetUp]
        public void Setup() {
            shift_jis_ = Encoding.GetEncoding(932, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            utf8_ = new UTF8Encoding(true, true);
            utf8n_ = new UTF8Encoding(false, true);
        }

        private static string WriteAndReadText(string file_path, string original_text, Encoding file_encoding, IJFile.EncodingPriority read_encoding)
        {
            string text;
            using (Stream output = File.Open(file_path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (TextWriter writer = new StreamWriter(output, file_encoding))
            {
                writer.Write(original_text);
            }
            text = IJFile.ReadVer2(file_path, read_encoding);
            return text;
        }

        [Test]
        public void ReadVer2Test1()
        {
            // ファイル：Shift_JIS
            // 読み込み：Auto
            Encoding file_encoding = shift_jis_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.Auto;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test1-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test1-2");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test1-3");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test1-4");
        }

        [Test]
        public void ReadVer2Test2()
        {
            // ファイル：Shift_JIS
            // 読み込み：Shift_JIS
            Encoding file_encoding = shift_jis_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.ShiftJIS;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test2-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test2-2");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test2-3");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test2-4");
        }

        [Test]
        public void ReadVer2Test3()
        {
            // ファイル：Shift_JIS
            // 読み込み：UTF-8
            Encoding file_encoding = shift_jis_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.UTF8;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test3-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test3-2");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test3-3");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test3-4");
        }

        [Test]
        public void ReadVer2Test4()
        {
            // ファイル：UTF-8 BOM有り
            // 読み込み：Auto
            Encoding file_encoding = utf8_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.Auto;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test4-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test4-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test4-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test4-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test4-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test4-6");
        }

        [Test]
        public void ReadVer2Test5()
        {
            // ファイル：UTF-8 BOM有り
            // 読み込み：Shift_JIS
            Encoding file_encoding = utf8_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.ShiftJIS;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test5-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test5-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test5-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test5-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test5-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test5-6");
        }

        [Test]
        public void ReadVer2Test6()
        {
            // ファイル：UTF-8 BOM有り
            // 読み込み：UTF-8
            Encoding file_encoding = utf8_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.UTF8;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test6-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test6-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test6-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test6-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test6-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test6-6");
        }

        [Test]
        public void ReadVer2Test7()
        {
            // ファイル：UTF-8 BOM無し
            // 読み込み：Auto
            Encoding file_encoding = utf8n_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.Auto;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test7-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test7-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test7-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test7-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test7-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test7-6");
        }

        [Test]
        public void ReadVer2Test8()
        {
            // ファイル：UTF-8 BOM無し
            // 読み込み：Shift_JIS
            Encoding file_encoding = utf8n_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.ShiftJIS;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test8-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test8-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test8-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test8-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test8-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test8-6");
        }

        [Test]
        public void ReadVer2Test9()
        {
            // ファイル：UTF-8 BOM無し
            // 読み込み：UTF-8
            Encoding file_encoding = utf8n_;
            IJFile.EncodingPriority read_encoding = IJFile.EncodingPriority.UTF8;

            string temp_directory_path = TestUtility.TestData[TestUtility.KEY_TEMP_DIRECTORY];
            DirectoryInfo temp_directory = new DirectoryInfo(temp_directory_path);
            Assert.That(TestUtility.InitDirectory(temp_directory), Is.True, "ReadVer2Test9-1");
            string file_path = Path.Combine(temp_directory_path, "test.txt");
            string text;
            string original_text;

            original_text = test_text_1;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test9-2");

            original_text = test_text_2;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test9-3");

            original_text = test_text_3;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test9-4");

            original_text = test_text_4;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test9-5");

            original_text = test_text_5;
            text = WriteAndReadText(file_path, original_text, file_encoding, read_encoding);
            Assert.That(text, Is.EqualTo(original_text), "ReadVer2Test9-6");
        }
    }
}
