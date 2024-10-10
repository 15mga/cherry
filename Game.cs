using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cherry.Attr;
using Cherry.Misc;
using Cherry.Pool;
using UnityEngine;

namespace Cherry
{
    [DisallowMultipleComponent]
    [AddComponentMenu("EU/Game")]
    public class Game : MonoBehaviour
    {
        private static readonly Dictionary<Type, object> Modules = new();
        private IGameHelper _helper;
        public static IMAsset Asset { get; private set; }
        public static IMAudio Audio { get; private set; }
        public static IMCamera Camera { get; private set; }
        public static IMCtrl Ctrl { get; private set; }
        public static IMFsm Fsm { get; private set; }
        public static IMHttp Http { get; private set; }
        
        public static IMScene Scene { get; private set; }
        public static IMLog Log { get; private set; }
        public static IMModel Model { get; private set; }
        public static IMNotice Notice { get; private set; }
        public static IMPool Pool { get; private set; }
        public static IMTask Task { get; private set; }
        public static IMTimer Timer { get; private set; }
        public static IMTrigger Trigger { get; private set; }
        public static IMView View { get; private set; }
        public static Game Instance { get; private set; }
        public static IPool<string> Id { get; private set; }

        private void Awake()
        {
            Instance = this;
            Modules.Clear();

            Id = new Pool<string>(GetGuid);

            DontDestroyOnLoad(this);

            //获取辅助类
            _helper = GetComponent<IGameHelper>();
            if (_helper == null) throw new Exception("not exist IGameHelper");

            //辅助类初始化,并赋值常用模块
            _helper.Init();

            Asset = Get<IMAsset>();
            Audio = Get<IMAudio>();
            Timer = Get<IMTimer>();
            Task = Get<IMTask>();
            Log = Get<IMLog>();
            Notice = Get<IMNotice>();
            Trigger = Get<IMTrigger>();
            Http = Get<IMHttp>();
            Fsm = Get<IMFsm>();
            Pool = Get<IMPool>();
            Model = Get<IMModel>();
            View = Get<IMView>();
            Ctrl = Get<IMCtrl>();
            Scene = Get<IMScene>();
            Camera = Get<IMCamera>();

            Log.Info("EU \n:https://blog.95eh.com");

            _helper.Begin();
        }

        private void Update()
        {
            if (OnOnce != null)
            {
                OnOnce();
                OnOnce = null;
            }

            OnBeforeUpdate?.Invoke();
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            OnDispose?.Invoke();
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
            OnDispose = null;
        }

        private void OnApplicationQuit()
        {
            OnQuit?.Invoke();
        }

        public static event Action OnBeforeUpdate;
        public static event Action OnUpdate;
        public static event Action OnOnce;

        public static event Action OnLateUpdate;

        public static event Action OnFixedUpdate;

        public static event Action OnDispose;

        public static event Action OnQuit;

        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static Transform CreateRoot(string name)
        {
            var root = new GameObject(name).transform;
            root.SetParent(Instance.transform);
            return root;
        }

        public static object Get(Type type)
        {
            return Modules.TryGetValue(type, out var module) ? module : null;
        }

        public static T Get<T>()
        {
            return Modules.TryGetValue(typeof(T), out var module) ? (T)module : default;
        }

        public static T2 Get<T1, T2>() where T2 : T1
        {
            return Modules.TryGetValue(typeof(T1), out var module) ? (T2)module : default;
        }

        public static bool Has(Type type)
        {
            return Modules.ContainsKey(type);
        }

        public static bool Has<T>()
        {
            return Modules.ContainsKey(typeof(T));
        }

        public static T2 Register<T1, T2>() where T2 : T1, new()
        {
            var module = new T2();
            var type = typeof(T1);
            if (Modules.ContainsKey(type))
            {
                Debug.Log($"exist {type}");
                return (T2)Modules[type];
            }

            Modules.Add(type, module);
            return module;
        }

        public static void Register<T>(T module)
        {
            Modules.Add(typeof(T), module);
        }

        public static T Register<T>() where T : new()
        {
            var module = new T();
            Modules.Add(typeof(T), module);
            return module;
        }

        public static void Register(Type type)
        {
            Modules.Add(type, Activator.CreateInstance(type));
        }

        public static object Remove(Type type)
        {
            if (Modules.ContainsKey(type)) return Modules.Remove(type);

            Log.Warn($"not exist type {type}");
            return null;
        }

        public static T Remove<T>()
        {
            return (T)Remove(typeof(T));
        }

        public static object Remove(object module)
        {
            var type = module.GetType();
            if (Modules.ContainsKey(type)) return Modules.Remove(type);

            Log.Warn($"not exist type {type}");
            return null;
        }

        public static Coroutine StartCo(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        public static void StopCo(IEnumerator coroutine)
        {
            Instance.StopCoroutine(coroutine);
        }

        public static void EndAllCo()
        {
            Instance.StopAllCoroutines();
        }

        public static Coroutine Delay(float seconds, Action action)
        {
            return Instance.StartCoroutine(DelayCall(seconds, action));
        }

        private static IEnumerator DelayCall(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);

            action();
        }

        public static void InitGameAssembly<DefaultStateT>() where DefaultStateT : IState
        {
            var filter = new AssemblyTypesFilter();

            var mainStateNameSpace = typeof(DefaultStateT).Namespace;
            filter.Add(typeof(IState).IsAssignableFrom, type =>
            {
                var name = type.Namespace;
                if (!name.StartsWith(mainStateNameSpace)) return;
                name = type.Namespace.Replace(mainStateNameSpace, "");
                IFsm fsm;
                if (string.IsNullOrEmpty(name))
                {
                    fsm = Fsm.Main;
                }
                else
                {
                    var fsmName = name.Substring(name.IndexOf(".") + 1);
                    fsm = Fsm.GetFsm(fsmName) ?? Fsm.AddFsm(fsmName);
                }

                fsm.RegisterState(type);
            });

            filter.Add(typeof(IModel).IsAssignableFrom, type => Model.Add(type));
            filter.Add(typeof(ICtrl).IsAssignableFrom, type => Ctrl.Add(type));

            var sceneObj = typeof(ISceneObj);
            filter.Add(sceneObj.IsAssignableFrom,
                type =>
                {
                    var attr = type.GetCustomAttribute<ASceneObjTags>();
                    if (attr == null) return;
                    Scene.BindObj(type, attr.Tags);
                });

            filter.Filter<DefaultStateT>();

            Fsm.Main.ChangeState<DefaultStateT>();
        }
    }
}