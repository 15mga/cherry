using System;
using System.Collections.Generic;

namespace Cherry.Pool
{
    public abstract class PoolBase<T> : IPoolBase<T>
    {
        private readonly Action<T> _disposer;
        protected readonly IPoolHelper<T> _helper;
        protected readonly List<T> _list = new();
        protected readonly Queue<T> _pool = new();
        protected bool _helperInitialized;

        protected PoolBase(Action<T> disposer, int max, int min, IPoolHelper<T> helper)
        {
            Max = max;
            Min = min;
            _disposer = disposer;
            _helper = helper;
        }

        public int Max { get; }
        public int Min { get; }
        public int UsedCount => _list.Count;
        public int PoolCount => _pool.Count;

        public virtual bool Recycle(T obj)
        {
            if (!_list.Contains(obj) || _pool.Contains(obj)) return false;

            _list.Remove(obj);
            if (_pool.Count < Max)
            {
                _pool.Enqueue(obj);
                return true;
            }

            _disposer?.Invoke(obj);
            return false;
        }

        public virtual void Every(Action<T> filter)
        {
            foreach (var item in _list) filter(item);
        }

        public virtual void Clear()
        {
            foreach (var item in _pool) _disposer?.Invoke(item);
            _pool.Clear();
            _list.Clear();
        }
    }
}