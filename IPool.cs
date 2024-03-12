using System;

namespace Cherry
{
    public interface IPoolBase<T>
    {
        int Max { get; }
        int Min { get; }
        int UsedCount { get; }
        int PoolCount { get; }
        bool Recycle(T obj);
        void Every(Action<T> filter);
        void Clear();
    }

    public interface IPool<T> : IPoolBase<T>
    {
        T Spawn();
    }

    public interface IPoolAsync<T> : IPoolBase<T>
    {
        void Spawn(Action<T> action);
    }

    /// <summary>
    ///     对象池
    /// </summary>
    public interface IPool : IPool<object>
    {
    }
}