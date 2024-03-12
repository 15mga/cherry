using System;

namespace Cherry
{
    public interface IManager<T> where T : IDisposable
    {
        bool Has(T type, string name = null);
        bool Has<CT>(string name = null);
        void Add(T instance, string name = null);
        T Add(Type type, string name = null);
        CT Add<CT>(string name = null) where CT : T, new();
        T Add(Type parentType, Type childType);
        CT Add<PT, CT>() where PT : T where CT : PT, new();
        T Get(string name);
        T Get(Type type);
        CT Get<CT>() where CT : T;
        CT Get<PT, CT>() where PT : T where CT : PT;
        T Remove(Type type);
        CT Remove<CT>() where CT : T;
        void Dispose(Action<string, T> onDispose = null);
        void Every(Action<T> action);
        void Every(Action<string, T> action);
    }
}