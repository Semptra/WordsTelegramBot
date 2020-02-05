using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WordsTelegramBot.Worker.Configuration;
using WordsTelegramBot.Worker.Database;

namespace WordsTelegramBot.Worker.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ILogger _logger;

        private readonly ITelegramBotClient _telegramBotClient;

        private readonly TelegramDbContext _context;

        public TelegramBotService(ILogger<TelegramBotService> logger, IOptions<WordsBotConfiguration> configuration, TelegramDbContext context)
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

            _logger.LogInformation("Migrating database...");
            await _context.Database.MigrateAsync();

            _logger.LogInformation("Setup done");
        }

        public async Task ProcessUpdatesAsync()
        {
            _logger.LogInformation("Start processing updates");

            var lastUpdate = await _context.Updates.OrderBy(x => x.LastUpdateId).LastOrDefaultAsync();
            var updates = await _telegramBotClient.GetUpdatesAsync(offset: lastUpdate?.LastUpdateId ?? 0);

            _logger.LogInformation("Found {0} new updates", updates.Length);

            var i = 1;
            foreach(var update in updates)
            {
                _logger.LogInformation("Processing update {0} ({1}/{2})", update.Id, i++, updates.Length);
                await ProcessUpdateAsync(update);
            }

            var telegramLastUpdate = updates.LastOrDefault();
            if (telegramLastUpdate != null)
            {
                await SetLastUpdateAsync(telegramLastUpdate);
            }

            _logger.LogInformation("Saving data to database...");
            await _context.SaveChangesAsync();

            _logger.LogInformation("End processing updates");
        }

        private async Task ProcessUpdateAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
            {
                _logger.LogInformation("Update type is {0}, but expected {1}. Skipped", update.Type, UpdateType.Message);
                return;
            }

            var telegramWords = update.Message.Text.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (telegramWords.Length != 1)
            {
                _logger.LogInformation("Message is too long or short, expected word length 1, but got {0}", telegramWords.Length);
                return;
            }

            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.TelegramId == update.Message.Chat.Id);
            if (chat == null)
            {
                _logger.LogInformation("Got message from a new chat with id {0}", update.Message.Chat.Id);
                _logger.LogInformation("Adding chat {0} to database...", update.Message.Chat.Id);

                chat = new Database.Models.Chat { TelegramId = update.Message.Chat.Id };
                await _context.Chats.AddAsync(chat);
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.TelegramId == update.Message.From.Id);
            if (user == null)
            {
                _logger.LogInformation("Got message from a new user with id {0}", update.Message.From.Id);
                _logger.LogInformation("Adding user {0} to database...", update.Message.From.Id);

                user = new Database.Models.User { TelegramId = update.Message.From.Id };
                await _context.Users.AddAsync(user);
            }

            var chatUser = await _context.ChatUsers.FirstOrDefaultAsync(x => x.ChatId == chat.Id && x.UserId == user.Id);
            if (chatUser == null)
            {
                _logger.LogInformation("Creating connection between user {0} and chat {1}...", user.TelegramId, chat.TelegramId);

                chatUser = new Database.Models.ChatUser { User = user, Chat = chat };
                await _context.ChatUsers.AddAsync(chatUser);
            }

            var telegramWord = telegramWords.Single();

            var chatUserWord = await _context.ChatUserWords.Include(x => x.Word).FirstOrDefaultAsync(x => x.ChatUser == chatUser && x.Word.Value == telegramWord);
            if (chatUserWord == null)
            {
                _logger.LogInformation("Got a new word {0} for user {1} in chat {2}", telegramWord, user.TelegramId, chat.TelegramId);
                _logger.LogInformation("Adding word {0} to database...", telegramWord);

                var word = new Database.Models.Word { Value = telegramWord };
                await _context.Words.AddAsync(word);

                _logger.LogInformation("Adding word connection between word {0}, user {1} and chat {2}...", word.Value, user.TelegramId, chat.TelegramId);

                chatUserWord = new Database.Models.ChatUserWord { Word = word, ChatUser = chatUser };
                await _context.ChatUserWords.AddAsync(chatUserWord);
            }
            else
            {
                _logger.LogInformation("Word {0} already exists for user {1} in chat {2}", chatUserWord.Word.Value, user.TelegramId, chat.TelegramId);
            }
        }

        private async Task SetLastUpdateAsync(Update telegramLastUpdate)
        {
            _logger.LogInformation("Setting last update id to {0}...", telegramLastUpdate.Id);

            var lastUpdate = new Database.Models.Update { LastUpdateId = telegramLastUpdate.Id };
            await _context.Updates.AddAsync(lastUpdate);
        }
    }
}
