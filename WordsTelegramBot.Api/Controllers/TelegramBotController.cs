using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WordsTelegramBot.Api.Services;

namespace WordsTelegramBot.Api.Controllers
{
    [ApiController]
    [Route("telegram")]
    public class TelegramBotController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        private readonly ITelegramBotService _telegramBotService;

        public TelegramBotController(ILogger<TestController> logger, ITelegramBotService telegramBotService)
        {
            _logger = logger;
            _telegramBotService = telegramBotService;
        }

        [HttpPost("update")]
        public async Task Update([FromBody]Update update)
        {
            if (update == null)
            {
                _logger.LogError("{0} is null, cannot process.", nameof(update));
            }
            else
            {
                await _telegramBotService.ProcessUpdateAsync(update);
            }
        }

        [HttpGet("webhook/info")]
        public async Task<WebhookInfo> GetWebhookInfo()
        {
            return await _telegramBotService.GetWebhookInfoAsync();
        }

        [HttpGet("webhook/delete")]
        public async Task<string> DeleteWebhook()
        {
            await _telegramBotService.DeleteWebhookAsync();
            return "Webhook deleted";
        }

        [HttpGet("webhook/set/{url}")]
        public async Task<string> SetWebhook(string url)
        {
            await _telegramBotService.SetWebhookAsync(url);
            return $"Webhook set to {url}";
        }
    }
}
