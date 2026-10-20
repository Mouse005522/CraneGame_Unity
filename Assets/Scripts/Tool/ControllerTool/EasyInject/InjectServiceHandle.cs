using System;

namespace Kun.Tool
{
    /// <summary>
    /// 讓外部也可以提供Service工廠的介面
    /// </summary>
    public interface InjectServiceHandle
    {
        public bool TryGetService (Type type, out object service);
    }
}