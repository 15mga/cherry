using System;
using System.Collections.Generic;
using Cherry.Extend;
using Cherry.Pool;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Cherry.Audio
{
    public class MAudio : IMAudio
    {
        private readonly Pool<Audio> _audioPool = new();

        private readonly Pool<GameObject> _audioSourcePool;
        private readonly Dictionary<string, List<string>> _groupToTag = new();

        private readonly Dictionary<string, Audio> _idToAudio = new();

        private readonly Dictionary<string, List<AudioClip>> _tagToClip = new();

        private readonly Dictionary<string, IPoolHelper<AudioSource>> _tagToHelper = new();

        private readonly Dictionary<string, List<string>> _tagToId = new();
        private readonly Dictionary<string, AudioMixer> _tagToMixers = new();

        private string _defaultMixer;

        private readonly Transform _root;

        public MAudio()
        {
            _root = Game.CreateRoot("MAudio");
            _root.gameObject.AddComponent<AudioListener>();
            _audioSourcePool = new Pool<GameObject>(() =>
            {
                var go = new GameObject("audio source");
                go.transform.SetParent(_root);
                return go;
            }, Object.Destroy);
            SetListener();
        }

        public void SetDefaultMixer(string tag)
        {
            _defaultMixer = tag;
        }

        public void SetListener(Transform parent = null)
        {
            SetListener(Vector3.zero, parent);
        }

        public void SetListener(Vector3 localPosition, Transform parent = null)
        {
            if (parent == null) parent = Game.Instance.transform;

            _root.SetParent(parent);
            _root.localPosition = localPosition;
        }

        public void AddMixer(string tag, AudioMixer mixer)
        {
            _tagToMixers[tag] = mixer;
        }

        public void LoadMixer(string tag, string key, Action onComplete = null)
        {
            if (_defaultMixer == null) _defaultMixer = tag;

            Game.Asset.Load<AudioMixer>(key, mixer =>
            {
                _tagToMixers.Add(tag, mixer);
                onComplete?.Invoke();
            });
        }

        public void UnloadMixer(string tag)
        {
            if (_tagToMixers.TryGetValue(tag, out var mixer))
            {
                if (_defaultMixer.Equals(tag)) _defaultMixer = null;
                _tagToMixers.Remove(tag);
                Game.Asset.Release(mixer);
            }
            else
            {
                Game.Log.Warn($"not exist tag:{tag}");
            }
        }

        public void LoadMixerGroup(string group, Func<string, string> keyToTagFunc, Action onComplete = null)
        {
            _groupToTag.Add(group, new List<string>());
            Game.Asset.LoadTag<AudioMixer>(group, onComplete, (key, mixer) =>
            {
                if (!_groupToTag.TryGetValue(group, out var list)) return;

                var tag = keyToTagFunc(key);
                list.Add(tag);
                _tagToMixers.Add(tag, mixer);
            });
        }

        public void UnloadMixerGroup(string group)
        {
            if (!_groupToTag.TryGetValue(group, out var list)) return;

            foreach (var tag in list)
            {
                if (_tagToMixers.TryGetValue(tag, out var mixer)) Game.Asset.Release(mixer);
                _tagToMixers.Remove(tag);
            }

            _groupToTag.Remove(group);
        }

        public void BindAudioSourceHelper(string key, IPoolHelper<AudioSource> helper)
        {
            _tagToHelper[key] = helper;
        }

        public string PlayWeb(string url, string tag = null, Action<AudioSource> onPlay = null,
            Action onComplete = null, IObjectHelper<AudioSource> helper = null, Transform target = null)
        {
            if (string.IsNullOrEmpty(tag)) tag = url;

            var id = Game.GetGuid();
            var audio = _audioPool.Spawn();
            audio.free = false;
            audio.tag = tag;

            _idToAudio[id] = audio;
            if (!_tagToId.TryGetValue(tag, out var list))
            {
                list = new List<string>();
                _tagToId.Add(tag, list);
            }

            list.Add(id);

            url.LoadWebAudio((clip, err) =>
            {
                if (!_idToAudio.ContainsKey(id)) return;

                var go = _audioSourcePool.Spawn();
                go.transform.SetParent(target ? target : _root);
                go.transform.localPosition = Vector3.zero;

                var audioSource = go.GetComp<AudioSource>();
                ResetAudio(tag, audioSource);
                audioSource.clip = clip;
                helper?.Set(audioSource);
                onPlay?.Invoke(audioSource);
                audio.Play(audioSource, () =>
                {
                    _idToAudio.Remove(id);
                    _tagToId[tag].Remove(id);
                    onComplete?.Invoke();
                }, _audioSourcePool.Recycle, ((AudioHelper)helper)?.repeat ?? 1);
            });
            return id;
        }

        public string PlaySource(string key, string tag = null, Action<AudioSource> onPlay = null,
            Action onComplete = null, IObjectHelper<AudioSource> helper = null, Transform target = null,
            bool autoRelease = false)
        {
            if (string.IsNullOrEmpty(tag)) tag = key;

            var id = Game.GetGuid();
            var audio = _audioPool.Spawn();
            audio.free = false;
            audio.tag = tag;

            _idToAudio[id] = audio;
            if (!_tagToId.TryGetValue(tag, out var list))
            {
                list = new List<string>();
                _tagToId.Add(tag, list);
            }

            list.Add(id);

            Game.Asset.Load<AudioClip>(key, clip =>
            {
                if (!_idToAudio.ContainsKey(id)) return;

                var go = _audioSourcePool.Spawn();
                go.transform.SetParent(target ? target : _root);
                go.transform.localPosition = Vector3.zero;

                var audioSource = go.GetComp<AudioSource>();
                ResetAudio(tag, audioSource);
                audioSource.clip = clip;
                helper?.Set(audioSource);
                onPlay?.Invoke(audioSource);
                audio.Play(audioSource, () =>
                {
                    _idToAudio.Remove(id);
                    _tagToId[tag].Remove(id);
                    if (autoRelease)
                    {
                        Game.Asset.Release(clip);
                    }
                    else
                    {
                        if (!_tagToClip.TryGetValue(tag, out var clips))
                        {
                            clips = new List<AudioClip>();
                            _tagToClip.Add(tag, clips);
                        }

                        clips.Add(clip);
                    }

                    onComplete?.Invoke();
                }, _audioSourcePool.Recycle, ((AudioHelper)helper)?.repeat ?? 1);
            });

            return id;
        }

        public string PlayClip(AudioClip clip, string tag = null, Action<AudioSource> onPlay = null,
            Action onComplete = null, IObjectHelper<AudioSource> helper = null, Transform target = null,
            bool autoRelease = false)
        {
            if (string.IsNullOrEmpty(tag)) tag = "";
            var id = Game.GetGuid();
            var audio = _audioPool.Spawn();
            audio.free = false;
            audio.tag = tag;
            
            _idToAudio[id] = audio;
            if (!_tagToId.TryGetValue(tag, out var list))
            {
                list = new List<string>();
                _tagToId.Add(tag, list);
            }

            list.Add(id);
            
            
            var go = _audioSourcePool.Spawn();
            go.transform.SetParent(target ? target : _root);
            go.transform.localPosition = Vector3.zero;

            var audioSource = go.GetComp<AudioSource>();
            ResetAudio(tag, audioSource);
            audioSource.clip = clip;
            helper?.Set(audioSource);
            onPlay?.Invoke(audioSource);
            audio.Play(audioSource, () =>
            {
                _idToAudio.Remove(id);
                _tagToId[tag].Remove(id);
                if (autoRelease)
                {
                    Game.Asset.Release(clip);
                }
                else
                {
                    if (!_tagToClip.TryGetValue(tag, out var clips))
                    {
                        clips = new List<AudioClip>();
                        _tagToClip.Add(tag, clips);
                    }

                    clips.Add(clip);
                }

                onComplete?.Invoke();
            }, _audioSourcePool.Recycle, ((AudioHelper)helper)?.repeat ?? 1);
            
            return id;
        }

        public void ReleaseSourceByTag(string tag)
        {
            if (!_tagToClip.TryGetValue(tag, out var list)) return;

            foreach (var t in list) Game.Asset.Release(t);

            _tagToClip.Remove(tag);
        }

        public void ReleaseAllSource()
        {
            foreach (var item in _tagToClip)
            {
                var list = item.Value;
                foreach (var t in list) Game.Asset.Release(t);
            }

            _tagToClip.Clear();
        }

        public void Pause(string id)
        {
            if (_idToAudio.TryGetValue(id, out var audio))
            {
                audio.Pause();
                return;
            }

            Game.Log.Error($"not exist audio id{id}");
        }

        public void UnPause(string id)
        {
            if (_idToAudio.TryGetValue(id, out var audio))
            {
                audio.UnPause();
                return;
            }

            Game.Log.Error($"not exist audio id{id}");
        }

        public void Stop(string id)
        {
            if (string.IsNullOrEmpty(id) || !_idToAudio.TryGetValue(id, out var audio)) return;

            audio.Stop();
            _idToAudio.Remove(id);
            _tagToId[audio.tag].Remove(id);
            _audioPool.Recycle(audio);
        }

        public void PauseByTag(string tag)
        {
            if (!_tagToId.TryGetValue(tag, out var list)) return;

            foreach (var item in list) _idToAudio[item].Pause();
        }

        public void UnPauseByTag(string tag)
        {
            if (!_tagToId.TryGetValue(tag, out var list)) return;

            foreach (var item in list) _idToAudio[item].UnPause();
        }

        public void StopByTag(string tag)
        {
            if (!_tagToId.TryGetValue(tag, out var list)) return;

            foreach (var id in list)
            {
                var audio = _idToAudio[id];
                audio.Stop();
                _idToAudio.Remove(id);
                _audioPool.Recycle(audio);
            }

            list.Clear();
        }

        public void TransitionSnapshot(string[] snapshots, float[] weights, float timeToReach, string mixer = null)
        {
            if (mixer == null) mixer = _defaultMixer;

            if (!_tagToMixers.TryGetValue(mixer, out var audioMixer))
            {
                Game.Log.Error($"not exist audio mixer {mixer}");
                return;
            }

            var ss = new List<AudioMixerSnapshot>();
            var ws = new List<float>(weights);

            var count = snapshots.Length - 1;
            for (var i = count; i > -1; i--)
            {
                var snapshot = snapshots[i];
                var s = audioMixer.FindSnapshot(snapshot);
                if (s == null)
                {
                    Game.Log.Error($"not exist snapshot {snapshot}");
                    ws.RemoveAt(i);
                    continue;
                }

                ss.Add(s);
            }

            audioMixer.TransitionToSnapshots(ss.ToArray(), ws.ToArray(), timeToReach);
        }

        public AudioMixerGroup[] GetGroup(string group, string mixer = null)
        {
            if (mixer == null)
            {
                if (_defaultMixer == null) return null;
                mixer = _defaultMixer;
            }

            if (!_tagToMixers.TryGetValue(mixer, out var audioMixer))
            {
                Game.Log.Error($"not exist audio mixer tag {mixer}");
                return null;
            }

            return audioMixer.FindMatchingGroups(group);
        }

        private void ResetAudio(string tag, AudioSource audioSource)
        {
            if (_tagToHelper.TryGetValue(tag, out var helper))
            {
                if (helper.Initialized)
                    helper.Set(audioSource);
                else
                    helper.Init(audioSource);
            }
            else
            {
                helper = new AudioPoolHelper();
                helper.Init(audioSource);
                _tagToHelper.Add(tag, helper);
            }
        }

        private class Audio
        {
            private Action onComplete;
            private Func<GameObject, bool> onDispose;
            private bool pause;
            private int repeat;
            private AudioSource source;
            private string timerId;
            public string tag { get; set; }
            public bool free { get; set; }

            public void Play(AudioSource src, Action onPlayComplete, Func<GameObject, bool> onGameObjectDispose,
                int rpt)
            {
                if (free) return;

                source = src;
                onComplete = onPlayComplete;
                onDispose = onGameObjectDispose;
                repeat = rpt;

                if (pause) return;

                DoPlay();
            }

            private void DoPlay()
            {
                if (repeat > 0)
                {
                    repeat--;
                    timerId = Game.Timer.Bind(source.clip.length, OnComplete);
                    source.loop = false;
                }
                else
                {
                    source.loop = true;
                }

                source.Play();
            }

            private void OnComplete()
            {
                if (repeat > 0)
                {
                    DoPlay();
                }
                else
                {
                    onComplete?.Invoke();
                    Dispose();
                }
            }

            public void Pause()
            {
                pause = true;
                if (source == null || !source.isPlaying) return;

                source.Pause();
                Game.Timer.Unbind(timerId);
            }

            public void UnPause()
            {
                pause = false;
                if (source == null || source.isPlaying) return;

                source.UnPause();
                timerId = Game.Timer.Bind(source.clip.length - source.time, OnComplete);
            }

            public void Stop()
            {
                Game.Timer.Unbind(timerId);

                Dispose();
            }

            private void Dispose()
            {
                if (free) return;
                free = true;
                pause = false;
                if (source != null)
                {
                    if (source.isPlaying) source.Stop();

                    source.clip = null;
                    onDispose?.Invoke(source.gameObject);
                    source = null;
                }

                timerId = null;
                onComplete = null;
                onDispose = null;
            }
        }
    }

    public class AudioHelper : IObjectHelper<AudioSource>
    {
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        public float dopplerLevel = 1;

        /// <summary>
        ///     AudioMixerGroup名称
        /// </summary>
        public string group;

        public float maxDistance = 500;
        public float minDistance = 1;

        /// <summary>
        ///     AudioMixer名称,如果group不为空,mixer默认为Master
        /// </summary>
        public string mixer;

        public bool mute;
        public float panStereo;
        public float pitch = 1;
        public bool playOnAwake = true;
        public int priority = 128;

        /// <summary>
        ///     对象池重置无效,播放音效有效,0:循环播放,>0重复播放次数, 默认=1
        /// </summary>
        public int repeat = 1;

        public float reverbZoneMix = 1;
        public float spatialBlend;
        public float spread;
        public float volume = 1;

        public void Set(AudioSource audioSource)
        {
            if (string.IsNullOrEmpty(mixer))
            {
                var groups = Game.Audio.GetGroup(group ?? "Master", mixer);
                if (groups != null && groups.Length > 0) audioSource.outputAudioMixerGroup = groups[0];
            }

            audioSource.mute = mute;
            audioSource.bypassEffects = bypassEffects;
            audioSource.bypassListenerEffects = bypassListenerEffects;
            audioSource.bypassReverbZones = bypassReverbZones;
            audioSource.playOnAwake = playOnAwake;
            audioSource.loop = false;
            audioSource.priority = priority;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.panStereo = panStereo;
            audioSource.spatialBlend = spatialBlend;
            audioSource.reverbZoneMix = reverbZoneMix;
            audioSource.dopplerLevel = dopplerLevel;
            audioSource.spread = spread;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }
    }

    public class AudioPoolHelper : AudioHelper, IPoolHelper<AudioSource>
    {
        public bool Initialized { get; private set; }

        public void Init(AudioSource audioSource)
        {
            Initialized = true;

            mute = audioSource.mute;
            bypassEffects = audioSource.bypassEffects;
            bypassListenerEffects = audioSource.bypassListenerEffects;
            bypassReverbZones = audioSource.bypassReverbZones;
            playOnAwake = audioSource.playOnAwake;
            priority = audioSource.priority;
            volume = audioSource.volume;
            pitch = audioSource.pitch;
            volume = audioSource.volume;
            panStereo = audioSource.panStereo;
            spatialBlend = audioSource.spatialBlend;
            reverbZoneMix = audioSource.reverbZoneMix;
            dopplerLevel = audioSource.dopplerLevel;
            spread = audioSource.spread;
            minDistance = audioSource.minDistance;
            maxDistance = audioSource.maxDistance;
        }
    }
}