using System;

namespace Cherry
{
    public interface IMRecorder
    {
        int MaxRecorder { get; set; }
        void Do(Action redo, Action undo);
        void Redo();
        void Undo();
    }
}