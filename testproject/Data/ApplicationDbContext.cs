using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using testproject.Models; // или testproject.Data.Entities

namespace testproject.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Дополнительные настройки если нужны
            builder.Entity<User>()
                .Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Entity<User>()
                .Property(u => u.LastName)
                .HasMaxLength(100);
        }
    }
}
