using System;
using System.Collections.Generic;

namespace Cherry.Notice
{
    public class MNotice : IMNotice
    {
        private readonly Dictionary<string, Action<object>> _actions = new();

        public void BindNotice(string name, Action<object> action)
        {
            if (_actions.ContainsKey(name))
                _actions[name] += action;
            else
                _actions[name] = action;
        }

        public void UnbindNotice(string name, Action<object> action)
        {
            if (_actions.ContainsKey(name)) _actions[name] -= action;
        }

        public void DispatchNotice(string name, object data = null)
        {
            if (_actions.TryGetValue(name, out var action)) action?.Invoke(data);
        }

        public void ClearNotices()
        {
            _actions.Clear();
        }
    }
}