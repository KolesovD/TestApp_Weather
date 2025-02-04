using UnityEngine.Networking;

namespace TestApp.Network.Requests
{
    public class FactInfoRequest : AbstractBaseRequest
    {
        protected override int Timeout => 5;

        private string _factId;

        public FactInfoRequest(string factId)
        {
            _factId = factId;

            SetupRequest();
        }

        protected override void SetupRequest()
        {
            _request = UnityWebRequest.Get($"https://dogapi.dog/api/v2/breeds/{_factId}");
        }
    }
}
