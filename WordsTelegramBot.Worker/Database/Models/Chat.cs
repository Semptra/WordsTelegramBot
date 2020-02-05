using System;
using System.Collections.Generic;

namespace WordsTelegramBot.Worker.Database.Models
{
    public class Chat
    {
        public Guid Id { get; set; }

        public long TelegramId { get; set; }

        public ICollection<ChatUser> ChatUsers { get; set; }
    }
}
