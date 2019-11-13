using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Inspector
{
    
    internal class InspectorCache<T> where T : class
    {

        private struct Entry
        {
            public WeakReference<object> Ref;
            public T Node;
        }
        
        private readonly Dictionary<int, LinkedList<Entry>> caches = new Dictionary<int, LinkedList<Entry>>();

        public void Put(object value, T node)
        {
            var hash = RuntimeHelpers.GetHashCode(value);
            if (!caches.TryGetValue(hash, out var list))
            {
                list = new LinkedList<Entry>();
                caches.Add(hash, list);
            }

            list.AddLast(new Entry
            {
                Ref = new WeakReference<object>(value),
                Node = node,
            });
        }
        
        public T Get(object value)
        {
            var hash = RuntimeHelpers.GetHashCode(value);
            if (!caches.TryGetValue(hash, out var list))
            {
                return null;
            }

            var node = list.First;
            while (node != null)
            {
                var current = node;
                node = node.Next;
                
                var entry = current.Value;
                if (!entry.Ref.TryGetTarget(out var target) || (target is Object obj && obj == null))
                {
                    list.Remove(current);
                    continue;
                }
                
                if(ReferenceEquals(target, value))
                {
                    return entry.Node;
                }
                
                if(target.GetType() != current.GetType())
                {
                    continue;
                }

                if (target.GetType().IsValueType && target.Equals(value))
                {
                    return entry.Node;
                }
            }
            
            return null;
        }

        public void Clear()
        {
            caches.Clear();
        }
    }
    
}