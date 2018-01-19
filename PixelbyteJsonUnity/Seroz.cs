using System;
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

        public static T Deserialize<T>(string json)
        {
            var parser = JSONParser.Parse(json);
            if (parser.Tokenizer.IsError)
            {
                //TODO: Make custom exceptio
                //show all parser errors
                throw new Exception();
            }
            else if (parser.IsError)
            {
                //TODO: Make custom exception
                //show all parser errors
                throw new Exception();
            }
            else if (parser.rootObject == null)
            {
                //TODO: Make custom exceptio
                throw new Exception("JSON root was not an object!");
            }
            else
            {
                return Deserialize<T>(parser.rootObject);
            }
        }

        static T Deserialize<T>(JSONObject jsonObj)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var obj = Activator.CreateInstance<T>();

            var fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            //What to do here? Nothing maybe?
            if (fieldInfos == null)
                throw new Exception();

            //TODO: populate it here
            foreach (var item in jsonObj.pairs)
            {
                var fi = fieldInfos.FindByName(item.name);
                if (fi != null)
                {
                    if (fi.FieldType == typeof(int))
                        fi.SetValue(obj, Convert.ToInt32(item.value));
                    else if (fi.FieldType == typeof(Single))
                        fi.SetValue(obj, Convert.ToSingle(item.value));
                    //https://stackoverflow.com/questions/2604743/setting-generic-type-at-runtime
                    else if (item.value is JSONObject) 
                        Console.WriteLine("ll");
                    //Deserialize < fi.FieldType > (item.value);
                        //fi.SetValue(obj, Deserialize < fi.FieldType> (item.value));
                    else
                        fi.SetValue(obj, item.value);
                }
            }

            return obj;
        }
    }
}
