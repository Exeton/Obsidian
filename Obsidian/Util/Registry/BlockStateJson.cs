﻿using Newtonsoft.Json;

namespace Obsidian.Util.Registry
{
    public class BlockStateJson
    {
        [JsonProperty("id")]
        public short Id { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }

        [JsonProperty("properties")]
        public BlockPropertiesJson Properties { get; set; }
    }

}
