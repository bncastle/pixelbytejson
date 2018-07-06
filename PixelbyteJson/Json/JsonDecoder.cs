using System;
using System.Collections;
using System.Collections.Generic;

namespace Pixelbyte.Json
{
    // Define other methods and classes here
    public static class JsonDecoder
    {
        //Signature for all Decode methods
        public delegate object DecodeMethod(Type targetType, JsonObject jsonObj);

        static Dictionary<Type, DecodeMethod> decoders;
        static DecodeMethod defaultDecoder;

        /// <summary>
        /// This method gets the given type from a string
        /// It may need to be overridden if the calling program 
        /// has custom types that it knows about that we don't
        /// </summary>
        static Func<string, Type> TypeFromString;

        static Func<Type, object> CreateObjectInstance;

        /// <summary>
        /// Set this to override how the Default Decoder creates the objects
        /// Note: I'm using this so I can call ScriptableObject.CreateInstance in Unity3D
        /// </summary>
        public static Func<Type, object> CreateInstanceMethod
        {
            get { return CreateObjectInstance; }
            set
            {
                if (value == null)
                    CreateObjectInstance = (type) => Activator.CreateInstance(type, true);
                else
                    CreateObjectInstance = value;
            }
        }

        public static Func<string, Type> TypeFromStringMethod
        {
            get { return TypeFromString; }
            set
            {
                if (value == null)
                    TypeFromString = (typeText) => Type.GetType(typeText);
                else
                    TypeFromString = value;
            }
        }

        static JsonDecoder()
        {
            decoders = new Dictionary<Type, DecodeMethod>();

            //Set these to their defaults
            CreateInstanceMethod = null;
            TypeFromStringMethod = null;

            AddDefaults();
        }

        static void AddDefaults()
        {
            SetDecoder(typeof(IDictionary), (type, jsonObj) =>
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

            SetDecoder(typeof(IList), (type, jsonObj) =>
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

            SetDecoder(typeof(Array), (type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");
                if (!jsonObj.IsArray) throw new ArgumentException("jsonObj: Expected a rootArray!");

                Type arrayElementType = type.GetElementType();
                //bool nullable = arrayElementType.IsNullable();
                var newArray = Array.CreateInstance(arrayElementType, jsonObj.rootArray.Count);

                for (int i = 0; i < jsonObj.rootArray.Count; i++)
                {
                    var value = DecodeValue(jsonObj.rootArray[i], arrayElementType);
                    //if (value != null || nullable) newArray.SetValue(value, i);
                    newArray.SetValue(value, i);
                }
                return newArray;
            });

            defaultDecoder = ((type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");

                object obj = null;
                if (jsonObj[JsonEncoder.TypeNameString] != null)
                {
                    obj = CreateObjectInstance(TypeFromStringMethod(jsonObj[JsonEncoder.TypeNameString].ToString()));
                    if (obj == null)
                        throw new JSONDecodeException(string.Format("Unable to create object of type '{0}'!", jsonObj[JsonEncoder.TypeNameString].ToString()));
                }
                else
                {
                    obj = CreateObjectInstance(type);
                    if (obj == null)
                        throw new JSONDecodeException($"Unable to create object of type '{type.Name}'!");
                }


                obj.EnumerateFields(JsonEncoder.DEFAULT_JSON_BINDING_FLAGS, (targetObj, field, jsonName) =>
                {
                    //Look for the field name in the json object's data
                    var parameter = jsonObj[jsonName];
                    if (jsonObj.KeyExists(jsonName))
                    {
                        try
                        {
                            field.SetValue(targetObj, DecodeValue(parameter, field.FieldType));
                        }
                        catch (Exception e)
                        {
                            throw new JSONDecodeException($"Unable to decode json name '{jsonName}' of type '{field.FieldType.GetFriendlyName()}'\r\n{e.Message}");
                        }
                    }
                    //else
                    //{
                    //    //TODO: An error or warning??
                    //}
                });

                return obj;
            });
        }

        #region Decoder Methods

        public static void SetDecoder(Type type, DecodeMethod decodeFunc) { decoders[type] = decodeFunc; }
        public static void RemoveDecoder(Type type) { decoders.Remove(type); }
        static DecodeMethod GetDecoder(Type type) { DecodeMethod callback = null; decoders.TryGetValue(type, out callback); return callback; }
        static DecodeMethod GetDecoderOrDefault(Type type)
        {
            DecodeMethod callback = GetDecoder(type);

            if (callback == null)
            {
                if (type.HasInterface(typeof(IDictionary)))
                    callback = GetDecoder(typeof(IDictionary));
                else if (type.HasInterface(typeof(IList)))
                    callback = GetDecoder(typeof(IList));
                //else if (type.HasInterface(typeof(IEnumerable)))
                //    callback = GetDecoder(typeof(IEnumerable));
                else if (type.IsArray)
                    callback = GetDecoder(typeof(Array));
                if (callback == null)
                    callback = defaultDecoder;
            }
            return callback;
        }
        public static void ClearDecoders() { decoders.Clear(); }

        #endregion

        public static T Decode<T>(string json)
        {
            var parser = JsonParser.Parse(json);
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
                return (T)Decode(parser.rootObject, typeof(T));
            }
        }

        static object Decode(JsonObject jsonObj, Type type)
        {
            if (jsonObj == null) throw new ArgumentNullException("jsonObj");

            var decoder = GetDecoderOrDefault(type);

            //See if this object implements the Deserialization callback interface
            var decodedObject = decoder(type, jsonObj);
            var callbacks = decodedObject as IJsonDecodeCallbacks;

            //Deserialized Callback
            if (callbacks != null) callbacks.OnJsonDecoded();

            return decodedObject;
        }

        #region Value Decode Methods

        static object DecodeValue(object value, Type toType)
        {
            if ((value == null && (toType.IsClass || toType.IsInterface)) ||
                (value.ToString() == "null" && toType != typeof(string)))
                return null;

            if (value is JsonObject)
            {
                var childObj = value as JsonObject;

                if (toType.HasInterface(typeof(IDictionary)))
                {
                    var decoder = GetDecoder(typeof(IDictionary));
                    return decoder(toType, childObj);
                }
                else
                    return Decode(childObj, toType);
            }
            else if (toType == typeof(string))
                value = value.ToString();
            else if (toType == typeof(bool))
                value = Convert.ToBoolean(value);
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
            {
                //parse an enum from a string
                if (value is string)
                    value = Enum.Parse(toType, value.ToString());
                else
                    //To parse an enum from a number
                    value = Enum.ToObject(toType, value);
            }
            else if (toType == typeof(DateTime))
            {
                DateTime dateTime;
                if (DateTime.TryParse(value.ToString(), out dateTime))
                    return dateTime;
                else
                    throw new Exception("DateTime value incorrect format: " + value.ToString());
            }
            //Lists, Dictionaries, Arrays...
            else if (value is List<object>)
            {
                var childObj = value as List<object>;
                DecodeMethod decoder = null;
                if (toType.IsGeneric(typeof(List<>)))
                    decoder = GetDecoder(typeof(IList));
                else if (toType.IsArray)
                    decoder = GetDecoder(typeof(Array));
                else
                    decoder = GetDecoderOrDefault(toType);
                return decoder(toType, new JsonObject(childObj));
            }
            return value;
        }
        #endregion
    }
}
