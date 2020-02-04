using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WordsTelegramBot.Api.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("status")]
        public string Status()
        {
            _logger.LogInformation("Ok called");
            return "Ok";
        }
    }
}
