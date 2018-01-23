﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pixelbyte.JsonUnity
{
    using EncodeCallback = Action<object, JSONCreator>;
    using DecodeCallback = Func<Type, JSONObject, object>;

    // Define other methods and classes here
    public static class Jsonizer
    {
        //Contains all supported JSON encoders
        static Dictionary<Type, EncodeCallback> encoders;
        static Dictionary<Type, DecodeCallback> decoders;
        static EncodeCallback defaultEncoder;
        static DecodeCallback defaultDecoder;

        //static Dictionary<Type, SerializationProxy> proxies;
        //static SerializationProxy defaultProxy;

        static Jsonizer()
        {
            encoders = new Dictionary<Type, EncodeCallback>();
            decoders = new Dictionary<Type, DecodeCallback>();
            AddDefaults();
        }

        static void AddDefaults()
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


            AddEncoder(typeof(IEnumerable), (obj, builder) =>
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

            AddEncoder(typeof(IDictionary), (obj, builder) =>
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

            AddDecoder(typeof(IDictionary), (type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");

                var obj = Activator.CreateInstance(type, true) as IDictionary;

                //Get the value type of a generic dictionary
                Type valueType = type.GetGenericArguments()[1];

                foreach (var kv in jsonObj)
                {
                    obj.Add(kv.Key, DecodeValue(kv.Value, valueType));
                }
                return obj;
            });

            AddDecoder(typeof(IList), (type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");
                if (!jsonObj.IsArray) throw new ArgumentException("jsonObj: Expected a rootArray!");

                //Get the value type of the generic enumerable
                Type listElementType = type.GetGenericArguments()[0];

                var newList = Activator.CreateInstance(type, true) as IList;

                for (int i = 0; i < jsonObj.rootArray.Count; i++)
                {
                    newList.Add(DecodeValue(jsonObj.rootArray[i], listElementType));
                }

                return newList;
            });

            AddDecoder(typeof(Array), (type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");
                if (!jsonObj.IsArray) throw new ArgumentException("jsonObj: Expected a rootArray!");

                Type arrayElementType = type.GetElementType();
                bool nullable = arrayElementType.IsNullable();
                var newArray = Array.CreateInstance(arrayElementType,  jsonObj.rootArray.Count);

                for (int i = 0; i < jsonObj.rootArray.Count; i++)
                {
                    var value = DecodeValue(jsonObj.rootArray[i], arrayElementType);
                    if (value != null || nullable) newArray.SetValue(value, i);
                }
                return newArray;
            });

            defaultDecoder = ((type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");

                var obj = Activator.CreateInstance(type, true);

                var fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                //What to do here? Nothing maybe?
                if (fieldInfos == null)
                    throw new Exception();

                foreach (var fi in fieldInfos)
                {
                    //Look for the field name in the json object's data
                    var parameter = jsonObj[fi.Name];
                    if (parameter == null && !jsonObj.KeyExists(fi.Name))
                    {
                        //TODO: An error or warning??
                        continue;
                    }
                    else
                        fi.SetValue(obj, DecodeValue(parameter, fi.FieldType));
                }

                return obj;
            });

        }

        #region Decoder Methods

        public static void AddDecoder(Type type, DecodeCallback decodeFunc) { decoders.Add(type, decodeFunc); }
        public static void RemoveDecoder(Type type) { decoders.Remove(type); }
        static DecodeCallback GetDecoder(Type type) { DecodeCallback callback = null; decoders.TryGetValue(type, out callback); return callback; }
        static DecodeCallback GetDecoderOrDefault(Type type) { DecodeCallback callback = GetDecoder(type); if (callback == null) callback = defaultDecoder; return callback; }
        public static void ClearDecoders() { decoders.Clear(); }

        #endregion

        #region Encoder Methods

        public static void AddEncoder(Type type, EncodeCallback encodeFunc) { encoders.Add(type, encodeFunc); }
        public static void RemoveEncoder(Type type) { encoders.Remove(type); }
        static EncodeCallback GetEncoder(Type type) { EncodeCallback callback = null; encoders.TryGetValue(type, out callback); return callback; }
        static EncodeCallback GetEncoderOrDefault(Type type) { EncodeCallback callback = GetEncoder(type); if (callback == null) callback = defaultEncoder; return callback; }
        public static void ClearEncoders() { encoders.Clear(); }

        #endregion

        public static string Serialize(object obj, bool prettyPrint = true)
        {
            JSONCreator creator = new JSONCreator(prettyPrint);
            Serialize(obj, creator);
            return creator.ToString();
        }

        static string Serialize(object obj, JSONCreator creator)
        {
            //Object to serialize can't be null [TODO: Or can it?]
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            //See if the object implements the Serialization callbacks interface
            var callbacks = obj as ISerializeCallbacks;
            var serializationControl = obj as ISerializationControl;

            if (callbacks != null) callbacks.PreSerialization();

            var encodeMethod = GetEncoderOrDefault(type);
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
                throw new JSONTokenizerException(parser.Tokenizer.AllErrors);
            }
            else if (!parser.Successful)
            {
                //TODO: Make custom exception to show all parser errors
                throw new JSONParserException(parser.AllErrors);
            }
            else if (parser.rootObject == null)
            {
                //TODO: Make custom exception
                throw new JSONParserException("JSON ionput did not contain an object!");
            }
            else
            {
                //Ok then, try to Deserialize
                return (T)Deserialize(parser.rootObject, typeof(T));
            }
        }

        static object Deserialize(JSONObject jsonObj, Type type)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var decoder = GetDecoderOrDefault(type);
            //See if this object implements the Deserialization callback interface
            var decodedObject = decoder(type, jsonObj);
            var callbacks = decodedObject as IDeserializationCallbacks;

            //Deserialized Callback
            if (callbacks != null) callbacks.OnDeserialized();

            return decodedObject;
        }

        #region Value Encode/Decode Methods

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
                    Serialize(value, builder);
            }
        }

        static object DecodeValue(object value, Type toType)
        {
            if (value == null) return null;

            if (value == typeof(bool) || value == typeof(string))
                return value;
            //Signed ints
            else if (toType == typeof(Int64))
                value = Convert.ToInt64(value);
            else if (toType == typeof(Int32))
                value = Convert.ToInt32(value);
            else if (toType == typeof(Int16))
                value = Convert.ToInt16(value);
            else if (toType == typeof(SByte))
                value = Convert.ToSByte(value);
            //Unsigned ints
            else if (toType == typeof(UInt64))
                value = Convert.ToUInt64(value);
            else if (toType == typeof(UInt32))
                value = Convert.ToUInt32(value);
            else if (toType == typeof(UInt16))
                value = Convert.ToUInt16(value);
            else if (toType == typeof(Byte))
                value = Convert.ToByte(value);
            //Float values
            else if (toType == typeof(Decimal))
                value = Convert.ToDecimal(value);
            else if (toType == typeof(Double))
                value = Convert.ToDouble(value);
            else if (toType == typeof(Single))
                value = Convert.ToSingle(value);
            else if (toType.IsEnum)
                value = Enum.Parse(toType, value.ToString());
            else if (toType == typeof(DateTime))
            {
                DateTime dateTime;
                if (DateTime.TryParse(value.ToString(), out dateTime))
                    return dateTime;
                else
                    throw new Exception("DateTime value incorrect format: " + value.ToString());
            }
            //Other classes
            else if (value is JSONObject)
            {
                var childObj = value as JSONObject;
                DecodeCallback decoder = null;

                if (toType.HasInterface(typeof(IDictionary)))
                    decoder = GetDecoder(typeof(IDictionary));
                else
                    decoder = GetDecoderOrDefault(toType);

                return decoder(toType, childObj);
            }
            //Lists, Dictionaries, Arrays...
            else if (value is List<object>)
            {
                var childObj = value as List<object>;
                DecodeCallback decoder = null;
                if (toType.IsGeneric(typeof(List<>)))
                    decoder = GetDecoder(typeof(IList));
                if (toType.IsArray)
                    decoder = GetDecoder(typeof(Array));
                else
                    decoder = GetDecoderOrDefault(toType);
                return decoder(toType, new JSONObject(childObj));
            }
            return value;
        } 
        #endregion
    }
}
