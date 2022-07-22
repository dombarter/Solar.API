using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Solar.Common.Roles;
using Solar.DTOs.Inbound;
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

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
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
    }
}
