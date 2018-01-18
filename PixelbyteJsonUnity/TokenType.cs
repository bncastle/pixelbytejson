using System.IO;

namespace Pixelbyte.JsonUnity
{
    enum TokenType
    {
        None = 0,

        //Single-Character Tokens
        OpenCurly = 0x001,
        ClosedCurly = 0x002,
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
        public static bool Contains(this TokenType tok, TokenType b) { return (tok & b) > 0; }
    }
}
