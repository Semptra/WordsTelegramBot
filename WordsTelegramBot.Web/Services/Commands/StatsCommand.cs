using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using WordsTelegramBot.Web.Configuration;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Services.Commands.Common;

namespace WordsTelegramBot.Web.Services.Commands
{
    public class StatsCommand : ITelegramCommand
    {
        private readonly ILogger _logger;

        private readonly WordsBotConfiguration _configuration;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IList<Predicate<Update>> _conditions;

        public StatsCommand(ILogger<WordCommand> logger, IOptions<WordsBotConfiguration> configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _scopeFactory = scopeFactory;
            _conditions = new List<Predicate<Update>>
            {
                Conditions.IsUpdateMessage,
                Conditions.IsMessageNotNullOrEmpty,
                Conditions.IsMessageSingleWord,
                Conditions.IsMessageCommand,
                update => Conditions.IsMessageBotCommand(update, _configuration.TelegramBotName, Tag)
            };
        }

        public string Name => nameof(StatsCommand);

        public string Tag => "/stats";

        public bool CanHandle(Update update)
        {
            return _conditions.All(x => x.Invoke(update));
        }

        public async Task ExecuteAsync(Update update)
        {
            _logger.LogInformation("Executing [{0}] command", Name);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<TelegramDbContext>();
            var telegramBotClient = scope.ServiceProvider.GetService<ITelegramBotClient>();
            var stats = await GetTotalWordsAsync(update.Message.Chat.Id, context);

            var sb = new StringBuilder();
            foreach(var (userId, totalWords) in stats)
            {
                sb.AppendLine($"User: {userId}. Words: {totalWords}");
            }

            await telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString());

            _logger.LogInformation("End executing [{0}] command", Name);
        }

        private async Task<List<(int, int)>> GetTotalWordsAsync(long chatId, TelegramDbContext context)
        {
            var chat = await context.Chats.FirstOrDefaultAsync(x => x.TelegramId == chatId);
            if (chat == null)
            {
                _logger.LogInformation("Got message from a new chat with id {0}", chatId);
                _logger.LogInformation("Adding chat {0} to database...", chatId);

                chat = new Database.Models.Chat { TelegramId = chatId };
                await context.Chats.AddAsync(chat);
                await context.SaveChangesAsync();
            }

            var chatUsers = await context.ChatUsers
                .Include(x => x.User)
                .Include(x => x.ChatUserWords)
                .Where(x => x.ChatId == chat.Id)
                .Select(x => new { User = x.User.TelegramId, TotalWords = x.ChatUserWords.Where(cuw => cuw.ChatUserId == x.Id).Count() })
                .ToListAsync();

            return chatUsers.Select(x => (x.User, x.TotalWords)).ToList();
        }
    }
}
