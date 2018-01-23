using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pixelbyte.JsonUnity
{
    using EncodeCallback = Action<object, JSONCreator>;
    using DecodeCallback = Func<JSONObject, Type, object>;

    // Define other methods and classes here
    public static class Jsonizer
    {
        //Contains all supported JSON encoders
        static Container<Type, EncodeCallback> encoders;
        static Container<Type, DecodeCallback> decoders;
        //static EncodeCallback defaultEncoder;
        static DecodeCallback defaultDecoder;

        //static Dictionary<Type, SerializationProxy> proxies;
        //static SerializationProxy defaultProxy;

        static Jsonizer()
        {
            encoders = new Container<Type, EncodeCallback>();
            decoders = new Container<Type, DecodeCallback>();
            AddDefaults();
        }

        static void AddDefaults()
        {
            encoders.SetDefaultValue((obj, builder) =>
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

            encoders.Add(typeof(IEnumerable), (obj, builder) =>
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

            encoders.Add(typeof(IDictionary), (obj, builder) =>
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

            decoders.Add(typeof(IDictionary), (jsonObj, type) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");

                var obj = Activator.CreateInstance(type) as IDictionary;

                foreach (var kv in jsonObj)
                {
                    obj.Add(kv.Key, kv.Value);
                }
                return obj;
            });
            decoders.SetDefaultValue((jsonObj, type) =>
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
                        else if (fi.FieldType == typeof(bool))
                            fi.SetValue(obj, parameter);
                        else if (fi.FieldType == typeof(string))
                            fi.SetValue(obj, parameter);
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
                        {
                            var childObj = parameter as JSONObject;
                            DecodeCallback decoder = null;
                            if (fi.FieldType.HasInterface(typeof(IDictionary)))
                                decoder = decoders[typeof(IDictionary)];
                            else
                                decoder = decoders[fi.FieldType];

                            var decodedObj = decoder(childObj, fi.FieldType);
                            fi.SetValue(obj, decodedObj);
                        }

                    }

                }
                //TODO: Issue a warning?

                //Deserialized Callback
                if (callbacks != null) callbacks.OnDeserialized();

                return obj;
            });

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

            var encodeMethod = encoders[type];
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
                    encode = encoders[typeof(IDictionary)];
                else if (type.HasInterface(typeof(IEnumerable)))
                    encode = encoders[typeof(IEnumerable)];
                else
                    encode = encoders[value.GetType()];
                if (encode != null)
                    encode(value, builder);
                else
                    Ser(value, builder);
            }
        }

        static object Deserialize(JSONObject jsonObj, Type type)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var decoder = decoders[type];
            //See if this object implements the Deserialization callback interface
            var decodedObject = decoder(jsonObj, type);
            var callbacks = decodedObject as IDeserializationCallbacks;

            //Deserialized Callback
            if (callbacks != null) callbacks.OnDeserialized();

            return decodedObject;
        }

        static T Deserialize<T>(JSONObject jsonObj)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var obj = Deserialize(jsonObj, typeof(T));
            return (T)obj;
        }
    }
}
