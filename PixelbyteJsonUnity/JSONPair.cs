namespace Pixelbyte.JsonUnity
{
    public class JSONPair
    {
        internal string name;
        internal BaseJsonValue value;

        internal JSONPair(string name, BaseJsonValue val)
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
