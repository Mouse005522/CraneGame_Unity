using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    public class SettingProvider
    {
        public Lazy<T> GetSetting<T> ()
        {
            return new Lazy<T> (() => Activator.CreateInstance<T> ());
        }
    }
}