using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Network.Commands
{
    public abstract class AbstractBaseCommand
    {
        protected UnityWebRequest _request;

        protected bool _isAborted = false;

        public AbstractBaseCommand()
        {
            SetupRequest();
        }

        protected abstract void SetupRequest();

        public async UniTask<(bool, string)> SendRequest()
        {
            await _request.SendWebRequest();

            if (_isAborted || _request.result != UnityWebRequest.Result.Success)
                return (false, null);

            return (true, _request.downloadHandler.text);
            //return (true, JsonConvert.DeserializeObject<T>(_request.downloadHandler.text));
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
