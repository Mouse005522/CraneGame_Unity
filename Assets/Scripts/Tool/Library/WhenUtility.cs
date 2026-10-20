using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kun.Tool
{
    public static class WhenUtility
    {
        public static void DoWhen<T> (object obj, Action<T> callback)
        {
            if (obj is IWhenProxy whenable)
            {
                whenable.When (callback);
            }
            else
            {
                if (obj is T t)
                {
                    callback.Invoke (t);
                }
            }
        }
    }
}
