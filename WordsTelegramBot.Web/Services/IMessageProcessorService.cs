using System.Threading.Tasks;

namespace WordsTelegramBot.Web.Services
{
    public interface IMessageProcessorService
    {
        Task ProcessUpdatesAsync();
    }
}
