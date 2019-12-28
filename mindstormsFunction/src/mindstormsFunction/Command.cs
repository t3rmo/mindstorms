using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace mindstormsFunction
{
    public class Command
    {
        [JsonProperty(PropertyName = "type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty(PropertyName = "command")]
        public string CmdName { get; set; } = "";

        [JsonProperty(PropertyName = "count")]
        public int DirectiveCount { get; set; } = 1;
    }

    public class CommandPalette
    {
        [JsonProperty(PropertyName = "type")]
        public string CommandType { get; set; } = "command";

        [JsonProperty(PropertyName = "command")]
        public string CmdName { get; set; } = "";

        [JsonProperty(PropertyName = "dirs")]
        public List<dynamic> Directives { get; set; } = new List<dynamic>();

        [JsonProperty(PropertyName = "count")]
        public int DirectiveCount { get; set; } = 0;
    }
}