using System.Threading.Tasks;

namespace WordsTelegramBot.Web.Services
{
    public interface ITelegramBotService
    {
        Task SetupAsync();

        Task ProcessUpdatesAsync();
    }
}
