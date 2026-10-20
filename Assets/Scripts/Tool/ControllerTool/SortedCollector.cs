using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class SortedCollector : MonoBehaviour
    {
        public List<GameObject> values = new List<GameObject> ();

        /// <summary>
        /// 對每個物件GetComponent後回傳
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetValuesByType<T> ()
        {
            List<T> coms = new List<T> ();

            foreach (var go in values)
            {
                if (go != null) 
                {
                    var com = go.GetComponent<T> ();

                    if (com != null) 
                    {
                        coms.Add (com);
                    }
                    else
                    {
                        LoggerRouter.Error ($"{this.gameObject.name}/{go.name} 上 不具有指定的Com -> {nameof (T)}");
                    }
                }
                else
                {
                    LoggerRouter.Error ($"{this.gameObject.name}上具有遺失的綁定");
                }
            }

            return coms;
        }
    }
}

