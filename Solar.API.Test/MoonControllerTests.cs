using System.Net;
using Solar.API.Test.Common;
using Xunit;

namespace Solar.API.Test
{
    public class MoonControllerTests : IntegrationTestsBase
    {
        [Theory]
        [InlineData("/moons/one")]
        [InlineData("/moons/two")]
        [InlineData("/moons/user")]
        public async Task Get_WhenUnauthenticated_ReturnsUnauthorized(string url)
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_OneMoon_AsUser()
        {
            // Arrange
            var client = GenerateClient();
            await AuthenticateAsUser(client);

            // Act
            var response = await client.GetAsync("/moons/one");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_TwoMoons_AsUser()
        {
            // Arrange
            var client = GenerateClient();
            await AuthenticateAsUser(client);

            // Act
            var response = await client.GetAsync("/moons/two");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
