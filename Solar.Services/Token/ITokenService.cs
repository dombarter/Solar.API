using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace Solar.Services.Token
{
    public interface ITokenService
    {
        public Task<string> GenerateJwtToken(IdentityUser user, TimeSpan expiration);
    }
}
