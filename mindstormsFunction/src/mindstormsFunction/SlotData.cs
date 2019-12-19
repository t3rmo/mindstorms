using Newtonsoft.Json;

namespace mindstormsFunction.obj
{
    public class SlotData
    {
        [JsonProperty("direction")]
        public string Direction { get; set; } = "";

        [JsonProperty("speed")]
        public int Speed { get; set; } = 0;

        [JsonProperty("duration")]
        public int Duration { get; set; } = 0;

    }
}