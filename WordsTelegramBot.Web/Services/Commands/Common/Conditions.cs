using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WordsTelegramBot.Web.Services.Commands.Common
{
    public static class Conditions
    {
        public static bool IsUpdateMessage(Update update)
        {
            return update.Type == UpdateType.Message;
        }

        public static bool IsMessageNotNullOrEmpty(Update update)
        {
            return !string.IsNullOrWhiteSpace(update.Message.Text);
        }

        public static bool IsMessageSingleWord(Update update)
        {
            return GetSingleWord(update) != null;
        }

        public static bool IsMessageWord(Update update)
        {
            var word = GetSingleWord(update);
            return char.IsLetter(word.First());
        }

        public static bool IsMessageCommand(Update update)
        {
            var word = GetSingleWord(update);
            return word.StartsWith('/');
        }

        public static bool IsMessageBotCommand(Update update, string botName, string command)
        {
            var word = GetSingleWord(update);
            return string.Equals($"/{command}", word) || string.Equals($"/{command}{botName}", word);
        }

        private static string GetSingleWord(Update update)
        {
            return update.Message.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).SingleOrDefault();
        }
    }
}
