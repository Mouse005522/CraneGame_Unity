using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kun.Tool
{
    /// <summary>
    /// 小長度的陣列才使用這個
    /// 當陣列長度超過16以上請考慮使用PackStream系列
    /// PackStream有對大長度的陣列進行優化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadWriteStream<T>
    {
        public bool CheckContains (T data)
        {
            return buffer.Contains (data);
        }

        public void Write (T data)
        {
            this.buffer.Add (data);
        }

        public void Write (T[] data)
        {
            this.buffer.AddRange (data);
        }

        List<T> buffer = new List<T> ();

        public bool TryRead (out T data) 
        {
            if (buffer.Count > 0) 
            {
                data = buffer[0];
                buffer.RemoveAt (0);
                return true;
            }
            else
            {
                data = default (T);
                return false;
            }
        }

        public T[] ReadAll () 
        {
            var result = buffer.ToArray ();
            buffer.Clear ();
            return result;
        }

        static ConcurrentQueue<ReadWriteStream<T>> pools;

        static ReadWriteStream ()
        {
            pools = new ConcurrentQueue<ReadWriteStream<T>> ();

            for (int i = 0; i < 100; i++)
            {
                ReadWriteStream<T> packStream = new ReadWriteStream<T> ();
                pools.Enqueue (packStream);
            }
        }

        public static ReadWriteStream<T> Pop ()
        {
            ReadWriteStream<T> pickStream = null;

            if (pools.TryDequeue (out pickStream) == false)
            {
                pickStream = new ReadWriteStream<T> ();
            }

            return pickStream;
        }

        /// <summary>
        /// 只清空沒回pool
        /// </summary>
        public void Clear () 
        {
            buffer.Clear ();
        }

        public void Dispose ()
        {
            Clear ();

            pools.Enqueue (this);
        }
    }
}
