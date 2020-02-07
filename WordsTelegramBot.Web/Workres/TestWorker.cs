using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Services;

namespace WordsTelegramBot.Web.Workers
{
    public class TestWorker : BackgroundService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IServiceScope _scope;

        public TestWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _scope = _scopeFactory.CreateScope();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var database = _scope.ServiceProvider.GetService<TelegramDbContext>();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
