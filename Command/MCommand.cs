using System;
using System.Collections.Generic;

namespace Cherry.Command
{
    public class MCommand : IMCommand
    {
        private readonly Dictionary<string, object> _nameToExecutor = new();

        public void Execute<T>(string name, Action<T> action)
        {
            if (!_nameToExecutor.TryGetValue(name, out var obj)) return;
            var executor = (Executor<T>)obj;
            action(executor.Param);
            executor.Action(executor.Param);
        }

        public void BindCommand<T>(string name, Action<T> action) where T : new()
        {
            var param = new T();
            if (_nameToExecutor.ContainsKey(name))
            {
                Game.Log.Warn($"exist type {name}");
                return;
            }

            _nameToExecutor.Add(name, new Executor<T> { Action = action, Param = param });
        }

        public void Execute(string name, object data = null)
        {
            if (!_nameToExecutor.TryGetValue(name, out var obj)) return;
            var executor = (Executor<object>)obj;
            executor.Action(data);
        }

        public void BindCommand(string name, Action<object> action)
        {
            if (_nameToExecutor.ContainsKey(name))
            {
                Game.Log.Warn($"exist type {name}");
                return;
            }

            _nameToExecutor.Add(name, new Executor<object> { Action = action });
        }

        public void BindCommand(ICommand command)
        {
            BindCommand(command.Name, command.Execute);
        }

        public void BindCommand<T>() where T : ICommand, new()
        {
            BindCommand(new T());
        }

        public void BindCommand(Type type)
        {
            BindCommand((ICommand)Activator.CreateInstance(type));
        }

        public void UnbindCommand<T>() where T : IParam
        {
            UnbindCommand(typeof(T).FullName);
        }

        public void UnbindCommand(string name)
        {
            if (_nameToExecutor.ContainsKey(name))
                _nameToExecutor.Remove(name);
            else
                Game.Log.Warn($"not exist name:{name}");
        }

        public void ClearCommands()
        {
            _nameToExecutor.Clear();
        }

        private class Executor<T>
        {
            public T Param { get; set; }
            public Action<T> Action { get; set; }
        }
    }
}