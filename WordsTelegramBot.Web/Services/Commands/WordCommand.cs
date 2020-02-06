using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WordsTelegramBot.Web.Database;

namespace WordsTelegramBot.Web.Services.Commands
{
    public class WordCommand : ICommand
    {
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly TelegramDbContext _context;

        private readonly IList<Predicate<Update>> _conditions;

        public WordCommand(ILogger<WordCommand> logger, TelegramDbContext context)
        {
            _logger = logger;
            _context = context;
            _conditions = new List<Predicate<Update>>
            {
                IsUpdateMessage,
                IsMessageNotNullOrEmpty,
                IsMessageSingleWord,
                IsMessageWord
            };
        }

        public string Name => nameof(WordCommand);

        public bool CanHandle(Update update)
        {
            return _conditions.All(x => x.Invoke(update));
        }

        #region Conditions

        private bool IsUpdateMessage(Update update)
        {
            return update.Type == UpdateType.Message;
        }

        private bool IsMessageNotNullOrEmpty(Update update)
        {
            return !string.IsNullOrWhiteSpace(update.Message.Text);
        }

        private bool IsMessageSingleWord(Update update)
        {
            var splittedMessage = update.Message.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return splittedMessage.Length == 1;
        }

        private bool IsMessageWord(Update update)
        {
            var word = update.Message.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).SingleOrDefault();
            return char.IsLetter(word.First());
        }

        #endregion

        public async Task ExecuteAsync(Update update)
        {
            _logger.LogInformation("Executing [{0}] command", Name);

            var updateWord = update.Message.Text;
            var chat = await GetChatAsync(update.Message.Chat.Id);
            var user = await GetUserAsync(update.Message.From.Id);
            var chatUser = await GetChatUserAsync(chat, user);
            var chatUserWord = await _context.ChatUserWords
                .Include(x => x.Word)
                .Include(x => x.ChatUser)
                .FirstOrDefaultAsync(x => x.ChatUser.ChatId == chat.Id && x.Word.Value == updateWord);

            if (chatUserWord == null)
            {
                _logger.LogInformation("Got a new word {0} for user {1} in chat {2}", updateWord, user.TelegramId, chat.TelegramId);
                _logger.LogInformation("Adding word {0} to database...", updateWord);

                var word = new Database.Models.Word { Value = updateWord };
                await _context.Words.AddAsync(word);

                _logger.LogInformation("Adding word connection between word {0}, user {1} and chat {2}...", word.Value, user.TelegramId, chat.TelegramId);

                chatUserWord = new Database.Models.ChatUserWord { Word = word, ChatUser = chatUser };
                await _context.ChatUserWords.AddAsync(chatUserWord);
            }
            else
            {
                _logger.LogInformation("Word {0} already exists in chat {1}", chatUserWord.Word.Value, chat.TelegramId);
            }

            _logger.LogInformation("End executing [{0}] command", Name);
        }

        private async Task<Database.Models.Chat> GetChatAsync(long chatId)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.TelegramId == chatId);
            if (chat == null)
            {
                _logger.LogInformation("Got message from a new chat with id {0}", chatId);
                _logger.LogInformation("Adding chat {0} to database...", chatId);

                chat = new Database.Models.Chat { TelegramId = chatId };
                await _context.Chats.AddAsync(chat);
            }

            return chat;
        }

        private async Task<Database.Models.User> GetUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.TelegramId == userId);
            if (user == null)
            {
                _logger.LogInformation("Got message from a new user with id {0}", userId);
                _logger.LogInformation("Adding user {0} to database...", userId);

                user = new Database.Models.User { TelegramId = userId };
                await _context.Users.AddAsync(user);
            }

            return user;
        }

        private async Task<Database.Models.ChatUser> GetChatUserAsync(Database.Models.Chat chat, Database.Models.User user)
        {
            var chatUser = await _context.ChatUsers.FirstOrDefaultAsync(x => x.ChatId == chat.Id && x.UserId == user.Id);
            if (chatUser == null)
            {
                _logger.LogInformation("Creating connection between user {0} and chat {1}...", user.TelegramId, chat.TelegramId);

                chatUser = new Database.Models.ChatUser { User = user, Chat = chat };
                await _context.ChatUsers.AddAsync(chatUser);
            }

            return chatUser;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
