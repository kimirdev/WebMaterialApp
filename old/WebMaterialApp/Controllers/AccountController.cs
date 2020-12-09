using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApiApplication.Models;
using WebMaterialApp.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiApplication.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        [HttpGet]
        public IActionResult IsAuthorize()
        {
            return Ok("I'm Authorized");
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        public IActionResult IsAdmin()
        {
            return Ok("I'm Admin");
        }

        [HttpPost]
        public IActionResult Register([FromBody] UserDTO model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email, FullName = model.FullName };
                // добавляем пользователя
                var result = _userManager.CreateAsync(user, model.Password);
                var identity = GetIdentityAsync(model.Email, model.Password).Result;
                var response = new
                {
                    access_token = GetJwtToken(identity),
                    username = identity.Name
                };

                return Ok(response);
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult Login([FromBody] UserDTO model)
        {
            var identity = GetIdentityAsync(model.Email, model.Password).Result;
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var response = new
            {
                access_token = GetJwtToken(identity),
                username = identity.Name
            };

            return Ok(response);
        }


        private string GetJwtToken(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: "LocalServer",
                    audience: "LocalClient",
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(10)),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes("this is my custom Secret key for authnetication")), 
                        SecurityAlgorithms.HmacSha256)
                    );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
        private async Task<ClaimsIdentity> GetIdentityAsync(string email, string password)
        {
            User user = await _userManager.FindByEmailAsync(email);
            var passwordChecker = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password);
            //Person person = people.FirstOrDefault(x => x.Login == username && x.Password == password);
            if (passwordChecker == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "Admin")
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }
    }
}
