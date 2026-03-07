using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Tools.DynamicsCRM.Exceptions;
using Xunit;

namespace Tools.DynamicsCRM.Tests
{
    public class DynamicsCrmClientRelationshipTests
    {
        private const string ApiPath = "api/data/v9.2";

        [Fact]
        public async Task AssociateAsync_RetourneTrue_QuandAssociationEstCreee()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            Guid relatedId = Guid.NewGuid();

            mockHttp
                .When(System.Net.Http.HttpMethod.Post,
                    $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})/account_contacts/$ref")
                .Respond(HttpStatusCode.NoContent);

            // Act
            bool result = await client.AssociateAsync("contacts", entityId, "account_contacts", "accounts", relatedId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AssociateAsync_LanceCrmTooManyRequestException_QuandReponseEst429()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            Guid relatedId = Guid.NewGuid();

            mockHttp
                .When(System.Net.Http.HttpMethod.Post,
                    $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})/account_contacts/$ref")
                .Respond(HttpStatusCode.TooManyRequests, "application/json", "{}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmTooManyRequestException>(
                () => client.AssociateAsync("contacts", entityId, "account_contacts", "accounts", relatedId));
        }

        [Fact]
        public async Task DisassociateAsync_RetourneTrue_QuandAssociationEstSupprimee()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            Guid relatedId = Guid.NewGuid();

            mockHttp
                .When(System.Net.Http.HttpMethod.Delete,
                    $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})/account_contacts({relatedId})/$ref")
                .Respond(HttpStatusCode.NoContent);

            // Act
            bool result = await client.DisassociateAsync("contacts", entityId, "account_contacts", relatedId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetOptionSetValueAsync_RetourneDictionnaire_QuandReponseEstValide()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            string entityName = "contacts";
            string optionSetName = "civilite";

            string json = /*lang=json,strict*/ """
                {
                  "LogicalName": "civilite",
                  "OptionSet": {
                    "Options": [
                      { "Value": 1, "Label": { "LocalizedLabels": [{ "Label": "M." }] } },
                      { "Value": 2, "Label": { "LocalizedLabels": [{ "Label": "Mme" }] } }
                    ]
                  }
                }
                """;

            mockHttp
                .When($"*EntityDefinitions*{entityName}*{optionSetName}*")
                .Respond("application/json", json);

            // Act
            Dictionary<int, string> result = await client.GetOptionSetValueAsync(entityName, optionSetName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("M.", result[1]);
            Assert.Equal("Mme", result[2]);
        }

        [Fact]
        public async Task GetOptionSetValueAsync_LanceCrmHttpResponseException_QuandReponseEst404()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();

            mockHttp
                .When($"*EntityDefinitions*")
                .Respond(HttpStatusCode.NotFound, "application/json", /*lang=json,strict*/ "{\"error\":{\"message\":\"Not found\"}}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmHttpResponseException>(
                () => client.GetOptionSetValueAsync("contacts", "civilite"));
        }
    }
}