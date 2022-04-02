﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mv.Core.JsonObjects
{
    public class AppFileList
    {
        [JsonProperty("privateFiles")]
        public List<AppFileMetadata> PrivateFiles { get; private set; }

        [JsonProperty("publicFiles")]
        public List<AppFileMetadata> PublicFiles { get; private set; }
    }
}
