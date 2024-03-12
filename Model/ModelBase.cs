using System;

namespace Cherry.Model
{
    public abstract class ModelBase : IModel
    {
        public virtual void Initialize(Action onComplete = null)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}