using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Tools.DynamicsCRM.Configuration;
using Tools.DynamicsCRM.Exceptions;

namespace Tools.DynamicsCRM
{
    public class DynamicsCrmAuthentication
    {
        private const string AuthorityUrl = "https://login.windows.net/{tenantid}/oauth2/token";
        private static readonly object _tokenlock = new();

        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _client;
        private readonly DynamicsCrmConfig _config;
        private static DynamicsCrmAuthenticationToken _token;

        public string Token
        {
            get
            {
                lock (_tokenlock)
                {
                    if (_token is null || _token.ExpiresOn < DateTime.UtcNow.AddMinutes(15))
                    {
                        GetNewToken();
                    }
                }
                return _token.AccessToken;
            }
        }

        public DynamicsCrmAuthentication(HttpClient client, DynamicsCrmConfig config)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(config);

            _client = client;
            _config = config;
        }

        private void GetNewToken()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            List<KeyValuePair<string, string>> requestData =
            [
                new("grant_type", "client_credentials"),
                new("client_id", _config.Authentication.ClientId),
                new("client_secret", _config.Authentication.SecretId),
                new("resource", _config.BaseUrl)
            ];

            FormUrlEncodedContent requestBody = new(requestData);
            var response = _client.PostAsync(AuthorityUrl.Replace("{tenantid}", _config.Authentication.TenantId), requestBody).Result;

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    DynamicsCrmAuthenticationToken token = JsonSerializer.Deserialize<DynamicsCrmAuthenticationToken>(jsonContent, _options);
                    token.ExpiresOn = DateTime.UtcNow.AddHours(1);
                    _token = token;
                }
                catch (Exception e)
                {
                    throw new CrmParseResponseException(e.Message);
                }
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new CrmAuthenticationResponseException(response.Content);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new CrmTooManyRequestException();
                }
                else
                {
                    throw new CrmHttpResponseException(response.Content);
                }
            }
        }

    }
}