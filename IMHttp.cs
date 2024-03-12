using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cherry
{
    public interface IMHttp
    {
        void SetUrl(string key, string uri);
        string GetUri(string key, string relativePath);
        void SetCertPubKey(string key, string pubKey);
        string GetCertPubKey(string key);
        void SetHeader(string key, Dictionary<string, string> header);
        Dictionary<string, string> GetHeader(string key);

        void Get(string key, string relativePath, Action<byte[], IError> onResponse,
            Dictionary<string, string> header = null);


        void Post(string key, string relativePath, string data, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);


        void Post(string key, string relativePath, WWWForm form, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);

        //
        void PostJson(string key, string relativePath, object obj = null,
            Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);

        void Post(string key, string relativePath, Dictionary<string, string> fields,
            Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);


        void Put(string key, string relativePath, string data, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);


        void Put(string key, string relativePath, byte[] data, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);


        void Delete(string key, string relativePath, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null);
    }
}