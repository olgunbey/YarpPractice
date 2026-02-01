using Microsoft.AspNetCore.Mvc;
using PermiCore.Auth.Models;
using PermiCore.Auth.Services;

namespace PermiCore.Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [HttpPost] 
        public IActionResult Login(LoginRequestModel loginRequestModel)
        {
            TokenService tokenService = new TokenService();
            string accessToken = tokenService.LoginToken(loginRequestModel);
            return Ok(accessToken);
        }
    }
}
