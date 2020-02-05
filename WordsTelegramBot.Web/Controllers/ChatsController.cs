using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Database.Models;

namespace WordsTelegramBot.Web.Controllers
{
    [ApiController]
    [Route("chats")]
    public class ChatsController : ControllerBase
    {
        private readonly TelegramDbContext _context;

        public ChatsController(TelegramDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Chat>> Get()
        {
            return await _context.Chats.ToListAsync();
        }
    }
}
