using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using TestApp.Data;
using TestApp.Network.Requests;

namespace TestApp.Network
{
    public class RequestSender : IRequestSender
    {
        private List<AbstractBaseRequest> _requests = new List<AbstractBaseRequest>();
        private AbstractBaseRequest _currentRequest = null;

        private void CheckNextRequest()
        {
            if (_requests.Count == 0)
                return;

            if (_currentRequest != null)
                return;

            SendNextRequest().Forget();
        }

        private async UniTask SendNextRequest()
        {
            var nextCommand = _requests[0];

            _currentRequest = nextCommand;
            _requests.RemoveAt(0);

            await nextCommand.SendRequest();
            nextCommand.Dispose();

            if (_currentRequest == nextCommand)
                _currentRequest = null;

            CheckNextRequest();
        }

        public async UniTask<WeatherServerData> GetWeatherData()
        {
            return await SendGetRequest<WeatherRequest, WeatherServerData>();
        }

        public void CancellWeatherDataRequest()
        {
            CancellRequestOfType<WeatherRequest>();

            CheckNextRequest();
        }

        private async UniTask<TData> SendGetRequest<TRequest, TData>() where TRequest : AbstractBaseRequest, new()
        {
            UniTaskCompletionSource<TData> requestCts = new UniTaskCompletionSource<TData>();

            TRequest nextRequest = new TRequest();
            nextRequest.SetOnRequestEnd(response =>
            {
                if (!response.success)
                {
                    requestCts.TrySetCanceled();
                    return;
                }

                UnityEngine.Debug.Log($"Request Data: {response.data}");

                requestCts.TrySetResult(JsonConvert.DeserializeObject<TData>(response.data));
            });

            _requests.Add(nextRequest);
            CheckNextRequest();

            return await requestCts.Task;
        }

        private void CancellRequestOfType<T>() where T : AbstractBaseRequest
        {
            for (int i = _requests.Count - 1; i >= 0; i--)
            {
                if (_requests[i] is T)
                {
                    _requests[i].Cancell();
                    _requests[i].Dispose();

                    _requests.RemoveAt(i);
                }
            }

            if (_currentRequest is T)
            {
                _currentRequest.Cancell();
                _currentRequest.Dispose();

                _currentRequest = null;
            }
        }

        public async UniTask<FactsArrayData> GetFactsData()
        {
            return await SendGetRequest<FactsRequest, FactsArrayData>();
        }

        public void CancellFactsDataRequest()
        {
            CancellRequestOfType<FactsRequest>();

            CheckNextRequest();
        }

        public async UniTask<FactsInfoData> GetFactInfoData(string factId)
        {
            UniTaskCompletionSource<FactsInfoData> factCts = new UniTaskCompletionSource<FactsInfoData>();

            FactInfoRequest nextRequest = new FactInfoRequest(factId);
            nextRequest.SetOnRequestEnd(response =>
            {
                if (!response.success)
                {
                    factCts.TrySetCanceled();
                    return;
                }

                UnityEngine.Debug.Log($"Fact Data: {response.data}");

                factCts.TrySetResult(JsonConvert.DeserializeObject<FactsInfoData>(response.data));
            });

            _requests.Add(nextRequest);
            CheckNextRequest();

            return await factCts.Task;
        }

        public void CancellFactInfoRequest()
        {
            CancellRequestOfType<FactInfoRequest>();

            CheckNextRequest();
        }
    }
}
