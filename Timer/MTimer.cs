using System;
using System.Collections.Generic;
using Cherry.Pool;
using UnityEngine;

namespace Cherry.Timer
{
    public class MTimer : IMTimer
    {
        private readonly Dictionary<string, TimerData> _idToTimers = new();
        private readonly List<TimerData> _list = new();
        private readonly Pool<TimerData> _timeDataPool = new();
        private readonly Queue<string> _unbindQueue = new();

        public MTimer()
        {
            Game.OnUpdate += OnUpdate;
        }

        public string Bind(float duration, Action<int> action, int repeat = 1, Action complete = null, float delay = 0,
            bool unscaled = false)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var timer = _timeDataPool.Spawn();
            timer.id = Game.GetGuid();
            timer.duration = duration;
            timer.remain = duration + delay;
            timer.action = action;
            timer.repeat = repeat;
            timer.count = 0;
            timer.complete = complete;
            timer.unscaled = unscaled;
            _list.Add(timer);
            _idToTimers.Add(timer.id, timer);

            return timer.id;
        }

        public string Bind(float duration, Action onComplete, float delay = 0, bool unscaled = false)
        {
            return Bind(duration, i => onComplete(), 1, null, delay, unscaled);
        }

        public void Unbind(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            _unbindQueue.Enqueue(id);
        }

        private void OnUpdate()
        {
            while (_unbindQueue.Count > 0)
            {
                var id = _unbindQueue.Dequeue();
                if (!_idToTimers.TryGetValue(id, out var data)) continue;

                _idToTimers.Remove(id);
                _list.Remove(data);
                _timeDataPool.Recycle(data);
            }

            if (_list.Count == 0) return;
            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            for (var index = _list.Count - 1; index > -1; index--)
            {
                var item = _list[index];
                if (!item.Update(item.unscaled ? unscaledDeltaTime : deltaTime)) continue;

                _list.RemoveAt(index);
                _timeDataPool.Recycle(item);
                _idToTimers.Remove(item.id);
            }
        }

        private class TimerData
        {
            public Action<int> action;
            public Action complete;
            public int count;
            public float duration;
            public string id;
            public float remain;
            public int repeat;
            public bool unscaled;

            public bool Update(float deltaTime)
            {
                remain -= deltaTime;
                if (remain > 0) return false;
                remain += duration;
                action(++count);
                if (repeat == 0) return false;
                if (repeat > 1)
                {
                    repeat--;
                    return false;
                }

                complete?.Invoke();
                return true;
            }
        }
    }
}