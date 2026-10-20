using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kun.Tool
{
    /// <summary>
    /// Service迭代器容器,
    /// 避免直接注入List
    /// </summary>
    public class ServiceIterContrainer<T>
    {
        public List<T> datas = new List<T> ();
    }
}

