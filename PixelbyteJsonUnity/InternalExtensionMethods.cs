using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    internal static class InternalExtensionMethods
    {
        internal static bool Contains(this char[] array, char c)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == c) return true;
            return false;
        }

        internal static bool Contains(this char[] array, int c) { if (c < 0) return false; return Contains(array, (char)c); }

        internal static int CountChar(this string text, char c)
        {
            int count = 0;
            int index = 0;
            while (index < text.Length)
                if (text[index++] == c) count++;
            return count;
        }

        internal static bool IsGeneric(this Type type, Type genericType)
        {
            //Traverse the types until we have either a generic type and it is == the genericType
            //or our base type is null
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType) return true;
                type = type.BaseType;
            }
            return false;
        }

        internal static bool IsNullable(this Type type) { return Nullable.GetUnderlyingType(type) != null || !type.IsPrimitive; }
        internal static bool IsNumeric(this Type type) { return IsInteger(type) || IsFloatingPoint(type); }
        internal static bool IsInteger(this Type type)
        {
            if (type.IsEnum) return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    var nullableType = Nullable.GetUnderlyingType(type);
                    return nullableType != null && IsInteger(nullableType);
                default: return false;
            }
        }

        internal static bool IsFloatingPoint(this Type type)
        {
            if (type.IsEnum) return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                case TypeCode.Object:
                    var nullableType = Nullable.GetUnderlyingType(type);
                    return nullableType != null && IsFloatingPoint(nullableType);
                default: return false;
            }
        }

        internal static bool IsValid(this TokenType token, TokenType validTokens)
        {
            return (token & validTokens) > 0;
        }

        internal static FieldInfo FindByName(this FieldInfo[] infos, string fieldName)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                //TOOD: Should it be case insensitive?
                if (fieldName == infos[i].Name) return infos[i];
            }
            return null;
        }
    }
}
