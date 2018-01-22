using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pixelbyte.JsonUnity
{
    // Define other methods and classes here
    public static class Jsonizer
    {
        //static Dictionary<Type, SerializationProxy> proxies;
        //static SerializationProxy defaultProxy;

        //static Jsonizer() { proxies = new Dictionary<Type, SerializationProxy>(); defaultProxy = new SerializationProxy(); }

        //public static void AddProxy(Type t, SerializationProxy proxy)
        //{
        //    if (proxies.ContainsKey(t))
        //        throw new ArgumentException(string.Format("Type of {0} already exists in the proxies table!", t.Name));
        //    else if (proxy == null)
        //        throw new ArgumentNullException("proxy");
        //    proxies.Add(t, proxy);
        //}

        //public static void RemoveProxy(Type t) { proxies.Remove(t); }
        //public static void ClearProxies() { proxies.Clear(); }

        //static SerializationProxy GetProxyFor(object obj) { if (obj == null) return null; else return GetProxyFor(obj.GetType()); }
        //static bool HasProxyFor(object obj) { if (obj == null) return false; return HasProxyFor(obj.GetType()); }
        //static bool HasProxyFor(Type type) { return proxies.ContainsKey(type); }

        //static SerializationProxy GetProxyFor(Type type)
        //{
        //    SerializationProxy p;
        //    if (proxies.TryGetValue(type, out p))
        //        return p;
        //    else
        //        return defaultProxy;
        //}

        public static string Ser(object obj, bool prettyPrint = true)
        {
            JSONCreator creator = new JSONCreator(prettyPrint);
            Ser(obj, creator);
            return creator.ToString();
        }

        static string Ser(object obj, JSONCreator creator)
        {
            //Object to serialize can't be null [TODO: Or can it?]
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            //See if the object implements the Serialization callbacks interface
            var callbacks = obj as ISerializeCallbacks;
            var serializationControl = obj as ISerializationControl;

            if (callbacks != null) callbacks.PreSerialization();

            //Traverse the type if it is a derived type we will want any/all fields from its base types as well
            creator.BeginObject();

            while (type != null)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    //If the field is private or protected we need to check and see if it has an attribute that allows us to include it
                    //Or if the field should be excluded, then skip it
                    if (((field.IsPrivate || field.IsFamily) && !field.HasAttribute<JsonIncludeAttribute>())
                        || field.HasAttribute<JsonExcludeAttribute>())
                        continue;

                    creator.String(field.Name);
                    creator.Colon();
                    var value = field.GetValue(obj);

                    if (creator.ValueSupported(value))
                        creator.Value(value);
                    else
                    {
                        //Style choice. For now we'll leave the opening curly on the same line
                        //creator.LineBreak();
                        Ser(value, creator);
                    }

                    //Format for another name value pair
                    creator.Comma();
                    creator.LineBreak();
                }
                type = type.BaseType;
            }
            creator.EndObject();
            if (callbacks != null) callbacks.PostSerialization();
            return creator.ToString();
        }

        public static T Deserialize<T>(string json)
        {
            var parser = JSONParser.Parse(json);
            if (!parser.Tokenizer.Successful)
            {
                //TODO: Make custom exception to show all tokenizer errors
                throw new Exception(parser.Tokenizer.AllErrors);
            }
            else if (!parser.Successful)
            {
                //TODO: Make custom exception to show all parser errors
                throw new Exception(parser.AllErrors);
            }
            else if (parser.rootObject == null)
            {
                //TODO: Make custom exception
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

            //See if this object implements the Deserialization callback interface
            var callbacks = obj as IDeserializationCallbacks;

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
                    //Signed ints
                    if (fi.FieldType == typeof(Int64))
                        fi.SetValue(obj, Convert.ToInt64(parameter));
                    else if (fi.FieldType == typeof(Int32))
                        fi.SetValue(obj, Convert.ToInt32(parameter));
                    else if (fi.FieldType == typeof(Int16))
                        fi.SetValue(obj, Convert.ToInt16(parameter));
                    else if (fi.FieldType == typeof(SByte))
                        fi.SetValue(obj, Convert.ToSByte(parameter));
                    //Unsigned ints
                    else if (fi.FieldType == typeof(UInt64))
                        fi.SetValue(obj, Convert.ToUInt64(parameter));
                    else if (fi.FieldType == typeof(UInt32))
                        fi.SetValue(obj, Convert.ToUInt32(parameter));
                    else if (fi.FieldType == typeof(UInt16))
                        fi.SetValue(obj, Convert.ToUInt16(parameter));
                    else if (fi.FieldType == typeof(Byte))
                        fi.SetValue(obj, Convert.ToByte(parameter));
                    //Float values
                    else if (fi.FieldType == typeof(Decimal))
                        fi.SetValue(obj, Convert.ToDecimal(parameter));
                    else if (fi.FieldType == typeof(Double))
                        fi.SetValue(obj, Convert.ToDouble(parameter));
                    else if (fi.FieldType == typeof(Single))
                        fi.SetValue(obj, Convert.ToSingle(parameter));
                    else if (fi.FieldType.IsEnum)
                        fi.SetValue(obj, Enum.Parse(fi.FieldType, parameter.ToString()));
                    else if(fi.FieldType == typeof(DateTime))
                    {
                        DateTime dateTime;
                        if (DateTime.TryParse(parameter.ToString(), out dateTime))
                            fi.SetValue(obj, dateTime);
                        else
                            throw new Exception("DateTime value incorrect format: " + parameter.ToString());
                    }
                    //Other classes
                    else if (parameter is JSONObject)
                        fi.SetValue(obj, Deserialize(parameter as JSONObject, fi.FieldType));
                    //strings, booleans
                    else
                        fi.SetValue(obj, parameter);
                }
                //TODO: Issue a warning?
            }

            //Deserialized Callback
            if (callbacks != null) callbacks.OnDeserialized();

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
