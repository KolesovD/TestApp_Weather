using Newtonsoft.Json;

namespace TestApp.Data
{
    public readonly struct WeatherServerData
    {
        [JsonProperty("properties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly WeatherServerProperties Properties;
    }

    public readonly struct WeatherServerProperties
    {
        [JsonProperty("periods", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly WeatherServerPeriod[] Periods;
    }

    public readonly struct WeatherServerPeriod
    {
        [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly int Number;

        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string Name;

        [JsonProperty("temperature", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly float Temperature;

        [JsonProperty("temperatureUnit", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string TemperatureUnit;

        [JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string Icon;
    }
}
