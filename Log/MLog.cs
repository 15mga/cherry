using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cherry.Log
{
    public class MLog : IMLog
    {
        private readonly HashSet<string> _includeTags = new();
        private readonly List<ILogWriter> _writer = new();

        public MLog() : this(ELogLevel.Info)
        {
        }

        public MLog(ELogLevel level)
        {
            _includeTags.Add(IMLog.DefaultTag);

            Level = level;

            Application.logMessageReceived += OutputLog;
            Game.OnDispose += () =>
            {
                for (var index = 0; index < _writer.Count; index++) _writer[index].Dispose();
            };
        }

        public ELogLevel Level { get; set; }

        public void Debug(string msg, string tag = IMLog.DefaultTag)
        {
            Log(ELogLevel.Debug, msg);
        }

        public void Info(string msg, string tag = IMLog.DefaultTag)
        {
            Log(ELogLevel.Info, msg);
        }

        public void Warn(string msg, string tag = IMLog.DefaultTag)
        {
            Log(ELogLevel.Warn, msg);
        }

        public void Error(string msg, string tag = IMLog.DefaultTag)
        {
            Log(ELogLevel.Error, msg);
        }

        public void Fatal(string msg, string tag = IMLog.DefaultTag)
        {
            Log(ELogLevel.Fatal, msg);
        }

        public void AddIncludeTag(string tag)
        {
            _includeTags.Add(tag);
        }

        public void RemoveIncludeTag(string tag)
        {
            _includeTags.Remove(tag);
        }

        public void ClearIncludeTags()
        {
            _includeTags.Clear();
        }

        public void RegisterLogWriter(ILogWriter writer)
        {
            _writer.Add(writer);
        }

        public void RemoveLogWriter(ILogWriter writer)
        {
            _writer.Remove(writer);
        }

        private void Log(ELogLevel level, string msg, string tag = IMLog.DefaultTag)
        {
            if (level < Level || !_includeTags.Contains(tag)) return;

            switch (level)
            {
                case ELogLevel.Debug:
                    UnityEngine.Debug.Log($"<color=green>{msg}</color>");
                    break;
                case ELogLevel.Info:
                    UnityEngine.Debug.Log(msg);
                    break;
                case ELogLevel.Warn:
                    UnityEngine.Debug.LogWarning(msg);
                    break;
                case ELogLevel.Error:
                    UnityEngine.Debug.LogError(msg);
                    break;
                case ELogLevel.Fatal:
                    UnityEngine.Debug.LogException(new Exception(msg));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private void OutputLog(string condition, string stacktrace, LogType type)
        {
            foreach (var t in _writer)
                t.Write(condition, stacktrace, type);
        }
    }
}