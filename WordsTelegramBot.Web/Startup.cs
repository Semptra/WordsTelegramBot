using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Configuration;
using WordsTelegramBot.Web.Services;
using WordsTelegramBot.Web.Workers;

namespace WordsTelegramBot.Web
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
            services
                .AddOptions<WordsBotConfiguration>()
                .Configure<IConfiguration>((messageResponderSettings, configuration) =>
                {
                    configuration.GetSection("WordsBotConfiguration").Bind(messageResponderSettings);
                });

            services.AddControllers();
            services.AddSingleton<ITelegramBotService, TelegramBotService>();
            services.AddDbContext<TelegramDbContext>();
            services.AddHostedService<TelegramBotWorker>();
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
