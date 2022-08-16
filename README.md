# Setup .NET Identity

Add the following packages

- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools

Create your database and add the connection string to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=localhost\\SQLEXPRESS...."
  }
}
```

Create your `DbContext` class, extending `IdentityDbContext`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Solar.Data
{
    public class SolarDbContext : IdentityDbContext<IdentityUser>
    {
        public SolarDbContext(DbContextOptions options) : base (options)
        {
        }
    }
}
```

Then add the following to your startup class - `Program`

```csharp
// Add the database connection
builder.Services.AddDbContext<SolarDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Setup identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<SolarDbContext>();
```

Then run the following commands in the package manager console

```
add-migration init
update-database
```

You will notice 5 or 6 tables have been created to store all the User and Role information.

We can now login and register using the `UserManager` and `SignInManager` as seen in the `AccountController`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Solar.DTOs.Inbound;

namespace Solar.API.Controllers
{
    [ApiController]
    [Route("user")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Create the user
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return new BadRequestObjectResult(result.Errors);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Login
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest("Incorrect email or password");
        }
    }
}

```

Based on:

- https://thecodeblogger.com/2020/01/23/adding-asp-net-core-identity-to-web-api-project/
- https://www.freecodespot.com/blog/asp-net-core-identity/

# .NET Identity Roles

To add roles first of all add this line to your startup code:

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>() // <--- add this line
.AddEntityFrameworkStores<SolarDbContext>();
```

Then, it is probably a good idea to make sure the roles are added to the db when your application starts up, which can you do in the same file using:

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Migrate the database
    var db = services.GetRequiredService<SolarDbContext>();
    db.Database.Migrate();

    // Add the roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync(Roles.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
    }
    if (!await roleManager.RoleExistsAsync(Roles.User))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.User));
    }
}
```

Where `Roles` looks like this:

```csharp
namespace Solar.Common.Roles
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}

```

You can then assign a user to a role using the user manager like so:

```csharp
var user = new IdentityUser
{
    UserName = model.Email,
    Email = model.Email
};

var createResult = await _userManager.CreateAsync(user, model.Password);

if (!createResult.Succeeded)
{
    return new BadRequestObjectResult(createResult.Errors);
}

// Assign the role
var assignRoleResult = await _userManager.AddToRoleAsync(user, Roles.User);

if (!assignRoleResult.Succeeded)
{
    return new BadRequestObjectResult(assignRoleResult.Errors);
}

return Ok();
```

Based on:

- https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-6.0
- https://www.c-sharpcorner.com/article/jwt-authentication-and-authorization-in-net-6-0-with-identity-framework/
- https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-6.0

# Adding JWTs

Install the following package:

- Microsoft.AspNetCore.Authentication.JwtBearer

Add the following lines to your startup class:

```csharp
// Add JWTs
builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

...

// Must be in this order
app.UseAuthentication();
app.UseAuthorization();
```

With the following values in your `appsettings.json`

```json
{
  "Jwt": {
    "Key": "ThisIsMySecretKey",
    "Issuer": "https://localhost:7234/",
    "Audience": "https://localhost:7234/"
  }
}
```

Now we need to create a service that will accept an Identity user and create a token for them to use on our site:

```csharp
// TokenService.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Solar.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public TokenService(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        async public Task<string> GenerateJwtToken(IdentityUser user, TimeSpan expiration)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach(var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.Add(expiration),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```

Which can be used in the Login action like so:

```csharp
[HttpPost]
[Route("login")]
[AllowAnonymous]
public async Task<ActionResult<string>> Login([FromBody] LoginDto model)
{
    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

    if (!result.Succeeded)
    {
        return BadRequest("Incorrect email or password");
    }

    // Generate JWT
    var user = await _userManager.FindByNameAsync(model.Email);
    var token = await _tokenService.GenerateJwtToken(user, TimeSpan.FromMinutes(30));

    return Ok(token);
}
```

The roles stored in the JWT are then correctly loaded in on each request, meaning you can use the `Authorize` attributes like normal:

```csharp
[HttpGet]
[Route("one")]
[Authorize(Roles = "User")]
public ActionResult<string> GetRandomMoon()
{
    return Moons[Random.Next(Moons.Count)];
}

[HttpGet]
[Route("two")]
[Authorize(Roles = "Admin")]
public ActionResult<string> GetTwoRandomMoons()
{
    return $"{Moons[Random.Next(Moons.Count)]}, {Moons[Random.Next(Moons.Count)]}";
}
```

You can also get the username we injected into the `sub` of the JWT using:

```csharp
[HttpGet]
[Route("user")]
[Authorize(Roles = "Admin, User")]
public async Task<ActionResult<IdentityUser>> GetLoggedInUser()
{
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var user = await _userManager.FindByNameAsync(username);
    return new OkObjectResult(user);
}
```

And finally, if you'd like to support the Authorize window in Swagger (adds the ability to pass the Bearer token with each subsequent request), add the following to your startup class:

```csharp
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement(){{ jwtSecurityScheme, new string[] {} }});
});
```

Based on:

- https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-6.0
- https://codewithmukesh.com/blog/aspnet-core-api-with-jwt-authentication/
- https://weblog.west-wind.com/posts/2021/Mar/09/Role-based-JWT-Tokens-in-ASPNET-Core
- https://www.c-sharpcorner.com/article/how-to-add-jwt-bearer-token-authorization-functionality-in-swagger/
- https://www.freecodespot.com/blog/use-jwt-bearer-authorization-in-swagger/
