using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ASubView : Attribute
    {
        public ASubView(Type type, params string[] names)
        {
            Type = type;
            Names = names;
        }

        public Type Type { get; }
        public string[] Names { get; }
    }
}