using Microsoft.EntityFrameworkCore;
using testproject.Models;

namespace testproject.Data
{
    /// <summary>
    /// Контекст базы данных Entity Framework
    /// Определяет структуру БД и связи между таблицами
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet представляют таблицы в базе данных
        public DbSet<User> Users { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); // Первичный ключ
                entity.HasIndex(e => e.Email).IsUnique(); // Уникальный индекс на email
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Настройка таблицы FriendRequests
            modelBuilder.Entity<FriendRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Составной индекс для быстрого поиска заявок между двумя пользователями
                entity.HasIndex(e => new { e.SenderId, e.ReceiverId });

                // Связь с таблицей Users (отправитель)
                entity.HasOne(e => e.Sender)
                      .WithMany(u => u.SentFriendRequests)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict); // Ограниченное удаление

                // Связь с таблицей Users (получатель)
                entity.HasOne(e => e.Receiver)
                      .WithMany(u => u.ReceivedFriendRequests)
                      .HasForeignKey(e => e.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка таблицы Friendships
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Уникальный индекс на пару пользователей
                entity.HasIndex(e => new { e.UserId, e.FriendId }).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Friendships)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Friend)
                      .WithMany()
                      .HasForeignKey(e => e.FriendId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка таблицы Messages
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.SenderId, e.ReceiverId });
                entity.HasIndex(e => e.SentDate); // Индекс для сортировки по дате

                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);

                entity.HasOne(e => e.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(e => e.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка таблицы Posts
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);

                entity.HasOne(e => e.Author)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WallOwner)
                      .WithMany()
                      .HasForeignKey(e => e.WallOwnerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}