using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Cherry
{
    public interface IMAsset
    {
        string Ver { get; }
        void Init(string pkg, Action onComplete);
        void Load<T>(string path, Action<T> onComplete = null, Action<float> onProgress = null) where T : Object;

        void Load(Type type, string path, Action<Object> onComplete = null, Action<float> onProgress = null);

        void Release<T>(T obj) where T : Object;

        void Load<T>(IList<string> paths, Action onComplete = null, Action<string, T> onCompleteItem = null,
            Action<float> onProgress = null) where T : Object;

        void Load(IList<string> paths, Action onComplete = null, Action<string, Type, Object> onCompleteItem = null,
            Action<float> onProgress = null);

        void LoadTag<T>(string tag, Action onComplete = null, Action<string, T> onCompleteItem = null,
            Action<float> onProgress = null) where T : Object;

        void LoadTag(string tag, Action onComplete = null, Action<string, Type, Object> onCompleteItem = null,
            Action<float> onProgress = null);

        string[] GetAddressWithTag(string tag);

        void Spawn(string path, Action<GameObject> action, Action<float> onProgress = null);

        void Destroy(params GameObject[] gameObjects);

        void LoadScene(string path, Action<Scene> onComplete = null, Action<float> onProgress = null);

        void UnloadScene(string path, Action onComplete = null);

        void LoadRaw(string path, Action<byte[]> onComplete, Action<float> onProgress = null);
        void UpdateVersion();
    }
}