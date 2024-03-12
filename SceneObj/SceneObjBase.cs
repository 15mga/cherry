using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cherry.Attr;
using Cherry.Extend;
using Cherry.Pool;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Cherry.SceneObj
{
    public class SceneObjBase : ISceneObj
    {
        private readonly Dictionary<string, Action<object>> NoticeHandler = new();
        private GameObject[] Roots;
        protected MPool Pool { get; private set; }
        public Scene Scene { get; private set; }

        public void Load(Scene scene, GameObject[] roots)
        {
            Scene = scene;
            Roots = roots;

            InitAttribute();

            OnLoaded();
        }

        public void Unload()
        {
            OnUnloaded();
        }

        private void InitAttribute()
        {
            var type = GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<AChild>();
                if (attr != null)
                {
                    var comName = attr.Name ?? field.Name;
                    var childComp = FindComp(field.FieldType, comName);
                    if (childComp == null)
                        Game.Log.Error($"{GetType()} not exist child {comName}:{field.FieldType}");
                    else
                        field.SetValue(this, childComp);
                }
            }

            {
                var attrs = type.GetCustomAttributes<APool>();
                foreach (var attr in attrs) BindPool(attr.Name, attr.Max, attr.Min);
            }
        }

        /// <summary>
        ///     获取子对象
        /// </summary>
        /// <param name="name">子对象名</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        protected T FindComp<T>(string name)
        {
            var list = Roots.Select(o => o.transform).ToList();
            var tnf = list.FindTnf(name);
            return tnf == null ? default : tnf.GetComponent<T>();
        }

        protected Component FindComp(Type type, string name)
        {
            var list = Roots.Select(o => o.transform).ToList();
            var tnf = list.FindTnf(name);
            return tnf == null ? default : tnf.GetComponent(type);
        }

        /// <summary>
        ///     通过节点名称广度优先搜索RectTransform
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected Transform FindTnf(string name)
        {
            var list = Roots.Select(o => o.transform).ToList();
            return list.FindTnf(name);
        }

        /// <summary>
        ///     场景加载完成后调用,
        /// </summary>
        protected virtual void OnLoaded()
        {
        }

        /// <summary>
        ///     场景卸载后调用
        /// </summary>
        protected virtual void OnUnloaded()
        {
            foreach (var kvp in NoticeHandler) Game.Notice.UnbindNotice(kvp.Key, kvp.Value);
            NoticeHandler.Clear();
        }

        protected void BindNotice(string notice, Action<object> action)
        {
            NoticeHandler.Add(notice, action);
            Game.Notice.BindNotice(notice, action);
        }

        protected void UnbindNotice(string notice, Action<object> action)
        {
            NoticeHandler.Remove(notice);
            Game.Notice.UnbindNotice(notice, action);
        }

        /// <summary>
        ///     注册对象池,用于动态生成多个UI实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public Transform BindPool(string name, int max = 10, int min = 0)
        {
            Pool ??= new MPool();
            var tnf = FindTnf(name);
            tnf.gameObject.SetActive(false);
            Pool.RegisterPool(name, () => Object.Instantiate(tnf.gameObject, tnf.parent),
                o => Object.Destroy((GameObject)o), max, min);
            return tnf;
        }

        /// <summary>
        ///     生成对象池GameObject实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Spawn<T>(string name)
        {
            var go = Pool.SpawnInstance<GameObject>(name);
            go.SetActive(true);
            return go.GetComponent<T>();
        }
        public GameObject Spawn(string name)
        {
            var go = Pool.SpawnInstance<GameObject>(name);
            go.SetActive(true);
            return go;
        }

        /// <summary>
        ///     回收对象池GameObject实例
        /// </summary>
        /// <param name="go"></param>
        public void Recycle(Transform tnf)
        {
            Pool.RecycleInstance(tnf.gameObject);
            tnf.gameObject.SetActive(false);
        }
    }
}