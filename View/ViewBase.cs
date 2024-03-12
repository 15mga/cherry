using System;
using System.Collections.Generic;
using System.Reflection;
using Cherry.Attr;
using Cherry.Extend;
using Cherry.Pool;
using Cherry.View.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Cherry.View
{
    public abstract class ViewBase : IView
    {
        private readonly Dictionary<string, IView> _idToChildren = new();
        private Dictionary<string, Action<object>> _noticeDic;
        private Dictionary<string, Tuple<Type, Transform>> _subViews = new();

        protected ViewBase()
        {
            Init();
        }

        protected MPool Pool { get; private set; }

        protected object Data { get; private set; }

        public string Id { get; private set; }
        public Transform Transform { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public bool Loaded { get; private set; }
        public bool Visible { get; private set; }
        public event Action OnLoaded;
        public event Action OnUnloaded;
        public event Action OnShowed;
        public event Action OnHidden;

        public void SetId(string id)
        {
            Id = id;
        }

        public void Load(Transform tnf)
        {
            Transform = tnf;
            RectTransform = Transform.GetComponent<RectTransform>();
            Loaded = true;

            InitAttributes();

            OnLoad();

            if (Visible)
                ShowTnf();
            else
                tnf.gameObject.SetActive(false);

            OnLoaded?.Invoke();
        }

        public void Unload()
        {
            ClearNotice();

            if (Loaded)
            {
                foreach (var item in _idToChildren) Game.View.UnloadView(item.Key);

                OnUnload();
            }

            if (Pool != null)
            {
                Pool.ClearPools();
                Pool.Dispose();
                Pool = null;
            }

            Loaded = false;
            Visible = false;

            OnUnloaded?.Invoke();
        }

        /// <summary>
        ///     显示UI,
        /// </summary>
        /// <param name="data">显示用到的数据</param>
        /// <param name="parent">父节点</param>
        public void Show(object data = null)
        {
            if (Visible) return;
            Visible = true;
            Data = data;
            if (Loaded) ShowTnf();
        }

        public void Hide()
        {
            if (!Visible) return;
            // if (EventSystem.current.currentSelectedGameObject != null &&
            //     !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            // {
            //     EventSystem.current.SetSelectedGameObject(null);
            // }

            Visible = false;
            var selectGameObject = EventSystem.current.currentSelectedGameObject;
            if (selectGameObject != null && selectGameObject.transform.IsChildOf(Transform))
                EventSystem.current.SetSelectedGameObject(null);

            if (!Loaded) return;

            OnHide();

            OnHidden?.Invoke();
        }

        /// <summary>
        ///     切换显示状态
        /// </summary>
        public void ChangeVisible()
        {
            if (Visible)
                Hide();
            else
                Show();
        }

        public void SetAlpha(float alpha)
        {
            Transform.GetComp<CanvasGroup>().alpha = alpha;
        }

        public void SetInteractable(bool interactable)
        {
            Transform.GetComp<CanvasGroup>().interactable = interactable;
        }

        public void SetBlocksRaycasts(bool blocksRaycasts)
        {
            Transform.GetComp<CanvasGroup>().blocksRaycasts = blocksRaycasts;
        }

        public void SetIgnoreParentGroups(bool ignoreParentGroups)
        {
            Transform.GetComp<CanvasGroup>().ignoreParentGroups = ignoreParentGroups;
        }

        /// <summary>
        ///     实例化时初始化
        /// </summary>
        protected virtual void Init()
        {
            
        }

        private FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        private void InitAttributes()
        {
            var t = GetType();
            var fields = new List<FieldInfo>();
            var tmpT = t;
            var baseType = typeof(ViewBase);
            do
            {
                fields.AddRange(GetFields(tmpT));
                tmpT = tmpT.BaseType;
            } while (tmpT != null && tmpT != baseType);

            foreach (var field in fields)
            {
                {
                    var attr = field.GetCustomAttribute<ATouch>();
                    if (attr != null)
                    {
                        var childComp =
                            FindChildComp<RectTransform>(attr.Name ?? field.Name);
                        if (childComp == null)
                            Game.Log.Error($"{GetType()} not exist child {field.Name}:{field.FieldType}");
                        else
                            field.SetValue(this, childComp.GetComp<VTouch>());
                    }
                }
                {
                    var attr = field.GetCustomAttribute<AEnter>();
                    if (attr != null)
                    {
                        var childComp =
                            FindChildComp<RectTransform>(attr.Name ?? field.Name);
                        if (childComp == null)
                            Game.Log.Error($"{GetType()} not exist child {field.Name}:{field.FieldType}");
                        else
                            field.SetValue(this, childComp.GetComp<VEnter>());
                    }
                }
                {
                    var attr = field.GetCustomAttribute<ADrag>();
                    if (attr != null)
                    {
                        var childComp =
                            FindChildComp<RectTransform>(attr.Name ?? field.Name);
                        if (childComp == null)
                            Game.Log.Error($"{GetType()} not exist child {field.Name}:{field.FieldType}");
                        else
                            field.SetValue(this, childComp.GetComp<VDrag>());
                    }
                }
                {
                    var attr = field.GetCustomAttribute<ASelf>();
                    if (attr != null) field.SetValue(this, Transform.GetComp(field.FieldType));
                }
                {
                    var attr = field.GetCustomAttribute<AChild>();
                    if (attr != null)
                    {
                        var comName = attr.Name ?? field.Name;
                        var childComp = FindChildComp(field.FieldType, comName);
                        if (childComp == null)
                            Game.Log.Error($"{GetType()} not exist child {comName}:{field.FieldType}");
                        else
                            field.SetValue(this, childComp);
                    }
                }
            }

            {
                var attrs = t.GetCustomAttributes<ASubView>();
                foreach (var attr in attrs)
                {
                    foreach (var name in attr.Names)
                    {
                        var childComp = FindChildTnf(name);
                        if (childComp == null)
                            Game.Log.Error($"{GetType()} not exist child {name}");
                        else
                        {
                            _subViews.Add(name, new Tuple<Type, Transform>(attr.Type, childComp));
                        }
                    }
                }
            }

            {
                var attrs = t.GetCustomAttributes<APool>();
                foreach (var attr in attrs)
                {
                    BindPool(attr.Name, attr.Max, attr.Min);
                }
            }
        }

        /// <summary>
        ///     加载UI后的回调
        /// </summary>
        /// <param name="OnClickCtrl"></param>
        protected virtual void OnLoad()
        {
            foreach (var kvp in _subViews)
            {
                var (type, tnf) = kvp.Value;
                LoadSubView(type, tnf, kvp.Key);
            }
        }

        /// <summary>
        ///     卸载UI前的回调
        /// </summary>
        protected virtual void OnUnload()
        {
            foreach (var kvp in _subViews)
            {
                var (type, _) = kvp.Value;
                UnLoadSubView(type, kvp.Key);
            }
        }

        protected void ShowSubViews(params string[] names)
        {
            foreach (var name in names)
            {
                Game.View.GetView(name).Show();
            }
        }

        protected void ShowSubViews(Dictionary<string, object> data)
        {
            foreach (var kvp in data)
            {
                Game.View.GetView(kvp.Key).Show(kvp.Value);
            }
        }

        protected void ShowAllSubViews()
        {
            foreach (var kvp in _subViews)
            {
                Game.View.GetView(kvp.Key).Show();
            }
        }

        protected void HideSubViews(params string[] names)
        {
            foreach (var name in names)
            {
                Game.View.GetView(name).Hide();
            }
        }
        protected void HideAllSubViews()
        {
            foreach (var kvp in _subViews)
            {
                Game.View.GetView(kvp.Key).Hide();
            }
        }

        private void ShowTnf()
        {
            OnShow();

            OnShowed?.Invoke();
        }

        /// <summary>
        ///     显示UI回调,实现UI显示的逻辑,例如动画
        /// </summary>
        protected virtual void OnShow()
        {
            Transform.gameObject.SetActive(true);
        }

        /// <summary>
        ///     隐藏UI回调,实现UI隐藏的逻辑,例如动画
        /// </summary>
        protected virtual void OnHide()
        {
            Transform.gameObject.SetActive(false);
        }

        /// <summary>
        ///     通过节点名称广度优先搜索组件
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindChildComp<T>(string name)
        {
            return Transform.FindComp<T>(name);
        }

        public Component FindChildComp(Type type, string name)
        {
            return Transform.FindComp(type, name);
        }

        /// <summary>
        ///     通过节点名称广度优先搜索Transform
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Transform FindChildTnf(string name)
        {
            return Transform.FindTnf(name);
        }

        /// <summary>
        ///     加载子界面
        /// </summary>
        /// <param name="tnf"></param>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadSubView<T>(Transform tnf, string id = null, Action<IView, Transform> onComplete = null)
            where T : IView
        {
            return (T)LoadSubView(typeof(T), tnf, id, onComplete);
        }

        /// <summary>
        ///     加载子界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tnf"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IView LoadSubView(Type type, Transform tnf, string id = null,
            Action<IView, Transform> onComplete = null)
        {
            id = MView.GetId(type, id);
            var ui = Game.View.LoadView(type, id, tnf, onComplete);
            _idToChildren.Add(id, ui);
            return ui;
        }

        /// <summary>
        ///     卸载子界面
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IView UnLoadSubView<T>(string id = null) where T : IView
        {
            return (T)UnLoadSubView(typeof(T), id);
        }

        /// <summary>
        ///     卸载子界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IView UnLoadSubView(Type type, string id = null)
        {
            id = MView.GetId(type, id);
            _idToChildren.Remove(id);
            return Game.View.UnloadView(id);
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
            var tnf = FindChildTnf(name);
            if (tnf == null)
            {
                throw new ArgumentException($"{GetType().FullName} not exist: {name}");
            }
            tnf.gameObject.SetActive(false);
            Pool.RegisterPool(name, () => Object.Instantiate(tnf.gameObject, tnf.parent),
                o => Object.Destroy((GameObject)o), max, min);
            return tnf;
        }

        /// <summary>
        ///     生成对象池UI实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Spawn<T>(string name, bool last = true)
        {
            var go = Pool.SpawnInstance<GameObject>(name);
            go.SetActive(true);
            if (last)
                go.transform.SetAsLastSibling();
            else
                go.transform.SetAsFirstSibling();
            return go.GetComponent<T>();
        }

        /// <summary>
        ///     回收对象池UI实例
        /// </summary>
        /// <param name="go"></param>
        public void Recycle(GameObject go)
        {
            go.SetActive(false);
            Pool.RecycleInstance(go);
        }

        /// <summary>
        ///     生成对象池子界面实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T SpawnView<T>(string name, string id) where T : IView
        {
            var tnf = Spawn<Transform>(name);
            tnf.name = id;
            return LoadSubView<T>(tnf, id);
        }

        /// <summary>
        ///     回收对象池界面实例
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IView RecycleView<T>(string id) where T : IView
        {
            var view = UnLoadSubView<T>(id);
            Recycle(view.Transform.gameObject);
            return view;
        }

        /// <summary>
        ///     生成对象池子界面实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IView SpawnView(Type type, string name, string id)
        {
            return LoadSubView(type, Spawn<Transform>(name), id);
        }

        /// <summary>
        ///     回收对象池界面实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IView RecycleView(Type type, string id)
        {
            return UnLoadSubView(type, id);
        }

        public void Execute(string name, object data = null)
        {
            Game.Command.Execute(name, data);
        }

        public void DispatchNotice(string name, object data = null)
        {
            Game.Notice.DispatchNotice(name, data);
        }

        public void BindNotice(string name, Action<object> action)
        {
            if (_noticeDic == null) _noticeDic = new Dictionary<string, Action<object>>();
            _noticeDic.Add(name, action);
            Game.Notice.BindNotice(name, action);
        }

        public void UnbindNotice(string name, Action<object> action)
        {
            if (_noticeDic == null || !_noticeDic.ContainsKey(name)) return;
            _noticeDic.Remove(name);
            Game.Notice.UnbindNotice(name, action);
        }

        private void ClearNotice()
        {
            if (_noticeDic == null) return;

            foreach (var kvp in _noticeDic) Game.Notice.UnbindNotice(kvp.Key, kvp.Value);
            _noticeDic.Clear();
        }
    }
}