using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordsTelegramBot.Web.Database;
using WordsTelegramBot.Web.Database.Models;

namespace WordsTelegramBot.Web.Controllers
{
    [ApiController]
    [Route("words")]
    public class WordsController : ControllerBase
    {
        private readonly TelegramDbContext _context;

        public WordsController(TelegramDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Word>> Get()
        {
            return await _context.Words.ToListAsync();
        }
    }
}
