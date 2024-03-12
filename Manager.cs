using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Cherry
{
    public class Manager<T> : IManager<T> where T : IDisposable
    {
        private readonly Dictionary<string, T> _nameToItem = new();

        public Manager()
        {
            Game.OnDispose += () => { Dispose(); };
        }

        public virtual bool Has(T type, string name = null)
        {
            name = name ?? type.GetType().FullName;
            return _nameToItem.ContainsKey(name);
        }

        public virtual bool Has<CT>(string name = null)
        {
            name = name ?? typeof(CT).FullName;
            return _nameToItem.ContainsKey(name);
        }

        public virtual void Add(T instance, string name = null)
        {
            name = name ?? instance.GetType().FullName;
            if (_nameToItem.ContainsKey(name))
            {
                Game.Log.Error($"exist name {name}");
                return;
            }

            _nameToItem.Add(name, instance);
        }

        public virtual T Add(Type type, string name = null)
        {
            Assert.IsTrue(typeof(T).IsAssignableFrom(type));

            var instance = (T)Activator.CreateInstance(type);
            Add(instance, name);
            return instance;
        }

        public virtual CT Add<CT>(string name = null) where CT : T, new()
        {
            var instance = new CT();
            Add(instance, name);
            return instance;
        }

        public virtual T Add(Type parentType, Type childType)
        {
            if (!typeof(T).IsAssignableFrom(parentType))
            {
                Game.Log.Warn($"{parentType.FullName} is not {typeof(T)}");
                return default;
            }

            if (parentType.IsAssignableFrom(childType))
            {
                Game.Log.Warn($"{childType.FullName} is not {parentType}");
                return default;
            }

            var name = parentType.FullName;
            if (_nameToItem.ContainsKey(name))
            {
                Game.Log.Error($"exist name {name}");
                return default;
            }

            var instance = (T)Activator.CreateInstance(childType);
            _nameToItem.Add(name, instance);
            return instance;
        }

        public virtual CT Add<PT, CT>() where PT : T where CT : PT, new()
        {
            var name = typeof(PT).FullName;
            if (_nameToItem.ContainsKey(name))
            {
                Game.Log.Error($"exist name {name}");
                return default;
            }

            var instance = new CT();
            _nameToItem.Add(name, instance);
            return instance;
        }

        public virtual T Get(string name)
        {
            _nameToItem.TryGetValue(name, out var instance);
            return instance;
        }

        public virtual T Get(Type type)
        {
            _nameToItem.TryGetValue(type.FullName, out var instance);
            return instance;
        }

        public virtual CT Get<CT>() where CT : T
        {
            _nameToItem.TryGetValue(typeof(CT).FullName, out var instance);
            return (CT)instance;
        }

        public virtual CT Get<PT, CT>() where PT : T where CT : PT
        {
            _nameToItem.TryGetValue(typeof(PT).FullName, out var instance);
            return (CT)instance;
        }

        public virtual T Remove(Type type)
        {
            return Remove(type.FullName);
        }

        public virtual CT Remove<CT>() where CT : T
        {
            return (CT)Remove(typeof(CT).FullName);
        }

        public virtual void Dispose(Action<string, T> onDispose = null)
        {
            foreach (var item in _nameToItem)
            {
                onDispose?.Invoke(item.Key, item.Value);
                item.Value.Dispose();
            }

            _nameToItem.Clear();
        }

        public void Every(Action<T> action)
        {
            foreach (var item in _nameToItem) action(item.Value);
        }

        public void Every(Action<string, T> action)
        {
            foreach (var item in _nameToItem) action(item.Key, item.Value);
        }

        public T Remove(string name)
        {
            if (!_nameToItem.TryGetValue(name, out var instance)) return default;

            _nameToItem.Remove(name);
            instance.Dispose();
            return instance;
        }
    }
}