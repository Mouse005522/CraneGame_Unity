using System;

namespace Kun.Tool
{
    public static class BoolPacketUtility 
    {
        public static byte CreateFromBools (bool[] flags)
        {
            if (flags.Length > 8)
            {
                LoggerRouter.Error ("flags 陣列長度不能超過 8");
                return 0;
            }
            else 
            {
                byte result = 0;
                for (int i = 0; i < flags.Length; i++)
                {
                    if (flags[i])
                    {
                        result |= (byte)(1 << i);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 避免GC, 寫入到一個已存在的 bool 陣列中
        /// 一個byte可以當8個bool使用
        /// </summary>
        /// <param name="container"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        public static void UnpackToBools (byte flag, bool[] container, int begin, int end)
        {
            int boolCount = end - begin;

            if (boolCount > 8)
            {
                LoggerRouter.Error ("boolCount 不能超過 8");
                return;
            }

            if (end >= container.Length) 
            {
                LoggerRouter.Error ("end 超過 container 陣列長度");
                return;
            }

            for (int i = 0; i < boolCount; i++)
            {
                bool value = (flag & (1 << i)) != 0;
                container[begin + i] = value;
            }
        }
    }

    /// <summary>
    /// 封裝 bool[] 與 byte[] 的轉換與修改
    /// </summary>
    public class BoolPacket
    {
        private byte[] packedData;
        private int boolCount;

        /// <summary>
        /// 取得打包後的 byte 陣列
        /// </summary>
        public byte[] PackedData => packedData;

        /// <summary>
        /// 取得 bool 的數量
        /// </summary>
        public int Count => boolCount;

        /// <summary>
        /// 從 bool[] 初始化
        /// </summary>
        public BoolPacket (bool[] flags)
        {
            boolCount = flags.Length;
            int byteLength = (flags.Length + 7) / 8;
            packedData = new byte[byteLength];

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    packedData[i / 8] |= (byte)(1 << (i % 8));
                }
            }
        }

        /// <summary>
        /// 修改指定 index 的 bool 值,直接修改內部的 byte[]
        /// </summary>
        /// <param name="index">bool 陣列的索引</param>
        /// <param name="value">要設定的值</param>
        public void SetBool (int index, bool value)
        {
            if (index < 0 || index >= boolCount)
            {
                throw new ArgumentOutOfRangeException (nameof (index));
            }

            int byteIndex = index / 8;
            int bitIndex = index % 8;
            byte mask = (byte)(1 << bitIndex);

            if (value)
            {
                // 設定為 true: 使用 OR 運算
                packedData[byteIndex] |= mask;
            }
            else
            {
                // 設定為 false: 使用 AND NOT 運算
                packedData[byteIndex] &= (byte)~mask;
            }
        }

        /// <summary>
        /// 取得指定 index 的 bool 值
        /// </summary>
        public bool GetBool (int index)
        {
            if (index < 0 || index >= boolCount)
            {
                throw new ArgumentOutOfRangeException (nameof (index));
            }

            return (packedData[index / 8] & (1 << (index % 8))) != 0;
        }

        /// <summary>
        /// 解包為 bool[]
        /// </summary>
        public bool[] UnpackBools ()
        {
            bool[] flags = new bool[boolCount];

            for (int i = 0; i < boolCount; i++)
            {
                flags[i] = (packedData[i / 8] & (1 << (i % 8))) != 0;
            }

            return flags;
        }
    }
}