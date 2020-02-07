using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Web.Configuration;
using WordsTelegramBot.Web.Services;

namespace WordsTelegramBot.Web.Workers
{
    public class TelegramBotWorker : IHostedService, IDisposable
    {
        private readonly ILogger _logger;

        private readonly WordsBotConfiguration _configuration;

        private readonly IServiceScopeFactory _scopeFactory;

        private Timer _timer;

        public TelegramBotWorker(ILogger<TelegramBotWorker> logger, IOptions<WordsBotConfiguration> configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var startupService = scope.ServiceProvider.GetService<IStartupService>();

            await startupService.SetupAsync();

            _timer = new Timer(ProcessUpdates, null, TimeSpan.Zero, TimeSpan.Parse(_configuration.GetUpdatesPeriod));

            _logger.LogInformation("[{0}] started", nameof(TelegramBotWorker));
        }

        private async void ProcessUpdates(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var messageProcessorService = scope.ServiceProvider.GetService<IMessageProcessorService>();

            await messageProcessorService.ProcessUpdatesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation("[{0}] stopped", nameof(TelegramBotWorker));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
