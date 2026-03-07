using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tools.DynamicsCRM.Exceptions
{
    public class CrmAuthenticationResponseExceptionModel
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }

        [JsonPropertyName("error_codes")]
        public List<int?> ErrorCodes { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("trace_id")]
        public string TraceId { get; set; }

        [JsonPropertyName("correlation_id")]
        public string CorrelationId { get; set; }

        [JsonPropertyName("error_uri")]
        public string ErrorUri { get; set; }
    }

    public class CrmAuthenticationResponseException : Exception
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CrmAuthenticationResponseException(HttpContent content) : base(ExtractMessageFromContent(content))
        { }

        private static string ExtractMessageFromContent(HttpContent content)
        {
            string jsonContent = content.ReadAsStringAsync().Result;
            CrmAuthenticationResponseExceptionModel error = JsonSerializer.Deserialize<CrmAuthenticationResponseExceptionModel>(jsonContent, _options);
            return error.ErrorDescription;
        }
    }
}