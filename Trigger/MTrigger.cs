using System;
using System.Collections.Generic;

namespace Cherry.Trigger
{
    public class MTrigger : IMTrigger
    {
        private readonly Dictionary<string, Trigger> _nameToTrigger = new();
        private readonly List<Trigger> _triggers = new();
        private int _triggerPriority;

        public void BindOnceTrigger(Func<bool> condition, Action action = null, int priority = 0)
        {
            CheckAdd();
            var cond = new Trigger
            {
                Condition = condition,
                Action = action,
                Priority = priority,
                Once = true
            };
            _triggers.Add(cond);
            _triggers.Sort();
        }

        public string BindTrigger(Func<bool> condition, Action action, int priority = 0)
        {
            var name = Game.GetGuid();
            BindTrigger(name, condition, action, priority);
            return name;
        }

        public void BindTrigger(string name, Func<bool> condition, Action action, int priority = 0)
        {
            if (_nameToTrigger.ContainsKey(name))
            {
                Game.Log.Error($"exist Trigger name {name}");
                return;
            }

            var cond = new Trigger
            {
                Condition = condition,
                Action = action,
                Priority = priority
            };
            CheckAdd();
            _nameToTrigger.Add(name, cond);
            _triggers.Add(cond);
            _triggers.Sort();
        }

        public void BindTrigger(string name, Action action, int priority = 0)
        {
            BindTrigger(name, null, action, priority);
        }


        public void SetCondition(string name, Func<bool> condition, int priority = 0)
        {
            if (_nameToTrigger.TryGetValue(name, out var trigger))
            {
                trigger.Condition = condition;
                if (trigger.Priority != priority) _triggers.Sort();
            }
            else
            {
                Game.Log.Warn($"not exist name {name}");
            }
        }

        public void UnbindTrigger(string name)
        {
            if (!_nameToTrigger.TryGetValue(name, out var trigger))
            {
                Game.Log.Warn($"not exist name {name}");
                return;
            }

            _triggers.Remove(trigger);
            _nameToTrigger.Remove(name);
            CheckRemove();
        }

        public void ClearTriggers()
        {
            _nameToTrigger.Clear();
            _triggers.Clear();
            Game.OnUpdate -= OnUpdate;
        }


        private void CheckAdd()
        {
            if (_triggers.Count > 0) return;
            Game.OnUpdate += OnUpdate;
        }

        private void CheckRemove()
        {
            if (_triggers.Count > 0) return;
            Game.OnUpdate -= OnUpdate;
        }

        private void OnUpdate()
        {
            _triggerPriority = 0;

            var removeList = new List<Trigger>();
            var list = new List<Trigger>(_triggers);
            foreach (var item in list)
            {
                if (item.Priority < _triggerPriority) return;
                if (item.Condition == null || !item.Condition()) continue;

                // Game.Log.Debug($"trigger: {item.GetHashCode()}");
                item.Action?.Invoke();
                _triggerPriority = item.Priority;
                if (item.Once) removeList.Add(item);
            }

            foreach (var trigger in removeList) _triggers.Remove(trigger);
        }

        private class Trigger : IComparable<Trigger>
        {
            public Action Action;
            public Func<bool> Condition;
            public bool Once;
            public int Priority;

            public int CompareTo(Trigger other)
            {
                if (ReferenceEquals(this, other)) return 0;
                return ReferenceEquals(null, other) ? 1 : Priority.CompareTo(other.Priority);
            }
        }
    }
}