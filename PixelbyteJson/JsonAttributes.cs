using System;

namespace Pixelbyte.Json
{
    ///If present on a public field, then it is NOT serialized
    public class JsonExcludeAttribute : Attribute { }

    ///If present on a protected or private field, then it is serialized. Otherwise, it isn't
    public class JsonIncludeAttribute : Attribute { }

    /// <summary>
    /// Any class with this attribute will have an additional 
    /// key value written to its JSON output telling the
    /// decoder exactly what type to use. Necessary if
    /// instantiating a class that is being put into a Baseclass type
    /// </summary>
    public class JsonTypeHint : Attribute { }

    ///If present on a float, double, or decimal field then it restricts the max decimal places to the number given
    //public class JsonMaxDecimalPlacesAttribute : Attribute
    //{
    //    string formatter;
    //    public JsonMaxDecimalPlacesAttribute(int maxDecimalPlaces) { formatter = string.Format("N{0}", maxDecimalPlaces); }
    //    public string Convert(decimal value) { return value.ToString(formatter); }
    //    public string Convert(double value) { return value.ToString(formatter); }
    //    public string Convert(float value) { return value.ToString(formatter); }
    //}
}
