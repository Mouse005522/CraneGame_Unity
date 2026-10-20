using System;
using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    public class ActionRouter<T>
    {
        public ActionRouter (bool singleCallback)
        {
            this.singleCallback = singleCallback;
        }

        bool singleCallback = false;

        public ActionRouter ()
        {
            this.singleCallback = false;
        }

        /// <summary>
        /// 是否還有註冊事件
        /// </summary>
        /// <returns></returns>
        public bool HasCallback () 
        {
            return caches.Values.Count > 0;
        }

        ModifiableList<Action<T>> caches = new ModifiableList<Action<T>> ();

        public bool HasCache { get; private set; } = false;
        public T CacheData { get; private set; } = default;

        public void Invoke (T data) 
        {
            HasCache = true;
            CacheData = data;

            using (var cachedList = caches.GetCachedList ()) 
            {
                foreach (var callback in cachedList.List)
                {
                    try
                    {
                        callback.Invoke (data);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Exception (e);
                    }
                }
            }
        }

        /// <summary>
        /// invokeHistory為如果註冊的當下有資料了
        /// 會立即用舊有資料觸發事件
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeHistory"></param>
        public void Bind (Action<T> action, bool invokeHistory = true) 
        {
            if (singleCallback && caches.Values.Count > 0) 
            {
                LoggerRouter.Error ($"只能寫入一個callback");
                caches.Values.Clear ();
            }

            if (caches.Values.Contains (action) == false)
            {
                caches.Add (action);

                if (invokeHistory && HasCache)
                {
                    try
                    {
                        action.Invoke (CacheData);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Exception (e);
                    }
                }
            }
            else
            {
                LoggerRouter.Error ($"can't bind twice");
            }
        }

        public void UnBind (Action<T> action) 
        {
            bool success = caches.Remove (action);

            if (success == false)
            {
                LoggerRouter.Error ($"can't unBind");
            }
        }

        public void UnBindAll ()
        {
            caches.Values.Clear ();
        }
    }

    public class ActionRouter
    {
        public ActionRouter (bool singleCallback)
        {
            this.singleCallback = singleCallback;
        }

        bool singleCallback = false;

        public ActionRouter ()
        {
            this.singleCallback = false;
        }

        /// <summary>
        /// 是否還有註冊事件
        /// </summary>
        /// <returns></returns>
        public bool HasCallback ()
        {
            return caches.Values.Count > 0;
        }

        ModifiableList<Action> caches = new ModifiableList<Action> ();

        public void Invoke ()
        {

            using (var cachedList = caches.GetCachedList ())
            {
                foreach (var callback in cachedList.List)
                {
                    try
                    {
                        callback.Invoke ();
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Exception (e);
                    }
                }
            }
        }

        /// <summary>
        /// invokeHistory為如果註冊的當下有資料了
        /// 會立即用舊有資料觸發事件
        /// </summary>
        /// <param name="action"></param>
        /// <param name="invokeHistory"></param>
        public void Bind (Action action, bool invokeHistory = true)
        {
            if (singleCallback && caches.Values.Count > 0)
            {
                LoggerRouter.Error ($"只能寫入一個callback");
                caches.Values.Clear ();
            }

            if (caches.Values.Contains (action) == false)
            {
                caches.Add (action);
            }
            else
            {
                LoggerRouter.Error ($"can't bind twice");
            }
        }

        public void UnBind (Action action)
        {
            bool success = caches.Remove (action);

            if (success == false)
            {
                LoggerRouter.Error ($"can't unBind");
            }
        }

        public void UnBindAll ()
        {
            caches.Values.Clear ();
        }
    }
}