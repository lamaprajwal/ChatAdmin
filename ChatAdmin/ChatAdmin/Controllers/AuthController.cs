using ChatAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatAdmin.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<chatUser> _userManager;
        private readonly SignInManager<chatUser> _signInManager;

        public AuthController(UserManager<chatUser> userManager, SignInManager<chatUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new chatUser { UserName = model.Name, Name = model.Name };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {

                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(user);
                }
            }
            return BadRequest();
           
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Find the user by username
            var user = await _userManager.FindByNameAsync(model.Name);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Check the credentials
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                user.Status = "online";
                return Ok(new
                {
                    message = "Login successful",
                    username = user.UserName,
                });
            }
            return Unauthorized("Invalid username or password.");
        }
                   




    }
}
