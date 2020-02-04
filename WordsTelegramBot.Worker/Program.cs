using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Worker.Configuration;
using WordsTelegramBot.Worker.Services;
using WordsTelegramBot.Worker.Workers;

namespace WordsTelegramBot.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddOptions<WordsBotConfiguration>()
                        .Configure<IConfiguration>((messageResponderSettings, configuration) =>
                        {
                            configuration.GetSection("WordsBotConfiguration").Bind(messageResponderSettings);
                        });

                    services.AddLogging();
                    services.AddSingleton<ITelegramBotService, TelegramBotService>();
                    services.AddHostedService<TelegramBotWorker>();
                });
    }
}
