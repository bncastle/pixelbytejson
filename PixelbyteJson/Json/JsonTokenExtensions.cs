using System.Collections.Generic;

namespace Pixelbyte.Json
{
    internal static class JsonTokenExtensions
    {
        static readonly Dictionary<JsonToken, string> tokenMap = new Dictionary<JsonToken, string>()
        {
            {JsonToken.None, "none" },
            {JsonToken.StartObject, "'{'" },
            {JsonToken.EndObject, "'}'" },
            {JsonToken.StartArray, "'['" },
            {JsonToken.EndArray, "']'" },
            {JsonToken.Colon, "':'" },
            {JsonToken.Comma, "','" },
            {JsonToken.String, "string" },
            {JsonToken.Number, "number" },
            {JsonToken.True, "true" },
            {JsonToken.False, "false" },
            {JsonToken.Null, "null" },
            {JsonToken.Value, "string, number, true, false, null, object" }
        };

        public static string Actual(this JsonToken tok) { return tokenMap[tok]; }
    }
}
