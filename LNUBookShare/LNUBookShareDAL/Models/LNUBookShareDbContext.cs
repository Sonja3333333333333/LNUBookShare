using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShareDAL.Models;

public partial class LNUBookShareDbContext : DbContext
{
    public LNUBookShareDbContext()
    {
    }

    public LNUBookShareDbContext(DbContextOptions<LNUBookShareDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Emailconfirmation> Emailconfirmations { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("book_status_enum", new[] { "available", "issued" })
            .HasPostgresEnum("image_type_enum", new[] { "book_cover", "avatar" });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("book_pkey");

            entity.ToTable("book");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Author)
                .HasMaxLength(100)
                .HasColumnName("author");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CoverId).HasColumnName("cover_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasColumnName("language");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Publisher)
                .HasMaxLength(100)
                .HasColumnName("publisher");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("book_category_id_fkey");

            entity.HasOne(d => d.Cover).WithMany(p => p.Books)
                .HasForeignKey(d => d.CoverId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("book_cover_id_fkey");

            entity.HasOne(d => d.Owner).WithMany(p => p.Books)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("book_owner_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Emailconfirmation>(entity =>
        {
            entity.HasKey(e => e.ConfirmationId).HasName("emailconfirmation_pkey");

            entity.ToTable("emailconfirmation");

            entity.HasIndex(e => e.UserId, "emailconfirmation_user_id_key").IsUnique();

            entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");
            entity.Property(e => e.ConfirmationToken)
                .HasMaxLength(100)
                .HasColumnName("confirmation_token");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expires_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Emailconfirmation)
                .HasForeignKey<Emailconfirmation>(d => d.UserId)
                .HasConstraintName("emailconfirmation_user_id_fkey");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyId).HasName("faculty_pkey");

            entity.ToTable("faculty");

            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("favorite_pkey");

            entity.ToTable("favorite");

            entity.HasIndex(e => new { e.UserId, e.BookId }, "unique_favorite").IsUnique();

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("favorite_book_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorite_user_id_fkey");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("image_pkey");

            entity.ToTable("image");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvatarId).HasColumnName("avatar_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsEmailConfirmed).HasColumnName("is_email_confirmed");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Avatar).WithMany(p => p.Users)
                .HasForeignKey(d => d.AvatarId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("User_avatar_id_fkey");

            entity.HasOne(d => d.Faculty).WithMany(p => p.Users)
                .HasForeignKey(d => d.FacultyId)
                .HasConstraintName("User_faculty_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
