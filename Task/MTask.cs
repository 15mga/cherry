using System;
using System.Collections.Generic;

namespace Cherry.Task
{
    public class MTask : IMTask
    {
        private readonly List<Tuple<Action, int>> _newTasks = new();

        private readonly List<Action> _safeActions = new();
        private readonly object _safeLocker = new();

        private readonly Dictionary<string, Action> _tagToSingleton = new();
        private readonly Queue<Tuple<Action, int>> _tasks = new();
        private int _weightPerFrame = 5;

        public MTask()
        {
            Game.OnUpdate += OnUpdate;
        }

        public void SetWeightPerFrame(int weightPerFrame)
        {
            _weightPerFrame = weightPerFrame;
        }

        public void PushTask(Action action, int weight = 1)
        {
            _newTasks.Add(new Tuple<Action, int>(action, weight));
        }

        public void PushAsyncTasks(Action onComplete, params Action<Action>[] actions)
        {
            var total = actions.Length;
            if (total == 0) onComplete?.Invoke();
            var count = total;
            for (var i = 0; i < total; i++)
                actions[i](() =>
                {
                    count--;
                    if (count == 0) onComplete?.Invoke();
                });
        }

        public void PushSingletonAsyncTasks(string tag, Action onComplete, params Action<Action>[] actions)
        {
            if (_tagToSingleton.ContainsKey(tag))
            {
                _tagToSingleton[tag] += onComplete;
            }
            else
            {
                _tagToSingleton.Add(tag, onComplete);
                PushAsyncTasks(() =>
                {
                    _tagToSingleton[tag]();
                    _tagToSingleton.Remove(tag);
                }, actions);
            }
        }

        public void PushTaskQueue(params Action<Action>[] actions)
        {
            PushTaskQueue(null, null, actions);
        }

        public void PushTaskQueue(Action onComplete, params Action<Action>[] actions)
        {
            PushTaskQueue(onComplete, null, actions);
        }

        public void PushTaskQueue(Action onComplete = null, Action<float> onProgress = null,
            params Action<Action>[] actions)
        {
            var index = 0;
            var total = actions.Length;
            onProgress?.Invoke(0);

            void DoTask()
            {
                if (index < total)
                    actions[index](() =>
                    {
                        index++;
                        onProgress?.Invoke(index * 1f / total);
                        DoTask();
                    });
                else
                    onComplete?.Invoke();
            }

            DoTask();
        }

        public void PushTaskQueue(Action onComplete, Action<float> onProgress,
            params Action<Action<float>, Action>[] actions)
        {
            var index = 0;
            var total = actions.Length;
            onProgress?.Invoke(0);

            void DoTask()
            {
                if (index < total)
                    actions[index](v => { onProgress?.Invoke((index + v) / total); }, () =>
                    {
                        index++;
                        DoTask();
                    });
                else
                    onComplete?.Invoke();
            }

            DoTask();
        }

        public void PushSafeTask(Action action)
        {
            lock (_safeLocker)
            {
                _safeActions.Add(action);
            }
        }

        private void OnUpdate()
        {
            for (var index = 0; index < _newTasks.Count; index++) _tasks.Enqueue(_newTasks[index]);

            _newTasks.Clear();

            var weight = 0;
            while (_tasks.Count > 0 && weight < _weightPerFrame)
            {
                var task = _tasks.Dequeue();
                weight += task.Item2;
                task.Item1();
            }

            lock (_safeLocker)
            {
                for (var i = 0; i < _safeActions.Count; i++) _safeActions[i]();
                _safeActions.Clear();
            }
        }
    }
}