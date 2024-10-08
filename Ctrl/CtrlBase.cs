using System;
using Cherry.Notice;

namespace Cherry.Ctrl
{
    public abstract class CtrlBase : ICtrl
    {
        private readonly NoticeListener _listener = new();

        public abstract void Initialize(Action onComplete = null);

        public void Dispose()
        {
            _listener.Dispose();
        }

        protected void BindNotice(string name, Action<object> action, int count = 0)
        {
            _listener.BindNotice(name, action, count);
        }

        protected void UnbindNotice(string name, Action<object> action)
        {
            _listener.UnbindNotice(name, action);
        }
    }

    public abstract class CtrlBase<T> : CtrlBase where T : IModel
    {
        protected CtrlBase()
        {
            Model = Game.Model.Get<T>();
        }

        protected T Model { get; }
    }
}