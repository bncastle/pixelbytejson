using System.IO;

namespace Pixelbyte.JsonUnity
{
    enum TokenType
    {
        None = 0,

        //Single-Character Tokens
        OpenCurly   = 0x01,
        ClosedCurly = 0x02,
        OpenBracket = 0x04,
        CloseBracket= 0x08,
        Colon       = 0x10,
        Comma       = 0x20,

        //Value types
        String      = 0x40,
        Number      = 0x80,
        True        = 0x100,
        False       = 0x200,
        Null        = 0x400,

        //Extras not used in the Tokenizer
        TrueFalse   = 0x300;
    }
}
