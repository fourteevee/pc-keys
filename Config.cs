using System.Collections.Generic;
using Newtonsoft.Json;

namespace pc_keys
{
    public struct BotConfig
    {
        [JsonProperty("appName")] public string Application;
        [JsonProperty("key")] public string ApiKey;
        [JsonProperty("prefix")] public string CommandPrefix;
        [JsonProperty("storageFile")] public string FileName;
        [JsonProperty("roleThreshold")] public Dictionary<int, ulong> Roles;
    }
}