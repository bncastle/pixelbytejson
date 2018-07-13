using System;

namespace Pixelbyte.Json
{
    ///If present on a public field, then it is NOT serialized
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonExcludeAttribute : Attribute { }

    /// <summary>
    /// If present on a protected or private field, then it is serialized. Otherwise, it isn't
    /// If a name string is specified, then the member will be serialized with that name
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonPropertyAttribute : Attribute
    {
        public string Name { get; }
        public JsonPropertyAttribute() { }
        public JsonPropertyAttribute(string name) { Name = name; }
    }

    /// <summary>
    /// Any class with this attribute will have an additional 
    /// key value written to its JSON output telling the
    /// decoder exactly what type to use. Necessary if
    /// instantiating a class that is being put into a Baseclass type (such as a List or an array)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonTypeHintAttribute : Attribute { }

    /// <summary>
    /// Add to a method to get a callback before a class is encoded to a JSON object
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonPreEncodeAttribute : Attribute { }
    /// <summary>
    /// Add to a method to get a callback after a class is encoded to a JSON object
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonEncodedAttribute : Attribute { }
    /// <summary>
    /// Add to a method to get a callback after a class is decoded to a JSON object
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonDecodedAttribute : Attribute { }

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
