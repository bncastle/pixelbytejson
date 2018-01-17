using System;
using System.IO;
using System.Reflection;

namespace Pixelbyte.JsonUnity
{

    // Define other methods and classes here
    static class Seroz
    {
        static bool IsPublic(FieldInfo field) { return field.IsPublic; }
        static bool IsPrivate(FieldInfo field) { return field.IsPrivate; }

        static bool HasAttribute<T>(FieldInfo fi) where T : class
        {
            return fi.GetCustomAttributes(typeof(T), false).Length > 0;
        }

        static T GetAttrbute<T>(FieldInfo fi) where T : class
        {
            var attrs = fi.GetCustomAttributes(typeof(T), false);
            if (attrs.Length == 0) return null;
            else return attrs[0] as T;
        }

        //	public static string Ser(object obj)
        public static void Ser(object obj)
        {
            var fi = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            //		var callback = obj as IDeserializeCallbacks;
            var callbacks = obj as ISerializeCallbacks;

            if (callbacks != null) callbacks.PreSerialization();

            foreach (var fieldInfo in fi)
            {
                //If the field is private or protected we need to check and see if it has an attribute that allows us to include it
                if (fieldInfo.IsPrivate || fieldInfo.IsFamily)
                {
                    if (!HasAttribute<SerializeField>(fieldInfo)) continue;
                }

                object value = fieldInfo.GetValue(obj);

                string stringVal = String.Empty;
                if (fieldInfo.FieldType == typeof(int))
                {
                    stringVal = value.ToString();
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    var attr = GetAttrbute<DecimalPlaces>(fieldInfo);
                    if (attr != null) stringVal = attr.Convert((float)value);
                }
                else if (fieldInfo.FieldType == typeof(double))
                {
                    var attr = GetAttrbute<DecimalPlaces>(fieldInfo);
                    if (attr != null) stringVal = attr.Convert((double)value);
                }
                else if (fieldInfo.FieldType == typeof(decimal))
                {
                    var attr = GetAttrbute<DecimalPlaces>(fieldInfo);
                    if (attr != null) stringVal = attr.Convert((decimal)value);
                }
                else if (value != null)
                {
                    stringVal = value.ToString();
                }
                Console.WriteLine("{0} = {1}", fieldInfo.Name, stringVal);
            }

            if (callbacks != null) callbacks.PostSerialization();
        }

        static bool IsGeneric(this Type type, Type genericType)
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

        static bool IsNullable(this Type type) { return Nullable.GetUnderlyingType(type) != null || !type.IsPrimitive; }
        static bool IsNumeric(this Type type) { return IsInteger(type) || IsFloatingPoint(type); }
        static bool IsInteger(this Type type)
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

        static bool IsFloatingPoint(this Type type)
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
    }


    enum TokenType
    {
        //Single-Character Tokens
        OpenCurly, ClosedCurly, OpenBracket, CloseBracket, Colon, Comma,

        Digit, DoubleQuote,

        //Value types
        String, Number, Object, Array,
        True, False, Null,

        None
    }
}
