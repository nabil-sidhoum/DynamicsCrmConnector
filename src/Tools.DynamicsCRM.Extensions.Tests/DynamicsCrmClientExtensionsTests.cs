using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Tools.DynamicsCRM.Extensions.Tests
{
    public class DynamicsCrmClientExtensionsTests
    {
        private sealed class ContactEntity : Entity
        {
            public static new readonly string EntitySetName = "contacts";
            public static new readonly string EntityLogicalName = "contact";
            public static new readonly string PrimaryIdAttribute = "contactid";

            public ContactEntity() : base() { }

            public ContactEntity(ExpandoObject expandoObject) : base(expandoObject) { }

            public string FirstName
            {
                get => GetAttributeValue<string>("firstname");
                set => SetAttributeValue("firstname", value);
            }
        }

        // ────────────────────────────────────────────
        // Retrieve<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task Retrieve_RetourneEntiteTypee_QuandRetrieveAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();

            mockClient
                .Setup(c => c.RetrieveAsync("contacts", contactId, null))
                .ReturnsAsync(new Dictionary<string, object>
                {
                    { "contactid", contactId.ToString() },
                    { "firstname", "Jean" }
                });

            // Act
            ContactEntity result = await mockClient.Object.Retrieve<ContactEntity>(contactId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jean", result.FirstName);
        }

        [Fact]
        public async Task Retrieve_AppelleRetrieveAsync_AvecBonEntitySetName()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();

            mockClient
                .Setup(c => c.RetrieveAsync("contacts", contactId, null))
                .ReturnsAsync([]);

            // Act
            await mockClient.Object.Retrieve<ContactEntity>(contactId);

            // Assert
            mockClient.Verify(c => c.RetrieveAsync("contacts", contactId, null), Times.Once);
        }

        // ────────────────────────────────────────────
        // RetrieveMultiple<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task RetrieveMultiple_RetourneListeEntitesTypees_QuandRetrieveMultipleAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId1 = Guid.NewGuid();
            Guid contactId2 = Guid.NewGuid();

            mockClient
                .Setup(c => c.RetrieveMultipleAsync("contacts?$select=firstname"))
                .ReturnsAsync(
                [
                    new() { { "contactid", contactId1.ToString() }, { "firstname", "Jean" } },
                    new() { { "contactid", contactId2.ToString() }, { "firstname", "Marie" } }
                ]);

            // Act
            IEnumerable<ContactEntity> result = await mockClient.Object.RetrieveMultiple<ContactEntity>("$select=firstname");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task RetrieveMultiple_RetourneListeVide_QuandAucunResultat()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();

            mockClient
                .Setup(c => c.RetrieveMultipleAsync("contacts?$select=firstname"))
                .ReturnsAsync([]);

            // Act
            IEnumerable<ContactEntity> result = await mockClient.Object.RetrieveMultiple<ContactEntity>("$select=firstname");

            // Assert
            Assert.Empty(result);
        }

        // ────────────────────────────────────────────
        // Create<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task Create_RetourneGuid_QuandCreateAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid newId = Guid.NewGuid();
            ContactEntity entity = new()
            {
                FirstName = "Jean"
            };

            mockClient
                .Setup(c => c.CreateAsync("contacts", It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(newId);

            // Act
            Guid result = await mockClient.Object.Create(entity);

            // Assert
            Assert.Equal(newId, result);
        }

        [Fact]
        public async Task Create_ExclutPrimaryIdAttribute_DansLesdonneesEnvoyees()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();
            ContactEntity entity = new()
            {
                Id = contactId,
                FirstName = "Jean"
            };

            mockClient
                .Setup(c => c.CreateAsync("contacts", It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            await mockClient.Object.Create(entity);

            // Assert — contactid ne doit pas être dans les données envoyées
            mockClient.Verify(c => c.CreateAsync("contacts",
                It.Is<Dictionary<string, object>>(d => !d.ContainsKey("contactid"))),
                Times.Once);
        }

        // ────────────────────────────────────────────
        // Update<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task Update_RetourneTrue_QuandUpdateAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();
            ContactEntity entity = new()
            {
                Id = contactId,
                FirstName = "Jean"
            };

            mockClient
                .Setup(c => c.UpdateAsync("contacts", contactId, It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(true);

            // Act
            bool result = await mockClient.Object.Update(entity);

            // Assert
            Assert.True(result);
        }

        // ────────────────────────────────────────────
        // Delete<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task Delete_RetourneTrue_QuandDeleteAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();

            mockClient
                .Setup(c => c.DeleteAsync("contacts", contactId))
                .ReturnsAsync(true);

            // Act
            bool result = await mockClient.Object.Delete<ContactEntity>(contactId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Delete_AppelleDeleteAsync_AvecBonEntitySetName()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid contactId = Guid.NewGuid();

            mockClient
                .Setup(c => c.DeleteAsync("contacts", contactId))
                .ReturnsAsync(true);

            // Act
            await mockClient.Object.Delete<ContactEntity>(contactId);

            // Assert
            mockClient.Verify(c => c.DeleteAsync("contacts", contactId), Times.Once);
        }

        // ────────────────────────────────────────────
        // Associate<T> / Disassociate<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task Associate_RetourneTrue_QuandAssociateAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid entityId = Guid.NewGuid();
            Guid relatedId = Guid.NewGuid();

            mockClient
                .Setup(c => c.AssociateAsync("contacts", entityId, "account_contacts", "accounts", relatedId))
                .ReturnsAsync(true);

            // Act
            bool result = await mockClient.Object.Associate<ContactEntity>(entityId, "account_contacts", "accounts", relatedId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Disassociate_RetourneTrue_QuandDisassociateAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            Guid entityId = Guid.NewGuid();
            Guid relatedId = Guid.NewGuid();

            mockClient
                .Setup(c => c.DisassociateAsync("contacts", entityId, "account_contacts", relatedId))
                .ReturnsAsync(true);

            // Act
            bool result = await mockClient.Object.Disassociate<ContactEntity>(entityId, "account_contacts", relatedId);

            // Assert
            Assert.True(result);
        }

        // ────────────────────────────────────────────
        // SendFetchXml<T>
        // ────────────────────────────────────────────

        [Fact]
        public async Task SendFetchXml_RetourneListeEntitesTypees_QuandSendFetchXmlAsyncReussit()
        {
            // Arrange
            Mock<IDynamicsCrmClient> mockClient = new();
            string fetchXml = "<fetch><entity name='contact'><attribute name='firstname'/></entity></fetch>";
            Guid contactId = Guid.NewGuid();

            mockClient
                .Setup(c => c.SendFetchXmlAsync("contacts", fetchXml, 5000))
                .ReturnsAsync(
                [
                    new() { { "contactid", contactId.ToString() }, { "firstname", "Jean" } }
                ]);

            // Act
            IEnumerable<ContactEntity> result = await mockClient.Object.SendFetchXml<ContactEntity>(fetchXml);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}