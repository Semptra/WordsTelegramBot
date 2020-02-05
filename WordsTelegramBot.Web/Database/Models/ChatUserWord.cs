using System;

namespace WordsTelegramBot.Web.Database.Models
{
    public class ChatUserWord
    {
        public Guid Id { get; set; }

        public Guid ChatUserId { get; set; }

        public ChatUser ChatUser { get; set; }

        public Guid WordId { get; set; }

        public Word Word { get; set; }
    }
}
