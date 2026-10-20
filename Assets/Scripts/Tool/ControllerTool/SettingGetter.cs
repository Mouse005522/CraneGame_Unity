using System;
using System.Threading;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// 讓使用者緩存Getter,
    /// 另一端隨時可以更新數值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingGetter<T>
    {
        public void UpdateValue (T newValue)
        {
            Value = newValue;

            if (onValueChanged != null) 
            {
                onValueChanged.Invoke(newValue);
            }
        }

        public T Value { get; private set; } = default;

        Action<T> onValueChanged;

        public void BindCallback (Action<T> callback)
        {
            this.onValueChanged += callback;
        }

        public void UnbindCallback (Action<T> callback)
        {
            this.onValueChanged -= callback;
        }
    }
}