using Xunit;

namespace Tools.DynamicsCRM.Tests
{
    public class DynamicsCrmClientPropertiesTests
    {
        [Fact]
        public void Client_ProprieteVersion_EstCorrectementInitialisee()
        {
            // Arrange
            (DynamicsCrmClient client, _) = DynamicsCrmClientBuilder.Build();

            // Act & Assert
            Assert.Equal(DynamicsCrmClientBuilder.Version, client.Version);
        }

        [Fact]
        public void Client_ProprieteWebApiPath_EstCorrectementInitialisee()
        {
            // Arrange
            (DynamicsCrmClient client, _) = DynamicsCrmClientBuilder.Build();

            // Act & Assert
            Assert.Equal(DynamicsCrmClientBuilder.WebApiPath, client.WebApiPath);
        }

        [Fact]
        public void Client_ProprieteApiCommonPath_EstCorrectementConstruit()
        {
            // Arrange
            (DynamicsCrmClient client, _) = DynamicsCrmClientBuilder.Build();

            // Act & Assert
            Assert.Equal("api/data/v9.2", client.ApiCommonPath);
        }

        [Fact]
        public void Client_ProprieteBaseUrl_EstCorrectementInitialisee()
        {
            // Arrange
            (DynamicsCrmClient client, _) = DynamicsCrmClientBuilder.Build();

            // Act & Assert
            Assert.Equal(DynamicsCrmClientBuilder.BaseUrl, client.BaseUrl);
        }
    }
}