using System;

namespace Cherry
{
    public interface IMCtrl : IManager<ICtrl>
    {
    }

    public interface ICtrl : IDisposable
    {
        void Initialize(Action onComplete = null);
    }
}