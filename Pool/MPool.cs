using System;
using System.Collections.Generic;
using System.Linq;

namespace Cherry.Pool
{
    /// <summary>
    ///     非线程安全,如果跨线程调用,需要使用Lock多态方法
    /// </summary>
    public class MPool : IMPool
    {
        private readonly Dictionary<object, string> _objToTag = new();
        private readonly Dictionary<string, IPool> _pools = new();

        public virtual IPool GetPool<T>()
        {
            return GetPool(typeof(T));
        }

        public virtual IPool GetPool(Type type)
        {
            return GetPool(type.FullName);
        }

        public virtual IPool GetPool(string tag)
        {
            return _pools.TryGetValue(tag, out var pool) ? pool : null;
        }

        public virtual bool HasPool<T>()
        {
            return HasPool(typeof(T));
        }

        public virtual bool HasPool(Type type)
        {
            return HasPool(type.FullName);
        }

        public virtual bool HasPool(string type)
        {
            return _pools.ContainsKey(type);
        }

        public virtual List<string> GetPoolKeys()
        {
            return _pools.Keys.ToList();
        }

        public virtual IPool RegisterPool(string tag, Func<object> creator, Action<object> disposer = null,
            int max = 10, int min = 0, IPoolHelper helper = null)
        {
            if (creator == null)
            {
                Game.Log.Error("func must not null");
                return null;
            }

            if (_pools.ContainsKey(tag))
            {
                Game.Log.Error($"exist pool {tag}");
                return null;
            }

            var pool = new global::Cherry.Pool.Pool(creator, disposer, max, min, helper);
            _pools.Add(tag, pool);
            return pool;
        }

        public virtual void RegisterPool(string tag, IPool pool)
        {
            _pools.Add(tag, pool);
        }

        public virtual IPool RegisterPool(Type type, Func<object> creator = null, Action<object> disposer = null,
            int max = 20, int min = 0, IPoolHelper helper = null)
        {
            return RegisterPool(type.FullName, creator ?? (() => Activator.CreateInstance(type)), disposer, max, min,
                helper);
        }

        public virtual void RegisterPool(Type type, IPool pool)
        {
            _pools.Add(type.FullName, pool);
        }

        public virtual IPool RegisterPool<T>(Func<object> creator = null, Action<object> disposer = null, int max = 10,
            int min = 0, IPoolHelper helper = null)
            where T : new()
        {
            return RegisterPool(typeof(T), creator ?? (() => new T()), disposer, max, min, helper);
        }

        public virtual void RegisterPool<T>(IPool pool)
        {
            RegisterPool(typeof(T), pool);
        }

        public virtual object SpawnInstance(string tag)
        {
            if (!_pools.TryGetValue(tag, out var pool))
            {
                Game.Log.Error($"not exist {tag}");
                return null;
            }

            var obj = pool.Spawn();
            _objToTag.Add(obj, tag);
            return obj;
        }

        public virtual object SpawnInstance(Type type)
        {
            if (!HasPool(type)) RegisterPool(type);

            return SpawnInstance(type.FullName);
        }

        public virtual T SpawnInstance<T>(string tag)
        {
            return (T)SpawnInstance(tag);
        }

        public virtual T SpawnInstance<T>() where T : new()
        {
            if (!HasPool<T>()) RegisterPool<T>();

            return (T)SpawnInstance(typeof(T));
        }

        public virtual void RecycleInstance(object obj)
        {
            if (obj == null) return;
            if (!_objToTag.TryGetValue(obj, out var tag)) return;

            _objToTag.Remove(obj);

            _pools[tag].Recycle(obj);
        }

        public virtual void ClearPool(string tag)
        {
            if (!_pools.TryGetValue(tag, out var pool))
            {
                Game.Log.Error($"not exist {tag}");
                return;
            }

            _pools.Remove(tag);

            pool.Every(obj => { _objToTag.Remove(obj); });
            pool.Clear();
        }

        public virtual void ClearPool(Type type)
        {
            ClearPool(type.FullName);
        }

        public virtual void ClearPool<T>()
        {
            ClearPool(typeof(T));
        }

        public virtual void ClearPools()
        {
            foreach (var item in _pools)
            {
                var pool = item.Value;
                pool.Every(obj => { _objToTag.Remove(obj); });
                pool.Clear();
            }

            _pools.Clear();
        }

        public void Dispose()
        {
            ClearPools();
        }
    }
}