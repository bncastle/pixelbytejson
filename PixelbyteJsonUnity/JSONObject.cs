using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JSONObject : IEnumerable<KeyValuePair<string, object>>
    {
        public Dictionary<string, object> pairs;

        public object this[string key]
        {
            get { object val = null; pairs.TryGetValue(key, out val); return val; }
            set { pairs[key] = value; }
        }

        public int Count { get { return pairs.Count; } }

        public JSONObject()
        {
            pairs = new Dictionary<string, object>();
        }

        public bool Add(string key, object value)
        {
            //TODO: Check for duplicates?
            pairs.Add(key, value);
            return true;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return pairs.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return pairs.GetEnumerator(); }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            foreach (var item in pairs)
                sb.AppendLine(item.ToString());
            return sb.ToString();
        }
    }
}
