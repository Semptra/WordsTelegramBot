using System;
using System.Collections.Generic;

namespace WordsTelegramBot.Web.Database.Models
{
    public class ChatUser
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }

        public Chat Chat { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public ICollection<ChatUserWord> ChatUserWords { get; set; }
    }
}
