using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JsonBool : BaseJsonValue
    {
        bool value;
        public JsonBool(bool val)  { value = val; ActualType = typeof(bool); }

        public override TypeCode GetTypeCode() { return TypeCode.Boolean; }

        public override bool ToBoolean(IFormatProvider provider) { return value; }
    }
}
