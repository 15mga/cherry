using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace Cherry.Asset
{
    public class MAsset : IMAsset
    {
        public const string N_ManifestSuccess = "asset_manifest_success";
        public const string N_Begin = "asset_update_begin";
        public const string N_NoNews = "asset_no_news";
        public const string N_Over = "asset_update_over";
        public const string N_Progress = "asset_progress";
        public const string N_Error = "asset_update_error";
        public const string N_Item = "asset_update_download_item";
        public class BeginInfo
        {
            public string LocalVer;
            public string RemoteVer;
            public int Count;
            public long Bytes;
        }

        public class ProgressInfo
        {
            public int TotalCount;
            public int CurrCount;
            public long TotalBytes;
            public long CurrBytes;
        }
        public class ErrorItem
        {
            public string Name;
            public string Error;
        }

        public class UpdateItem
        {
            public string Name;
            public long Bytes;
        }
        public class OverInfo
        {
            public bool Success;
            public string Error;
        }

        private readonly Dictionary<GameObject, AssetHandle> _goToHandle = new();
        private readonly Dictionary<Object, AssetHandle> _objToHandle = new();
        private readonly Dictionary<string, Scene> _sceneToHandle = new();

        private ResourcePackage _package;
        private InitializeParameters _initParams;

        public string Ver => _package.GetPackageVersion();

        private EDefaultBuildPipeline _buildPipeline;

        public static IMAsset CreateEditor(string manifestFilePath = "", EDefaultBuildPipeline buildPipeline = EDefaultBuildPipeline.ScriptableBuildPipeline)
        {
            return new MAsset
            {
                _initParams = new EditorSimulateModeParameters
                {
                    SimulateManifestFilePath = manifestFilePath
                },
                _buildPipeline = buildPipeline,
            };
        }

        public static IMAsset CreateOffline()
        {
            return new MAsset
            {
                _initParams = new OfflinePlayModeParameters(),
            };
        }

        public static IMAsset CreateHost(HostPlayModeParameters hostParameters)
        {
            return new MAsset
            {
                _initParams = hostParameters
            };
        }

        private MAsset()
        {
            
        }

        public void Init(string pkg, Action onComplete)
        {
            YooAssets.Initialize(new Logger());
            _package = YooAssets.CreatePackage(pkg);
            if (_initParams is EditorSimulateModeParameters p)
            {
                p.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(_buildPipeline, pkg);
            }
            _package.InitializeAsync(_initParams).Completed += op =>
            {
                if (op.Status != EOperationStatus.Succeed)
                {
                    Game.Log.Error($"InitializeYooAsset error:{op.Error}");
                    return;
                }
                onComplete();
            };
        }

        public void Load<T>(string path, Action<T> onComplete = null, Action<float> onProgress = null) where T : Object
        {
            var handle = _package.LoadAssetAsync<T>(path);
            if (onProgress != null) Game.Trigger.BindTrigger(() => handle.IsDone, () => onProgress(handle.Progress));
            handle.Completed += h =>
            {
                if (h.Status != EOperationStatus.Succeed)
                {
                    Game.Log.Error($"load fail:{h.LastError}");
                    return;
                }
                if (h.AssetObject is T o)
                {
                    _objToHandle[o] = h;
                    onComplete(o);
                }
                else
                {
                    Game.Log.Error($"wrong type {typeof(T)}");
                }
            };
        }

        public void Load(Type type, string path, Action<Object> onComplete = null, Action<float> onProgress = null)
        {
            var handle = _package.LoadAssetAsync(path, type);
            if (onProgress != null) Game.Trigger.BindTrigger(() => handle.IsDone, () => onProgress(handle.Progress));
            handle.Completed += h =>
            {
                if (h.Status != EOperationStatus.Succeed)
                {
                    Game.Log.Error($"load fail:{h.LastError}");
                    return;
                }
                _objToHandle[h.AssetObject] = h;
                onComplete(h.AssetObject);
            };
        }

        public void Release<T>(T obj) where T : Object
        {
            if (!_objToHandle.TryGetValue(obj, out var handle)) return;
            _objToHandle.Remove(obj);
            handle.Release();
        }

        public void Load<T>(IList<string> paths, Action onComplete = null, Action<string, T> onCompleteItem = null,
            Action<float> onProgress = null) where T : Object
        {
            var count = paths.Count;
            var actions = new Action<Action>[count];
            for (var i = 0; i < count; i++)
            {
                var m = i;
                actions[m] = action =>
                {
                    var key = paths[m];
                    Load<T>(key, o =>
                    {
                        onCompleteItem?.Invoke(key, o);
                        action?.Invoke();
                    }, onProgress);
                };
            }

            Game.Task.PushAsyncTasks(onComplete, actions);
        }

        public void Load(IList<string> paths, Action onComplete = null,
            Action<string, Type, Object> onCompleteItem = null, Action<float> onProgress = null)
        {
            var count = paths.Count;
            var actions = new Action<Action>[count];
            for (var i = 0; i < count; i++)
                actions[i] = action =>
                {
                    var key = paths[i];
                    Load<Object>(key, o =>
                    {
                        onCompleteItem(key, o.GetType(), o);
                        action();
                    }, onProgress);
                };

            Game.Task.PushAsyncTasks(onComplete, actions);
        }

        public void LoadTag<T>(string tag, Action onComplete = null, Action<string, T> onCompleteItem = null,
            Action<float> onProgress = null) where T : Object
        {
            Load(GetAddressWithTag(tag), onComplete, onCompleteItem, onProgress);
        }

        public void LoadTag(string tag, Action onComplete = null, Action<string, Type, Object> onCompleteItem = null,
            Action<float> onProgress = null)
        {
            Load(GetAddressWithTag(tag), onComplete, onCompleteItem, onProgress);
        }

        public string[] GetAddressWithTag(string tag)
        {
            var assetInfos = _package.GetAssetInfos(tag);
            return assetInfos.Select(assetInfo => assetInfo.Address).ToArray();
        }

        public void Spawn(string path, Action<GameObject> action, Action<float> onProgress = null)
        {
            var handle = _package.LoadAssetAsync<GameObject>(path);
            if (onProgress != null) Game.Trigger.BindTrigger(() => handle.IsDone, () => onProgress(handle.Progress));
            handle.Completed += h =>
            {
                var op = h.InstantiateAsync();
                if (onProgress != null) Game.Trigger.BindTrigger(() => op.IsDone, () => onProgress(op.Progress));
                op.Completed += _ =>
                {
                    _goToHandle[op.Result] = handle;
                    action(op.Result);
                };
            };
        }

        public void Destroy(params GameObject[] gameObjects)
        {
            foreach (var o in gameObjects)
            {
                if (!_goToHandle.TryGetValue(o, out var handle)) continue;
                Object.Destroy(o);
                _goToHandle.Remove(o);
                handle.Release();
            }
        }

        public void LoadScene(string path, Action<Scene> onComplete = null, Action<float> onProgress = null)
        {
            var handle = _package.LoadSceneAsync(path, LoadSceneMode.Additive);
            if (onProgress != null) Game.Trigger.BindTrigger(() => handle.IsDone, () => onProgress(handle.Progress));
            handle.Completed += handle =>
            {
                _sceneToHandle.Add(path, handle.SceneObject);
                onComplete(handle.SceneObject);
            };
        }

        public void UnloadScene(string path, Action onComplete = null)
        {
            if (!_sceneToHandle.TryGetValue(path, out var scene)) return;
            _sceneToHandle.Remove(path);
            var op = SceneManager.UnloadSceneAsync(scene);
            if (onComplete != null)
                op.completed += _ => { onComplete(); };
        }

        public void LoadRaw(string path, Action<byte[]> onComplete, Action<float> onProgress = null)
        {
            var handle = _package.LoadRawFileAsync(path);
            if (onProgress != null)
                Game.Trigger.BindTrigger(() => handle.IsDone, () => onProgress(handle.Progress));
            handle.Completed += h =>
            {
                onComplete(h.GetRawFileData());
                handle.Release();
            };
        }

        public void UpdateAssets()
        {
            GetLocalVer();
        }

        private void GetLocalVer()
        {
            var ver = _package.GetPackageVersion();
            GetRemoteVer(ver);
        }
        
        private void GetRemoteVer(string localVer)
        {
            var op = _package.UpdatePackageVersionAsync();
            op.Completed += _ =>
            {
                if (op.Status != EOperationStatus.Succeed)
                {
                    Game.Notice.DispatchNotice(N_Over, new OverInfo
                    {
                        Success = false,
                        Error = op.Error,
                    });
                    return;
                }
                GetManifest(localVer, op.PackageVersion);
            };
        }
        
        private void GetManifest(string localVer, string remoteVer)
        {
            var op = _package.UpdatePackageManifestAsync(remoteVer);
            op.Completed += op =>
            {
                if (op.Status != EOperationStatus.Succeed)
                {
                    Game.Notice.DispatchNotice(N_Over, new OverInfo
                    {
                        Success = false,
                        Error = op.Error,
                    });
                    return;
                }

                Game.Notice.DispatchNotice(N_ManifestSuccess);
                DownloadAssets(localVer, remoteVer);
            };
        }

        private void DownloadAssets(string localVer, string remoteVer)
        {
            var op = _package.CreateResourceDownloader(10, 3);
            if (op.TotalDownloadCount == 0)
            {
                Game.Notice.DispatchNotice(N_NoNews);
                return;
            }
            Game.Notice.DispatchNotice(N_Begin, new BeginInfo
            {
                LocalVer = localVer,
                RemoteVer = remoteVer,
                Count = op.TotalDownloadCount,
                Bytes = op.TotalDownloadBytes,
            });
            op.OnDownloadProgressCallback = (count, downloadCount, bytes, downloadBytes) =>
            {
                Game.Notice.DispatchNotice(N_Progress, new ProgressInfo
                {
                    TotalCount = count,
                    CurrCount = downloadCount,
                    TotalBytes = bytes,
                    CurrBytes = downloadBytes,
                });
            };
            op.OnDownloadErrorCallback = (name, error) =>
            {
                Game.Notice.DispatchNotice(N_Error, new ErrorItem
                {
                    Name = name,
                    Error = error,
                });
            };
            op.OnStartDownloadFileCallback = (name, bytes) =>
            {
                Game.Notice.DispatchNotice(N_Item, new UpdateItem
                {
                    Name = name,
                    Bytes = bytes,
                });
            };
            op.OnDownloadOverCallback = _ =>
            {
                Game.Notice.DispatchNotice(N_Over, new OverInfo
                {
                    Success = true,
                });
            };
        }

        private class Logger : YooAsset.ILogger
        {
            public void Log(string message)
            {
                Game.Log.Info(message);
            }

            public void Warning(string message)
            {
                Game.Log.Warn(message);
            }

            public void Error(string message)
            {
                Game.Log.Error(message);
            }

            public void Exception(Exception exception)
            {
                Game.Log.Error(exception.Message);
            }
        }
    }
}