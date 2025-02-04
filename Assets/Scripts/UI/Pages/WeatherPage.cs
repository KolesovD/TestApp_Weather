using Cysharp.Threading.Tasks;
using System;
using TestApp.Data;
using TestApp.Managers;
using TestApp.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

        private IRequestSender _commandSender;

        private IDisposable _getDataSub;

        [Inject]
        private void Inject(IRequestSender commandSender)
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
                var weatherData = await _commandSender.GetWeatherData();

                ShowResults(weatherData);
            }
            catch (OperationCanceledException ex)
            {
                //
            }
        }

        private void ShowResults(WeatherServerData weatherServerData)
        {
            if (weatherServerData.Properties.Periods == null || weatherServerData.Properties.Periods.Length == 0)
                return;

            _weatherLoaderContainer.SetActive(false);
            _weatherDataContainer.SetActive(true);

            var recentData = weatherServerData.Properties.Periods[0];

            _weatherImage.LoadImageFromURL(recentData.Icon).Forget();
            _weatherText.text = $"Сегодня - {recentData.Temperature}{recentData.TemperatureUnit}";
        }
    }
}
