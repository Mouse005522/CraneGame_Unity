using System;

namespace Kun.Tool
{
    public class EasyInjectBindingAttribute : Attribute
    {
        public EasyInjectBindingAttribute ()
        {
            this.flag = null;
        }

        public EasyInjectBindingAttribute (int flag)
        {
            this.flag = flag;
        }

        public int? flag { get; private set; }
    }

    public interface EasyInjectBindingHandler
    {
        void OnServiceBinding ();
    }
}