using System.Collections.Generic;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JSONObject
    {
        public string name;
        public List<JSONPair> pairs;

        public JSONObject(string name = null)
        {
            this.name = name;
            pairs = new List<JSONPair>();
        }

        public bool Add(JSONPair pair)
        {
            //TODO: Check for duplicates?
            pairs.Add(pair);
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append(":");
            sb.AppendLine();
            for (int i = 0; i < pairs.Count; i++)
            {
                sb.Append(pairs[i].ToString());
                if (i < pairs.Count - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
