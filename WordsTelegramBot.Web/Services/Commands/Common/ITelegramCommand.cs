namespace WordsTelegramBot.Web.Services.Commands.Common
{
    public interface ITelegramCommand : ICommand
    {
        string Tag { get; }
    }
}
