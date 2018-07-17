using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pixelbyte.Json
{
    /// <summary>
    /// Parses the given JSON string
    /// Note: When compiling for a .NET 3.5 Target, this is meant 
    /// to be compatible with Unity3D
    /// reference: https://www.json.org/
    /// </summary>
    public class JsonParser
    {
        public static JsonObject ParseFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();
            byte[] bytes = File.ReadAllBytes(filename);
            return Parse(bytes);
        }

        public static JsonObject Parse(string json) => Parse(Encoding.UTF8.GetBytes(json));

        public static JsonObject Parse(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            else if (bytes.Length == 0)
                throw new ArgumentException("bytes parameter must contain data!");

            Tokenizer tokenizer = new Tokenizer(bytes);
            JsonObject jsonObj = null;

            var token = tokenizer.CurrentToken();

            if (token == JsonToken.None)
                throw new JSONParserException("No valid JSON tokens found!");
            else if (token == JsonToken.StartObject)
                jsonObj = ReadObj(tokenizer);
            else if (token == JsonToken.StartArray)
            {
                var rootArray = ParseJsonArray(tokenizer);
                jsonObj = new JsonObject(rootArray);
            }
            else
                tokenizer.Throw("Expected either '{' or '[' as a root!");
            return jsonObj;
        }

        static JsonObject ReadObj(Tokenizer tokenizer)
        {
            JsonObject obj = new JsonObject();

            //Eat the OpenCurly Object
            tokenizer.ReadOrThrow('{');

            var tok = tokenizer.CurrentToken();
            while (tok != JsonToken.None && tok != JsonToken.EndObject)
            {
                //It could be an empty object
                if (tok == JsonToken.EndObject) break;

                var pair = ParseJsonPair(tokenizer);
                if (pair == null)
                    tokenizer.Throw("Expected a \"name\" : value pair!");
                else
                {
                    if (!obj.Add(pair.name, pair.value))
                        tokenizer.Throw($"Duplicate key found: {pair.name}");
                }

                //At the end of the pair we must either have a ',' or a '}'
                if (tokenizer.ReadIfFound(','))
                {
                    //If there is no name/value pair after the ',' then error
                    if (tokenizer.CurrentToken() == JsonToken.EndObject)
                        tokenizer.Throw("Expected a \"name\" : value pair!");
                }
                else if (tokenizer.CurrentToken() != JsonToken.EndObject)
                    tokenizer.Throw("Expected ',' or '}'");
                tok = tokenizer.CurrentToken();
            }

            //Eat the ending '}'
            tokenizer.ReadOrThrow('}');
            return obj;
        }

        private static List<object> ParseJsonArray(Tokenizer tokenizer)
        {
            tokenizer.ReadOrThrow('[');

            List<object> array = new List<object>();
            var tok = tokenizer.CurrentToken();
            while (tok != JsonToken.None && tok != JsonToken.EndArray)
            {
                if (tok == JsonToken.True || tok == JsonToken.False)
                    array.Add(tokenizer.ReadBool());
                else if (tok == JsonToken.Null)
                {
                    tokenizer.ReadNull();
                    array.Add(null);
                }
                else if (tok == JsonToken.String)
                    array.Add(tokenizer.ReadString());
                else if (tok == JsonToken.Number)
                    array.Add(tokenizer.ReadNumber());
                else if (tok == JsonToken.StartArray)
                    array.Add(ParseJsonArray(tokenizer));
                else if (tok == JsonToken.StartObject)
                    array.Add(ReadObj(tokenizer));
                else
                {
                    tokenizer.ThrowIfFound(':');
                    tokenizer.ThrowIfFound(',');
                }

                //At the end of the value we must either have a ',' or a ']'
                if (tokenizer.ReadIfFound(','))
                {
                    //Expect another json value or object, NOT a closing ']'
                    tokenizer.ThrowIfFound(JsonToken.EndArray);
                }
                else if (tokenizer.CurrentToken() != JsonToken.EndArray)
                    tokenizer.Throw("Expected ',' or ']'");
                tok = tokenizer.CurrentToken();
            }

            tokenizer.ReadOrThrow(']');
            return array;
        }

        private static JsonPair ParseJsonPair(Tokenizer tokenizer)
        {
            var name = tokenizer.ReadString();

            tokenizer.ReadOrThrow(':');

            //Now the value will either be an object, array, string, nubmer, or literal
            var tok = tokenizer.CurrentToken();
            if (tok == JsonToken.Null) return new JsonPair(name, tokenizer.ReadNull());
            else if (tok == JsonToken.True || tok == JsonToken.False) return new JsonPair(name, tokenizer.ReadBool());
            else if (tok == JsonToken.String) return new JsonPair(name, tokenizer.ReadString());
            else if (tok == JsonToken.Number) return new JsonPair(name, tokenizer.ReadNumber());
            else if (tok == JsonToken.StartObject) return new JsonPair(name, ReadObj(tokenizer));
            else if (tok == JsonToken.StartArray) return new JsonPair(name, ParseJsonArray(tokenizer));
            else
            {
                tokenizer.Throw($"Unexpected token: '{tok.Actual()}'");
                return null;
            }
        }
    }
}