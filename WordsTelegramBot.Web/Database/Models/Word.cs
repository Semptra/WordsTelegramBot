﻿using System;
using System.Collections.Generic;

namespace WordsTelegramBot.Web.Database.Models
{
    public class Word
    {
        public Guid Id { get; set; }

        public string Value { get; set; }

        public ICollection<ChatUserWord> ChatUserWords { get; set; }
    }
}
