using System;

namespace Kun.Tool
{
    public class EasyInjectAttribute : Attribute
    {
        public EasyInjectAttribute ()
        {
            this.flag = null;
        }

        public EasyInjectAttribute (int flag)
        {
            this.flag = flag;
        }

        public int? flag { get; private set; }
    }
}