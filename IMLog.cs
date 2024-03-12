using System;
using UnityEngine;

namespace Cherry
{
    /// <summary>
    ///     日志级别
    /// </summary>
    public enum ELogLevel
    {
        /// <summary>
        ///     调试
        /// </summary>
        Debug,

        /// <summary>
        ///     信息
        /// </summary>
        Info,

        /// <summary>
        ///     警告
        /// </summary>
        Warn,

        /// <summary>
        ///     错误,不影响运行
        /// </summary>
        Error,
        Fatal
    }

    /// <summary>
    ///     日志模块
    /// </summary>
    public interface IMLog
    {
        public const string DefaultTag = "default";
        ELogLevel Level { get; set; }

        /// <summary>
        ///     调试
        /// </summary>
        void Debug(string msg, string tag = DefaultTag);

        /// <summary>
        ///     信息
        /// </summary>
        void Info(string msg, string tag = DefaultTag);

        /// <summary>
        ///     警告
        /// </summary>
        void Warn(string msg, string tag = DefaultTag);

        /// <summary>
        ///     错误,不影响运行
        /// </summary>
        void Error(string msg, string tag = DefaultTag);

        void Fatal(string msg, string tag = DefaultTag);

        void AddIncludeTag(string tag);
        void RemoveIncludeTag(string tag);
        void ClearIncludeTags();

        void RegisterLogWriter(ILogWriter writer);
        void RemoveLogWriter(ILogWriter writer);
    }

    public interface ILogger
    {
        void Log(ELogLevel level, string msg);
    }

    public interface ILogWriter : IDisposable
    {
        void Write(string condition, string stacktrace, LogType type);
    }
}