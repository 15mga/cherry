using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ADrag : Attribute
    {
        public ADrag(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}