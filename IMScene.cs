using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cherry
{
    public interface IMScene
    {
        void BindObj<T>(params string[] tags) where T : ISceneObj;
        void BindObj(Type type, params string[] tags);
        void UnbindObj<T>(params string[] tags) where T : ISceneObj;
        void UnbindObj(Type type, params string[] tags);
        void LoadScene(string name, string tag = null, LoadSceneMode mode = LoadSceneMode.Additive, bool active = true, 
            Action onComplete = null, Action<float> onProgress = null, bool assetScene = true);
        void LoadScene(string name, Action onComplete = null);

        void UnloadScene(string key, Action onComplete = null);
        void UnloadSceneByTag(string tag, Action onComplete = null);
        T GetObj<T>() where T : ISceneObj;
    }

    public interface ISceneObj
    {
        void Load(Scene scene, GameObject[] roots);
        void Unload();
    }
}