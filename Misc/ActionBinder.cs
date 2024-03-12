using System;
using System.Collections.Generic;

namespace Cherry.Misc
{
    public class ActionBinder
    {
        private readonly List<string> _cmdNames = new();

        public virtual void Dispose()
        {
            for (var index = 0; index < _cmdNames.Count; index++)
            {
                var name = _cmdNames[index];
                Game.Command.UnbindCommand(name);
            }
        }

        protected void BindCommand(string name, Action<object> action)
        {
            _cmdNames.Add(name);
            Game.Command.BindCommand(name, action);
        }

        protected void BindCommand<T>(string name, Action<T> action) where T : IParam, new()
        {
            _cmdNames.Add(name);
            Game.Command.BindCommand(name, action);
        }
    }
}