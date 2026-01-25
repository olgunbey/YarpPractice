using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YarpExample.BasketAPI.Models;

namespace YarpExample.BasketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        [HttpPost]
        public IActionResult SaveBasket([FromBody]SaveBasketRequest saveBasketRequest, [FromHeader] string testId)
        {
            return Ok("Basket saved successfully. "+ testId);
        }
    }
}
