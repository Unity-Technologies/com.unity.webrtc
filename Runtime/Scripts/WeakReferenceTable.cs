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
            Hashtable.Synchronized(m_table).Add(key, new WeakReference(value));
        }

        public void Remove(object key)
        {
            Hashtable.Synchronized(m_table).Remove(key);
        }

        public object this[object key]
        {
            get
            {
                var table = Hashtable.Synchronized(m_table);
                WeakReference reference = table[key] as WeakReference;
                return reference.NullOrValue();
            }
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            if (!ContainsKey(key))
            {
                value = default;
                return false;
            }
            value = (T)this[key];
            return true;
        }

        public ICollection CopiedValues
        {
            get
            {
                var table = Hashtable.Synchronized(m_table);
                var array = new object[table.Count];
                int i = 0;
                foreach (var value in table.Values)
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
            Hashtable.Synchronized(m_table).Clear();
        }

        public bool ContainsKey(object key)
        {
            return Hashtable.Synchronized(m_table).ContainsKey(key);
        }
        public bool TryGetValue(object key, out object value)
        {
            value = null;
            if (!ContainsKey(key))
                return false;
            value = this[key];
            return true;
        }
    }
}
