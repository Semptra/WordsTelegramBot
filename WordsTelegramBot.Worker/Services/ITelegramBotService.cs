using System.Threading.Tasks;

namespace WordsTelegramBot.Worker.Services
{
    public interface ITelegramBotService
    {
        Task SetupAsync();

        Task ProcessUpdatesAsync();
    }
}
