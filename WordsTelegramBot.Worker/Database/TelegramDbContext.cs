using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WordsTelegramBot.Worker.Configuration;
using WordsTelegramBot.Worker.Database.Models;

namespace WordsTelegramBot.Worker.Database
{
    public class TelegramDbContext : DbContext
    {
        private readonly WordsBotConfiguration _configuration;

        public TelegramDbContext(IOptions<WordsBotConfiguration> configuration) : base()
        {
            _configuration = configuration.Value;
        }

        public DbSet<Update> Updates { get; set; }

        public DbSet<Chat> Chats { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ChatUser> ChatUsers { get; set; }

        public DbSet<Word> Words { get; set; }

        public DbSet<ChatUserWord> ChatUserWords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_configuration.ConnectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Chat

            modelBuilder.Entity<Chat>()
                .HasKey(x => x.Id);

            #endregion

            #region User

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id);

            #endregion

            #region ChatUsers

            modelBuilder.Entity<ChatUser>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ChatUser>()
                .HasOne(x => x.Chat)
                .WithMany(x => x.ChatUsers)
                .HasForeignKey(x => x.ChatId);

            modelBuilder.Entity<ChatUser>()
                .HasOne(x => x.User)
                .WithMany(x => x.ChatUsers)
                .HasForeignKey(x => x.UserId);

            #endregion

            #region Word

            modelBuilder.Entity<Word>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Word>()
                .HasIndex(x => x.Value)
                .IsUnique(true);

            #endregion

            #region ChatUserWord

            modelBuilder.Entity<ChatUserWord>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ChatUserWord>()
                .HasOne(x => x.ChatUser)
                .WithMany(x => x.ChatUserWords)
                .HasForeignKey(x => x.ChatUserId);

            modelBuilder.Entity<ChatUserWord>()
                .HasOne(x => x.Word)
                .WithMany(x => x.ChatUserWords)
                .HasForeignKey(x => x.WordId);

            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}
