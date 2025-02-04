using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestApp.Data
{
    public readonly struct FactData
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string Id;

        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly string Type;

        [JsonProperty("attributes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly Dictionary<string, object> Attributes;

        private const string NAME_FIELD = "name";
        public string Name
        {
            get
            {
                if (Attributes != null && Attributes.TryGetValue(NAME_FIELD, out object name))
                    return name.ToString();

                return string.Empty;
            }
        }

        private const string DESC_FIELD = "description";
        public string Description
        {
            get
            {
                if (Attributes != null && Attributes.TryGetValue(DESC_FIELD, out object desc))
                    return desc.ToString();

                return string.Empty;
            }
        }
    }
}
