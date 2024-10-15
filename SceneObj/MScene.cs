using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Cherry.SceneObj
{
    public class MScene : IMScene
    {
        private readonly Dictionary<string, Scene> _sceneNameToScene = new();
        private readonly Dictionary<string, string> _sceneNameToTag = new();
        private readonly Dictionary<string, string> _tagToSceneName = new();
        private readonly Dictionary<string, List<Type>> _tagToSceneObjType = new();
        private readonly Dictionary<Type, ISceneObj> _sceneObjTypeToSceneObj = new();

        public void BindObj<T>(params string[] tags) where T : ISceneObj
        {
            BindObj(typeof(T), tags);
        }

        public void BindObj(Type type, params string[] tags)
        {
#if UNITY_EDITOR
            if (!typeof(ISceneObj).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            Assert.IsTrue(tags.Length > 0);
#endif
            foreach (var tag in tags)
            {
                if (_tagToSceneObjType.TryGetValue(tag, out var list))
                {
                    list.Add(type);
                }
                else
                {
                    _tagToSceneObjType[tag] = new List<Type>{type};
                }
            }
        }

        public void UnbindObj<T>(params string[] tags) where T : ISceneObj
        {
            UnbindObj(typeof(T));
        }

        public void UnbindObj(Type type, params string[] tags)
        {
#if UNITY_EDITOR
            if (!typeof(ISceneObj).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            Assert.IsTrue(tags.Length > 0);
#endif
            var delTags = new List<string>();
            foreach (var tag in tags)
            {
                if (!_tagToSceneObjType.TryGetValue(tag, out var list)) continue;
                list.Remove(type);
                if (list.Count == 0)
                {
                    delTags.Add(tag);
                }
            }

            foreach (var tag in delTags)
            {
                _tagToSceneObjType.Remove(tag);
            }
        }

        public void LoadScene(string name, string tag = null, LoadSceneMode mode = LoadSceneMode.Additive, bool active = true, 
            Action onComplete = null, Action<float> onProgress = null, bool assetScene = true)
        {
            tag ??= name;
            if (_tagToSceneName.ContainsKey(tag))
            {
                Game.Log.Warn($"exist tag {tag}");
                return;
            }

            if (_sceneNameToTag.ContainsKey(name))
            {
                Game.Log.Warn($"exist scene {name}");
                return;
            }
            
            _sceneNameToTag.Add(name, tag);
            _tagToSceneName.Add(tag, name);
            if (assetScene)
            {
                Game.Asset.LoadScene(name, scene =>
                {
                    LoadSceneComplete(scene, name, tag, onComplete, active);
                });
            }
            else
            {
                LoadScene(name, mode, scene =>
                {
                    LoadSceneComplete(scene, name, tag, onComplete, active);
                }, onProgress);
            }
        }

        private void LoadSceneComplete(Scene scene, string name, string tag, Action onComplete = null, bool active = true)
        {
            if (active) SceneManager.SetActiveScene(scene);

            _sceneNameToScene.Add(name, scene);

            var list = new List<GameObject>();
            scene.GetRootGameObjects(list);
            var rootList = new List<GameObject>();
            foreach (var gameObject in list)
            {
                rootList.Add(gameObject);
            }


            if (_tagToSceneObjType.TryGetValue(tag, out var types))
            {
                var roots = rootList.ToArray();
                foreach (var type in types)
                {
                    var s = (ISceneObj)Activator.CreateInstance(type);
                    _sceneObjTypeToSceneObj.Add(type, s);

                    s.Load(scene, roots);
                }
            }
            onComplete?.Invoke();
        }

        private void LoadScene(string name, LoadSceneMode mode = LoadSceneMode.Additive, Action<Scene> onComplete = null, Action<float> onProgress = null)
        {
            var op = SceneManager.LoadSceneAsync(name, mode);
            if (onProgress != null) Game.Trigger.BindTrigger(() => op.isDone, () => onProgress(op.progress));
            op.completed += _ =>
            {
                var scene = SceneManager.GetSceneByName(name);
                onComplete(scene);
            };
        }

        public void LoadScene(string name, Action onComplete = null)
        {
            LoadScene(name, name, LoadSceneMode.Additive, true, onComplete);
        }

        public void UnloadScene(string name, Action onComplete = null)
        {
            if (!_sceneNameToScene.ContainsKey(name)) return;

            _sceneNameToScene.Remove(name);
            var tag = _tagToSceneName[name];
            _tagToSceneName.Remove(name);
            _sceneNameToTag.Remove(tag);
            if (_tagToSceneObjType.TryGetValue(tag, out var list))
            {
                foreach (var type in list)
                {
                    _sceneObjTypeToSceneObj[type].Unload();
                    _sceneObjTypeToSceneObj.Remove(type);
                }
            }
            _tagToSceneObjType.Remove(tag);
            Game.View.UnloadView(name);
            var op = SceneManager.UnloadSceneAsync(name);
            if (op != null)
            {
                op.completed += _ => { onComplete?.Invoke(); };
            }
        }

        public void UnloadSceneByTag(string tag, Action onComplete = null)
        {
            if (_tagToSceneName.TryGetValue(tag, out var key)) UnloadScene(key, onComplete);
        }

        public T GetObj<T>() where T : ISceneObj
        {
            return _sceneObjTypeToSceneObj.TryGetValue(typeof(T), out var scene) ? (T)scene : default;
        }
    }
}