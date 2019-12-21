using Newtonsoft.Json;

namespace mindstormsFunction.obj
{
    public class SpeedData
    {
        [JsonProperty("type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty("command")]
        public string Command { get; set; } = "";

        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;

    }

    public class DirectionData
    {

        [JsonProperty("type")]
        public string CommandType { get; set; } = "move";

        [JsonProperty("direction")]
        public string Direction { get; set; } = "forward";

        [JsonProperty("duration")]
        public int Duration { get; set; } = 1;

        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;
    }
}