using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Solar.Common.Roles;
using Solar.DTOs.Inbound;
using Solar.DTOs.Outbound;
using Solar.Services.Token;
using System.IdentityModel.Tokens.Jwt;

namespace Solar.API.Controllers
{
    [ApiController]
    [Route("user")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _config = config;
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
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
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
            {
                return BadRequest("Incorrect email or password");
            }

            // Generate JWT
            var user = await _userManager.FindByNameAsync(model.Email);
            var token = await _tokenService.GenerateJwtToken(user, TimeSpan.FromMinutes(30));


            Response.Cookies.Append(_config["Jwt:CookieName"], token, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(30),
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            return new OkObjectResult(
                new LoginResultDto(user.Email, await _userManager.GetRolesAsync(user))
            );
        }

        [HttpPost]
        [Route("logout")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            Response.Cookies.Append(_config["Jwt:CookieName"], string.Empty, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                MaxAge = TimeSpan.Zero,
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            return Ok();
        }
    }
}
