using System;

namespace Cherry
{
    public interface IMTask
    {
        void SetWeightPerFrame(int weightPerFrame);
        void PushTask(Action action, int weight = 1);
        void PushAsyncTasks(Action onComplete, params Action<Action>[] actions);
        void PushSingletonAsyncTasks(string tag, Action onComplete, params Action<Action>[] actions);
        void PushTaskQueue(params Action<Action>[] actions);
        void PushTaskQueue(Action onComplete, params Action<Action>[] actions);
        void PushTaskQueue(Action onComplete = null, Action<float> onProgress = null, params Action<Action>[] actions);
        void PushTaskQueue(Action onComplete, Action<float> onProgress, params Action<Action<float>, Action>[] actions);
        void PushSafeTask(Action action);
    }
}