using System;
using System.Reflection;

namespace Pixelbyte.Json
{
    public static class ExtensionMethods
    {
        internal static bool Contains(this char[] array, char c)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == c) return true;
            return false;
        }

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

        internal static bool HasInterface(this Type type, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(type);
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

        public static bool HasAttribute<T>(this FieldInfo fi) where T : Attribute
        {
            return fi.GetCustomAttributes(typeof(T), false).Length > 0;
        }

        public static bool HasAttribute<T>(this Type type, bool inherit) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Length > 0;
        }

        public static T GetFirstAttribute<T>(this FieldInfo fi) where T : Attribute
        {
            var attrs = fi.GetCustomAttributes(typeof(T), false);
            if (attrs.Length == 0) return null;
            else return attrs[0] as T;
        }

        //public static void EnumerateFields(this Type type, Action<FieldInfo,string> fieldOperation)
        //{
        //    type.EnumerateFields(JsonEncoder.DEFAULT_JSON_BINDING_FLAGS, fieldOperation);
        //}

        public static void EnumerateFields(this Type type, BindingFlags flags, Action<FieldInfo, string> fieldOperation)
        {
            //Run through all upstream types of this object to make sure we get all upstream private variables
            //that should be serialized
            while (type != null)
            {
                var fieldInfos = type.GetFields(flags);

                foreach (var field in fieldInfos)
                {
                    if (((field.IsPrivate || field.IsFamily) && !field.HasAttribute<JsonIncludeAttribute>())
                        || field.HasAttribute<JsonExcludeAttribute>())
                        continue;

                    //See if the field has a JsonName attribute
                    var nameAttribute = field.GetFirstAttribute<JsonName>();
                    var jsonName = (nameAttribute != null) ? nameAttribute.Name : field.Name;

                    //Operate on the field
                    fieldOperation(field, jsonName);
                }
                type = type.BaseType;
            }
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }
            return friendlyName;
        }
    }
}
