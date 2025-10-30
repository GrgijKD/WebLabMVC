using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace WebLabMVC.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Shops)
                .WithMany(s => s.Books);

            modelBuilder.Entity<Author>()
                .HasMany(a => a.Genres)
                .WithMany(g => g.Authors);

            modelBuilder.Entity<Publisher>()
                .HasMany(p => p.Books)
                .WithOne(b => b.Publisher)
                .HasForeignKey(b => b.PublisherId);

            // First Admin
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Administrator1",
                Email = "admin@weblab.com",
                IsAdmin = true,
                IsClient = false,
                IsWorker = false,
                MoneyAmount = 9999
            });
        }
    }
}
