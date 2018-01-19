using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JsonObject : BaseJsonValue, IEnumerable<KeyValuePair<string, BaseJsonValue>>
    {
        Dictionary<string, BaseJsonValue> table;

        public BaseJsonValue this[string key]
        {
            get { BaseJsonValue val = null; table.TryGetValue(key, out val); return val; }
            set { table[key] = value; }
        }

        public int Count { get { return table.Count; } }

        public void Add(string key, BaseJsonValue val) { table.Add(key, val); }
        //public void Remove(string key, BaseJsonValue val) { table.Remove(key, val); }
        //public bool Exists(string key) { return table.ContainsKey(key); }

        public JsonObject()
        {
            table = new Dictionary<string, BaseJsonValue>();
        }

        public override TypeCode GetTypeCode() { return TypeCode.Object; }

        public IEnumerator<KeyValuePair<string, BaseJsonValue>> GetEnumerator() { return table.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return table.GetEnumerator(); }
    }
}
