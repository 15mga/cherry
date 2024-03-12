using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AChild : Attribute
    {
        public AChild(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}