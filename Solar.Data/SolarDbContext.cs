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
