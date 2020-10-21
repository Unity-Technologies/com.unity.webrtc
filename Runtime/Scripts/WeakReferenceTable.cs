using System;
using System.Collections;

namespace Unity.WebRTC
{
    static class WeakReferenceExtension
    {
        public static object NullOrValue(this WeakReference reference)
        {
            return reference.IsAlive ? reference.Target : null;
        }
    }

    internal class WeakReferenceTable
    {
        private Hashtable m_table = new Hashtable();

        public void Add(object key, object value)
        {
            m_table.Add(key, new WeakReference(value));
        }

        public void Remove(object key)
        {
            m_table.Remove(key);
        }

        public object this[object key]
        {
            get
            {
                WeakReference reference = m_table[key] as WeakReference;
                return reference.NullOrValue();
            }
        }

        public ICollection CopiedValues
        {
            get
            {
                var array = new object[m_table.Count];
                int i = 0;
                foreach (var value in m_table.Values)
                {
                    var reference = value as WeakReference;
                    array[i] = reference.NullOrValue();
                    i++;
                }
                return array;
            }
        }

        public void Clear()
        {
            m_table.Clear();
        }

        public bool ContainsKey(object key)
        {
            return m_table.ContainsKey(key);
        }
    }
}
