using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WordsTelegramBot.Web.Services.Commands
{
    public interface ICommand
    {
        string Name { get; }

        bool CanHandle(Update update);

        Task ExecuteAsync(Update update);
    }
}
