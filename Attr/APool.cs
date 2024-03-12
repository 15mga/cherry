using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class APool : Attribute
    {
        public APool(string name, int max = 10, int min = 0)
        {
            Name = name;
            Max = max;
            Min = min;
        }

        public string Name { get; }
        public int Max { get; }
        public int Min { get; }
    }
}