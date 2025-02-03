using Newtonsoft.Json;

namespace TestApp.Data
{
    public readonly struct FactsArrayData
    {
        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly FactData[] Data;

        public FactsArrayData(FactData[] data)
        {
            Data = data;
        }
    }
}
