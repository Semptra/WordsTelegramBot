using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Database.Models;

namespace WordsTelegramBot.Web.Controllers
{
    [ApiController]
    [Route("updates")]
    public class UpdatesController : ControllerBase
    {
        private readonly TelegramDbContext _context;

        public UpdatesController(TelegramDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Update>> Get()
        {
            return await _context.Updates.ToListAsync();
        }
    }
}
