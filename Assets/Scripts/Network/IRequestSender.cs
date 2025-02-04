using Cysharp.Threading.Tasks;
using TestApp.Data;

namespace TestApp.Network
{
    public interface IRequestSender
    {
        public UniTask<WeatherServerData> GetWeatherData();
        public void CancellWeatherDataRequest();

        public UniTask<FactsArrayData> GetFactsData();
        public void CancellFactsDataRequest();

        public UniTask<FactsInfoData> GetFactInfoData(string factId);
        public void CancellFactInfoRequest();
    }
}
