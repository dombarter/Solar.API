using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Solar.API.Controllers
{
    [ApiController]
    [Route("moons")]
    [Authorize]
    public class MoonController : Controller
    {
        private readonly List<string> Moons = new List<string> { "Moon", "Europa", "Titan", "Ganymede", "Milmas", "Hyperion", "Dione", "Kiviuq" };
        private readonly Random Random = new Random();


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
