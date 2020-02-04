using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Api.Configuration;

namespace WordsTelegramBot.Api.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly WordsBotConfiguration _wordsBotConfiguration;

        private readonly ILogger _logger;

        public TelegramBotService(IOptions<WordsBotConfiguration> configuration, ILogger<TelegramBotService> logger)
        {
            _wordsBotConfiguration = configuration.Value;
            _logger = logger;
        }

        public async Task ProcessUpdatesAsync()
        {
            _logger.LogInformation("ProcessUpdatesAsync called");
        }
    }
}
