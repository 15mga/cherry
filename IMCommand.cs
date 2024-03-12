using System;

namespace Cherry
{
    public interface IMCommand : IAction
    {
        void UnbindCommand<T>() where T : IParam;
        void Execute(string name, object data = null);
        void BindCommand(string name, Action<object> action);
        void BindCommand(ICommand command);
        void BindCommand<T>() where T : ICommand, new();
        void BindCommand<T>(string name, Action<T> action) where T : new();
        void BindCommand(Type type);

        void UnbindCommand(string name);

        void ClearCommands();
    }

    public interface IParam
    {
    }

    public interface ICommand
    {
        string Name { get; }
        void Execute(object data);
    }
}