using System;

namespace Cherry
{
    public interface IMTrigger
    {
        void BindTrigger(string name, Func<bool> condition, Action action, int priority = 0);

        void BindTrigger(string name, Action action, int priority = 0);

        string BindTrigger(Func<bool> condition, Action action, int priority = 0);

        void BindOnceTrigger(Func<bool> condition, Action action = null, int priority = 0);

        void SetCondition(string name, Func<bool> condition, int priority = 0);

        void UnbindTrigger(string name);

        void ClearTriggers();
    }
}