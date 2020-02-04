using System.Threading.Tasks;

namespace WordsTelegramBot.Api.Services
{
    public interface ITelegramBotService
    {
        Task ProcessUpdatesAsync();
    }
}
