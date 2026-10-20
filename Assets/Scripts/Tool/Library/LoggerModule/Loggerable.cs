using System;

namespace Kun.Tool
{
    public interface Loggerable
    {
        void Log (object msg);

        void Warn (object msg);

        void Error (object msg);

        void Exception (Exception e);
    }
}