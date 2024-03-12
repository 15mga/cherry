using System;

namespace Cherry
{
    public interface IMModel : IManager<IModel>
    {
    }

    public interface IModel : IDisposable
    {
        void Initialize(Action onComplete = null);
    }
}