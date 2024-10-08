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

        protected T AddModel<T>() where T : IModel, new()
        {
            _modelList.Add(typeof(T));
            return Game.Model.Add<T>();
        }

        protected void InitModel<T>(Action onComplete = null) where T : IModel
        {
            Game.Model.Get<T>().Initialize(onComplete);
        }

        protected T AddCtrl<T>() where T : ICtrl, new()
        {
            if (Game.Ctrl.Has<T>())
            {
                return Game.Ctrl.Get<T>();
            }
            _ctrlList.Add(typeof(T));
            return Game.Ctrl.Add<T>();
        }

        protected void InitCtrl<T>() where T : ICtrl
        {
            Game.Ctrl.Get<T>().Initialize();
        }

        protected T LoadView<T>(string key = null) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(key);
            return Game.View.LoadView<T>();
        }

        protected T LoadView<T>(GameObject go) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(go);
            return Game.View.LoadView<T>();
        }

        protected IView LoadView(Type type, string key = null)
        {
            _viewList.Add(type);
            Game.View.BindView(type, key);
            return Game.View.LoadView(type);
        }

        protected IView LoadView(Type type, GameObject go)
        {
            _viewList.Add(type);
            Game.View.BindView(type, go);
            return Game.View.LoadView(type);
        }

        protected T LoadViewWithLayer<T>(string layer, string key = null) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(key);
            return Game.View.LoadViewWithLayer<T>(layer);
        }

        protected T LoadViewWithLayer<T>(string layer, GameObject go) where T : IView, new()
        {
            _viewList.Add(typeof(T));
            Game.View.BindView<T>(go);
            return Game.View.LoadViewWithLayer<T>(layer);
        }

        protected IView LoadViewWithLayer(Type type, string layer, string key = null)
        {
            _viewList.Add(type);
            Game.View.BindView(type, key);
            return Game.View.LoadViewWithLayer(type, layer);
        }

        protected IView LoadViewWithLayer(Type type, string layer, GameObject go)
        {
            _viewList.Add(type);
            Game.View.BindView(type, go);
            return Game.View.LoadViewWithLayer(type, layer);
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