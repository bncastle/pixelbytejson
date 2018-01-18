using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    enum TokenType : UInt16
    {
        None = 0,

        //Single-Character Tokens
        OpenCurly = 0x001,
        CloseCurly = 0x002,
        OpenBracket = 0x004,
        CloseBracket = 0x008,
        Colon = 0x010,
        Comma = 0x020,

        //Value types
        String = 0x040,
        Number = 0x080,
        True = 0x100,
        False = 0x200,
        Null = 0x400,

        //Extras not used in the Tokenizer
        Value = 0x7C0
    }

    internal static class TokenTypeExtensions
    {
        static readonly Dictionary<TokenType, string> tokenMap = new Dictionary<TokenType, string>()
        {
            {TokenType.None, "none" },
            {TokenType.OpenCurly, "'{'" },
            {TokenType.CloseCurly, "'}'" },
            {TokenType.OpenBracket, "'['" },
            {TokenType.CloseBracket, "']'" },
            {TokenType.Colon, "':'" },
            {TokenType.Comma, "','" },
            {TokenType.String, "string" },
            {TokenType.Number, "number" },
            {TokenType.True, "true" },
            {TokenType.False, "false" },
            {TokenType.Null, "null" },
            {TokenType.Value, "string, number, true, false, null, object" }
        };

        public static bool Contains(this TokenType tok, TokenType b) { return (tok & b) > 0; }
        public static bool IsExpected(this TokenType tok, TokenType expected) { return expected == TokenType.None || expected.Contains(tok); }
        public static string Actual(this TokenType tok) { return tokenMap[tok]; }
        public static string ListActual(this TokenType tok)
        {
            if (tok == 0) return TokenType.None.Actual();

            StringBuilder sb = new StringBuilder();
            //TokenType is a 16-bit value so we'll check all 16 bits
            for (int i = 0; i < 16; i++)
            {
                if (((1 << i) & (UInt16)tok) > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(((TokenType)(1 << i)).Actual());
                }
            }
            return sb.ToString();
        }

        public static string List(this TokenType tok)
        {
            if (tok == 0) return "none";

            StringBuilder sb = new StringBuilder();
            //TokenType is a 16-bit value so we'll check all 16 bits
            for (int i = 0; i < 16; i++)
            {
                if (((1 << i) & (UInt16)tok) > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(((TokenType)(1 << i)).ToString());
                }
            }
            return sb.ToString();
        }
    }
}
