using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Tools.DynamicsCRM.Exceptions;
using Xunit;

namespace Tools.DynamicsCRM.Tests
{
    public class DynamicsCrmClientRetrieveTests
    {
        private const string ApiPath = "api/data/v9.2";

        [Fact]
        public async Task RetrieveAsync_RetourneDictionnaire_QuandReponseEstValide()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond("application/json", "{\"contactid\":\"" + entityId + "\",\"firstname\":\"Jean\"}");

            // Act
            Dictionary<string, object> result = await client.RetrieveAsync("contacts", entityId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("firstname"));
        }

        [Fact]
        public async Task RetrieveAsync_LanceCrmHttpResponseException_QuandReponseEst404()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.NotFound, "application/json", /*lang=json,strict*/ "{\"error\":{\"message\":\"Entity not found\"}}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmHttpResponseException>(
                () => client.RetrieveAsync("contacts", entityId));
        }

        [Fact]
        public async Task RetrieveAsync_LanceCrmTooManyRequestException_QuandReponseEst429()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.TooManyRequests, "application/json", "{}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmTooManyRequestException>(
                () => client.RetrieveAsync("contacts", entityId));
        }

        [Fact]
        public async Task RetrieveAsync_AppliqueSelectQuery_QuandColumnsetEstFourni()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            string[] columnset = ["firstname", "lastname"];

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})?$select=firstname,lastname")
                .Respond("application/json", /*lang=json,strict*/ "{\"firstname\":\"Jean\",\"lastname\":\"Dupont\"}");

            // Act
            Dictionary<string, object> result = await client.RetrieveAsync("contacts", entityId, columnset);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("lastname"));
        }

        [Fact]
        public async Task RetrieveMultipleAsync_RetourneListeEntites_QuandReponseEstValide()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            string query = "contacts?$select=firstname&$top=2";

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/{query}")
                .Respond("application/json", /*lang=json,strict*/ "{\"value\":[{\"firstname\":\"Jean\"},{\"firstname\":\"Marie\"}]}");

            // Act
            IEnumerable<Dictionary<string, object>> result = await client.RetrieveMultipleAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, System.Linq.Enumerable.Count(result));
        }

        [Fact]
        public async Task RetrieveMultipleAsync_GereLaPagination_QuandNextLinkEstPresent()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            string query = "contacts?$select=firstname&$top=1";
            string nextPageUrl = $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts?$select=firstname&$skiptoken=page2";

            mockHttp
                .When($"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/{query}")
                .Respond("application/json",
                    "{\"@odata.nextLink\":\"" + nextPageUrl + "\"," +
                    "\"value\":[{\"firstname\":\"Jean\"}]}");

            mockHttp
                .When(nextPageUrl)
                .Respond("application/json", /*lang=json,strict*/ "{\"value\":[{\"firstname\":\"Marie\"}]}");

            // Act
            IEnumerable<Dictionary<string, object>> result = await client.RetrieveMultipleAsync(query);

            // Assert
            Assert.Equal(2, System.Linq.Enumerable.Count(result));
        }
    }
}