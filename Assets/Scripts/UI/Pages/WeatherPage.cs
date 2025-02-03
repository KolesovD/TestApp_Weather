using System;
using TestApp.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using TestApp.Data;
using TestApp.Managers;

namespace TestApp.UI.Pages
{
    public class WeatherPage : AbstractMenuPage
    {
        [SerializeField] private GameObject _weatherLoaderContainer;
        [Space]
        [SerializeField] private GameObject _weatherDataContainer;
        [SerializeField] private Image _weatherImage;
        [SerializeField] private TMP_Text _weatherText;

        private const double TIMER_INTERVALS = 5d;

        private ICommandSender _commandSender;

        private CancellationTokenSource _cts;
        private IDisposable _getDataSub;

        [Inject]
        private void Inject(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        protected override void OnShow()
        {
            SetLoader();

            StartGetWeatherTimer();
        }

        protected override void OnHide()
        {
            _getDataSub?.Dispose();
            _getDataSub = null;

            DisposeCancellationToken();

            _commandSender.CancellWeatherDataRequest();
        }

        private void SetLoader()
        {
            _weatherLoaderContainer.SetActive(true);
            _weatherDataContainer.SetActive(false);
        }

        private void StartGetWeatherTimer()
        {
            _getDataSub?.Dispose();
            _getDataSub = Observable.Interval(TimeSpan.FromSeconds(TIMER_INTERVALS))
                .StartWith(0L)
                .Subscribe(_ => SendWeatherRequestAndShowResults().Forget())
                .AddTo(this);
        }

        private async UniTask SendWeatherRequestAndShowResults()
        {
            Debug.Log("Try send weather request");

            try
            {
                if (_cts == null)
                    CreateNewCancellationToken();

                var weatherData = await _commandSender.GetWeatherData(_cts.Token);

                //ShowResults(weatherData);
            }
            catch (OperationCanceledException ex)
            {
                //
            }
        }

        private void ShowResults(WeatherData weatherData)
        {
            _weatherLoaderContainer.SetActive(false);
            _weatherDataContainer.SetActive(true);

            _weatherImage.LoadImageFromURL(weatherData.WeatherIconPath).Forget();
            _weatherText.text = $"Сегодня - {weatherData.Temperature}{weatherData.WeatherUnit}";
        }

        private void CreateNewCancellationToken()
        {
            _cts = new CancellationTokenSource();
        }

        private void DisposeCancellationToken()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
