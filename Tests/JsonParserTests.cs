using NUnit.Framework;
using Pixelbyte.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestFixture]
    public class JsonParserTests
    {
        [Test]
        public void JSONParserEmptyException()
        {
            Assert.Throws<ArgumentException>(() => JsonParser.Parse(String.Empty));
        }

        [Test]
        public void JSONParserNonExistentFile()
        {
            Assert.Throws<FileNotFoundException>(() => JsonParser.ParseFile("nonexistentfile.json"));
        }

        [Test]
        public void NonJSONData()
        {
            Assert.Throws<JSONParserException>(() => JsonParser.Parse("hfjdsgkds"));
        }

        [Test]
        public void EmptyObject()
        {
            var parser = JsonParser.Parse("{}");
            //Assert.IsTrue(parser.Tokenizer.Successful);
            //Assert.IsTrue(parser.Successful);
        }

        [Test]
        public void BadNumber()
        {
            Assert.Throws<JSONParserException>( () => JsonParser.Parse(@"{""Number"" : 908,82f}"));
        }

        [Test]
        public void EncodeDecodeCallbacks()
        {
            TestCallbacks tc = new TestCallbacks("Black", 23);
            string json = JsonEncoder.Encode(tc);
            Assert.IsTrue(tc.PreEncodedMethodCalled);
            Assert.IsTrue(tc.EncodedMethodCalled);

            var decoded = JsonDecoder.Decode<TestCallbacks>(json);
            Assert.IsTrue(decoded.DecodedMethodCalled);
        }
    }
    class TestCallbacks : IJsonDecoded, IJsonPreEncode, IJsonEncoded
    {
        string name;
        int age;

        public bool EncodedMethodCalled { get; private set; }
        public bool PreEncodedMethodCalled { get; private set; }
        public bool DecodedMethodCalled { get; private set; }

        public TestCallbacks() { }

        public TestCallbacks(string name, int age)
        {
            this.name = name;
            this.age = age;
        }

        void IJsonDecoded.OnJsonDecoded() => DecodedMethodCalled = true;

        void IJsonPreEncode.OnJsonPreEncode() => PreEncodedMethodCalled = true;

        void IJsonEncoded.OnJsonEncoded() => EncodedMethodCalled = true;
    }
}
