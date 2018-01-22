using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pixelbyte.JsonUnity
{
    using EncodeCallback = Action<object, JSONCreator>;

    // Define other methods and classes here
    public static class Jsonizer
    {
        //Contains all supported JSON encoders
        static Dictionary<Type, EncodeCallback> encoders;
        static EncodeCallback defaultEncoder;

        //static Dictionary<Type, SerializationProxy> proxies;
        //static SerializationProxy defaultProxy;

        static Jsonizer()
        {
            encoders = new Dictionary<Type, EncodeCallback>();
            AddDefaultEncoders();
        }

        static void AddDefaultEncoders()
        {
            defaultEncoder = ((obj, builder) =>
            {
                Type type = obj.GetType();
                builder.BeginObject();

                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    //If the field is private or protected we need to check and see if it has an attribute that allows us to include it
                    //Or if the field should be excluded, then skip it
                    if (((field.IsPrivate || field.IsFamily) && !field.HasAttribute<JsonIncludeAttribute>())
                        || field.HasAttribute<JsonExcludeAttribute>())
                        continue;

                    var value = field.GetValue(obj);
                    EncodePair(field.Name, value, builder);

                    //Format for another name value pair
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndObject();
            });

            AddEncoder<IEnumerable>((obj, builder) =>
            {
                builder.BeginArray();
                builder.LineBreak();
                foreach (var item in (IEnumerable)obj)
                {
                    EncodeValue(item, builder);
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndArray();
            });

            AddEncoder<IDictionary>((obj, builder) =>
            {
                builder.BeginObject();
                var table = obj as IDictionary;
                foreach (var key in table.Keys)
                {
                    EncodePair(key.ToString(), table[key], builder);
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndObject();
            });
        }

        public static void AddEncoder<T>(Action<object, JSONCreator> encodeMethod)
        {
            encoders[typeof(T)] = encodeMethod;
        }

        public static void RemoveEncoder<T>() { encoders.Remove(typeof(T)); }
        public static bool HasEncoder(object value) { return value != null && encoders.ContainsKey(value.GetType()); }

        public static EncodeCallback GetEncoder(Type type)
        {
            EncodeCallback callback;
            if (encoders.TryGetValue(type, out callback)) return callback;
            else return defaultEncoder;
        }

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

            var encodeMethod = GetEncoder(type);
            encodeMethod(obj, creator);

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

        static void EncodePair(string name, object value, JSONCreator builder)
        {
            builder.String(name);
            builder.Colon();
            EncodeValue(value, builder);
        }

        static void EncodeValue(object value, JSONCreator builder)
        {
            if (builder.ValueSupported(value))
                builder.Value(value);
            else
            {
                var type = value.GetType();

                EncodeCallback encode = null;
                //Try a dictionary first
                if (type.HasInterface(typeof(IDictionary)))
                    encode = GetEncoder(typeof(IDictionary));
                else if (type.HasInterface(typeof(IEnumerable)))
                    encode = GetEncoder(typeof(IEnumerable));
                else
                    encode = GetEncoder(value.GetType());
                if (encode != null)
                    encode(value, builder);
                else
                    Ser(value, builder);
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
                    else if (fi.FieldType == typeof(DateTime))
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

                    //TOOD: Enable decoding arrays, lists, dictionaries
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
