using System;
using System.Collections.Generic;

namespace Cherry.Misc
{
    public class AssemblyTypesFilter
    {
        private readonly Dictionary<Func<Type, bool>, Action<Type>> filters = new();

        public void Add(Func<Type, bool> filter, Action<Type> action)
        {
            filters.Add(filter, action);
        }

        public void Filter(Type type)
        {
            var types = type.Assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.IsAbstract) continue;

                foreach (var item in filters)
                    if (item.Key(t))
                        item.Value(t);
            }
        }

        public void Filter<T>()
        {
            Filter(typeof(T));
        }

        public void Filter(object obj)
        {
            Filter(obj.GetType());
        }
    }
}