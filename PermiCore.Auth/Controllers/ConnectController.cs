using Microsoft.AspNetCore.Mvc;
using PermiCore.Auth.Models;

namespace PermiCore.Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Token(TokenRequestModel TokenRequestModel)
        {
            return Ok();
        }
    }
}
