using System;

namespace Cherry.Pool
{
    public class Pool<T> : PoolBase<T>, IPool<T>
    {
        private readonly Func<T> _creator;

        private bool _checkMin;

        public Pool(Func<T> creator = null, Action<T> disposer = null, int max = 10,
            int min = 0, IPoolHelper<T> helper = null) : base(disposer, max, min, helper)
        {
            _creator = creator ?? (() => (T)Activator.CreateInstance(typeof(T)));
            if (min > 0) CheckMin();
        }

        public virtual T Spawn()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                _helper?.Set(obj);

                if (_pool.Count < Min) CheckMin();
            }
            else
            {
                obj = _creator();
                if (_helper != null)
                {
                    if (_helperInitialized)
                    {
                        _helper.Set(obj);
                    }
                    else
                    {
                        _helper.Init(obj);
                        _helperInitialized = true;
                    }
                }
            }

            _list.Add(obj);
            return obj;
        }

        private void CheckMin()
        {
            if (_checkMin) return;
            _checkMin = true;
            Game.OnUpdate += OnCheckMin;
        }

        private void OnCheckMin()
        {
            if (_pool.Count < Min)
            {
                _pool.Enqueue(_creator());
            }
            else
            {
                _checkMin = false;
                Game.OnUpdate -= OnCheckMin;
            }
        }
    }

    public class Pool : Pool<object>, IPool
    {
        public Pool(Func<object> creator = null, Action<object> disposer = null, int max = 10, int min = 5,
            IPoolHelper helper = null) : base(creator, disposer, max, min, helper)
        {
        }
    }
}