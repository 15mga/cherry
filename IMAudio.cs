using System;
using Cherry;
using UnityEngine;
using UnityEngine.Audio;

public interface IMAudio
{
    /// <summary>
    ///     设置默认混合器
    /// </summary>
    /// <param name="tag">混合器标签</param>
    void SetDefaultMixer(string tag);

    /// <summary>
    ///     设置声音监听对象, 在目标销毁前记得设置回来
    /// </summary>
    /// <param name="parent">监听对象</param>
    void SetListener(Transform parent = null);

    void SetListener(Vector3 localPosition, Transform parent = null);

    /// <summary>
    ///     添加混合器
    /// </summary>
    /// <param name="tag">混合器标签</param>
    /// <param name="mixer">混合器</param>
    void AddMixer(string tag, AudioMixer mixer);

    /// <summary>
    ///     加载混合器
    /// </summary>
    /// <param name="tag">混合器标签</param>
    /// <param name="key">混合器key</param>
    /// <param name="onComplete">加载完成后的回调</param>
    void LoadMixer(string tag, string key, Action onComplete = null);

    void UnloadMixer(string tag);

    /// <summary>
    ///     加载混合器
    /// </summary>
    /// <param name="tagToKey">tag key 的映射字典</param>
    /// <param name="keyToTagFunc">key转tag</param>
    /// <param name="onComplete">加载完成后的回调</param>
    void LoadMixerGroup(string group, Func<string, string> keyToTagFunc, Action onComplete = null);

    void UnloadMixerGroup(string group);

    /// <summary>
    ///     设置音频辅助器,如果不设置,默认使用资源的设置
    ///     每次实例化新的音源都使用该辅助器重置相关参数
    /// </summary>
    /// <param name="key"></param>
    /// <param name="helper"></param>
    void BindAudioSourceHelper(string key, IPoolHelper<AudioSource> helper);

    /// <summary>
    ///     播放音源
    /// </summary>
    /// <param name="key">资源key</param>
    /// <param name="tag">标签</param>
    /// <param name="onPlay"></param>
    /// <param name="onComplete"></param>
    /// <param name="helper">配置选项</param>
    /// <param name="target">播放的对象,如果对象为空,则挂载到声音监听对象</param>
    /// <param name="autoRelease"></param>
    /// <returns>AudioSource的id,可用于暂停,停止</returns>
    string PlaySource(string key, string tag = null, Action<AudioSource> onPlay = null, Action onComplete = null,
        IObjectHelper<AudioSource> helper = null, Transform target = null, bool autoRelease = false);

    string PlayWeb(string url, string tag = null, Action<AudioSource> onPlay = null, Action onComplete = null,
        IObjectHelper<AudioSource> helper = null, Transform target = null);

    string PlayClip(AudioClip clip, string tag = null, Action<AudioSource> onPlay = null,
        Action onComplete = null, IObjectHelper<AudioSource> helper = null, Transform target = null,
        bool autoRelease = false);
    void ReleaseSourceByTag(string tag);
    void ReleaseAllSource();

    /// <summary>
    ///     通过id暂停音源
    /// </summary>
    /// <param name="id"></param>
    void Pause(string id);

    /// <summary>
    ///     通过id回复音源播放
    /// </summary>
    /// <param name="id"></param>
    void UnPause(string id);

    /// <summary>
    ///     通过id停止播放
    /// </summary>
    /// <param name="id"></param>
    void Stop(string id);

    /// <summary>
    ///     按标签暂停
    /// </summary>
    /// <param name="tag"></param>
    void PauseByTag(string tag);

    /// <summary>
    ///     按标签恢复播放
    /// </summary>
    /// <param name="tag"></param>
    void UnPauseByTag(string tag);

    /// <summary>
    ///     按标签停止
    /// </summary>
    /// <param name="tag"></param>
    void StopByTag(string tag);

    /// <summary>
    ///     切换到快照设置
    /// </summary>
    /// <param name="snapshots"></param>
    /// <param name="weights"></param>
    /// <param name="timeToReach"></param>
    /// <param name="tag"></param>
    void TransitionSnapshot(string[] snapshots, float[] weights, float timeToReach, string mixer = null);

    /// <summary>
    ///     获取混合器组
    /// </summary>
    /// <param name="group"></param>
    /// <param name="mixer"></param>
    /// <returns></returns>
    AudioMixerGroup[] GetGroup(string group, string mixer = null);
}