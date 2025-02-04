using UnityEngine.Networking;

namespace TestApp.Network.Requests
{
    public class WeatherRequest : AbstractBaseRequest
    {
        protected override void SetupRequest()
        {
            _request = UnityWebRequest.Get("https://api.weather.gov/gridpoints/TOP/32,81/forecast");
        }
    }
}
