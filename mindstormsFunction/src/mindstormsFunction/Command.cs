using Newtonsoft.Json;

namespace mindstormsFunction
{
    public class Command
    {
        [JsonProperty("type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty(PropertyName = "command")]
        public string CmdName { get; set; } = "";
    }
}