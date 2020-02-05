using System;
using System.Net;
using System.Net.Sockets;
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

        private readonly ITelegramBotService _telegramBotService;

        private readonly TcpListener _listener;

        private Timer _timer;

        public TelegramBotWorker(ILogger logger, ITelegramBotService telegramBotService)
        {
            _logger = logger;
            _telegramBotService = telegramBotService;
            _listener = new TcpListener(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("PORT")));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //_listener.Start();

            await _telegramBotService.SetupAsync();

            _timer = new Timer(ProcessUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            _logger.LogInformation("{0} started", nameof(TelegramBotWorker));
        }

        private async void ProcessUpdates(object state)
        {
            await _telegramBotService.ProcessUpdatesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            //_listener.Stop();

            _logger.LogInformation("{0} stopped", nameof(TelegramBotWorker));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
