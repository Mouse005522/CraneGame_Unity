using System;
using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    public abstract class BatchTask<T>
    {
        /// <summary>
        /// 當排程更新時傳入新的排程物件
        /// 排程主體會檢查是否所有排程都完成了
        /// 如果所有任務都結束了那排成主體也會提早結束
        /// </summary>
        /// <param name="scheduleObj"></param>
        public abstract void Invoke (T scheduleObj, out bool isDone);
    }
}