using System;
using System.Text;

namespace Pixelbyte.Json
{
    class Tokenizer
    {
        readonly char[] NULL_CHARS = { 'n', 'u', 'l', 'l' };
        readonly char[] TRUE_CHARS = { 't', 'r', 'u', 'e' };
        readonly char[] FALSE_CHARS = { 'f', 'a', 'l', 's', 'e' };

        int offset = 0;
        readonly byte[] bytes;

        public int Column { get; private set; }
        public int Row { get; private set; }

        public Tokenizer(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public JsonToken CurrentToken()
        {
            //Skip any whitespace
            SkipWhitespace();
            if (offset < bytes.Length)
            {
                switch (bytes[offset])
                {
                    //--Single-Character Tokens
                    case (byte)'{': return JsonToken.StartObject;
                    case (byte)'}': return JsonToken.EndObject;
                    case (byte)'[': return JsonToken.StartArray;
                    case (byte)']': return JsonToken.EndArray;
                    case (byte)':': return JsonToken.Colon;
                    case (byte)',': return JsonToken.Comma;
                    //--Literals
                    case (byte)'t': return JsonToken.True;
                    case (byte)'f': return JsonToken.False;
                    case (byte)'n': return JsonToken.Null;
                    //--Value types
                    case (byte)'\"': return JsonToken.String;
                    //We must catch anything a number can START with
                    case (byte)'-':
                    case (byte)'0':
                    case (byte)'1':
                    case (byte)'2':
                    case (byte)'3':
                    case (byte)'4':
                    case (byte)'5':
                    case (byte)'6':
                    case (byte)'7':
                    case (byte)'8':
                    case (byte)'9':
                        return JsonToken.Number;
                    default:
                        return JsonToken.None;
                }
            }
            else
                return JsonToken.None;
        }

        public void AdvanceToken(int amount = 1)
        {
            offset += amount;
            SkipWhitespace();
        }

        bool IsNewline() => offset < bytes.Length && (bytes[offset] == '\r' || bytes[offset] == '\n');

        private void SkipWhitespace()
        {
            for (int i = offset; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case (byte)' ': //space
                    case (byte)'\t': //tab
                        Column++;
                        break;
                    case (byte)'\r': //carriage return
                        break;
                    case (byte)'\n': //newline
                        Row++;
                        Column = 0;
                        break;
                    //case (byte)'/': //a comment
                    //    i = ReadComment(bytes, i);
                    //    continue;
                    default:
                        offset = i;
                        return;
                }
            }
            offset = bytes.Length;
        }

        internal bool ReadNull()
        {
            if (ReadChars(NULL_CHARS)) return true;
            else Throw($"Expected 'null'");
            return false;
        }

        bool ReadChars(char[] expected)
        {
            if (offset + expected.Length < bytes.Length)
            {
                for (int i = 0; i < expected.Length; i++)
                    if (bytes[offset + i] != expected[i]) return false;
                offset += expected.Length;
                Column += expected.Length;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an exception if the expected character is not found
        /// Otherwise, the expected character is consumed
        /// </summary>
        /// <param name="c"></param>
        public void ReadOrThrow(char c)
        {
            if (!ReadIfFound(c))
                Throw($"Expected: '{c}'");
        }

        public bool ReadIfFound(char c) => ReadIfFound((byte)c);

        public bool ReadIfFound(byte b)
        {
            SkipWhitespace();
            if (offset < bytes.Length && bytes[offset] == b)
            {
                AdvanceToken();
                return true;
            }
            return false;
        }

        public void ThrowIfFound(char c)
        {
            SkipWhitespace();
            if (offset < bytes.Length && bytes[offset] == (byte)c)
                Throw($"Expected: '{c}'");
        }

        public void ThrowIfFound(JsonToken token)
        {
            if (CurrentToken() == token)
                Throw($"Expected: '{token.Actual()}'");
        }

        public bool ReadBool()
        {
            SkipWhitespace();
            if (bytes[offset] == 't')
            {
                if (ReadChars(TRUE_CHARS)) return true;
                else Throw("Expected 'true'");
            }
            else if (bytes[offset] == 'f')
            {
                if (ReadChars(FALSE_CHARS)) return false;
                else Throw("Expected 'false'");
            }
            Throw("Expected either 'true' or 'false'");
            return false;
        }

        public string ReadString()
        {
            SkipWhitespace();
            if (bytes[offset] != '\"')
                Throw("Expected a '\"' to start a string");
            //Eat the "
            offset++;
            Column++;

            int stringStart = offset;

            while (offset < bytes.Length && !IsNewline() && bytes[offset] != '\"')
            {
                offset++;
            }
            if (bytes[offset] != '\"')
                Throw($"Unterminated string. Expected a '\"' to end string");

            //Eat the "
            offset++;
            Column += (offset);

            if ((offset - 1) - stringStart == 0) return string.Empty;

            return Encoding.UTF8.GetString(bytes, stringStart, (offset - 1) - stringStart);
        }

        internal object ReadNumber()
        {
            int index = offset;
            int bytesRead = 0;
            bool decimalSign = false;
            decimal value = 0;
            int sign = 1;
            byte p = 1;

            ///TODO: Need to support full number format with eE +-
            ///https://www.json.org/

            if (bytes[index] == '-')
            {
                offset++;
                bytesRead++;
                sign = -1;
            }

            for (int i = index; i < bytes.Length; i++)
            {
                if (bytes[i] == '.')
                {
                    if (!decimalSign)
                    {
                        decimalSign = true;
                        bytesRead++;
                        continue;
                    }
                    else
                    {
                        Throw("Unexpected '.'");
                    }
                }
                var digit = (bytes[i] - '0');
                if (digit < 0 || digit > 9) break;

                //NOTE: this is used only IF the number is an integer
                //Convert the byte to an actual number and add it to the current value
                //long.MinValue causes an overflow so we'll use unchecked here
                if (!decimalSign)
                    value = unchecked(value * 10 + digit);
                else
                {
                    value = unchecked(value + digit * (decimal)Math.Pow(10, -p));
                    p++;
                }
                bytesRead++;
            }

            Column += bytesRead;
            AdvanceToken(bytesRead);

            if (!decimalSign)
            {
                if (sign == 1)
                    return (ulong)value;
                else
                    return (long)value;
            }
            else
            {
                return checked(sign * value);
                //return checked(double.Parse(Encoding.UTF8.GetString(bytes, index, bytesRead)));
            }
        }

        public void Throw(string msg) => throw new JSONParserException($"[{Row}:{Column}] {msg}");
    }
}
