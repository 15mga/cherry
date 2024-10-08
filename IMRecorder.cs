using System;

namespace Cherry
{
    public interface IMRecorder
    {
        bool CanRedo { get; }
        bool CanUndo { get; }
        int MaxRecorder { get; set; }
        void Do(Action redo, Action undo);
        void Redo();
        void Undo();
    }
}