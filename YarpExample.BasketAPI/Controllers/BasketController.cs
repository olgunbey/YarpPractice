using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YarpExample.BasketAPI.Models;

namespace YarpExample.BasketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public IActionResult SaveBasket([FromBody]SaveBasketRequest saveBasketRequest)
        {
            return Ok("Basket saved successfully. ");
        }
    }
}
