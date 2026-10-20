using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kun.Tool
{
    public class GoPool : UnityPool<GameObject>
    {
        public GoPool (GameObject prefab, Transform root) : base (root)
        {
            this.prefab = prefab;
        }

        GameObject prefab;

        protected override GameObject CreateItem ()
        {
            return MonoBehaviour.Instantiate (prefab, root);
        }

        protected override void SetActive (GameObject item, bool active)
        {
            item.SetActive (active);
        }

        protected override void SetParent (GameObject item, Transform parent)
        {
            item.transform.SetParent (parent);
        }

        protected override void DisposeItem (GameObject item)
        {
            MonoBehaviour.Destroy (item);
        }
    }

    /// <summary>
    /// 沒有參考的prefab,
    /// 每次都會產出一個乾淨的component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TempMonoPool<T> : UnityPool<T> where T : Component
    {
        public TempMonoPool (Transform root, Action<T> onCreate = null) : base (root)
        {
            this.onCreate = onCreate;
        }

        Action<T> onCreate;

        protected override T CreateItem ()
        {
            var newGo = new GameObject ($"[{typeof (T)}]");
            newGo.transform.SetParent (root);

            var newOne = newGo.AddComponent<T> ();

            if (onCreate != null)
            {
                onCreate.Invoke (newOne);
            }

            return newOne;
        }

        protected override void SetActive (T item, bool active)
        {
            item.gameObject.SetActive (active);
        }

        protected override void SetParent (T item, Transform parent)
        {
            item.gameObject.transform.SetParent (parent);
        }

        protected override void DisposeItem (T item)
        {
            MonoBehaviour.Destroy (item.gameObject);
        }
    }

    public class MonoPool<T> : UnityPool<T> where T : Component
    {
        public MonoPool (T prefab, Transform root, Action<T> onCreate = null) : base (root)
        {
            this.prefab = prefab;
            this.onCreate = onCreate;
        }

        T prefab;

        Action<T> onCreate;

        protected override T CreateItem ()
        {
            var newOne = MonoBehaviour.Instantiate (prefab, root);

            if (onCreate != null)
            {
                onCreate.Invoke (newOne);
            }

            return newOne;
        }

        protected override void SetActive (T item, bool active)
        {
            item.gameObject.SetActive (active);
        }

        protected override void SetParent (T item, Transform parent)
        {
            item.gameObject.transform.SetParent (parent);
        }

        protected override void DisposeItem (T item)
        {
            MonoBehaviour.Destroy (item.gameObject);
        }
    }

    public class ProxyPool<T> : UnityPool<T> where T : Componentable
    {
        public ProxyPool (Func<GameObject, T> constr, GameObject prefab, Transform root) : base (root)
        {
            this.prefab = prefab;
            this.constr = constr;
        }

        GameObject prefab;

        Func<GameObject, T> constr;

        protected override T CreateItem ()
        {
            var newEntity = MonoBehaviour.Instantiate (prefab, root);

            T newItem = constr.Invoke (newEntity);

            return newItem;
        }

        protected override void SetActive (T item, bool active)
        {
            item.GetEntity ().SetActive (active);
        }

        protected override void SetParent (T item, Transform parent)
        {
            item.GetEntity ().transform.SetParent (parent);
        }

        protected override void DisposeItem (T item)
        {
            MonoBehaviour.Destroy (item.GetEntity ());
        }
    }

    public abstract class UnityPool<T> : Pool<T>
    {
        public UnityPool (Transform root)
        {
            this.root = root;
        }

        protected override void OnDequeue (T t)
        {
            base.OnDequeue (t);

            SetActive (t, true);
        }

        protected override void OnEnqueue (T item)
        {
            base.OnEnqueue (item);

            SetActive (item, false);

            SetParent (item, root);
        }

        protected abstract void SetActive (T item, bool active);

        protected abstract void SetParent (T item, Transform parent);

        protected Transform root;
    }

    /// <summary>
    /// 自動將無參數建構式當作factory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TempClassPool<T> : ClassPool<T> where T : new()
    {
        /// <summary>
        /// 自動將無參數建構式當作factory
        /// </summary>
        public TempClassPool () : base (() => new T ())
        {
        }
    }

    public class ClassPool<T> : Pool<T>
    {
        public ClassPool (Func<T> factory)
        {
            this.factory = factory;
        }

        Func<T> factory;

        protected override T CreateItem ()
        {
            var item = factory.Invoke ();
            return item;
        }

        protected override void DisposeItem (T t)
        {
            
        }
    }

    public abstract class Pool<T> 
    {
        public void Enqueue (T item)
        {
            queue.Enqueue (item);
            OnEnqueue (item);
        }

        protected virtual void OnEnqueue (T item)
        {

        }

        public T Dequeue ()
        {
            T item;

            if (queue.Count > 0)
            {
                item = queue.Dequeue ();
                OnDequeue (item);
            }
            else
            {
                item = CreateItem ();
            }

            return item;
        }

        protected virtual void OnDequeue (T t) 
        {

        }

        protected abstract T CreateItem ();

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public void Dispose () 
        {
            foreach (var q in queue)
            {
                DisposeItem (q);
            }

            queue.Clear ();
        }

        protected abstract void DisposeItem (T t);

        Queue<T> queue = new Queue<T> ();
    }
}
