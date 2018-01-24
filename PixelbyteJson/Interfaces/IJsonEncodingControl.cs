using System.Collections;
using System.Collections.Generic;

namespace Pixelbyte.Json
{
    public class EncodeData : IEnumerable
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

        public EncodeData() { table = new Dictionary<string, string>(); }

        public bool HasKey(string key) { return table.ContainsKey(key); }

        public IEnumerator GetEnumerator()
        {
            return table.GetEnumerator();
        }
    }

    public interface IJsonEncodingControl
    {
        void GetSerializedData(object obj, EncodeData info);
    }
}
