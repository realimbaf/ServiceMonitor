using System;

namespace Wiki.Api.Help.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HelpShowOpenUrlAttribute : Attribute
    {
        public bool IsShow { get; private set; }

        public HelpShowOpenUrlAttribute(bool isShow)
        {
            this.IsShow = isShow;
        }
    }
}
