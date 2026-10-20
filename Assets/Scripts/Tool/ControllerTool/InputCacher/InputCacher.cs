using System.Collections.Generic;

namespace Kun.Tool
{
    public class InputCacher
    {
        public void AddCacheModule (InputCacheModule inputCacheModule)
        {
            inputCacheModules.Add (inputCacheModule);
        }

        List<InputCacheModule> inputCacheModules = new List<InputCacheModule> ();

        public void UpdateInput ()
        {
            inputCacheModules.ForEach (input => input.UpdateInput ());
        }

        public void OnFlushInput ()
        {
            inputCacheModules.ForEach (input => input.OnFlushInput ());
        }
    }

    public interface InputCacheModule
    {
        public void UpdateInput ();

        /// <summary>
        /// 取用之後把flag拿掉
        /// </summary>
        public void OnFlushInput ();
    }
}
