using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Services.Commands.Common;

namespace WordsTelegramBot.Web.Services.Commands
{
    public class WordCommand : ICommand
    {
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IList<Predicate<Update>> _conditions;

        public WordCommand(ILogger<WordCommand> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _conditions = new List<Predicate<Update>>
            {
                Conditions.IsUpdateMessage,
                Conditions.IsMessageNotNullOrEmpty,
                Conditions.IsMessageSingleWord,
                Conditions.IsMessageWord
            };
        }

        public string Name => nameof(WordCommand);

        public bool CanHandle(Update update)
        {
            return _conditions.All(x => x.Invoke(update));
        }

        public async Task ExecuteAsync(Update update)
        {
            _logger.LogInformation("Executing [{0}] command", Name);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<TelegramDbContext>();
            var updateWord = update.Message.Text;
            var chat = await GetChatAsync(update.Message.Chat.Id, context);
            var user = await GetUserAsync(update.Message.From.Id, context);
            var chatUser = await GetChatUserAsync(chat, user, context);
            var chatUserWord = await context.ChatUserWords
                .Include(x => x.Word)
                .Include(x => x.ChatUser)
                .FirstOrDefaultAsync(x => x.ChatUser.ChatId == chat.Id && x.Word.Value == updateWord);

            if (chatUserWord == null)
            {
                _logger.LogInformation("Got a new word {0} for user {1} in chat {2}", updateWord, user.TelegramId, chat.TelegramId);
                _logger.LogInformation("Adding word {0} to database...", updateWord);

                var word = new Database.Models.Word { Value = updateWord };
                await context.Words.AddAsync(word);
                await context.SaveChangesAsync();

                _logger.LogInformation("Adding word connection between word {0}, user {1} and chat {2}...", word.Value, user.TelegramId, chat.TelegramId);

                chatUserWord = new Database.Models.ChatUserWord { Word = word, ChatUser = chatUser };
                await context.ChatUserWords.AddAsync(chatUserWord);
                await context.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Word {0} already exists in chat {1}", chatUserWord.Word.Value, chat.TelegramId);
            }

            _logger.LogInformation("End executing [{0}] command", Name);
        }

        private async Task<Database.Models.Chat> GetChatAsync(long chatId, TelegramDbContext context)
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

            return chat;
        }

        private async Task<Database.Models.User> GetUserAsync(int userId, TelegramDbContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user == null)
            {
                _logger.LogInformation("Got message from a new user with id {0}", userId);
                _logger.LogInformation("Adding user {0} to database...", userId);

                user = new Database.Models.User { TelegramId = userId };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            return user;
        }

        private async Task<Database.Models.ChatUser> GetChatUserAsync(Database.Models.Chat chat, Database.Models.User user, TelegramDbContext context)
        {
            var chatUser = await context.ChatUsers.FirstOrDefaultAsync(x => x.ChatId == chat.Id && x.UserId == user.Id);
            if (chatUser == null)
            {
                _logger.LogInformation("Creating connection between user {0} and chat {1}...", user.TelegramId, chat.TelegramId);

                chatUser = new Database.Models.ChatUser { User = user, Chat = chat };
                await context.ChatUsers.AddAsync(chatUser);
                await context.SaveChangesAsync();
            }

            return chatUser;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
