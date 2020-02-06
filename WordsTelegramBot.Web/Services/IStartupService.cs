using System.Threading.Tasks;

namespace WordsTelegramBot.Web.Services
{
    public interface IStartupService
    {
        Task SetupAsync();
    }
}
