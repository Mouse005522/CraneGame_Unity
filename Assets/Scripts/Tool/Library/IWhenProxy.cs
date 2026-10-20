using Kun.Tool;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    public interface IWhenProxy
    {
        public void When<T> (Action<T> callback);
    }
}
