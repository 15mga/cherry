using System;

namespace Cherry
{
    public interface IDynaClass
    {
        void AddField(string name, object value);
        object GetValue(string name);
        T GetValue<T>(string name);
        void AddAction(string name, Action<IDynaClass> action);
        void Action(string name);
        void AddAction(string name, Action<IDynaClass, object> action);
        void Action(string name, object data);
        void AddAction(string name, Action<IDynaClass, object[]> action);
        void Action(string name, params object[] data);
        void AddFunc(string name, Func<IDynaClass, object> func);
        object Func(string name);
        T Func<T>(string name);
        void AddFunc(string name, Func<IDynaClass, object, object> func);
        object Func(string name, object data);
        T Func<T>(string name, object data);
        void AddFunc(string name, Func<IDynaClass, object[], object> func);
        object Func(string name, params object[] data);
        T Func<T>(string name, params object[] data);
    }
}