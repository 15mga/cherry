using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Cherry.Extend;
using Cherry.Misc;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Cherry.Http
{
    public class MHttp : IMHttp
    {
        private readonly Dictionary<string, CertPubKey> _keyToCertificateHandler = new();

        private readonly Dictionary<string, Dictionary<string, string>> _keyToHeader = new();

        private readonly Dictionary<string, string> _keyToUri = new();

        public void SetUrl(string key, string uri)
        {
            _keyToUri[key] = uri;
        }

        public string GetUri(string key, string relativePath)
        {
            return _keyToUri.TryGetValue(key, out var uri) ? $"{uri}/{relativePath}" : $"{key}/{relativePath}";
        }

        public void SetCertPubKey(string key, string pubKey)
        {
            _keyToCertificateHandler[key] = new CertPubKey { PubKey = pubKey };
        }

        public string GetCertPubKey(string key)
        {
            return _keyToCertificateHandler.TryGetValue(key, out var certificateHandler)
                ? certificateHandler.PubKey
                : null;
        }

        public void SetHeader(string key, Dictionary<string, string> header)
        {
            _keyToHeader[key] = header;
        }

        public Dictionary<string, string> GetHeader(string key)
        {
            return _keyToHeader.TryGetValue(key, out var defaultHeader)
                ? defaultHeader
                : null;
        }

        public virtual void Get(string key, string relativePath, Action<byte[], IError> onResponse,
            Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Get(GetUri(key, relativePath)), onResponse, header);
        }

        public virtual void Post(string key, string relativePath, string data,
            Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () =>
            {
                if (string.IsNullOrEmpty(data)) data = "{}";

                var url = GetUri(key, relativePath);
                var req = UnityWebRequest.Put(url, data);
                req.method = UnityWebRequest.kHttpVerbPOST;
                return req;
            }, onResponse, header);
        }

        public virtual void PostJson(string key, string relativePath, object obj = null,
            Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () =>
            {
                var str = JsonConvert.SerializeObject(obj ?? new Dictionary<string, object>());
                var uri = GetUri(key, relativePath);
                var req = new UnityWebRequest(uri, "POST");
                req.uploadHandler = new UploadHandlerRaw(str.ToUTF8Bytes());
                req.SetRequestHeader("Content-Type", "application/json");
                return req;
            }, onResponse, header);
        }

        public virtual void Post(string key, string relativePath, WWWForm form,
            Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Post(GetUri(key, relativePath), form), onResponse, header);
        }

        public virtual void Post(string key, string relativePath, Dictionary<string, string> fields,
            Action<byte[], IError> onResponse = null, Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Post(GetUri(key, relativePath), fields), onResponse, header);
        }

        public virtual void Put(string key, string relativePath, string data, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Put(GetUri(key, relativePath), data), onResponse, header);
        }

        public virtual void Put(string key, string relativePath, byte[] data, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Put(GetUri(key, relativePath), data), onResponse, header);
        }

        public virtual void Delete(string key, string relativePath, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            Request(key, () => UnityWebRequest.Delete(GetUri(key, relativePath)), onResponse, header);
        }

        private void Request(string key, Func<UnityWebRequest> request, Action<byte[], IError> onResponse = null,
            Dictionary<string, string> header = null)
        {
            var req = request();
            req.timeout = 5;
            if (_keyToCertificateHandler.TryGetValue(key, out var handler)) req.certificateHandler = handler;

            if (_keyToHeader.TryGetValue(key, out var defaultHeader)) WriteHeader(req, defaultHeader);

            WriteHeader(req, header);

            req.SendWebRequest().completed += asyncOperation =>
            {
                if (string.IsNullOrEmpty(req.error))
                    onResponse?.Invoke(req.downloadHandler.data, null);
                else
                    onResponse?.Invoke(null, new Error(req.error));
                req.Dispose();
            };
        }

        public static void WriteHeader(UnityWebRequest request, Dictionary<string, string> header)
        {
            if (header == null) return;

            foreach (var item in header) request.SetRequestHeader(item.Key, item.Value);
        }

        private class CertPubKey : CertificateHandler
        {
            private readonly X509Certificate2 _certificate = new();
            public string PubKey { get; set; }

            protected override bool ValidateCertificate(byte[] certificateData)
            {
                _certificate.Import(certificateData);
                var pk = _certificate.GetPublicKeyString();
                return PubKey.Equals(pk);
            }
        }
    }
}