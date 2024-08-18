using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cherry
{
    public interface IMView
    {
        string DefaultLayer { get; set; }

        int Width { get; set; }
        int Height { get; set; }

        event Action<IView> OnLoaded;

        event Action<IView> OnUnloaded;

        void SetResolution(int width, int height);

        void AllLayer(Action<Canvas> action);

        Canvas AddLayer(string name, bool defaultLayer = false, string before = null, CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize);

        void RemoveLayer(string name);

        Canvas GetLayer(string name = null);

        void SetLayerAlpha(string name, float alpha);

        void SetLayerInteractable(string name, bool interactable);

        void SetLayerBlocksRaycasts(string name, bool blocksRaycasts);

        void BindView<T>(string key = null) where T : IView, new();

        void BindView<T>(GameObject go) where T : IView, new();

        void BindView(Type type, string key);

        void BindView(Type type, GameObject go);

        void UnbindView<T>() where T : IView, new();

        void UnbindView(Type type);

        IView GetView(Type type);

        IView GetView(string id);

        T GetView<T>(string id = null) where T : IView;

        bool HasViewLoaded(Type type);

        bool HasViewLoaded<T>();

        bool HasViewLoaded(string id);

        T LoadView<T>(string id = null, Transform tnf = null, Action<IView, RectTransform> onComplete = null)
            where T : IView;

        IView LoadView(Type type, string id = null, Transform tnf = null,
            Action<IView, RectTransform> onComplete = null);

        T LoadViewWithLayer<T>(string layer, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null) where T : IView;

        IView LoadViewWithLayer(Type type, string layer, string id = null, Transform tnf = null,
            Action<IView, RectTransform> onComplete = null);

        T LoadWorldView<T>(string id = null, Transform tnf = null, Action<IView, Transform> onComplete = null)
            where T : IView;

        IView LoadWorldView(Type type, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null);

        T LoadGameObjectView<T>(string id = null, Transform tnf = null, Action<IView, Transform> onComplete = null)
            where T : IView;

        IView LoadGameObjectView(Type type, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null);

        T LoadCameraView<T>(Camera camera, float planeDistance = 0.1f, string id = null, Transform tnf = null,
            Action<IView, Transform> onComplete = null) where T : IView;

        IView LoadCameraView(Type type, Camera camera, float planeDistance = 0.1f, string id = null,
            Transform tnf = null,
            Action<IView, Transform> onComplete = null);

        T UnloadView<T>() where T : IView;

        IView UnloadView(Type type);

        IView UnloadView(string id);

        void UnloadAllView();

        void EveryView(Action<string, IView> action);

        void EveryView(Type type, Action<string, IView> action);

        void EveryView<T>(Action<string, T> action);

        bool AnyView(Func<string, IView, bool> func);

        bool AnyView(Type type, Func<string, IView, bool> func);

        bool AnyView<T>(Func<string, T, bool> func);
    }

    public interface IView
    {
        string Id { get; }

        Transform Transform { get; }
        RectTransform RectTransform { get; }

        bool Loaded { get; }

        bool Visible { get; }

        event Action OnLoaded;

        event Action OnUnloaded;

        event Action OnShowed;

        event Action OnHidden;

        void SetId(string id);

        void Load(Transform tnf);

        void Unload();

        void Show(object data = null);

        void Hide();

        void ChangeVisible();

        void SetAlpha(float alpha);

        void SetInteractable(bool interactable);

        void SetBlocksRaycasts(bool blocksRaycasts);

        void SetIgnoreParentGroups(bool ignoreParentGroups);
    }
}