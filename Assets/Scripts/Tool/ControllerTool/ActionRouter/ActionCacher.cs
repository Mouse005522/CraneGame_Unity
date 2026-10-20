using System;

namespace Kun.Tool
{
    public class ActionCacher<T> : ActionCacher
    {
        public ActionCacher (ActionRouter<T> router, Action<T> cacheAction)
        {
            this.router = router;
            this.cacheAction = cacheAction;
        }

        ActionRouter<T> router;
        Action<T> cacheAction;

        /// <summary>
        /// 把自己註冊進原始事件上
        /// </summary>
        public override void Enable ()
        {
            router.Bind (cacheAction);
        }

        /// <summary>
        /// 把自己從原始事件反註冊
        /// </summary>
        public override void Disable ()
        {
            router.UnBind (cacheAction);
        }

        public bool HasCache => router.HasCache;
        public T CacheData => router.CacheData;

        /// <summary>
        /// 強制刷新事件
        /// </summary>
        public void ForceUpdate ()
        {
            if (HasCache) 
            {
                cacheAction.Invoke (CacheData);
            }
        }
    }

    public abstract class ActionCacher
    {
        public abstract void Enable ();
        public abstract void Disable ();
    }
}