using System;
using System.Collections.Generic;

namespace WordsTelegramBot.Worker.Database.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public int TelegramId { get; set; }

        public ICollection<ChatUser> ChatUsers { get; set; }
    }
}
