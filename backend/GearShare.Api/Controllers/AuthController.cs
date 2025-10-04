using System;
using System.Linq;
using System.Threading.Tasks;
using GearShare.Api.Contracts;
using GearShare.Api.Models;
using GearShare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GearShare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IJwtTokenService _jwt;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IJwtTokenService jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwt = jwt;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var allowedRoles = new[] { "OWNER", "RENTER" }; // ADMIN only via seed/admin panel
            var role = allowedRoles.Contains(request.Role?.ToUpperInvariant() ?? string.Empty)
                ? request.Role!.ToUpperInvariant()
                : "RENTER";

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expires) = _jwt.CreateToken(user, roles);
            return Ok(new AuthResponse(
                token,
                expires,
                new UserDto(user.Id.ToString(), user.Email!, user.DisplayName, roles.ToArray())
            ));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null) return Unauthorized();

            var res = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!res.Succeeded) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expires) = _jwt.CreateToken(user, roles);
            return Ok(new AuthResponse(
                token,
                expires,
                new UserDto(user.Id.ToString(), user.Email!, user.DisplayName, roles.ToArray())
            ));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me()
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (id is null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserDto(user.Id.ToString(), user.Email!, user.DisplayName, roles.ToArray()));
        }
    }
}
