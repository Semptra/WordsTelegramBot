using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using WordsTelegramBot.Api.Configuration;
using WordsTelegramBot.Api.Services;

namespace WordsTelegramBot.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services
               .AddOptions<WordsBotConfiguration>()
               .Configure<IConfiguration>((messageResponderSettings, configuration) =>
               {
                   configuration.GetSection("WordsBotConfiguration").Bind(messageResponderSettings);
               });

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            services.AddSingleton<ITelegramBotClient>(_ =>
                new TelegramBotClient(Configuration.GetSection("WordsBotConfiguration").GetValue<string>("TelegramApiToken")));

            services.AddSingleton<ITelegramBotService, TelegramBotService>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
