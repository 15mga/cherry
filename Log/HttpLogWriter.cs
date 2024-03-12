using System.Collections.Generic;
using UnityEngine;

namespace Cherry.Log
{
    public class HttpLogWriter : ILogWriter
    {
        public HttpLogWriter(string uri, ELogLevel level)
        {
            Uri = uri;
        }

        public string Uri { get; }

        public ELogLevel Level { get; private set; } = ELogLevel.Info;

        public void Write(string condition, string stacktrace, LogType type)
        {
            var start = condition.IndexOf(">");
            var end = condition.LastIndexOf("<");
            if (start > -1) condition = condition.Substring(start + 1, end - start - 1);
            Game.Http.PostJson(Uri, "log", new Dictionary<string, string>
            {
                { "condition", condition },
                { "stacktrace", stacktrace },
                { "type", type.ToString() }
            });
        }

        public void Dispose()
        {
            Game.Http.Post(Uri, "close", "close");
        }
    }
}