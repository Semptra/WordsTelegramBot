using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordsTelegramBot.Web.Services;

namespace WordsTelegramBot.Web.Workers
{
    public class TelegramBotWorker : IHostedService, IDisposable
    {
        private readonly ILogger _logger;

        private readonly IStartupService _startupService;

        private readonly IMessageProcessorService _messageProcessorService;

        private Timer _timer;

        public TelegramBotWorker(ILogger<TelegramBotWorker> logger, IStartupService startupService, IMessageProcessorService messageProcessorService)
        {
            _logger = logger;
            _startupService = startupService;
            _messageProcessorService = messageProcessorService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _startupService.SetupAsync();

            _timer = new Timer(ProcessUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            _logger.LogInformation("[{0}] started", nameof(TelegramBotWorker));
        }

        private async void ProcessUpdates(object state)
        {
            await _messageProcessorService.ProcessUpdatesAsync();
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
