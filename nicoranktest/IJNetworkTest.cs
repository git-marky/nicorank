using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using IJLib;
using NUnit.Framework.SyntaxHelpers;

namespace nicoranktest
{
    [TestFixture]
    public class IJNetworkTest
    {
        [Test]
        public void ConstructPostDataTest1()
        {
            string data = IJNetwork.ConstructPostData(
                "key");
            Assert.That(data, Is.EqualTo("key="), "ConstructPostDataTest1-1");

            data = IJNetwork.ConstructPostData(
                "key", "value");
            Assert.That(data, Is.EqualTo("key=value"), "ConstructPostDataTest1-2");

            data = IJNetwork.ConstructPostData(
                "key", "val",
                "キー");
            Assert.That(data, Is.EqualTo("key=val&%E3%82%AD%E3%83%BC="), "ConstructPostDataTest1-3");

            data = IJNetwork.ConstructPostData(
                "key", "val",
                "キー", "値");
            Assert.That(data, Is.EqualTo("key=val&%E3%82%AD%E3%83%BC=%E5%80%A4"), "ConstructPostDataTest1-4");

            data = IJNetwork.ConstructPostData(
                "key", "val",
                "キー", "値1 値2");
            Assert.That(data, Is.EqualTo("key=val&%E3%82%AD%E3%83%BC=%E5%80%A41%20%E5%80%A42"), "ConstructPostDataTest1-5");
        }

        [Test]
        public void ConstructPostDataTest2()
        {
            string data = IJNetwork.ConstructPostData("key", "");
            Assert.That(data, Is.EqualTo("key="), "ConstructPostDataTest2-1");

            data = IJNetwork.ConstructPostData("key", null);
            Assert.That(data, Is.EqualTo("key="), "ConstructPostDataTest2-2");

            data = IJNetwork.ConstructPostData("key");
            Assert.That(data, Is.EqualTo("key="), "ConstructPostDataTest2-3");

            data = IJNetwork.ConstructPostData("", "val");
            Assert.That(data, Is.EqualTo("=val"), "ConstructPostDataTest2-4");

            data = IJNetwork.ConstructPostData("", "");
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-5");

            data = IJNetwork.ConstructPostData("", null);
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-6");

            data = IJNetwork.ConstructPostData("");
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-7");

            data = IJNetwork.ConstructPostData(null, "val");
            Assert.That(data, Is.EqualTo("=val"), "ConstructPostDataTest2-8");

            data = IJNetwork.ConstructPostData(null, "");
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-9");

            data = IJNetwork.ConstructPostData(null, null);
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-10");

            data = IJNetwork.ConstructPostData(((string)null));
            Assert.That(data, Is.EqualTo("="), "ConstructPostDataTest2-11");

            data = IJNetwork.ConstructPostData();
            Assert.That(data, Is.EqualTo(""), "ConstructPostDataTest2-12");
        }

        [Test]
        public void ConstructPostDataTest3()
        {
            string data = IJNetwork.ConstructPostData(new string[0]);
            Assert.That(data, Is.EqualTo(""), "ConstructPostDataTest3-1");

            data = IJNetwork.ConstructPostData(((string[])null));
            Assert.That(data, Is.EqualTo(""), "ConstructPostDataTest3-2");
        }
    }
}
