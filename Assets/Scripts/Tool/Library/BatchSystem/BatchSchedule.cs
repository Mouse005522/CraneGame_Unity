using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    /// <summary>
    /// 此次排程所依附主體
    /// 譬如搜尋Componenet的任務會不斷地收到GameObject
    /// 那他的排程主體就是GameObject
    /// 不拘束成Component是可能之後有其他應用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchSchedule<T>
    {
        BatchSchedule (IAsyncEnumerable<T> scheduleObjGetter) 
        {
            this.scheduleObjGetter = scheduleObjGetter;
        }

        IAsyncEnumerable<T> scheduleObjGetter;

        public static BatchSchedule<T> CreateScehedule (IAsyncEnumerable<T> scheduleObjGetter) 
        {
            return new BatchSchedule<T> (scheduleObjGetter);
        }

        public void AddTask (BatchTask<T> task, Action onFininsh = null) 
        {
            BatchTaskCache batchTaskCache = new BatchTaskCache (task, onFininsh);

            waitingBatchTasks.Add (batchTaskCache);
        }


        List<BatchTaskCache> waitingBatchTasks = new List<BatchTaskCache> ();

        List<BatchTaskCache> finishBatchTasks = new List<BatchTaskCache> ();

        /// <summary>
        /// 透過一個async不斷的取得item
        /// 當async結束還有沒完成的任務會被標記成失敗
        /// </summary>
        /// <param name="scheduleObjGetter"></param>
        /// <returns></returns>
        public async UniTask RunTask () 
        {
            await foreach (var obj in scheduleObjGetter)
            {
                PushScheduleObj (obj);

                if (IsAllTaskDone ()) 
                {
                    break;
                }
            }

            //schedule結束了還沒找到的強制觸發完成由各個task各自觸發結算
            waitingBatchTasks.ToList ().ForEach (task =>
            {
                task.InvokeFinish ();
                finishBatchTasks.Add (task);
                waitingBatchTasks.Remove (task);
            });
        }

        void PushScheduleObj (T scheduleObj) 
        {
            waitingBatchTasks.ToList ().ForEach (task =>
            {
                task.InvokeScheduleObj (scheduleObj, out bool isDone);

                if (isDone)
                {
                    task.InvokeFinish ();
                    finishBatchTasks.Add (task);
                    waitingBatchTasks.Remove (task);
                }
            });
        }

        public bool IsAllTaskDone () 
        {
            return waitingBatchTasks.Count == 0;
        }

        class BatchTaskCache
        {
            public BatchTaskCache (BatchTask<T> batchTask, Action onFinish)
            {
                this.batchTask = batchTask;
                this.onFinish = onFinish;
            }

            BatchTask<T> batchTask;

            Action onFinish;

            public void InvokeScheduleObj (T scheduleObj, out bool isDone)
            {
                batchTask.Invoke (scheduleObj, out isDone);
            }

            public void InvokeFinish () 
            {
                if (onFinish != null)
                {
                    try
                    {
                        onFinish ();
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Exception (e);
                    }
                }
            }
        }
    }
}
