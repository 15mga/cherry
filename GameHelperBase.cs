using System;
using Cherry.Asset;
using Cherry.Cam;
using Cherry.Command;
using Cherry.Ctrl;
using Cherry.Fsm;
using Cherry.Http;
using Cherry.Log;
using Cherry.Model;
using Cherry.Notice;
using Cherry.Pool;
using Cherry.SceneObj;
using Cherry.Task;
using Cherry.Timer;
using Cherry.Trigger;
using Cherry.View;
using UnityEngine;
using YooAsset;

namespace Cherry
{
    public enum EAssetMode
    {
        Editor,
        Offline,
        Host,
    }
    public abstract class GameHelperBase : MonoBehaviour, IGameHelper
    {
        [Header("资源加载类型")] [SerializeField] protected EAssetMode _assetMode;
        
        [Header("启用HTTP模块")] [SerializeField] protected bool http;

        [Header("日志级别")] public ELogLevel logLevel = ELogLevel.Debug;

        [Header("启用Http Log")] [SerializeField]
        protected string httpLog = "";

        /// <summary>
        ///     初始化内置模块
        /// </summary>
        public virtual void Init()
        {
            InitTask();
            InitPool();
            InitTimer();
            InitLog();
            InitTrigger();

            InitFsm();
            InitCommand();
            InitNotice();
            if (http || !string.IsNullOrEmpty(httpLog)) InitHttp();

            InitAsset();
            InitModel();
            InitCtrl();
            InitView();

            InitLayer();

            InitCamera();
        }

        /// <summary>
        ///     框架初始化完成,开始业务逻辑
        /// </summary>
        public virtual void Begin()
        {
            if (!string.IsNullOrEmpty(httpLog)) Game.Log.RegisterLogWriter(new HttpLogWriter(httpLog, logLevel));

#if !UNITY_EDITOR
                Game.Log.RegisterLogWriter(new FileLogWriter());
#endif
        }

        protected virtual void InitAsset()
        {
            switch (_assetMode)
            {
                case EAssetMode.Editor:
                    Game.Register(MAsset.CreateEditor());
                    break;
                case EAssetMode.Offline:
                    Game.Register(MAsset.CreateOffline());
                    break;
                case EAssetMode.Host:
                    Game.Register(MAsset.CreateHost(HostPlayModeParameters()));
                    break;
                default:
                    throw new ArgumentException($"not support mode: {_assetMode}");
            }
        }

        protected virtual void InitTimer()
        {
            Game.Register<IMTimer, MTimer>();
        }

        protected virtual void InitLog()
        {
            Game.Register<IMLog, MLog>().Level = logLevel;
        }

        protected virtual void InitNotice()
        {
            Game.Register<IMNotice, MNotice>();
        }

        protected virtual void InitTrigger()
        {
            Game.Register<IMTrigger, MTrigger>();
        }

        private void InitTask()
        {
            Game.Register<IMTask, MTask>();
        }

        protected virtual void InitHttp()
        {
            Game.Register<IMHttp, MHttp>();
        }

        protected virtual void InitFsm()
        {
            Game.Register<IMFsm, MFsm>();
        }

        protected virtual void InitPool()
        {
            Game.Register<IMPool, MPool>();
        }

        protected virtual void InitCommand()
        {
            Game.Register<IMCommand, MCommand>();
        }

        protected virtual void InitView()
        {
            Game.Register<IMView, MView>();
        }

        protected virtual void InitModel()
        {
            Game.Register<IMModel, MModel>();
        }

        protected virtual void InitCtrl()
        {
            Game.Register<IMCtrl, MCtrl>();
        }

        protected virtual void InitLayer()
        {
            Game.Register<IMScene, MScene>();
        }

        protected virtual void InitCamera()
        {
            Game.Register<IMCamera, MCamera>();
        }

        protected virtual HostPlayModeParameters HostPlayModeParameters()
        {
            throw new NotImplementedException();
        }
    }
}