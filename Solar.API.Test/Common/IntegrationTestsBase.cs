using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Solar.Data;
using Solar.DTOs.Inbound;
using System.Net.Http.Json;

namespace Solar.API.Test.Common
{
    public class IntegrationTestsBase
    {
        protected HttpClient GenerateClient()
        {
            // Build up the API, using an in memory database
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the db context
                        services.RemoveAll(typeof(DbContextOptions<SolarDbContext>));

                        // Replace the connection with an in memory equivalent
                        var dbName = Guid.NewGuid().ToString();
                        services.AddDbContext<SolarDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(dbName);
                        });
                    });
                });

            // Generate the client
            var client = application.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            return client;
        }

        protected async Task<HttpResponseMessage> Register(HttpClient client, RegisterDto registerDto)
        {
            // Act
            var response = await client.PostAsJsonAsync("/user/register", registerDto);
            return response;
        }

        protected async Task<HttpResponseMessage> Login(HttpClient client, LoginDto loginDto)
        {
            // Act
            var response = await client.PostAsJsonAsync("/user/login", loginDto);
            return response;
        }

        protected async Task AuthenticateAsUser(HttpClient client)
        {
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
        }
    }
}
