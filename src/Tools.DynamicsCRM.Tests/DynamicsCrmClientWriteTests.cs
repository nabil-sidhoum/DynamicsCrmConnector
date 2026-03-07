using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Tools.DynamicsCRM.Exceptions;
using Xunit;

namespace Tools.DynamicsCRM.Tests
{
    public class DynamicsCrmClientWriteTests
    {
        private const string ApiPath = "api/data/v9.2";

        [Fact]
        public async Task CreateAsync_RetourneGuid_QuandEntiteEstCreee()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid newId = Guid.NewGuid();
            Dictionary<string, object> entityData = new() { { "firstname", "Jean" } };

            MockedRequest request = mockHttp
                .When(System.Net.Http.HttpMethod.Post, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts")
                .Respond(response =>
                {
                    System.Net.Http.HttpResponseMessage msg = new(HttpStatusCode.NoContent);
                    msg.Headers.Add("OData-EntityId", $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({newId})");
                    return msg;
                });

            // Act
            Guid result = await client.CreateAsync("contacts", entityData);

            // Assert
            Assert.Equal(newId, result);
        }

        [Fact]
        public async Task CreateAsync_LanceCrmTooManyRequestException_QuandReponseEst429()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Dictionary<string, object> entityData = new() { { "firstname", "Jean" } };

            mockHttp
                .When(System.Net.Http.HttpMethod.Post, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts")
                .Respond(HttpStatusCode.TooManyRequests, "application/json", "{}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmTooManyRequestException>(
                () => client.CreateAsync("contacts", entityData));
        }

        [Fact]
        public async Task UpdateAsync_RetourneTrue_QuandEntiteEstMiseAJour()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            Dictionary<string, object> entityData = new() { { "firstname", "Jean" } };

            mockHttp
                .When(System.Net.Http.HttpMethod.Patch, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.NoContent);

            // Act
            bool result = await client.UpdateAsync("contacts", entityId, entityData);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_LanceCrmTooManyRequestException_QuandReponseEst429()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();
            Dictionary<string, object> entityData = new() { { "firstname", "Jean" } };

            mockHttp
                .When(System.Net.Http.HttpMethod.Patch, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.TooManyRequests, "application/json", "{}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmTooManyRequestException>(
                () => client.UpdateAsync("contacts", entityId, entityData));
        }

        [Fact]
        public async Task DeleteAsync_RetourneTrue_QuandEntiteEstSupprimee()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();

            mockHttp
                .When(System.Net.Http.HttpMethod.Delete, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.NoContent);

            // Act
            bool result = await client.DeleteAsync("contacts", entityId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_LanceCrmTooManyRequestException_QuandReponseEst429()
        {
            // Arrange
            (DynamicsCrmClient client, MockHttpMessageHandler mockHttp) = DynamicsCrmClientBuilder.Build();
            Guid entityId = Guid.NewGuid();

            mockHttp
                .When(System.Net.Http.HttpMethod.Delete, $"{DynamicsCrmClientBuilder.BaseUrl}{ApiPath}/contacts({entityId})")
                .Respond(HttpStatusCode.TooManyRequests, "application/json", "{}");

            // Act & Assert
            await Assert.ThrowsAsync<CrmTooManyRequestException>(
                () => client.DeleteAsync("contacts", entityId));
        }
    }
}