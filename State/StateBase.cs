using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cherry.State
{
    public class StateBase : IState
    {
        private readonly List<Type> _modelList = new();
        private readonly List<Type> _ctrlList = new();
        private readonly List<Type> _viewList = new();
        private readonly List<string> _sceneList = new();
        private readonly Dictionary<Type, string[]> _sceneObjToTags = new();
        private readonly Dictionary<string, Action<object>> _notices = new();
        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
            foreach (var type in _modelList)
            {
                Game.Model.Remove(type);
            }
            foreach (var type in _ctrlList)
            {
                Game.Ctrl.Remove(type);
            }
            foreach (var type in _viewList)
            {
                Game.View.UnloadView(type);
                Game.View.UnbindView(type);
            }

            foreach (var name in _sceneList)
            {
                Game.Scene.UnloadScene(name);
            }
            foreach (var kvp in _sceneObjToTags)
            {
                Game.Scene.UnbindObj(kvp.Key, kvp.Value);
            }

            foreach (var kvp in _notices)
            {
                Game.Notice.UnbindNotice(kvp.Key, kvp.Value);
            }
        }

        protected void AddModel<T>() where T : IModel, new()
        {
            _modelList.Add(typeof(T));
            Game.Model.Add<T>();
        }

        protected void InitModel<T>() where T : IModel
        {
            Game.Model.Get<T>().Initialize();
        }

        protected void AddCtrl<T>() where T : ICtrl, new()
        {
            if (Game.Ctrl.Has<T>())
            {
                return;
            }
            _ctrlList.Add(typeof(T));
            Game.Ctrl.Add<T>();
        }

        protected void InitCtrl<T>() where T : ICtrl
        {
            Game.Ctrl.Get<T>().Initialize();
        }

        protected void LoadView<T>(string key = null) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(key);
            Game.View.LoadView<T>();
        }

        protected void LoadView<T>(GameObject go) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(go);
            Game.View.LoadView<T>();
        }

        protected void LoadView(Type type, string key = null)
        {
            _viewList.Add(type);
            Game.View.BindView(type, key);
            Game.View.LoadView(type);
        }

        protected void LoadView(Type type, GameObject go)
        {
            _viewList.Add(type);
            Game.View.BindView(type, go);
            Game.View.LoadView(type);
        }

        protected void LoadScene(string name, string tag = null, bool active = true, 
            Action onComplete = null, Action<float> onProgress = null)
        {
            _sceneList.Add(name);
            Game.Scene.LoadScene(name, tag, LoadSceneMode.Additive, active, onComplete, onProgress);
        }

        protected void LoadScene(string name, Action onComplete = null)
        {
            _sceneList.Add(name);
            Game.Scene.LoadScene(name, onComplete);
        }

        protected void BindSceneObj<T>(params string[] tags) where T : ISceneObj
        {
            _sceneObjToTags.Add(typeof(T), tags);
            Game.Scene.BindObj<T>(tags);
        }

        protected void BindNotice(string name, Action<object> action)
        {
            _notices.Add(name, action);
            Game.Notice.BindNotice(name, action);
        }
    }
}