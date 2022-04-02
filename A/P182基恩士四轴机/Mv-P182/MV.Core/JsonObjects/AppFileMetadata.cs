using System;
using Newtonsoft.Json;

namespace Mv.Core.JsonObjects
{
    public class AppFileMetadata
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("version")]
        public Version Version { get; private set; }
    }
}
