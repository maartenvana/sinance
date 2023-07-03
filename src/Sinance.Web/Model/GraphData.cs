using Newtonsoft.Json;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Graph data to provide for Highcharts graphs
    /// </summary>
    [JsonObject]
    public class GraphData
    {
        /// <summary>
        /// Y-axis point data
        /// </summary>
        [JsonProperty("data")]
        public decimal[] Data { get; set; }

        /// <summary>
        /// Name of the data
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}