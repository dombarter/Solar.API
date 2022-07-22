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
