using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    public static class PositionCalculator
    {
        /// <summary>
        /// 計算元素的對稱分佈位置
        /// </summary>
        /// <param name="index">當前元素的索引</param>
        /// <param name="count">總元素數量</param>
        /// <returns>元素的對應位置</returns>
        public static float CalculatePosition(int index, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException("Count must be greater than 0.");
            }

            // 偶數和奇數模式的通用公式
            return index - (count - 1) / 2.0f;
        }

        /// <summary>
        /// 返回所有元素的對應位置
        /// </summary>
        /// <param name="count">總元素數量</param>
        /// <returns>所有元素的對應位置列表(正規化值)</returns>
        public static List<(float normalOffset, float horzOffset)> GetPositions (int count)
        {
            List<(float normalOffset, float horzOffset)> positions = new List<(float, float)> ();

            // 計算第一行和第二行的數量
            // count=5: 3,2 | count=8: 5,3
            int firstRowCount = (count + 2) / 2;  // 向上取整+1,確保第一行數量較多
            int secondRowCount = count - firstRowCount;

            int firstRowIndex = 0;
            int secondRowIndex = 0;

            for (int i = 0; i < count; i++)
            {
                float normalOffset;
                float horzOffset;

                // 前排先放,前排滿了放後排
                if (firstRowIndex < firstRowCount)
                {
                    // 第一行 (前排) - 居中對齊
                    normalOffset = 0;
                    horzOffset = firstRowIndex - (firstRowCount - 1) / 2.0f;
                    firstRowIndex++;
                }
                else
                {
                    // 第二行 (後排) - 靠左對齊,交錯排列
                    normalOffset = -1.0f;
                    horzOffset = secondRowIndex - (firstRowCount - 1) / 2.0f + 0.5f;
                    secondRowIndex++;
                }

                positions.Add ((normalOffset, horzOffset));
            }

            return positions;
        }
    }
}
