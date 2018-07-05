using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.Json
{
    public class JSONTokenizerException : Exception
    {
        public JSONTokenizerException(string message) : base(message) { }
    }

    public class JSONParserException : Exception
    {
        public JSONParserException(string message) : base(message) { }
    }

    public class JSONEncodeException : Exception
    {
        public JSONEncodeException(string message) : base(message) { }
    }

    public class JSONDecodeException : Exception
    {
        public JSONDecodeException(string message) : base(message) { }
    }
}
