using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolApp.Api.Data;
using SchoolApp.Api.Data.ViewModels;
using SchoolApp.Api.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all the required fields");

            }
            var userExists = await _userManager.FindByNameAsync(registerVM.UserName);
            if (userExists != null)
            {
                return BadRequest($"User {registerVM.EmailAddress} Already Exsits");
            }
            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.EmailAddress,
                UserName = registerVM.UserName,
                SecurityStamp = System.Guid.NewGuid().ToString()

            };
            var result = await _userManager.CreateAsync(newUser, registerVM.Password);
            if (!result.Succeeded)
            {
                return BadRequest("User creation failed! Please check user details and try again.");
            }
            return Ok($"User {registerVM.EmailAddress} Created Successfully!");

        }


        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        { 
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all the required fields");
            }
            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                var tokenValue = await GenerateJwtTokenAsync(user);
                return Ok(tokenValue);
            }
            return Unauthorized();
        }

        private async Task<AuthResultVM> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString())
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var response = new AuthResultVM()
            {
                Token = jwtToken,
                ExpiresAt = token.ValidTo
            };
            return response;
        }
    }
}
