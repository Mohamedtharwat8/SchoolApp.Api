using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SchoolApp.Api.Data;
using SchoolApp.Api.Data.ViewModels;
using SchoolApp.Api.Models;
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
            if(userExists != null)
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
            if(!result.Succeeded)
            {
                return BadRequest("User creation failed! Please check user details and try again.");
            }
            return Ok($"User {registerVM.EmailAddress} Created Successfully!");

        }

    }
}
