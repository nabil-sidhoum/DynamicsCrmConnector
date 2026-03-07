using System;
using System.Collections.Generic;
using System.Dynamic;
using Tools.DynamicsCRM.Extensions;
using Xunit;

namespace Tools.DynamicsCRM.Tests
{
    public class EntityTests
    {
        // Entité de test minimale pour les tests
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
        // Constructeur par défaut
        // ────────────────────────────────────────────

        [Fact]
        public void Constructeur_InitialiseAttributesEtFormattedValues_Vides()
        {
            // Act
            ContactEntity entity = new();

            // Assert
            Assert.NotNull(entity.Attributes);
            Assert.NotNull(entity.FormattedValues);
            Assert.Empty(entity.Attributes);
            Assert.Empty(entity.FormattedValues);
        }

        // ────────────────────────────────────────────
        // Constructeur ExpandoObject
        // ────────────────────────────────────────────

        [Fact]
        public void Constructeur_MappeStringSimple_DepuisExpandoObject()
        {
            // Arrange
            dynamic expando = new ExpandoObject();
            expando.firstname = "Jean";

            // Act
            ContactEntity entity = new((ExpandoObject)expando);

            // Assert
            Assert.Equal("Jean", entity.GetAttributeValue<string>("firstname"));
        }

        [Fact]
        public void Constructeur_MappeGuid_DepuisStringExpandoObject()
        {
            // Arrange
            Guid contactId = Guid.NewGuid();
            dynamic expando = new ExpandoObject();
            expando.contactid = contactId.ToString();

            // Act
            ContactEntity entity = new((ExpandoObject)expando);

            // Assert
            Assert.Equal(contactId, entity.GetAttributeValue<Guid>("contactid"));
        }

        [Fact]
        public void Constructeur_MappeFormattedValue_DepuisExpandoObject()
        {
            // Arrange
            dynamic expando = new ExpandoObject();
            expando.firstname = "Jean";
            IDictionary<string, object> dict = expando;
            dict["statecode@OData.Community.Display.V1.FormattedValue"] = "Actif";

            // Act
            ContactEntity entity = new((ExpandoObject)expando);

            // Assert
            Assert.True(entity.FormattedValues.ContainsKey("statecode"));
            Assert.Equal("Actif", entity.FormattedValues["statecode"]);
        }

        [Fact]
        public void Constructeur_IgnoreValeurNull_DepuisExpandoObject()
        {
            // Arrange
            dynamic expando = new ExpandoObject();
            expando.firstname = null;

            // Act
            ContactEntity entity = new((ExpandoObject)expando);

            // Assert
            Assert.Empty(entity.Attributes);
        }

        // ────────────────────────────────────────────
        // GetAttributeValue / SetAttributeValue
        // ────────────────────────────────────────────

        [Fact]
        public void GetAttributeValue_RetourneValeur_QuandAttributExiste()
        {
            // Arrange
            ContactEntity entity = new();
            entity.SetAttributeValue("firstname", "Jean");

            // Act
            string result = entity.GetAttributeValue<string>("firstname");

            // Assert
            Assert.Equal("Jean", result);
        }

        [Fact]
        public void GetAttributeValue_RetourneDefault_QuandAttributNExistePas()
        {
            // Arrange
            ContactEntity entity = new();

            // Act
            string result = entity.GetAttributeValue<string>("firstname");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SetAttributeValue_MetsAJourValeur_QuandAttributExisteDeja()
        {
            // Arrange
            ContactEntity entity = new();
            entity.SetAttributeValue("firstname", "Jean");

            // Act
            entity.SetAttributeValue("firstname", "Marie");

            // Assert
            Assert.Equal("Marie", entity.GetAttributeValue<string>("firstname"));
        }

        // ────────────────────────────────────────────
        // Id
        // ────────────────────────────────────────────

        [Fact]
        public void Id_RetourneGuid_QuandPrimaryIdAttributEstDefini()
        {
            // Arrange
            ContactEntity entity = new();
            Guid expectedId = Guid.NewGuid();
            entity.SetAttributeValue("contactid", expectedId);

            // Act & Assert
            Assert.Equal(expectedId, entity.Id);
        }

        [Fact]
        public void Id_SetAttributeValue_QuandAssigne()
        {
            // Arrange
            ContactEntity entity = new();
            Guid expectedId = Guid.NewGuid();

            // Act
            entity.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, entity.GetAttributeValue<Guid>("contactid"));
        }

        // ────────────────────────────────────────────
        // Propriétés statiques
        // ────────────────────────────────────────────

        [Fact]
        public void EntitySetName_RetourneValeurStatique()
        {
            // Act & Assert
            Assert.Equal("contacts", ContactEntity.EntitySetName);
        }

        [Fact]
        public void PrimaryIdAttribute_RetourneValeurStatique()
        {
            // Act & Assert
            Assert.Equal("contactid", ContactEntity.PrimaryIdAttribute);
        }

        // ────────────────────────────────────────────
        // ToExpandoObject
        // ────────────────────────────────────────────

        [Fact]
        public void ToExpandoObject_RetourneExpandoAvecAttributs()
        {
            // Arrange
            ContactEntity entity = new();
            entity.SetAttributeValue("firstname", "Jean");

            // Act
            IDictionary<string, object> result = entity.ToExpandoObject();

            // Assert
            Assert.True(result.ContainsKey("firstname"));
            Assert.Equal("Jean", result["firstname"]);
        }

        [Fact]
        public void ToExpandoObject_ConvertitCleEnMinuscules()
        {
            // Arrange
            ContactEntity entity = new();
            entity.SetAttributeValue("FirstName", "Jean");

            // Act
            IDictionary<string, object> result = entity.ToExpandoObject();

            // Assert
            Assert.True(result.ContainsKey("firstname"));
        }

        [Fact]
        public void ToExpandoObject_ConvertitEntityReference_EnCheminOData()
        {
            // Arrange
            ContactEntity entity = new();
            Guid accountId = Guid.NewGuid();
            entity.SetAttributeValue("parentaccountid", new EntityReference("accounts", accountId));

            // Act
            IDictionary<string, object> result = entity.ToExpandoObject();

            // Assert
            Assert.Equal($"/accounts({accountId})", result["parentaccountid"]);
        }

        // ────────────────────────────────────────────
        // ToEntityReference
        // ────────────────────────────────────────────

        [Fact]
        public void ToEntityReference_RetourneEntityReference_AvecIdEtEntitySetName()
        {
            // Arrange
            ContactEntity entity = new();
            Guid contactId = Guid.NewGuid();
            entity.Id = contactId;

            // Act
            EntityReference reference = entity.ToEntityReference();

            // Assert
            Assert.Equal(contactId, reference.EntityId);
            Assert.Equal("contacts", reference.EntitySetName);
        }
    }
}