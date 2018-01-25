using System.Collections;
using System.Collections.Generic;

namespace Pixelbyte.Json
{
    /// <summary>
    /// This class holds data returned by a class that implements
    /// the IJsonEncodeControl interface. It is then used to encode the
    /// object into JSON
    /// </summary>
    public class EncodeInfo : IEnumerable
    {
        Dictionary<string, object> table;

        public object this[string key]
        {
            get
            {
                object val;
                table.TryGetValue(key, out val);
                return val;
            }
            set { table[key] = value; }
        }
        public int Count { get { return table.Count; } }

        public EncodeInfo() { table = new Dictionary<string, object>(); }

        public bool HasKey(string key) { return table.ContainsKey(key); }


        public IEnumerator GetEnumerator()
        {
            return table.GetEnumerator();
        }
    }
}
