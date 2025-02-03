using Cysharp.Threading.Tasks;
using System.Threading;
using TestApp.Data;
using TestApp.Utils;
using System.Collections.Generic;

namespace TestApp.Network
{
    public class FakeCommandSender : ICommandSender
    {
        private float WEATHER_RESULT_DELAY = 3f;

        private static readonly string[] _weatherDayTimes = new string[] { "night", "day" };
        private static readonly string[] _weatherTypes = new string[] { "few", "sct", "bkn" };

        public async UniTask<WeatherServerData> GetWeatherData(CancellationToken cancellationToken)
        {
            return new WeatherServerData();

            //cancellationToken.ThrowIfCancellationRequested();

            //await UniTask.WaitForSeconds(WEATHER_RESULT_DELAY, cancellationToken: cancellationToken, cancelImmediately: true);

            //return new WeatherData(GetRandomWeatherIconPath(), UnityEngine.Random.Range(22, 57), "F");
        }

        private string GetRandomWeatherIconPath()
        {
            string dayTime = _weatherDayTimes.RandomElement();
            string type = _weatherTypes.RandomElement();

            return $"https://api.weather.gov/icons/land/{dayTime}/{type}?size=medium";
        }

        public void CancellWeatherDataRequest()
        {
            //throw new System.NotImplementedException();
        }

        public async UniTask<FactsArrayData> GetFactsData(CancellationToken cancellationToken)
        {
            return new FactsArrayData(new FactData[]
            {
                new FactData("1", "one", new Dictionary<string, object>() { { "name", "First fact" }, { "description", "Description 1" } }),
                new FactData("2", "two", new Dictionary<string, object>() { { "name", "Second fact" }, { "description", "Description 2" } }),
                new FactData("3", "three", new Dictionary<string, object>() { { "name", "Third fact" }, { "description", "Description 3" } }),
            });
        }

        public async UniTask<FactData> GetOneFactData(string factId, CancellationToken cancellationToken)
        {
            return new FactData(factId, factId, new Dictionary<string, object>() { { "name", $"Fact {factId}" }, { "description", $"Description {factId}" } });
        }
    }
}
