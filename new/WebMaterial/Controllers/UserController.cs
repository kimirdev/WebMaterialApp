using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebMaterial.DAL.Models;
using WebMaterial.DTO;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebMaterial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // POST api/<userController>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<User>> Register([FromForm] UserDto newUser)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = newUser.Email, FirstName = newUser.Name, UserName = newUser.Email};
                var result = await _userManager.CreateAsync(user, newUser.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await _userManager.AddToRoleAsync(user, "reader");
                    return Ok(user);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] UserDto model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok(model.Name);
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("edit")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EditRoles(string email)
        {
            User user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "initiator");
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
