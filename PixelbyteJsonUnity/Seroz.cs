﻿using System;
using System.Reflection;

namespace Pixelbyte.JsonUnity
{
    // Define other methods and classes here
    public static class Seroz
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
            if (!parser.Tokenizer.Successful)
            {
                //TODO: Make custom exception
                //show all parser errors
                throw new Exception(String.Join(Environment.NewLine, 
                    parser.Tokenizer.Errors.ToArray()));
            }
            else if (!parser.Successful)
            {
                //TODO: Make custom exception
                //show all parser errors
                throw new Exception(String.Join(Environment.NewLine,
                    parser.Errors.ToArray()));
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

        static object Deserialize(JSONObject jsonObj, Type type)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var obj = Activator.CreateInstance(type);

            var fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            //What to do here? Nothing maybe?
            if (fieldInfos == null)
                throw new Exception();

            foreach (var fi in fieldInfos)
            {
                //Look for the field name in the json object's data
                var parameter = jsonObj[fi.Name];
                if (parameter != null)
                {
                    if (fi.FieldType == typeof(int))
                        fi.SetValue(obj, Convert.ToInt32(parameter));
                    else if (fi.FieldType == typeof(Single))
                        fi.SetValue(obj, Convert.ToSingle(parameter));
                    else if (fi.FieldType == typeof(bool))
                        fi.SetValue(obj, Convert.ToBoolean(parameter));
                    else if (parameter is JSONObject)
                        fi.SetValue(obj, Deserialize(parameter as JSONObject, fi.FieldType));
                    else
                        fi.SetValue(obj, parameter);
                }
                //TODO: Issue a warning?
            }
            return obj;
        }

        static T Deserialize<T>(JSONObject jsonObj)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var obj = Deserialize(jsonObj, typeof(T));
            return (T)obj;
        }
    }
}
