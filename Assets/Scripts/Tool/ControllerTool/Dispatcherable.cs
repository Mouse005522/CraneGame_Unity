using System;

namespace Kun.Tool
{
    public interface Dispatcherable 
    {
        void EnqueueCallback (Action callback);
    }
}