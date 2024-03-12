using System;

namespace Cherry
{
    public interface IMTimer
    {
        string Bind(float duration, Action<int> action, int repeat = 1, Action complete = null, float delay = 0,
            bool unscaled = false);

        string Bind(float duration, Action onComplete, float delay = 0, bool unscaled = false);

        void Unbind(string id);
    }
}