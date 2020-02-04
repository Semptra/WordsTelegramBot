using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WordsTelegramBot.Api.Services
{
    public interface ITelegramBotService
    {
        Task ProcessUpdateAsync(Update update);

        Task<WebhookInfo> GetWebhookInfoAsync();

        Task DeleteWebhookAsync();

        Task SetWebhookAsync(string url);
    }
}
