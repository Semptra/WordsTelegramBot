using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using WordsTelegramBot.Web.Configuration;
using WordsTelegramBot.Web.Database;

namespace WordsTelegramBot.Web.Services
{
    public class StartupService : IStartupService
    {
        private readonly ILogger _logger;

        private readonly ITelegramBotClient _telegramBotClient;

        private readonly TelegramDbContext _context;

        public StartupService(ILogger<StartupService> logger, IOptions<WordsBotConfiguration> configuration, TelegramDbContext context)
        {
            _telegramBotClient = new TelegramBotClient(configuration.Value.TelegramApiToken);
            _logger = logger;
            _context = context;
        }

        public async Task SetupAsync()
        {
            _logger.LogInformation("Start setup");

            _logger.LogInformation("Checking for existing telegram webhook...");
            var webhookInfo = await _telegramBotClient.GetWebhookInfoAsync();
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                _logger.LogInformation("Webhook found with url {0}", webhookInfo.Url);
                _logger.LogInformation("Deleting telegram webhook...");
                await _telegramBotClient.DeleteWebhookAsync();
            }

            _logger.LogInformation("Creating database...");
            await _context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Setup done");
        }
    }
}
