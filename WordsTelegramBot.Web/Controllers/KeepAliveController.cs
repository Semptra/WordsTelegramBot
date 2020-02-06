using Microsoft.AspNetCore.Mvc;

namespace WordsTelegramBot.Web.Controllers
{
    [ApiController]
    [Route("keepalive")]
    public class KeepAliveController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
