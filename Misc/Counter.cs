using System;

namespace Cherry.Misc
{
    public class Counter
    {
        private long _count;

        private string _timerId;

        public event Action<long> onCount;

        public void Add(long val = 1)
        {
            _count += val;
        }

        public void Start(float duration = 1, bool unscaled = false)
        {
            _timerId = Game.Timer.Bind(duration, count =>
            {
                onCount?.Invoke(_count);
                _count = 0;
            }, 0, null, 0, unscaled);
        }

        public void Dispose()
        {
            Game.Timer.Unbind(_timerId);
            onCount = null;
        }
    }
}