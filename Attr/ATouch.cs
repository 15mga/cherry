using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ATouch : Attribute
    {
        public ATouch(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}