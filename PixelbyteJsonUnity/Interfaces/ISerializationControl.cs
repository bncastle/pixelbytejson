using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class SerializationData : IEnumerable
    {
        Dictionary<string, string> table;

        public string this[string key]
        {
            get
            {
                string val;
                table.TryGetValue(key, out val);
                return val;
            }
            set { table[key] = value; }
        }

        public SerializationData() { table = new Dictionary<string, string>(); }

        public bool HasKey(string key) { return table.ContainsKey(key); }

        public IEnumerator GetEnumerator()
        {
            return table.GetEnumerator();
        }
    }

    public interface ISerializationControl
    {
        void GetSerializedData(object obj, SerializationData info);
    }
}
