namespace Pixelbyte.JsonUnity
{
    public class JSONPair
    {
        public string name;
        public object value;

        public JSONPair(string name, object val)
        {
            this.name = name;
            value = val;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", name, value.ToString());
        }
    }
}
