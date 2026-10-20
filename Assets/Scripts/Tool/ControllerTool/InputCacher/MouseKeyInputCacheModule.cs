using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class MouseKeyInputCacheModule : InputCacheModule
    {
        public MouseKeyInputCacheModule (Loggerable loggerable) 
        {
            this.loggerable = loggerable;
        }

        Loggerable loggerable;

        List<KeyCodeCache> recorderKeys = new List<KeyCodeCache> ();

        /// <summary>
        /// 會被down跟up得先放這裡註冊
        /// </summary>
        /// <param name="keyCode"></param>
        public void AddRecorderKey (KeyCode keyCode)
        {
            if (recorderKeys.Exists (cache => cache.keyCode == keyCode) == false)
            {
                var cache = new KeyCodeCache (keyCode);
                recorderKeys.Add (cache);
            }
            else
            {
                Debug.LogError ($"add same key -> {keyCode}");
            }
        }

        public bool GetKey (KeyCode keyCode)
        {
            return Input.GetKey (keyCode);
        }

        public bool GetKeyDown (KeyCode keyCode)
        {
            if (TryGetKeyCodeCache (keyCode, out KeyCodeCache keyCodeCache))
            {
                return keyCodeCache.hasDown;
            }
            else
            {
                return false;
            }
        }

        public bool GetKeyUp (KeyCode keyCode)
        {
            if (TryGetKeyCodeCache (keyCode, out KeyCodeCache keyCodeCache))
            {
                return keyCodeCache.hasUp;
            }
            else
            {
                return false;
            }
        }

        void InputCacheModule.OnFlushInput ()
        {
            recorderKeys.ForEach (key => key.Clear ());
        }

        void InputCacheModule.UpdateInput ()
        {
            recorderKeys.ForEach (cache =>
            {
                if (Input.GetKeyDown (cache.keyCode))
                {
                    cache.hasDown = true;
                }

                if (Input.GetKeyUp (cache.keyCode))
                {
                    cache.hasUp = true;
                }
            });
        }

        bool TryGetKeyCodeCache (KeyCode keyCode, out KeyCodeCache keyCodeCache)
        {
            keyCodeCache = recorderKeys.Find (cache => cache.keyCode == keyCode);

            if (keyCodeCache == null)
            {
                Debug.LogError ($"not exist key -> {keyCode}");
            }

            return keyCodeCache != null;
        }

        class KeyCodeCache
        {
            public KeyCodeCache (KeyCode keyCode)
            {
                this.keyCode = keyCode;
            }

            public KeyCode keyCode;

            public bool hasUp;
            public bool hasDown;

            public void Clear ()
            {
                hasUp = false;
                hasDown = false;
            }
        }
    }
}