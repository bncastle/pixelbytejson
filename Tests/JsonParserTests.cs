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
        public void JSONParserNullException()
        {
            Assert.Throws<ArgumentNullException>( () => JSONParser.Parse(String.Empty) );
        }

        [Test]
        public void JSONParserNonExistentFile()
        {
            Assert.Throws<FileNotFoundException>( () => JSONParser.ParseFile("nonexistentfile.json"));
        }

        [Test]
        public void NonJSONData()
        {
            var parser = JSONParser.Parse("hfjdsgkds");
            Assert.IsFalse(parser.Tokenizer.Successful);
            Assert.IsFalse(parser.Successful);
        }

        [Test]
        public void EmptyObject()
        {
            var parser = JSONParser.Parse("{}");
            Assert.IsTrue(parser.Tokenizer.Successful);
            Assert.IsTrue(parser.Successful);
        }

        [Test]
        public void BadNumber()
        {
            var parser = JSONParser.Parse(@"{""Number"" : 908,82f}");
            Assert.IsFalse(parser.Tokenizer.Successful);
            Console.WriteLine(parser.Tokenizer.AllErrors);
            Console.WriteLine(parser.AllErrors);
        }
    }
}
