using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Kun.Tool
{
    public class UnityDispatcher : MonoBehaviour, Dispatcherable
    {
        public static UnityDispatcher Instance 
        {
            get 
            {
                if (instance == null) 
                {
                    instance = new GameObject ("[UnityDispatcher]").AddComponent<UnityDispatcher> ();
                    MonoBehaviour.DontDestroyOnLoad (instance.gameObject);
                }

                return instance;
            }
        }

        static UnityDispatcher instance;

        ReaderWriterLockSlim locker;

        List<Action> callbacks = new List<Action> ();

        void Awake ()
        {
            locker = new ReaderWriterLockSlim ();
        }

        public void EnqueueCallback (Action callback) 
        {
            locker.EnterWriteLock ();
            callbacks.Add (callback);
            locker.ExitWriteLock ();
        }

        // Update is called once per frame
        void Update ()
        {
            var copyCallbacks = new List<Action> ();
            locker.EnterWriteLock ();
            copyCallbacks = callbacks.ToList ();
            callbacks.Clear ();
            locker.ExitWriteLock ();

            if (copyCallbacks.Count > 0) 
            {
                copyCallbacks.ForEach (c => c.Invoke ());
            }
        }
    }
}
