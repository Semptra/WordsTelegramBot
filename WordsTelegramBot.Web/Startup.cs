using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Configuration;
using WordsTelegramBot.Web.Services;
using WordsTelegramBot.Web.Workers;
using WordsTelegramBot.Web.Services.Commands;
using Microsoft.Extensions.Options;

namespace WordsTelegramBot.Web
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables("WordsBotConfiguration_");

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<WordsBotConfiguration>()
                .Configure<IConfiguration>((messageResponderSettings, configuration) =>
                {
                    configuration.GetSection("WordsBotConfiguration").Bind(messageResponderSettings);
                });

            services.AddControllers();

            services.AddDbContext<TelegramDbContext>();

            #region Configuration

            services.Configure<WordsBotConfiguration>(Configuration.GetSection("WordsBotConfiguration"));

            #endregion

            #region Commands

            services.AddScoped<ICommand, WordCommand>();

            #endregion

            #region Services

            services.AddHttpClient();

            services.AddScoped<ITelegramBotClient>(services =>
                new TelegramBotClient(services.GetService<IOptions<WordsBotConfiguration>>().Value.TelegramApiToken));

            services.AddScoped<IStartupService, StartupService>();
            services.AddScoped<IMessageProcessorService, MessageProcessorService>();

            #endregion

            #region Workers

            services.AddHostedService<TelegramBotWorker>();
            services.AddHostedService<KeepAliveWorker>();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStatusCodePages();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
