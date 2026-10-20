using System;
using System.Collections.Concurrent;

namespace Kun.Tool
{
    /// <summary>
    /// 目前T只有支援基礎型別
    /// 透過重用Array的方式避免GC
    /// 把Array當作物件池, 避免反覆寫入的過程寫入到新的Array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SegmentBuffer<T>
    {
        public ArraySegment<T> GetSegment ()
        {
            if (end != begin)
            {
                return new ArraySegment<T> (buffer, begin, (end - begin) + 1);
            }
            else
            {
                return new ArraySegment<T> ();
            }
        }

        T[] buffer;

        int begin = -1;
        int end = -1;

        /// <summary>
        /// out為超過的範圍
        /// isEnd為是否填滿了
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="joinLength"></param>
        /// <param name="outLength"></param>
        /// <param name="isBufferEnd"></param>
        public void PostBuffer (ArraySegment<T> segment, out int joinLength, out int outLength, out bool isBufferEnd)
        {
            CalculatePostResult (segment.Count, out joinLength, out outLength);

            Buffer.BlockCopy (segment.Array, segment.Offset, this.buffer, this.end + 1, joinLength);
            this.end += joinLength;

            if (begin == -1)
            {
                begin = 0;
            }

            isBufferEnd = this.end == (BufferSize - 1);
        }

        void CalculatePostResult (int postLength, out int joinLength, out int outLength)
        {
            //剩餘多少容量
            var remaining = BufferSize - (end + 1);

            outLength = postLength - remaining;

            if (outLength < 0)
            {
                outLength = 0;
            }

            joinLength = 0;

            //夠寫入, 那就塞多少寫多少
            if (outLength == 0)
            {
                joinLength = postLength;
            }
            else
            {
                //不夠寫
                //能寫多少剩多少
                joinLength = remaining;
            }
        }

        /// <summary>
        /// isEnd為此buffer的指標是否到底了
        /// 考慮到多線程的維護性
        /// 指標移動後會暫時鎖定array直到完全讀取後才釋放
        /// </summary>
        /// <param name="size"></param>
        /// <param name="outSize"></param>
        /// <returns></returns>
        public ArraySegment<T> PopSegment (int size, out int outSize, out bool isEnd)
        {
            ArraySegment<T> segment;

            int length = (end - begin) + 1;

            if (size <= length)
            {
                outSize = 0;

                segment = new ArraySegment<T> (this.buffer, begin, size);
                begin += size;
            }
            else
            {
                outSize = size - length;

                segment = new ArraySegment<T> (this.buffer, begin, length);
                begin += length;

            }

            isEnd = begin > end;
            return segment;
        }

        SegmentBuffer (int size)
        {
            buffer = new T[size];

            ClearFlag ();
        }

        static ConcurrentQueue<SegmentBuffer<T>> pools;

        static SegmentBuffer ()
        {
            pools = new ConcurrentQueue<SegmentBuffer<T>> ();

            for (int i = 0; i < BufferCount; i++)
            {
                SegmentBuffer<T> linkBuffer = new SegmentBuffer<T> (BufferSize);
                pools.Enqueue (linkBuffer);
            }
        }

        const int BufferCount = 1000;
        const int BufferSize = 256;

        public static SegmentBuffer<T> Pop ()
        {
            if (pools.TryDequeue (out SegmentBuffer<T> buffer))
            {
                return buffer;
            }
            else
            {
                return new SegmentBuffer<T> (BufferSize);
            }
        }

        public void Dispose ()
        {
            ClearFlag ();
            pools.Enqueue (this);
        }

        void ClearFlag ()
        {
            begin = -1;
            end = -1;
        }
    }
}