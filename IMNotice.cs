using System;

namespace Cherry
{
    public interface IMNotice
    {
        void BindNotice(string name, Action<object> action);

        void UnbindNotice(string name, Action<object> action);

        void DispatchNotice(string name, object data = null);

        void ClearNotices();
    }

    public interface INoticeListener
    {
        void BindNotice(string name, Action<object> action, int count = 0);

        void UnbindNotice(string name, Action<object> action);

        void Dispose();
    }
}