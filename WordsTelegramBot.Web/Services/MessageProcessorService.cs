using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Services.Commands;

namespace WordsTelegramBot.Web.Services
{
    public class MessageProcessorService : IMessageProcessorService
    {
        private readonly ILogger _logger;

        private readonly IList<ICommand> _commands;

        private readonly IServiceScopeFactory _scopeFactory;

        public MessageProcessorService(ILogger<MessageProcessorService> logger, IEnumerable<ICommand> commands, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _commands = commands.ToList();
            _scopeFactory = scopeFactory;
        }

        public async Task ProcessUpdatesAsync()
        {
            _logger.LogInformation("Start processing updates");

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<TelegramDbContext>();
            var telegramBotClient = scope.ServiceProvider.GetService<ITelegramBotClient>();
            var lastUpdate = await context.Updates.SingleOrDefaultAsync();
            var updates = await telegramBotClient.GetUpdatesAsync(offset: lastUpdate?.LastUpdateId ?? -1);

            _logger.LogInformation("Found {0} new updates", updates.Length);

            if (updates.Length != 0)
            {
                foreach (var update in updates)
                {
                    await ProcessUpdateAsync(update);
                }

                await SetLastUpdateAsync(updates.Last(), lastUpdate, context);
            }

            _logger.LogInformation("End processing updates");
        }

        private async Task ProcessUpdateAsync(Update update)
        {
            var processCommand = _commands.SingleOrDefault(x => x.CanHandle(update));

            if (processCommand == null)
            {
                _logger.LogError("Cannot find command to process message '{0}'", update.Id);
            }
            else
            {
                _logger.LogInformation("Found command [{0}] to process message '{1}'", processCommand.Name, update.Id);
                await processCommand.ExecuteAsync(update);
            }
        }

        private async Task SetLastUpdateAsync(Update telegramLastUpdate, Database.Models.Update lastUpdate, TelegramDbContext context)
        {
            var newLastUpdateId = telegramLastUpdate.Id + 1;
            _logger.LogInformation("Setting last update id to {0}...", newLastUpdateId);

            if (lastUpdate == null)
            {
                lastUpdate = new Database.Models.Update { LastUpdateId = newLastUpdateId };
                await context.Updates.AddAsync(lastUpdate);
            }
            else
            {
                lastUpdate.LastUpdateId = newLastUpdateId;
            }

            await context.SaveChangesAsync();
        }
    }
}
