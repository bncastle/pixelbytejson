namespace Pixelbyte.Json
{
    public enum JsonToken : byte
    {
        None = 0,
        //**Single-Character Tokens
        StartObject = 1,  //{
        EndObject = 2,    //}
        StartArray = 3,  //[
        EndArray = 4, //]
        //**Separators
        Colon = 5,
        Comma = 6,
        //--Value types
        String  = 7,
        Number  = 8,
        //--Literals
        True    = 9,
        False   = 0,
        Null    = 11,
        //**Extras not used in the Tokenizer
        Value = 12
    }
}
