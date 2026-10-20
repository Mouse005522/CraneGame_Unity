using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    /// <summary>
    /// 外部指定一個enum當作觸發時機
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelayCmdBuffer<T> where T:Enum
    {
        /// <summary>
        /// 傳入想要延後觸發的時機
        /// </summary>
        /// <param name="timing"></param>
        /// <param name="callback"></param>
        public void AddCallback (T timing, Action callback) 
        {
            commandBuffers.Add ((timing, callback));
        }

        /// <summary>
        /// 觸發流程中塞入的事件
        /// </summary>
        /// <param name="timing"></param>
        public void TriggerCallbacks (T timing) 
        {
            var pairs = commandBuffers.FindAll (pair => pair.key.Equals (timing));

            //只清空做完的, command可能之後才加入, 下一個frame補畫, 所以不要固定在結束的時候全清
            pairs.ForEach (pair=> 
            {
                commandBuffers.Remove (pair);
                pair.callback.Invoke ();
            });
        }

        List<(T key, Action callback)> commandBuffers = new List<(T key, Action callback)> ();
    }
}
