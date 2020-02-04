using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WordsTelegramBot.Api.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ILogger _logger;

        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramBotService(ILogger<TelegramBotService> logger, ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        public Task<WebhookInfo> GetWebhookInfoAsync()
        {
            return _telegramBotClient.GetWebhookInfoAsync();
        }

        public Task SetWebhookAsync(string url)
        {
            return _telegramBotClient.SetWebhookAsync(url);
        }

        public Task DeleteWebhookAsync()
        {
            return _telegramBotClient.DeleteWebhookAsync();
        }

        public async Task ProcessUpdateAsync(Update update)
        {
            _logger.LogInformation("ProcessUpdatesAsync called");
            _logger.LogInformation("Processing update: {0}", update.Id);
        }
    }
}
