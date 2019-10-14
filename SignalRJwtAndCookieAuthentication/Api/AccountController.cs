using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SignalRJwtAndCookieAuthentication.Dtos;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace SignalRJwtAndCookieAuthentication.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private static readonly SigningCredentials SigningCreds = new SigningCredentials(Startup.SecurityKey, SecurityAlgorithms.HmacSha256);
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("[Action]")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");

                if (HttpContext.Request.Cookies["Token"] != null)
                {
                    HttpContext.Response.Cookies.Delete("Token");
                    HttpContext.Response.Cookies.Delete("UserName");
                }
            }
            catch (Exception ex)
            {
                Ok($"Error: {ex.Message}");
            }

            return Ok("Logged out");
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody]TokenUserCommand tokenUserCommand)
        {
            var email = tokenUserCommand.Email;
            var password = tokenUserCommand.Password;
            try
            {
                // Check the password but don't "sign in" (which would set a cookie)
                var user = await _signInManager.UserManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Ok("Login failed");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);
                    var token = new JwtSecurityToken(
                        "SignalRAuthenticationSample",
                        "SignalRAuthenticationSample",
                        principal.Claims,
                        expires: DateTime.UtcNow.AddDays(30),
                        signingCredentials: SigningCreds);

                    var writtenToken = _tokenHandler.WriteToken(token);

                    if (HttpContext.Request.Cookies["Token"] == null || HttpContext.Request.Cookies["UserName"] != email)
                    {
                        var cookieOptions = new CookieOptions();
                        cookieOptions.Expires = DateTime.Now.AddDays(30);
                        HttpContext.Response.Cookies.Append("Token", writtenToken, cookieOptions);
                        HttpContext.Response.Cookies.Append("UserName", email, cookieOptions);
                    }

                    return Ok(writtenToken);
                }
                else
                {
                    var error = result.IsLockedOut ? "User is locked out" : "Login failed";
                    return Ok(error);
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                return Ok(error);
            }
        }

        [AllowAnonymous]
        [HttpGet("[Action]")]
        public IActionResult CheckToken()
        {
            if (HttpContext.Request.Cookies["Token"] != null)
            {
                var token = HttpContext.Request.Cookies["Token"];
                var userName = HttpContext.Request.Cookies["UserName"];
                return Ok(new { token, userName });
            }

            return Ok();
        }
    }
}
