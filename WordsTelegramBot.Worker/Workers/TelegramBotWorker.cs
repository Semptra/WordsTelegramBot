using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordsTelegramBot.Worker.Services;

namespace WordsTelegramBot.Worker.Workers
{
    public class TelegramBotWorker : IHostedService, IDisposable
    {
        private readonly ILogger<TelegramBotWorker> _logger;

        private readonly ITelegramBotService _telegramBotService;

        private readonly TcpListener _listener;

        private Timer _timer;

        public TelegramBotWorker(ILogger<TelegramBotWorker> logger, ITelegramBotService telegramBotService)
        {
            _logger = logger;
            _telegramBotService = telegramBotService;
            _listener = new TcpListener(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("PORT")));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _listener.Start();

            _logger.LogInformation("Timed Hosted Service running.");

            await _telegramBotService.SetupAsync();

            _timer = new Timer(ProcessUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private async void ProcessUpdates(object state)
        {
            _logger.LogInformation("Running ProcessUpdates");
            await _telegramBotService.ProcessUpdatesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TelegramBotWorker is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
