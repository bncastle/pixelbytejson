using System;

namespace Pixelbyte.JsonUnity
{
    ///If present on a protected or private field, then it is serialized. Otherwise, it isn't
    public class JsonIncludeAttribute : Attribute { }
}
