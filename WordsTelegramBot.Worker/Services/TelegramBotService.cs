using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using WordsTelegramBot.Worker.Configuration;

namespace WordsTelegramBot.Worker.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ILogger _logger;

        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramBotService(ILogger<TelegramBotService> logger, IOptions<WordsBotConfiguration> configuration)
        {
            _telegramBotClient = new TelegramBotClient(configuration.Value.TelegramApiToken);
            _logger = logger;
        }

        public async Task SetupAsync()
        {
            var webhookInfo = await _telegramBotClient.GetWebhookInfoAsync();
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                await _telegramBotClient.DeleteWebhookAsync();
            }
        }

        public async Task ProcessUpdatesAsync()
        {
            var updates = await _telegramBotClient.GetUpdatesAsync();
            _logger.LogInformation("ProcessUpdatesAsync called");
            _logger.LogInformation("Updates count: {0}", updates.Length);
        }
    }
}
