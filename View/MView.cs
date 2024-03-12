using System;
using System.Collections.Generic;
using System.Linq;
using Cherry.Extend;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Cherry.View
{
    public class MView : IMView
    {
        private readonly Dictionary<string, IView> _idToView = new();
        private readonly Dictionary<string, Canvas> _nameToLayer = new();

        private readonly Transform _root2D;

        private readonly Dictionary<string, GameObject> _typeToGameObject = new();
        private readonly Dictionary<string, string> _typeToKey = new();

        private Transform _root3D;

        private Transform _rootCamera;

        public MView()
        {
            _root2D = Game.CreateRoot("MUI_2D_root");
        }

        public string DefaultLayer { get; set; }

        public int Width { get; set; } = Screen.width;
        public int Height { get; set; } = Screen.height;
        public event Action<IView> OnLoaded;
        public event Action<IView> OnUnloaded;

        public void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
            foreach (var kvp in _nameToLayer)
            {
                var scaler = kvp.Value.GetComponent<CanvasScaler>();
                scaler.referenceResolution = new Vector2(Width, Height);
            }
        }

        public void AllLayer(Action<Canvas> action)
        {
            foreach (var kvp in _nameToLayer) action(kvp.Value);
        }

        public Canvas AddLayer(string name, bool defaultLayer = false, string before = null)
        {
            if (_nameToLayer.ContainsKey(name))
            {
                Game.Log.Error($"exist layer {name}");
                return null;
            }

            if (defaultLayer) DefaultLayer = name;

            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(_root2D);

            if (!string.IsNullOrEmpty(before))
            {
                var bc = GetLayer(before);
                if (bc == null)
                    Game.Log.Warn($"not exist layer {before}");
                else
                    go.transform.SetSiblingIndex(bc.transform.GetSiblingIndex());
            }

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(Width, Height);
            var layer = go.GetComponent<Canvas>();
            layer.sortingOrder = _nameToLayer.Count;
            _nameToLayer.Add(name, layer);
            layer.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Width, Height);
            return layer;
        }

        public void RemoveLayer(string name)
        {
            if (!_nameToLayer.ContainsKey(name))
            {
                Game.Log.Warn($"not exist layer {name}");
                return;
            }

            var canvas = _nameToLayer[name];
            _nameToLayer.Remove(name);

            Object.Destroy(canvas);
        }

        public Canvas GetLayer(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(DefaultLayer))
                {
                    Game.Log.Error("defaultLayer is null");
                    return null;
                }

                name = DefaultLayer;
            }

            return _nameToLayer.TryGetValue(name, out var layer) ? layer : null;
        }

        public void SetLayerAlpha(string name, float alpha)
        {
            var layer = GetLayer(name);
            if (layer == null)
            {
                Game.Log.Error($"not exist layer {name}");
                return;
            }

            layer.GetComp<CanvasGroup>().alpha = alpha;
        }

        public void SetLayerInteractable(string name, bool interactable)
        {
            var layer = GetLayer(name);
            if (layer == null)
            {
                Game.Log.Error($"not exist layer {name}");
                return;
            }

            layer.GetComp<CanvasGroup>().interactable = interactable;
        }

        public void SetLayerBlocksRaycasts(string name, bool blocksRaycasts)
        {
            var layer = GetLayer(name);
            if (layer == null)
            {
                Game.Log.Error($"not exist layer {name}");
                return;
            }

            layer.GetComp<CanvasGroup>().blocksRaycasts = blocksRaycasts;
        }

        public void BindView<T>(string key = null) where T : IView, new()
        {
            var type = typeof(T);
            var id = type.FullName;
            if (_typeToKey.ContainsKey(id))
            {
                Game.Log.Error($"exist {id}");
                return;
            }

            key ??= type.Name;

            _typeToKey.Add(id, key);
        }

        public void BindView<T>(GameObject go) where T : IView, new()
        {
#if UNITY_EDITOR
            if (go == null) throw new ArgumentNullException(nameof(go));
#endif

            var id = typeof(T).FullName;
            if (_typeToGameObject.ContainsKey(id))
            {
                Game.Log.Error($"exist {id}");
                return;
            }

            _typeToGameObject.Add(id, go);
        }

        public void BindView(Type type, string key = null)
        {
#if UNITY_EDITOR
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
#endif

            var id = type.FullName;
            if (_typeToKey.ContainsKey(id))
            {
                Game.Log.Error($"exist {id}");
                return;
            }

            key ??= type.Name;
            _typeToKey.Add(id, key);
        }

        public void BindView(Type type, GameObject go)
        {
#if UNITY_EDITOR
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            if (go == null) throw new ArgumentNullException(nameof(go));
#endif

            var id = type.FullName;
            if (_typeToGameObject.ContainsKey(id))
            {
                Game.Log.Error($"exist {id}");
                return;
            }

            _typeToGameObject.Add(id, go);
        }

        public void UnbindView<T>() where T : IView, new()
        {
            var id = typeof(T).FullName;
            _typeToKey.Remove(id);
            _typeToGameObject.Remove(id);
        }

        public void UnbindView(Type type)
        {
            var id = type.FullName;
            _typeToKey.Remove(id);
            _typeToGameObject.Remove(id);
        }

        public IView GetView(string id)
        {
            return _idToView.TryGetValue(id, out var view) ? view : null;
        }

        public IView GetView(Type type)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            return GetView(type.FullName);
        }

        public T GetView<T>(string id = null) where T : IView
        {
            return (T)GetView(string.IsNullOrEmpty(id) ? typeof(T).FullName : id);
        }

        public bool HasViewLoaded(Type type)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            return HasViewLoaded(type.FullName);
        }

        public bool HasViewLoaded<T>()
        {
            return HasViewLoaded(typeof(T).FullName);
        }

        public bool HasViewLoaded(string id)
        {
            return _idToView.TryGetValue(id, out var view) && view.Loaded;
        }

        public T LoadView<T>(string id = null, Transform tnf = null,
            Action<IView, RectTransform> onComplete = null) where T : IView
        {
            return (T)LoadView(typeof(T), id, tnf, onComplete);
        }

        public IView LoadView(Type type, string id = null, Transform tnf = null,
            Action<IView, RectTransform> onComplete = null)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            return InstantiateUI(id, type, tnf, (ui, tnfm) =>
            {
                if (tnf == null) tnfm.SetParent(GetLayer().transform, false);
                onComplete?.Invoke(ui, tnfm.GetComponent<RectTransform>());
            });
        }

        public T LoadViewWithLayer<T>(string layer, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null) where T : IView
        {
            return (T)LoadViewWithLayer(typeof(T), layer, id, tnf, onComplete);
        }

        public IView LoadViewWithLayer(Type type, string layer, string id = null, Transform tnf = null,
            Action<IView, RectTransform> onComplete = null)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            if (string.IsNullOrEmpty(layer)) throw new ArgumentNullException(nameof(layer));
            return InstantiateUI(id, type, tnf, (ui, tnfm) =>
            {
                var canvas = GetLayer(layer);
                if (canvas == null)
                {
                    Game.Log.Error($"not exist layer {layer}");
                    return;
                }

                tnfm.SetParent(canvas.transform, false);

                onComplete?.Invoke(ui, tnfm.GetComponent<RectTransform>());
            });
        }

        public T LoadWorldView<T>(string id = null, Transform tnf = null, Action<IView, Transform> onComplete = null)
            where T : IView
        {
            return (T)LoadWorldView(typeof(T), id, tnf, onComplete);
        }

        public IView LoadWorldView(Type type, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            if (_root3D == null) _root3D = Game.CreateRoot("MUI_3D_root");

            return InstantiateUI(id, type, tnf, (ui, tnfm) =>
            {
                var canvas = tnfm.GetComponent<Canvas>();
                if (canvas == null)
                {
                    Game.Log.Error($"{type} not exist Canvas");
                    return;
                }

                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Game.Camera.Main;
                if (tnf == null) tnfm.SetParent(_root3D, false);

                onComplete?.Invoke(ui, tnfm);
            });
        }

        public T LoadGameObjectView<T>(string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null) where T : IView
        {
            return (T)LoadGameObjectView(typeof(T), id, tnf, onComplete);
        }

        public IView LoadGameObjectView(Type type, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            if (_root3D == null) _root3D = Game.CreateRoot("MUI_3D_root");

            return InstantiateUI(id, type, tnf, (ui, tnfm) =>
            {
                if (tnf == null) tnfm.SetParent(_root3D, false);

                onComplete?.Invoke(ui, tnfm);
            });
        }

        public T LoadCameraView<T>(Camera camera, float planeDistance = 0.1f, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null) where T : IView
        {
            return (T)LoadCameraView(typeof(T), camera, planeDistance, id, tnf, onComplete);
        }

        public IView LoadCameraView(Type type, Camera camera, float planeDistance = 0.1f, string id = null,
            Transform tnf = null, Action<IView, Transform> onComplete = null)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            if (camera == null) throw new ArgumentNullException(nameof(camera));

            if (_rootCamera == null) _rootCamera = Game.CreateRoot("MUI_camera_root");

            return InstantiateUI(id, type, tnf, (ui, tnfm) =>
            {
                var canvas = tnfm.GetComponent<Canvas>();
                if (canvas == null)
                {
                    Game.Log.Error($"{type} not exist Canvas");
                    return;
                }

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = planeDistance;
                if (tnf == null) tnfm.SetParent(_rootCamera, false);

                onComplete?.Invoke(ui, tnfm);
            });
        }

        public T UnloadView<T>() where T : IView
        {
            return (T)UnloadView(typeof(T).FullName);
        }

        public IView UnloadView(Type type)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));

            return UnloadView(type.FullName);
        }

        public IView UnloadView(string id)
        {
            if (!_idToView.ContainsKey(id)) return null;

            var ui = _idToView[id];
            if (ui.Visible) ui.Hide();

            OnUnloaded?.Invoke(ui);

            if (ui.Transform != null)
            {
                if (_typeToGameObject.ContainsKey(id))
                    Object.Destroy(ui.Transform.gameObject);
                else
                    Game.Asset.Destroy(ui.Transform.gameObject);
            }

            ui.Unload();
            _idToView.Remove(id);
            return ui;
        }

        public void UnloadAllView()
        {
            var dic = _idToView.ToDictionary(item => item.Key, item => item.Value);
            _idToView.Clear();
            foreach (var item in dic)
            {
                var id = item.Key;
                var ui = item.Value;
                if (ui.Visible) ui.Hide();

                OnUnloaded?.Invoke(ui);

                if (ui.Transform != null)
                {
                    if (_typeToGameObject.ContainsKey(id))
                        Object.Destroy(ui.Transform.gameObject);
                    else
                        Game.Asset.Destroy(ui.Transform.gameObject);
                }

                ui.Unload();
            }
        }

        public void EveryView(Action<string, IView> action)
        {
            foreach (var item in _idToView) action(item.Key, item.Value);
        }

        public void EveryView(Type type, Action<string, IView> action)
        {
            foreach (var item in _idToView)
                if (item.Value.GetType() == type)
                    action(item.Key, item.Value);
        }

        public void EveryView<T>(Action<string, T> action)
        {
            var type = typeof(T);
            foreach (var item in _idToView)
                if (item.Value.GetType() == type)
                    action(item.Key, (T)item.Value);
        }

        public bool AnyView(Func<string, IView, bool> func)
        {
            foreach (var item in _idToView)
                if (func(item.Key, item.Value))
                    return true;

            return false;
        }

        public bool AnyView(Type type, Func<string, IView, bool> func)
        {
            foreach (var item in _idToView)
            {
                if (item.Value.GetType() != type) continue;

                if (func(item.Key, item.Value)) return true;
            }

            return false;
        }

        public bool AnyView<T>(Func<string, T, bool> func)
        {
            var type = typeof(T);
            foreach (var item in _idToView)
            {
                if (item.Value.GetType() != type) continue;

                if (func(item.Key, (T)item.Value)) return true;
            }

            return false;
        }

        internal static string GetId(Type type, string id)
        {
            if (!typeof(IView).IsAssignableFrom(type)) throw new ArgumentException(nameof(type));
            return string.IsNullOrEmpty(id) ? type.FullName : id;
        }

        private IView InstantiateUI(string id, Type type, Transform tnf, Action<IView, Transform> onComplete)
        {
            id = GetId(type, id);

            if (_idToView.TryGetValue(id, out var view))
            {
                Game.Log.Warn($"exist ui {id}");
                return view;
            }

            var ui = (IView)Activator.CreateInstance(type);
            ui.SetId(id);
            _idToView.Add(id, ui);

            var typeFullName = type.FullName;
            if (tnf != null)
            {
                _idToView[id].Load(tnf);
                onComplete(ui, tnf);

                OnLoaded?.Invoke(ui);
            }
            else if (_typeToGameObject.TryGetValue(typeFullName, out var gameObject))
            {
                Game.Task.PushTask(() =>
                {
                    if (!_idToView.TryGetValue(id, out var vw)) return;

                    var go = tnf == null ? Object.Instantiate(gameObject) : tnf.gameObject;
                    vw.Load(go.transform);
                    onComplete(ui, go.transform);

                    OnLoaded?.Invoke(ui);
                });
            }
            else if (_typeToKey.TryGetValue(typeFullName, out var key))
            {
                Game.Asset.Spawn(key, go =>
                {
                    if (go == null) return;
                    if (!_idToView.TryGetValue(id, out var vw))
                    {
                        Game.Asset.Destroy(go);
                        return;
                    }

                    vw.Load(go.transform);
                    onComplete(ui, go.transform);

                    OnLoaded?.Invoke(ui);
                });
            }
            else
            {
                Game.Log.Error($"transform is null and not bind View type:{typeFullName}");
            }

            return ui;
        }
    }
}