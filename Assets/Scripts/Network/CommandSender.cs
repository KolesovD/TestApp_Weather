using Cysharp.Threading.Tasks;
using System.Threading;
using TestApp.Data;
using TestApp.Utils;
using System.Collections.Generic;
using Network.Commands;

namespace TestApp.Network
{
    public class CommandSender : ICommandSender
    {

        private List<AbstractBaseCommand> _commands = new List<AbstractBaseCommand>();
        private AbstractBaseCommand _currentCommand = null;

        private void CheckNextCommand()
        {
            if (_commands.Count == 0)
                return;

            if (_currentCommand != null)
                return;

            SendNextCommand().Forget();
        }

        private async UniTask SendNextCommand()
        {
            var nextCommand = _commands[0];

            _currentCommand = nextCommand;
            _commands.RemoveAt(0);

            var result = await nextCommand.SendRequest();
        }

        public async UniTask<WeatherServerData> GetWeatherData(CancellationToken cancellationToken)
        {
            return new WeatherServerData();

            //cancellationToken.ThrowIfCancellationRequested();

            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken, cancelImmediately: true);

            //return new WeatherData("", UnityEngine.Random.Range(22, 57), "F");
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
