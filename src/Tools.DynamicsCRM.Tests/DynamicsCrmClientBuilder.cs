using System;
using System.Net.Http;
using RichardSzalay.MockHttp;
using Tools.DynamicsCRM.Configuration;

namespace Tools.DynamicsCRM.Tests
{
    internal static class DynamicsCrmClientBuilder
    {
        internal const string BaseUrl = "https://test-org.crm.dynamics.com/";
        internal const string WebApiPath = "api/data";
        internal const string Version = "v9.2";
        internal const string FakeToken = "fake-bearer-token";

        internal static (DynamicsCrmClient Client, MockHttpMessageHandler MockHttp) Build()
        {
            MockHttpMessageHandler mockHttp = new();

            HttpClient httpClient = new(mockHttp)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            DynamicsCrmConfig config = new()
            {
                BaseUrl = BaseUrl,
                WebApiPath = WebApiPath,
                Version = Version,
                Authentication = new DynamicsCrmAuthenticationConfig
                {
                    TenantId = "fake-tenant",
                    ClientId = "fake-client",
                    SecretId = "fake-secret"
                }
            };

            DynamicsCrmAuthentication authentication = BuildAuthentication(config);
            DynamicsCrmClient client = new(httpClient, authentication, config);

            return (client, mockHttp);
        }

        private static DynamicsCrmAuthentication BuildAuthentication(DynamicsCrmConfig config)
        {
            MockHttpMessageHandler mockAuth = new();
            mockAuth
                .When("https://login.windows.net/fake-tenant/oauth2/token")
                .Respond("application/json", $"{{\"access_token\":\"{FakeToken}\"}}");

            HttpClient authClient = new(mockAuth);
            return new DynamicsCrmAuthentication(authClient, config);
        }
    }
}