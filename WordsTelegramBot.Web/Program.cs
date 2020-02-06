using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WordsTelegramBot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.UseKestrel(options =>
                    {
                        var port = System.Environment.GetEnvironmentVariable("PORT");
                        if (!string.IsNullOrEmpty(port))
                        {
                            options.ListenAnyIP(int.Parse(port));
                        }
                    });
                })
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.With<RemovePropertiesEnricher>()
                    .WriteTo.Console(),
                writeToProviders: true);
    }

    class RemovePropertiesEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory _)
        {
            logEvent.RemovePropertyIfPresent("SourceContext");
            logEvent.RemovePropertyIfPresent("RequestId");
            logEvent.RemovePropertyIfPresent("RequestPath");
        }
    }
}
