namespace TestApp.Data
{
    public readonly struct WeatherData
    {
        public readonly string WeatherIconPath;

        public readonly float Temperature;

        public readonly string WeatherUnit;

        public WeatherData(string weatherIconPath, float temperature, string weatherUnit)
        {
            WeatherIconPath = weatherIconPath;
            Temperature = temperature;
            WeatherUnit = weatherUnit;
        }
    }
}
