using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    /// <summary>
    /// 目前T只有支援基礎型別
    /// 透過重用Array的方式避免GC
    /// 把Array當作物件池, 避免反覆寫入的過程寫入到新的Array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PackStream<T>
    {
        public T[] GetFullBuffer ()
        {
            var segments = this.segments.SelectMany (b => b.GetSegment ());

            return segments.ToArray ();
        }

        public List<ArraySegment<T>> GetSegments ()
        {
            var segments = this.segments.ConvertAll (b => b.GetSegment ());
            return segments;
        }

        List<SegmentBuffer<T>> segments = new List<SegmentBuffer<T>> ();

        /// <summary>
        /// 指標經過的會先標記起來
        /// 等到array copy完再放回去
        /// </summary>
        List<SegmentBuffer<T>> waitingDisposes = new List<SegmentBuffer<T>> ();

        public int Size { get; private set; } = 0;

        /// <summary>
        /// 從前面移除byte並回傳
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool TryDequeueBuffer (int size, out List<ArraySegment<T>> segments)
        {
            if (this.Size >= size)
            {
                segments = DequeueBuffer (size);
                return true;
            }
            else
            {
                segments = null;
                return false;
            }
        }

        /// <summary>
        /// 從前面移除byte並回傳
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<ArraySegment<T>> DequeueBuffer (int size)
        {
            List<ArraySegment<T>> segments = new List<ArraySegment<T>> ();

            int remaining = size;

            while (true)
            {
                var cur = this.segments[0];

                var segment = cur.PopSegment (remaining, out remaining, out bool isEnd);
                segments.Add (segment);

                if (isEnd)
                {
                    MarkHeadEnd ();
                }

                if (remaining == 0)
                {
                    break;
                }
            }

            this.Size -= size;

            return segments;
        }

        /// <summary>
        /// 把第一個從buffer區拿出來
        /// 放到等待釋放區
        /// </summary>
        void MarkHeadEnd ()
        {
            var head = segments[0];
            waitingDisposes.Add (head);
            segments.RemoveAt (0);
        }

        /// <summary>
        /// 把兩個stream合併
        /// 被合併的會自動釋放
        /// </summary>
        /// <param name="stream"></param>
        public void InsertStream (PackStream<T> stream)
        {
            this.segments.InsertRange (0, stream.segments);
            this.Size += stream.Size;

            //dispose的時候會把segments返回
            //因為segments被合併回來了所以直接釋放
            stream.segments.Clear ();
            stream.Dispose ();
        }

        public void EnqueueBuffer (T[] buffer)
        {
            EnqueueBuffer (buffer, buffer.Length);
        }

        public void EnqueueBuffer (T[] buffer, int length)
        {
            ArraySegment<T> segment = new ArraySegment<T> (buffer, 0, length);

            EnqueueBuffer (segment);
        }

        public void EnqueueBuffer (List<ArraySegment<T>> segments)
        {
            segments.ForEach (segment =>
            {
                EnqueueBuffer (segment);
            });
        }

        public void EnqueueBuffer (ArraySegment<T> segment)
        {
            SegmentBuffer<T> end = GetEndBuffer ();

            ArraySegment<T> curSegment = segment;

            while (true)
            {
                end.PostBuffer (curSegment, out int joinLength, out int outLength, out bool isEnd);

                if (isEnd)
                {
                    end = SegmentBuffer<T>.Pop ();
                    segments.Add (end);
                }

                if (outLength == 0)
                {
                    break;
                }
                else
                {
                    var oldSegment = curSegment;
                    var newOffset = oldSegment.Offset + joinLength;

                    curSegment = new ArraySegment<T> (segment.Array, newOffset, outLength);
                }
            }


            Size += segment.Count;
        }

        /// <summary>
        /// 取得最尾端的那個buffer
        /// </summary>
        /// <returns></returns>
        SegmentBuffer<T> GetEndBuffer ()
        {
            SegmentBuffer<T> endBuffer;

            if (segments.Count == 0)
            {
                endBuffer = SegmentBuffer<T>.Pop ();

                segments.Add (endBuffer);
            }
            else
            {
                endBuffer = segments[segments.Count - 1];
            }

            return endBuffer;
        }

        static ConcurrentQueue<PackStream<T>> pools;

        static PackStream ()
        {
            pools = new ConcurrentQueue<PackStream<T>> ();

            for (int i = 0; i < 100; i++)
            {
                PackStream<T> packStream = new PackStream<T> ();
                pools.Enqueue (packStream);
            }
        }

        public static PackStream<T> Pop ()
        {
            PackStream<T> pickStream = null;

            if (pools.TryDequeue (out pickStream) == false)
            {
                pickStream = new PackStream<T> ();
            }

            return pickStream;
        }

        public void Dispose ()
        {
            segments.ForEach (buffer => buffer.Dispose ());
            segments.Clear ();

            waitingDisposes.ForEach (waitingFlush => waitingFlush.Dispose ());
            waitingDisposes.Clear ();

            Size = 0;

            pools.Enqueue (this);
        }
    }
}