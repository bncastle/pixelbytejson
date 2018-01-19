namespace Pixelbyte.JsonUnity
{
    internal class JSONPair
    {
        internal string name;
        internal object value;

        internal JSONPair(string name, object val)
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
