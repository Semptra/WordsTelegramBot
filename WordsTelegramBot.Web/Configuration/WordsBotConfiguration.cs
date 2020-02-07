namespace WordsTelegramBot.Web.Configuration
{
    public class WordsBotConfiguration
    {
        public string TelegramApiToken { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseFileName { get; set; }

        public string KeepAliveUrl { get; set; }

        public string GetUpdatesPeriod { get; set; }
    }
}
