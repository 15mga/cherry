using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AEnter : Attribute
    {
        public AEnter(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}