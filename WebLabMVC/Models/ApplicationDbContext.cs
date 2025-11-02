using Microsoft.EntityFrameworkCore;

namespace WebLabMVC.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Publisher> Publishers { get; set; } = null!;
        public DbSet<Shop> Shops { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity(j => j.ToTable("BookAuthor"));

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books)
                .UsingEntity(j => j.ToTable("BookGenre"));

            modelBuilder.Entity<Author>()
                .HasMany(a => a.Genres)
                .WithMany(g => g.Authors)
                .UsingEntity(j => j.ToTable("AuthorGenre"));

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Shops)
                .WithMany(s => s.Books)
                .UsingEntity(j => j.ToTable("BookShop"));

            modelBuilder.Entity<Publisher>()
                .HasMany(p => p.Books)
                .WithOne(b => b.Publisher)
                .HasForeignKey(b => b.PublisherId);

            // First admin
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Administrator1",
                Email = "admin@bookshop.com",
                IsAdmin = true,
                IsClient = false,
                IsWorker = false,
                MoneyAmount = 9999
            });
        }
    }
}