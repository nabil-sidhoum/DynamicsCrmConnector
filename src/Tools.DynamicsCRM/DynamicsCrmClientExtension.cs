using System.Net.Http;
using System.Net.Http.Headers;

namespace Tools.DynamicsCRM
{
    internal static class DynamicsCrmClientExtension
    {
        internal static HttpRequestMessage ConfigHeaders(this HttpRequestMessage request, string token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Prefer", "odata.include-annotations=*");

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }
    }
}
