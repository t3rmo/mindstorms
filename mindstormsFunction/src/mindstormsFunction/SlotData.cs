using Newtonsoft.Json;

namespace mindstormsFunction.obj
{
    public class SlotData
    {
        [JsonProperty("direction")]
        public string Direction { get; set; } = "gerade aus";

        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;

        [JsonProperty("duration")]
        public int Duration { get; set; } = 1;

    }
}