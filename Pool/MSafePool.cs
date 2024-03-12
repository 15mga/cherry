using System;

namespace Cherry.Pool
{
    public class MSafePool : MPool
    {
        private readonly object lockObj = new();

        public override IPool GetPool<T>()
        {
            lock (lockObj)
            {
                return base.GetPool<T>();
            }
        }

        public override IPool GetPool(Type type)
        {
            lock (lockObj)
            {
                return base.GetPool(type);
            }
        }

        public override IPool GetPool(string tag)
        {
            lock (lockObj)
            {
                return base.GetPool(tag);
            }
        }

        public override bool HasPool<T>()
        {
            lock (lockObj)
            {
                return base.HasPool<T>();
            }
        }

        public override bool HasPool(Type type)
        {
            lock (lockObj)
            {
                return base.HasPool(type);
            }
        }

        public override bool HasPool(string type)
        {
            lock (lockObj)
            {
                return base.HasPool(type);
            }
        }

        public override IPool RegisterPool(string tag, Func<object> creator, Action<object> disposer = null,
            int max = 20, int min = 0, IPoolHelper helper = null)
        {
            lock (lockObj)
            {
                return base.RegisterPool(tag, creator, disposer, max, min, helper);
            }
        }

        public override void RegisterPool(string tag, IPool pool)
        {
            lock (lockObj)
            {
                base.RegisterPool(tag, pool);
            }
        }

        public override IPool RegisterPool(Type type, Func<object> creator = null, Action<object> disposer = null,
            int max = 20, int min = 5, IPoolHelper helper = null)
        {
            lock (lockObj)
            {
                return base.RegisterPool(type, creator, disposer, max, min, helper);
            }
        }

        public override void RegisterPool(Type type, IPool pool)
        {
            lock (lockObj)
            {
                base.RegisterPool(type, pool);
            }
        }

        public override IPool RegisterPool<T>(Func<object> creator = null, Action<object> disposer = null, int max = 20,
            int min = 5,
            IPoolHelper helper = null)
        {
            lock (lockObj)
            {
                return base.RegisterPool<T>(creator, disposer, max, min, helper);
            }
        }

        public override void RegisterPool<T>(IPool pool)
        {
            lock (lockObj)
            {
                base.RegisterPool<T>(pool);
            }
        }

        public override object SpawnInstance(string tag)
        {
            lock (lockObj)
            {
                return base.SpawnInstance(tag);
            }
        }

        public override object SpawnInstance(Type type)
        {
            lock (lockObj)
            {
                return base.SpawnInstance(type);
            }
        }

        public override T SpawnInstance<T>(string tag)
        {
            lock (lockObj)
            {
                return base.SpawnInstance<T>(tag);
            }
        }

        public override T SpawnInstance<T>()
        {
            lock (lockObj)
            {
                return base.SpawnInstance<T>();
            }
        }

        public override void RecycleInstance(object obj)
        {
            lock (lockObj)
            {
                base.RecycleInstance(obj);
            }
        }

        public override void ClearPool(string tag)
        {
            lock (lockObj)
            {
                base.ClearPool(tag);
            }
        }

        public override void ClearPool(Type type)
        {
            lock (lockObj)
            {
                base.ClearPool(type);
            }
        }

        public override void ClearPool<T>()
        {
            lock (lockObj)
            {
                base.ClearPool<T>();
            }
        }

        public override void ClearPools()
        {
            lock (lockObj)
            {
                base.ClearPools();
            }
        }
    }
}