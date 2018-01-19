using NUnit.Framework;
using Pixelbyte.JsonUnity;
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
        }
    }
}
