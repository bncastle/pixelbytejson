using System;

namespace Pixelbyte.Json
{
    ///If present on a public field, then it is NOT serialized
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonIgnoreAttribute: Attribute { }

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
}
