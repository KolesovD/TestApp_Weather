using Network.Commands;
using UnityEngine.Networking;

namespace TestApp.Network.Commands
{
    public class WeatherCommand : AbstractBaseCommand
    {
        protected override void SetupRequest()
        {
            _request = UnityWebRequest.Get("https://api.weather.gov/gridpoints/TOP/32,81/forecast");
        }
    }
}
