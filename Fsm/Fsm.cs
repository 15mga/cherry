using System;
using System.Collections.Generic;
using Cherry.State;

namespace Cherry.Fsm
{
    public class Fsm : IFsm
    {
        private readonly Dictionary<string, IFsm> _nameToChild = new();
        private readonly Dictionary<string, IState> _nameToState = new();

        public Fsm()
        {
        }

        public Fsm(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public string PreviousState { get; private set; }

        public string CurrStateName { get; private set; }
        public IState CurrState { get; private set; }
        public IFsm Parent { get; private set; }
        public event Action<string> OnChangeState;

        public void AddChild(IFsm fsm)
        {
            _nameToChild.Add(fsm.Name, fsm);
            ((Fsm)fsm).Parent = this;
        }

        public void RemoveChild(IFsm fsm)
        {
            var name = fsm.Name;
            if (!_nameToChild.ContainsKey(name))
            {
                Game.Log.Warn($"not exist child:{name}");
                return;
            }

            _nameToChild.Remove(name);
            Game.Fsm.RemoveFsm(name);
        }

        public IFsm GetChild(string name)
        {
            return _nameToChild.TryGetValue(name, out var child) ? child : null;
        }

        public void EveryChild(Action<string, IFsm> action)
        {
            foreach (var item in _nameToChild) action(item.Key, item.Value);
        }

        public void EveryChild(Action<IFsm> action)
        {
            foreach (var item in _nameToChild) action(item.Value);
        }

        public T ChangeState<T>(bool forcibly = false) where T : IState
        {
            return (T)ChangeState(typeof(T), forcibly);
        }

        public IState ChangeState(Type type, bool forcibly = false)
        {
            return ChangeState(type.FullName, forcibly);
        }

        public IState ChangeState(string name, bool forcibly = false)
        {
            if (!_nameToState.TryGetValue(name, out var state))
            {
                Game.Log.Error($"not exist state {name}");
                return null;
            }

            if (CurrStateName != null)
            {
                if (!forcibly && CurrStateName == name) return _nameToState[CurrStateName];

                _nameToState[CurrStateName].Exit();
                PreviousState = CurrStateName;
            }

            CurrStateName = name;
            CurrState = state;
            state.Enter();
            OnChangeState?.Invoke(name);
            return state;
        }

        public T UnregisterState<T>() where T : IState
        {
            return (T)UnregisterState(typeof(T));
        }

        public IState UnregisterState(Type type)
        {
            return UnregisterState(type.FullName);
        }

        public IState UnregisterState(string name)
        {
            if (CurrStateName == name) Reset();

            if (PreviousState == name) PreviousState = null;

            var state = _nameToState[name];
            _nameToState.Remove(name);
            return state;
        }

        public T RegisterState<T>() where T : IState, new()
        {
            var type = typeof(T);
            var name = type.FullName;
            if (_nameToState.ContainsKey(name))
            {
                Game.Log.Warn($"exist state {type}");
                return default;
            }

            var state = new T();
            _nameToState.Add(name, state);
            return state;
        }

        public IState RegisterState(Type type)
        {
            if (!typeof(IState).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            var name = type.FullName;
            if (_nameToState.ContainsKey(name))
            {
                Game.Log.Warn($"exist state {name}");
                return null;
            }

            var state = (IState)Activator.CreateInstance(type);
            _nameToState.Add(name, state);
            return state;
        }

        public IState RegisterState(string name, Action enter, Action exit)
        {
            var state = new InnerState(enter, exit);
            RegisterState(name, state);
            return state;
        }

        public void RegisterState(string name, IState state)
        {
            if (_nameToState.ContainsKey(name))
            {
                Game.Log.Warn($"exist state {name}");
                return;
            }

            _nameToState.Add(name, state);
        }

        public bool IsState<T>() where T : IState
        {
            return IsState(typeof(T));
        }

        public bool IsState(Type type)
        {
            if (!typeof(IState).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            return IsState(type.FullName);
        }

        public bool IsState(string name)
        {
            return CurrStateName.Equals(name);
        }

        public T GetState<T>() where T : IState
        {
            return (T)GetState(typeof(T).FullName);
        }

        public IState GetState(Type type)
        {
            if (!typeof(IState).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            return GetState(type.FullName);
        }

        public IState GetState(string name)
        {
            if (_nameToState.ContainsKey(name)) return _nameToState[name];

            Game.Log.Error($"not exist state {name}");
            return null;
        }

        public bool HasState<T>() where T : IState
        {
            return HasState(typeof(T));
        }

        public bool HasState(Type type)
        {
            return HasState(type.FullName);
        }

        public bool HasState(string name)
        {
            return _nameToState.ContainsKey(name);
        }

        public void Reset(bool includeChild = true)
        {
            if (includeChild)
                EveryChild(fsm => { fsm.Reset(includeChild); });
            if (CurrStateName != null)
            {
                _nameToState[CurrStateName].Exit();
                CurrStateName = null;
            }
        }

        public void Dispose()
        {
            EveryChild(fsm => { fsm.Dispose(); });

            PreviousState = null;

            if (CurrStateName != null)
            {
                _nameToState[CurrStateName].Exit();
                CurrStateName = null;
            }

            _nameToState.Clear();
        }

        private class InnerState : StateBase
        {
            private readonly Action _enter;

            private readonly Action _exit;

            public InnerState(Action enter, Action exit)
            {
                _enter = enter;
                _exit = exit;
            }

            public override void Enter()
            {
                base.Enter();
                _enter?.Invoke();
            }

            public override void Exit()
            {
                base.Exit();
                _exit?.Invoke();
            }
        }
    }
}