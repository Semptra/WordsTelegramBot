using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Web.Configuration;

namespace WordsTelegramBot.Web.Controllers
{
    [ApiController]
    [Route("database")]
    public class DatabaseController : ControllerBase
    {
        private readonly WordsBotConfiguration _configuration;

        public DatabaseController(IOptions<WordsBotConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var file = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), _configuration.DatabaseFileName));

            if (file.Exists)
            {
                var tempFileName = file.FullName + ".tmp";
                System.IO.File.Copy(file.FullName, tempFileName, true);
                var fileStream = file.OpenRead();
                return new FileStreamResult(fileStream, "application/octet-stream")
                {
                    FileDownloadName = "TelegramDatabase.db"
                };
            }
            else
            {
                var currentDirectoryContent = Directory.EnumerateFiles(Directory.GetCurrentDirectory());
                return Content($"File not found. Current directory content:\n{currentDirectoryContent}");
            }
        }
    }
}
