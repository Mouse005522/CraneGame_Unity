using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeChatRoomClient.Services
{
    public class TaskSchedule
    {
        /// <summary>
        /// 透過switcher把堆疊清空,
        /// 避免無窮堆疊不易偵錯
        /// </summary>
        /// <param name="invokeOnFreshStack"></param>
        public TaskSchedule (Action<Action> invokeOnFreshStack = null)
        {
            if (invokeOnFreshStack != null)
            {
                this.invokeOnFreshStack = invokeOnFreshStack;
            }
            else
            {
                this.invokeOnFreshStack = (action) =>
                {
                    action.Invoke ();
                };
            }
        }

        Action<Action> invokeOnFreshStack;

        bool inTask = false;

        public async Task<T> DoSchedule<T> (Func<Task<T>> task)
        {
            var tcs = new TaskCompletionSource<T> ();

            queues.Enqueue (async () =>
            {
                try
                {
                    var result = await task ();
                    tcs.SetResult (result);
                }
                catch (Exception ex)
                {
                    tcs.SetException (ex);
                }
                finally
                {
                    inTask = false;
                    CheckCycle ();
                }
            });

            CheckCycle ();

            var result = await tcs.Task;

            return result;
        }

        void CheckCycle ()
        {
            if (inTask == false)
            {
                if (queues.Count > 0)
                {
                    //先把flag改掉, 避免競爭
                    inTask = true;

                    invokeOnFreshStack (() =>
                    {
                        var newTask = queues.Dequeue ();

                        newTask.Invoke ();
                    });
                }
            }
        }

        Queue<Func<Task>> queues = new Queue<Func<Task>> ();
    }
}
