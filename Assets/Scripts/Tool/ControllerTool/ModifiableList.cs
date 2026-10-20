using System.Collections;
using System.Collections.Generic;
using System;

namespace Kun.Tool
{
    public class ModifiableList<T>
    {
        public void Add (T value)
        {
            Values.Add (value);
        }

        public bool Remove (T value)
        {
            return Values.Remove (value);
        }

        /// <summary>
        /// 避免在foreach的過程中List變動
        /// 產生出當前items的副本
        /// GetCacheList底層呼叫
        /// </summary>
        /// <returns></returns>
        private List<T> GetCaches ()
        {
            List<T> buffer = bufferPools.Count > 0 ? bufferPools.Dequeue () : new List<T> ();

            buffer.Clear ();
            buffer.AddRange (Values);

            return buffer;
        }

        /// <summary>
        /// 返回caches的副本回歸到物件池中
        /// GetCacheList底層呼叫
        /// </summary>
        /// <param name="caches"></param>
        private void ReturnCaches (List<T> caches)
        {
            bufferPools.Enqueue (caches);
        }

        public ModifiableListCache<T> GetCachedList ()
        {
            var buffer = GetCaches ();
            return new ModifiableListCache<T> (buffer, this);
        }

        public List<T> Values { get; private set; } = new List<T> ();

        static Queue<List<T>> bufferPools = new Queue<List<T>> ();

        public struct ModifiableListCache<T> : IDisposable
        {
            public List<T> List { get; }

            private readonly ModifiableList<T> _pool;

            public ModifiableListCache (List<T> list, ModifiableList<T> pool)
            {
                List = list;
                _pool = pool;
            }

            public void Dispose ()
            {
                // 使用完自動歸還
                _pool.ReturnCaches (List);
            }
        }
    }
}

