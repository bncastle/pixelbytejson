using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    /// <summary>
    /// This class allows us to refer to a single baseclass for all values
    /// The folliwng was used as a refenrence when constructing this library:
    /// https://github.com/pbhogan/TinyJSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseJsonValue : IConvertible
    {
        //At this point, we are only actually using the number conversions so...
        static IFormatProvider numericProvider = new NumberFormatInfo();

        /// <summary>
        /// Returns the underlying type of the object we are storing
        /// </summary>
        public Type ActualType { get; protected set; }

        public abstract TypeCode GetTypeCode();

        public virtual bool ToBoolean(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Boolean", GetTypeCode())); }

        public virtual char ToChar(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Char", GetTypeCode())); }

        public virtual sbyte ToSByte(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to SByte", GetTypeCode())); }

        public virtual byte ToByte(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Byte", GetTypeCode())); }

        public virtual short ToInt16(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Int16", GetTypeCode())); }

        public virtual ushort ToUInt16(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to UInt16", GetTypeCode())); }

        public virtual int ToInt32(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Int32", GetTypeCode())); }

        public virtual uint ToUInt32(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to UInt32", GetTypeCode())); }

        public virtual long ToInt64(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Int64", GetTypeCode())); }

        public virtual ulong ToUInt64(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to UInt64", GetTypeCode())); }

        public virtual float ToSingle(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Single", GetTypeCode())); }

        public virtual double ToDouble(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Double", GetTypeCode())); }

        public virtual decimal ToDecimal(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to Decimal", GetTypeCode())); }

        public virtual DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to DateTime", GetTypeCode())); }

        public virtual string ToString(IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to String", GetTypeCode())); }

        public virtual object ToType(Type conversionType, IFormatProvider provider) { throw new InvalidCastException(string.Format("Can't convert {0} to {1}", GetTypeCode(), conversionType.Name)); }

        public bool ToBoolean() { return ToBoolean(numericProvider); }

        public char ToChar() { return ToChar(numericProvider); }

        public sbyte ToSByte() { return ToSByte(numericProvider); }

        public byte ToByte() { return ToByte(numericProvider); }

        public short ToInt16() { return ToInt16(numericProvider); }

        public ushort ToUInt16( ) { return ToUInt16(numericProvider); }

        public int ToInt32( ) { return ToInt32(numericProvider); }

        public uint ToUInt32() { return ToUInt32(numericProvider); }

        public long ToInt64() {  return ToInt64(numericProvider); } 

        public ulong ToUInt64() {  return ToUInt64(numericProvider); } 

        public float ToSingle() {  return ToSingle(numericProvider); } 

        public double ToDouble() {  return ToDouble(numericProvider); } 

        public decimal ToDecimal() { return ToDecimal(numericProvider); }

        public DateTime ToDateTime() { return ToDateTime(numericProvider); }

        //public static implicit operator String(BaseJsonValue jsonVal) { return jsonVal.ToString(); }
    }
}
