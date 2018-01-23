using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    internal class Container<K,V> where V : class
    {
        Dictionary<K, V> table;

        /// <summary>
        /// If the key was not found, the container will return this reuslt
        /// </summary>
        V defaultResult;
        public V this[K key]
        {
            get
            {
                V val;
                if (table.TryGetValue(key, out val)) return val;
                return defaultResult;
            }
            set
            {
                table[key] = value;
            }
        }

        public Container()
        {
            table = new Dictionary<K, V>();
        }

        public void SetDefaultValue(V def) { defaultResult = def; }

        public bool Exists(K key) { return table.ContainsKey(key); }

        public void Add(K key, V val) { table.Add(key, val); }
        public void Remove(K key) { table.Remove(key); }    

        public void Clear() { table.Clear(); }

    }
}
