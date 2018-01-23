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

        /// <summary>
        /// If the root of this object is an array, then this will be populated
        /// and the ISArray bool will be true
        /// </summary>
        public List<object> rootArray;

        public object this[string key]
        {
            get { object val = null; pairs.TryGetValue(key, out val); return val; }
            set { pairs[key] = value; }
        }

        public bool IsArray { get; private set; }

        public int Count { get { return pairs.Count; } }

        public JSONObject()
        {
            pairs = new Dictionary<string, object>();
        }

        public JSONObject(List<object> objectArray) : this()
        {
            rootArray = objectArray;
            IsArray = true;
        }

        public bool KeyExists(string key) { return pairs.ContainsKey(key); }

        public bool Add(string key, object value)
        {
            if (pairs.ContainsKey(key))
                return false;
            else
            {
                pairs.Add(key, value);
                return true;
            }
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
