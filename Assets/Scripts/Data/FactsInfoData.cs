using Newtonsoft.Json;

namespace TestApp.Data
{
    public readonly struct FactsInfoData
    {
        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly FactData Data;
    }
}
