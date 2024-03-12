using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AView : Attribute
    {
        public AView(string name = null)
        {
            Name = name;
        }

        public string Name { get; }
    }
}