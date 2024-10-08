using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Cherry.Recorder
{
    public class MRecorder : IMRecorder
    {
        private readonly Queue<action> _redo = new();
        private readonly Queue<action> _undo = new();

        public int MaxRecorder { get; set; } = 100;

        public void Do(Action redo, Action undo)
        {
            Assert.IsNotNull(redo);
            Assert.IsNotNull(undo);
            redo();
            _undo.Enqueue(new action{redo = redo, undo = undo});
            if (_undo.Count > MaxRecorder) _undo.Dequeue();
        }

        public void Redo()
        {
            var a = _redo.Dequeue();
            a.redo();
            _undo.Enqueue(a);
        }

        public void Undo()
        {
            var a = _undo.Dequeue();
            a.undo();
            _redo.Enqueue(a);
        }
        
        public class action
        {
            public Action redo { get; set; }
            public Action undo { get; set; }
        }
    }
}