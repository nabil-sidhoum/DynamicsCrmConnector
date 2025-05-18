using System;
using System.Text.Json.Serialization;

namespace Tools.DynamicsCRM
{
    public class DynamicsCrmAuthenticationToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonIgnore]
        public DateTime ExpiresOn { get; set; }
    }
}