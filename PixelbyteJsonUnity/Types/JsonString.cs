using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JsonString : BaseJsonValue
    {
        string value;
        public JsonString(string val) { value = val; ActualType = typeof(string); }

        public override TypeCode GetTypeCode() { return TypeCode.String; }

        public override string ToString() { return value; }
        public override string ToString(IFormatProvider provider) { return value; }
    }
}
