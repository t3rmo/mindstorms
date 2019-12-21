using Newtonsoft.Json;

namespace mindstormsFunction.obj
{
    public class SpeedData
    {
        
        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;

        [JsonProperty("type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty("command")]
        public string Command { get; set; } = "";

    }

    public class DirectionData
    {

        [JsonProperty("direction")]
        public string Direction { get; set; } = "forward";

        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;

        [JsonProperty("type")]
        public string CommandType { get; set; } = "move";

        [JsonProperty("duration")]
        public int Duration { get; set; } = 1;

    }
}