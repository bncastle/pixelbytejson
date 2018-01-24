namespace Pixelbyte.Json
{
    /// <summary>
    /// Implementation of the JSON 
    /// String : Value
    /// portion of a JSON object
    /// </summary>
    public class JSONPair
    {
        public string name;
        public object value;

        internal JSONPair(string name, object val) { this.name = name; value = val; }

        public override string ToString() { return string.Format("{0} : {1}", name, value.ToString()); }
    }
}
