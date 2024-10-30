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
        public const string N_Begin = "asset_update_begin";
        public const string N_NoNews = "asset_no_news";
        public const string N_Over = "asset_update_over";
        public const string N_Progress = "asset_progress";
        public const string N_Error = "asset_update_error";
        public const string N_Item = "asset_update_download_item";

        private readonly Dictionary<GameObject, AssetHandle> _goToHandle = new();
        private readonly Dictionary<Object, AssetHandle> _objToHandle = new();
        private readonly Dictionary<string, Scene> _sceneToHandle = new();

        public static InitializeParameters InitParams;

        private ResourcePackage _package;

        public string Ver => _package.GetPackageVersion();

        public void Init(string pkg, Action onComplete)
        {
            YooAssets.Initialize(new Logger());
            _package = YooAssets.CreatePackage(pkg);
            _package.InitializeAsync(InitParams).Completed += op =>
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
                    if (op.Result == null)
                    {
                        Game.Log.Error($"spawn failed:{path}");
                        return;
                    }
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

        public void UpdateVersion()
        {
            var op = _package.RequestPackageVersionAsync();
            op.Completed += _ =>
            {
                if (op.Status != EOperationStatus.Succeed)
                {
                    Game.Log.Error(op.Error);
                    Game.Notice.DispatchNotice(N_NoNews);
                    return;
                }

                UpdateManifest(op.PackageVersion);
            };
        }

        private void UpdateManifest(string ver)
        {
            var op = _package.UpdatePackageManifestAsync(ver);
            op.Completed += _ =>
            {
                if (op.Status != EOperationStatus.Succeed)
                {
                    Game.Log.Error(op.Error);
                    Game.Notice.DispatchNotice(N_NoNews);
                    return;
                }

                Download(ver);
            };
        }

        private void Download(string version)
        {
            var op = _package.CreateResourceDownloader(10, 3);
            if (op.TotalDownloadCount == 0)
            {
                Game.Notice.DispatchNotice(N_NoNews);
                return;
            }
            Game.Log.Debug($"version:{version} count:{op.TotalDownloadCount}");
            Game.Notice.DispatchNotice(N_Begin, new BeginInfo
            {
                RemoteVer = version,
                Count = op.TotalDownloadCount,
                Bytes = op.TotalDownloadBytes
            });
            op.OnDownloadProgressCallback = (count, downloadCount, bytes, downloadBytes) =>
            {
                Game.Notice.DispatchNotice(N_Progress, new ProgressInfo
                {
                    TotalCount = count,
                    CurrCount = downloadCount,
                    TotalBytes = bytes,
                    CurrBytes = downloadBytes
                });
            };
            op.OnDownloadErrorCallback = (name, error) =>
            {
                Game.Notice.DispatchNotice(N_Error, new ErrorItem
                {
                    Name = name,
                    Error = error
                });
            };
            op.OnStartDownloadFileCallback = (name, bytes) =>
            {
                Game.Notice.DispatchNotice(N_Item, new UpdateItem
                {
                    Name = name,
                    Bytes = bytes
                });
            };
            op.OnDownloadOverCallback = _ =>
            {
                Game.Notice.DispatchNotice(N_Over, new OverInfo
                {
                    Success = true
                });
            };
            op.BeginDownload();
        }

        public class BeginInfo
        {
            public long Bytes;
            public int Count;
            public string RemoteVer;
        }

        public class ProgressInfo
        {
            public long CurrBytes;
            public int CurrCount;
            public long TotalBytes;
            public int TotalCount;
        }

        public class ErrorItem
        {
            public string Error;
            public string Name;
        }

        public class UpdateItem
        {
            public long Bytes;
            public string Name;
        }

        public class OverInfo
        {
            public string Error;
            public bool Success;
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