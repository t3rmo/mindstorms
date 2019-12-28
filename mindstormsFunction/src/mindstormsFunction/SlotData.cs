using Newtonsoft.Json;

namespace mindstormsFunction.obj
{
    public class SpeedData
    {
        [JsonProperty(PropertyName = "islist")]
        public bool isList { get; } = false;

        [JsonProperty(PropertyName = "speed")]
        public int Speed { get; set; } = 100;

        [JsonProperty(PropertyName = "type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; } = "";

        [JsonProperty(PropertyName = "count")]
        public int DirectiveCount { get; set; } = 1;

    }

    public class DirectionData
    {

        [JsonProperty(PropertyName = "islist")]
        public bool isList { get; } = false;

        [JsonProperty("direction")]
        public string Direction { get; set; } = "forward";

        [JsonProperty("speed")]
        public int Speed { get; set; } = 100;

        [JsonProperty("type")]
        public string CommandType { get; set; } = "move";

        [JsonProperty("duration")]
        public int Duration { get; set; } = 1;

        [JsonProperty(PropertyName = "count")]
        public int DirectiveCount { get; set; } = 1;

    }
}