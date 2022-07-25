using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Solar.API.Controllers
{
    [ApiController]
    [Route("moons")]
    [Authorize]
    public class MoonController : Controller
    {
        private readonly List<string> Moons = new List<string> { "Moon", "Europa", "Titan", "Ganymede", "Milmas", "Hyperion", "Dione", "Kiviuq" };
        private readonly Random Random = new Random();

        private readonly UserManager<IdentityUser> _userManager;

        public MoonController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Route("user")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<IdentityUser>> GetLoggedInUser()
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByNameAsync(username);
            return new OkObjectResult(user);
        }

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
    }
}
