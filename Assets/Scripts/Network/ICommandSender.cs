using Cysharp.Threading.Tasks;
using System.Threading;
using TestApp.Data;

namespace TestApp.Network
{
    public interface ICommandSender
    {
        public UniTask<WeatherServerData> GetWeatherData(CancellationToken cancellationToken);
        public void CancellWeatherDataRequest();

        public UniTask<FactsArrayData> GetFactsData(CancellationToken cancellationToken);
        public UniTask<FactData> GetOneFactData(string factId, CancellationToken cancellationToken);
    }
}
