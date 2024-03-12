using System;

namespace Cherry.Misc
{
    /// <summary>
    ///     可加锁的对象
    /// </summary>
    public interface ILockable
    {
        void Lock(Action action);
        T Lock<T>(Func<T> func);
    }

    public class Lockable : ILockable
    {
        private readonly object lockObj = new();

        public void Lock(Action action)
        {
            lock (lockObj)
            {
                action();
            }
        }

        public T Lock<T>(Func<T> func)
        {
            T result;
            lock (lockObj)
            {
                result = func();
            }

            return result;
        }
    }

    public class LockData<T>
    {
        private readonly object lockObj = new();

        private T data;

        public void Get(Action<T> action)
        {
            lock (lockObj)
            {
                action(data);
            }
        }

        public void Set(Func<T, T> func)
        {
            lock (lockObj)
            {
                data = func(data);
            }
        }
    }
}