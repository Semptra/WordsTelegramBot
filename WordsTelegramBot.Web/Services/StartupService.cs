using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using WordsTelegramBot.Web.Database;

namespace WordsTelegramBot.Web.Services
{
    public class StartupService : IStartupService
    {
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        public StartupService(ILogger<StartupService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task SetupAsync()
        {
            _logger.LogInformation("Start setup");

            using var scope = _scopeFactory.CreateScope();
            var telegramBotClient = scope.ServiceProvider.GetService<ITelegramBotClient>();
            var context = scope.ServiceProvider.GetService<TelegramDbContext>();

            _logger.LogInformation("Checking for existing telegram webhook...");

            var webhookInfo = await telegramBotClient.GetWebhookInfoAsync();
            if (!string.IsNullOrEmpty(webhookInfo.Url))
            {
                _logger.LogInformation("Webhook found with url {0}", webhookInfo.Url);
                _logger.LogInformation("Deleting telegram webhook...");
                await telegramBotClient.DeleteWebhookAsync();
            }

            _logger.LogInformation("Creating database...");
            await context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Setup done");
        }
    }
}
