using System;
using System.Collections.Generic;

namespace Cherry.Misc
{
    public class DynaClass : IDynaClass
    {
        private static readonly Dictionary<string, object> _nameToStaticValue = new();

        private static readonly Dictionary<string, Action> _nameToStaticAction1 = new();

        private static readonly Dictionary<string, Action<object>> _nameToStaticAction2 = new();

        private static readonly Dictionary<string, Action<object[]>> _nameToStaticAction3 = new();

        private static readonly Dictionary<string, Func<object>> _nameToStaticFunc1 = new();

        private static readonly Dictionary<string, Func<object, object>> _nameToStaticFunc2 = new();

        private static readonly Dictionary<string, Func<object[], object>> _nameToStaticFunc3 = new();

        private readonly Dictionary<string, Action<IDynaClass>> _nameToAction1 = new();

        private readonly Dictionary<string, Action<IDynaClass, object>> _nameToAction2 = new();

        private readonly Dictionary<string, Action<IDynaClass, object[]>> _nameToAction3 = new();

        private readonly Dictionary<string, Func<IDynaClass, object>> _nameToFunc1 = new();

        private readonly Dictionary<string, Func<IDynaClass, object, object>> _nameToFunc2 = new();

        private readonly Dictionary<string, Func<IDynaClass, object[], object>> _nameToFunc3 = new();

        private readonly Dictionary<string, object> _nameToValue = new();

        public void AddField(string name, object value)
        {
            _nameToValue[name] = value;
        }

        public object GetValue(string name)
        {
            return _nameToValue.TryGetValue(name, out var value) ? value : null;
        }

        public T GetValue<T>(string name)
        {
            return _nameToValue.TryGetValue(name, out var value) ? (T)value : default;
        }

        public void AddAction(string name, Action<IDynaClass> action)
        {
            _nameToAction1[name] = action;
        }

        public void AddAction(string name, Action<IDynaClass, object> action)
        {
            _nameToAction2[name] = action;
        }

        public void Action(string name)
        {
            if (_nameToAction1.TryGetValue(name, out var action)) action(this);
        }

        public void Action(string name, object data)
        {
            if (_nameToAction2.TryGetValue(name, out var action)) action(this, data);
        }

        public void AddAction(string name, Action<IDynaClass, object[]> action)
        {
            _nameToAction3[name] = action;
        }

        public void Action(string name, params object[] data)
        {
            if (_nameToAction3.TryGetValue(name, out var action)) action(this, data);
        }

        public void AddFunc(string name, Func<IDynaClass, object> func)
        {
            _nameToFunc1[name] = func;
        }

        public object Func(string name)
        {
            return _nameToFunc1.TryGetValue(name, out var func) ? func(this) : null;
        }

        public T Func<T>(string name)
        {
            if (_nameToFunc1.TryGetValue(name, out var func)) return (T)func(this);

            return default;
        }

        public void AddFunc(string name, Func<IDynaClass, object, object> func)
        {
            _nameToFunc2[name] = func;
        }

        public object Func(string name, object data)
        {
            return _nameToFunc2.TryGetValue(name, out var func) ? func(this, data) : null;
        }

        public T Func<T>(string name, object data)
        {
            return _nameToFunc2.TryGetValue(name, out var func) ? (T)func(this, data) : default;
        }

        public void AddFunc(string name, Func<IDynaClass, object[], object> func)
        {
            _nameToFunc3[name] = func;
        }

        public object Func(string name, params object[] data)
        {
            return _nameToFunc3.TryGetValue(name, out var func) ? func(this, data) : null;
        }

        public T Func<T>(string name, params object[] data)
        {
            return _nameToFunc3.TryGetValue(name, out var func) ? (T)func(this, data) : default;
        }

        public static void AddStaticField(string name, object value)
        {
            _nameToStaticValue[name] = value;
        }

        public static object GetStaticValue(string name)
        {
            return _nameToStaticValue.TryGetValue(name, out var value) ? value : null;
        }

        public static T GetStaticValue<T>(string name)
        {
            return _nameToStaticValue.TryGetValue(name, out var value) ? (T)value : default;
        }

        public static void AddStaticAction(string name, Action action)
        {
            _nameToStaticAction1.Add(name, action);
        }

        public static void StaticAction(string name)
        {
            if (_nameToStaticAction1.TryGetValue(name, out var action)) action();
        }

        public static void AddStaticAction(string name, Action<object> action)
        {
            _nameToStaticAction2.Add(name, action);
        }

        public static void StaticAction(string name, object data)
        {
            if (_nameToStaticAction2.TryGetValue(name, out var action)) action(data);
        }

        public static void AddStaticAction(string name, Action<object[]> action)
        {
            _nameToStaticAction3.Add(name, action);
        }

        public static void StaticAction(string name, params object[] data)
        {
            if (_nameToStaticAction3.TryGetValue(name, out var action)) action(data);
        }

        public static void AddStaticFunc(string name, Func<object> func)
        {
            _nameToStaticFunc1[name] = func;
        }

        public static object StaticFunc(string name)
        {
            return _nameToStaticFunc1.TryGetValue(name, out var func) ? func() : null;
        }

        public static T StaticFunc<T>(string name)
        {
            return _nameToStaticFunc1.TryGetValue(name, out var func) ? (T)func() : default;
        }

        public static void AddStaticFunc(string name, Func<object, object> func)
        {
            _nameToStaticFunc2[name] = func;
        }

        public static object StaticFunc(string name, object data)
        {
            return _nameToStaticFunc2.TryGetValue(name, out var func) ? func(data) : null;
        }

        public static T StaticFunc<T>(string name, object data)
        {
            return _nameToStaticFunc2.TryGetValue(name, out var func) ? (T)func(data) : default;
        }

        public static void AddStaticFunc(string name, Func<object[], object> func)
        {
            _nameToStaticFunc3[name] = func;
        }

        public static object StaticFunc(string name, params object[] data)
        {
            return _nameToStaticFunc3.TryGetValue(name, out var func) ? func(data) : null;
        }

        public static T StaticFunc<T>(string name, params object[] data)
        {
            return _nameToStaticFunc3.TryGetValue(name, out var func) ? (T)func(data) : default;
        }
    }
}