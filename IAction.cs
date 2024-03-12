using System;

namespace Cherry
{
    public interface IAction
    {
        void Execute<T>(string name, Action<T> action);
    }
}