﻿using System;

namespace WordsTelegramBot.Worker.Database.Models
{
    public class Update
    {
        public Guid Id { get; set; }

        public int LastUpdateId { get; set; }
    }
}
