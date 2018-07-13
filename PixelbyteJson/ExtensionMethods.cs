using System;
using System.Reflection;

namespace Pixelbyte.Json
{
    internal static class ExtensionMethods
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

        internal static void EnumerateFields(this object targetObj, BindingFlags flags, Action<object, FieldInfo, string> fieldOperation)
        {
            if (targetObj == null) return;

            var type = targetObj.GetType();
            //Run through all upstream types of this object to make sure we get all upstream private variables
            //that should be serialized
            while (type != null)
            {
                var fieldInfos = type.GetFields(flags);

                foreach (var field in fieldInfos)
                {
                    if (((field.IsPrivate || field.IsFamily) && field.GetCustomAttribute<JsonPropertyAttribute>() == null)
                        || field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                        continue;

                    //See if the field has a JsonName attribute
                    var nameAttribute = field.GetCustomAttribute<JsonPropertyAttribute>();
                    var jsonName = (nameAttribute != null && nameAttribute.Name != null) ? nameAttribute.Name : field.Name;

                    //Operate on the field
                    fieldOperation(targetObj, field, jsonName);
                }
                type = type.BaseType;
            }
        }

        internal static MethodInfo FindMethodWith<T>(this Type t, BindingFlags flags) where T : Attribute
        {
            while (t != null)
            {
                var methods = t.GetMethods(flags);
                if (methods != null)
                {
                    for (int i = 0; i < methods.Length; i++)
                    {
                        if (methods[i].GetCustomAttribute<T>() != null)
                            return methods[i];
                    }
                }
                t = t.BaseType;
            }
            return null;
        }

        internal static string FriendlyName(this Type type)
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
                    string typeParamName = FriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }
            return friendlyName;
        }
    }
}
