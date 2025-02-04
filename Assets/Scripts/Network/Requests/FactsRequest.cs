using UnityEngine.Networking;

namespace TestApp.Network.Requests
{
    public class FactsRequest : AbstractBaseRequest
    {
        protected override void SetupRequest()
        {
            _request = UnityWebRequest.Get("https://dogapi.dog/api/v2/breeds");
        }
    }
}
