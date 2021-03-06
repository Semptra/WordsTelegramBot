﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Web.Configuration;

namespace WordsTelegramBot.Web.Workers
{
    public class KeepAliveWorker : IHostedService, IDisposable
    {
        private readonly ILogger _logger;

        private readonly WordsBotConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;

        private Timer _timer;

        public KeepAliveWorker(ILogger<KeepAliveWorker> logger, IOptions<WordsBotConfiguration> configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(KeepAliveAsync, null, TimeSpan.Zero, TimeSpan.Parse(_configuration.KeepAlivePeriod));

            _logger.LogInformation("[{0}] started", nameof(KeepAliveWorker));

            return Task.CompletedTask;
        }

        private async void KeepAliveAsync(object state)
        {
            _logger.LogInformation("Keep alive started");

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(_configuration.KeepAliveUrl);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Keep alive success. Status code: {0}", response.StatusCode);
            }
            else
            {
                _logger.LogError("Keep alive failure. Status code: {0}", response.StatusCode);
            }

            _logger.LogInformation("Keep alive ended");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation("[{0}] stopped", nameof(KeepAliveWorker));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
