using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Models;
using PortalBeritaApp.Models.Enums;

namespace PortalBeritaApp.Data
{
    /// <summary>
    /// Application database context inheriting from IdentityDbContext with Guid keys.
    /// Per PRD §9: Normalized relational design with proper indexes, constraints, and FK behavior.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<ArticleLike> ArticleLikes => Set<ArticleLike>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ──────────────────────────────────────
            // ApplicationUser configuration
            // ──────────────────────────────────────
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.DisplayName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);
            });

            // ──────────────────────────────────────
            // Category configuration
            // ──────────────────────────────────────
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Slug)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                // Unique constraints (PRD §49)
                entity.HasIndex(c => c.Name)
                    .IsUnique()
                    .HasDatabaseName("UX_Categories_Name");

                entity.HasIndex(c => c.Slug)
                    .IsUnique()
                    .HasDatabaseName("UX_Categories_Slug");
            });

            // ──────────────────────────────────────
            // Article configuration
            // ──────────────────────────────────────
            builder.Entity<Article>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(a => a.Slug)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(a => a.Excerpt)
                    .HasMaxLength(500);

                entity.Property(a => a.Content)
                    .IsRequired();

                entity.Property(a => a.FeaturedImageUrl)
                    .HasMaxLength(500);

                entity.Property(a => a.Status)
                    .HasDefaultValue(ArticleStatus.Draft)
                    .HasConversion<int>();

                // Unique index on Slug (PRD §15, §49)
                entity.HasIndex(a => a.Slug)
                    .IsUnique()
                    .HasDatabaseName("UX_Articles_Slug");

                // Indexes (PRD §50)
                entity.HasIndex(a => a.AuthorId)
                    .HasDatabaseName("IX_Articles_AuthorId");

                entity.HasIndex(a => a.CategoryId)
                    .HasDatabaseName("IX_Articles_CategoryId");

                entity.HasIndex(a => a.Status)
                    .HasDatabaseName("IX_Articles_Status");

                entity.HasIndex(a => a.PublishedAt)
                    .HasDatabaseName("IX_Articles_PublishedAt");

                // Relationships
                entity.HasOne(a => a.Author)
                    .WithMany(u => u.Articles)
                    .HasForeignKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Category)
                    .WithMany(c => c.Articles)
                    .HasForeignKey(a => a.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ──────────────────────────────────────
            // Comment configuration
            // ──────────────────────────────────────
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(c => c.Status)
                    .HasDefaultValue(CommentStatus.Published)
                    .HasConversion<int>();

                // Indexes (PRD §50)
                entity.HasIndex(c => c.ArticleId)
                    .HasDatabaseName("IX_Comments_ArticleId");

                entity.HasIndex(c => c.UserId)
                    .HasDatabaseName("IX_Comments_UserId");

                entity.HasIndex(c => c.CreatedAt)
                    .HasDatabaseName("IX_Comments_CreatedAt");

                // Relationships
                entity.HasOne(c => c.Article)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(c => c.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ──────────────────────────────────────
            // ArticleLike configuration
            // ──────────────────────────────────────
            builder.Entity<ArticleLike>(entity =>
            {
                entity.HasKey(al => al.Id);

                // Unique composite index (PRD §30, §49)
                entity.HasIndex(al => new { al.ArticleId, al.UserId })
                    .IsUnique()
                    .HasDatabaseName("UX_ArticleLikes_ArticleId_UserId");

                // Individual indexes (PRD §50)
                entity.HasIndex(al => al.ArticleId)
                    .HasDatabaseName("IX_ArticleLikes_ArticleId");

                entity.HasIndex(al => al.UserId)
                    .HasDatabaseName("IX_ArticleLikes_UserId");

                // Relationships
                entity.HasOne(al => al.Article)
                    .WithMany(a => a.Likes)
                    .HasForeignKey(al => al.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(al => al.User)
                    .WithMany(u => u.ArticleLikes)
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
