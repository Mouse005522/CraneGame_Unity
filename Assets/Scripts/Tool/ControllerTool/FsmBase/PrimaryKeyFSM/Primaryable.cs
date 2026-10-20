using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Kun.Tool
{
    public interface Primaryable<T> where T : struct
    {
        /// <summary>
        /// 回傳唯一key
        /// </summary>
        T PrimaryKey { get; }
    }
}

