using System;

namespace Cherry
{
    public interface IMFsm
    {
        IFsm Main { get; }

        IFsm GetFsm(string name);

        IFsm AddFsm(string name);

        void RemoveFsm(string name);
    }

    public interface IFsm
    {
        string Name { get; }
        string PreviousState { get; }

        string CurrStateName { get; }
        IState CurrState { get; }

        IFsm Parent { get; }

        event Action<string> OnChangeState;

        void AddChild(IFsm fsm);
        void RemoveChild(IFsm fsm);

        IFsm GetChild(string name);

        void EveryChild(Action<string, IFsm> action);

        void EveryChild(Action<IFsm> action);

        bool IsState<T>() where T : IState;

        bool IsState(Type type);

        bool IsState(string name);

        T RegisterState<T>() where T : IState, new();

        IState RegisterState(Type type);

        void RegisterState(string name, IState state);

        IState RegisterState(string name, Action enter, Action exit);

        T ChangeState<T>(bool forcibly = false) where T : IState;

        IState ChangeState(Type type, bool forcibly = false);

        IState ChangeState(string name, bool forcibly = false);

        T UnregisterState<T>() where T : IState;

        IState UnregisterState(Type type);

        IState UnregisterState(string name);

        bool HasState<T>() where T : IState;

        bool HasState(Type type);

        bool HasState(string name);

        T GetState<T>() where T : IState;

        IState GetState(Type type);

        IState GetState(string name);

        void Reset(bool includeChild = true);

        void Dispose();
    }

    public interface IState
    {
        void Enter();

        void Exit();
    }
}