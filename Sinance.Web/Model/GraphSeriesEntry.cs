using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class GraphSeriesEntry<T>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("data")]
        public IEnumerable<T> Data { get; set; } 

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
