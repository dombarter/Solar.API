using System.Net;
using Solar.API.Test.Common;
using Solar.DTOs.Inbound;
using Xunit;

namespace Solar.API.Test
{
    public class AccountControllerTests : IntegrationTestsBase
    {
        [Fact]
        public async Task Post_Register_ReturnsOK()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var response = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_Register_DuplicateUser_ReturnsBadRequest()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var response1 = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            var response2 = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [Fact]
        public async Task Post_Register_BadPassword_ReturnsBadRequest()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var response = await Register(client, new RegisterDto
            {
                Email = "user@email.com",
                Password = "password"
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_Login_ReturnsOK()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var register = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            var login = await Login(client, new LoginDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        [Fact]
        public async Task Post_Login_IncorrectEmail_ReturnsBadRequest()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var register = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            var login = await Login(client, new LoginDto
            {
                Email = "user@email.com",
                Password = "PA55word#"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, login.StatusCode);
        }

        [Fact]
        public async Task Post_Login_IncorrectPassword_ReturnsBadRequest()
        {
            // Arrange
            var client = GenerateClient();

            // Act
            var register = await Register(client, new RegisterDto
            {
                Email = "dom@email.com",
                Password = "PA55word#"
            });

            var login = await Login(client, new LoginDto
            {
                Email = "dom@email.com",
                Password = "password"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, login.StatusCode);
        }
    }
}
