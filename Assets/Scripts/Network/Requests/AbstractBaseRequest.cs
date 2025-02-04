using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace TestApp.Network.Requests
{
    public abstract class AbstractBaseRequest
    {
        protected UnityWebRequest _request;
        protected Action<(bool, string)> _onRequestEnd;

        protected bool _isAborted = false;

        protected virtual int Timeout => 3;

        public AbstractBaseRequest()
        {
            SetupRequest();

            if (_request != null)
                _request.timeout = Timeout;
        }

        protected abstract void SetupRequest();

        public void SetOnRequestEnd(Action<(bool success, string data)> onRequestEnd)
        {
            _onRequestEnd = onRequestEnd;
        }

        public async UniTask SendRequest()
        {
            await _request.SendWebRequest();

            if (_isAborted || _request.result != UnityWebRequest.Result.Success)
                _onRequestEnd?.Invoke((false, null));
            else
                _onRequestEnd?.Invoke((true, _request.downloadHandler.text));
        }

        public void Cancell()
        {
            _isAborted = true;

            if (_request != null)
                _request.Abort();
        }

        public void Dispose()
        {
            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }
        }
    }
}
