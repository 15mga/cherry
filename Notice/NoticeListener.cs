using System;
using System.Collections.Generic;
using System.Linq;

namespace Cherry.Notice
{
    public class NoticeListener : INoticeListener
    {
        private readonly List<NoticeObj> _noticeObjs = new();

        public void BindNotice(string name, Action<object> action, int count = 0)
        {
            var item = new NoticeObj(name, action, count);
            if (count > -1) item.onComplete += () => { UnbindNotice(name, action); };
            _noticeObjs.Add(item);
        }

        public void UnbindNotice(string name, Action<object> action)
        {
            _noticeObjs.Any(item =>
            {
                if (item.name != name || item.action != action) return false;

                item.Dispose();
                _noticeObjs.Remove(item);
                return true;
            });
        }

        public virtual void Dispose()
        {
            foreach (var item in _noticeObjs) item.Dispose();

            _noticeObjs.Clear();
        }

        private class NoticeObj
        {
            public NoticeObj(string name, Action<object> action, int count)
            {
                this.name = name;
                this.action = action;
                this.count = count;
                Game.Notice.BindNotice(name, OnNotice);
            }

            public string name { get; }
            public Action<object> action { get; private set; }
            public int count { get; private set; }
            public event Action onComplete;

            private void OnNotice(object data)
            {
                action(data);
                if (count > 0)
                {
                    count--;
                    if (count == 0) onComplete?.Invoke();
                }
            }

            public void Dispose()
            {
                action = null;
                onComplete = null;
                Game.Notice.UnbindNotice(name, OnNotice);
            }
        }
    }
}