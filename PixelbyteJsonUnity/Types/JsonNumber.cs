using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JsonNumber : BaseJsonValue
    {
        IConvertible value;
        public JsonNumber(float val)  { value = val;  ActualType = typeof(float); }
        public JsonNumber(decimal val)  { value = val;  ActualType = typeof(decimal); }
        public JsonNumber(double val)  { value = val;  ActualType = typeof(double); }
        public JsonNumber(Int64 val)  { value = val;  ActualType = typeof(Int64); }
        public JsonNumber(UInt64 val)  { value = val;  ActualType = typeof(UInt64); }
        public JsonNumber(Int32 val)  { value = val; ActualType = typeof(Int32); }
        public JsonNumber(UInt32 val)  { value = val;  ActualType = typeof(UInt32); }

        public override TypeCode GetTypeCode() { return Type.GetTypeCode(ActualType); }

        public override byte ToByte(IFormatProvider provider) { return value.ToByte(provider); }

        public override float ToSingle(IFormatProvider provider) { return value.ToSingle(provider); }

        public override double ToDouble(IFormatProvider provider) { return value.ToDouble(provider); }

        public override decimal ToDecimal(IFormatProvider provider) { return value.ToDecimal(provider); }

        public override short ToInt16(IFormatProvider provider) { return value.ToInt16(provider); }

        public override int ToInt32(IFormatProvider provider) { return value.ToInt32(provider); }

        public override long ToInt64(IFormatProvider provider) { return value.ToInt64(provider); }

        public override sbyte ToSByte(IFormatProvider provider) { return value.ToSByte(provider); }

        public override ushort ToUInt16(IFormatProvider provider) { return value.ToUInt16(provider); }

        public override uint ToUInt32(IFormatProvider provider) { return value.ToUInt32(provider); }

        public override ulong ToUInt64(IFormatProvider provider) { return value.ToUInt64(provider); }
    }
}
