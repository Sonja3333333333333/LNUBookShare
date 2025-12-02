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
        _ = modelBuilder
            .HasPostgresEnum("book_status_enum", new[] { "available", "issued" })
            .HasPostgresEnum("image_type_enum", new[] { "book_cover", "avatar" });

        _ = modelBuilder.Entity<Book>(entity =>
        {
            _ = entity.HasKey(e => e.BookId).HasName("book_pkey");

            _ = entity.ToTable("book");

            _ = entity.Property(e => e.BookId).HasColumnName("book_id");
            _ = entity.Property(e => e.Author)
                .HasMaxLength(100)
                .HasColumnName("author");
            _ = entity.Property(e => e.CategoryId).HasColumnName("category_id");
            _ = entity.Property(e => e.CoverId).HasColumnName("cover_id");
            _ = entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            _ = entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            _ = entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasColumnName("language");
            _ = entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            _ = entity.Property(e => e.Publisher)
                .HasMaxLength(100)
                .HasColumnName("publisher");
            _ = entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            _ = entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            _ = entity.Property(e => e.Year).HasColumnName("year");

            _ = entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("book_category_id_fkey");

            _ = entity.HasOne(d => d.Cover).WithMany(p => p.Books)
                .HasForeignKey(d => d.CoverId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("book_cover_id_fkey");

            _ = entity.HasOne(d => d.Owner).WithMany(p => p.Books)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("book_owner_id_fkey");
        });

        _ = modelBuilder.Entity<Category>(entity =>
        {
            _ = entity.HasKey(e => e.CategoryId).HasName("category_pkey");

            _ = entity.ToTable("category");

            _ = entity.Property(e => e.CategoryId).HasColumnName("category_id");
            _ = entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        _ = modelBuilder.Entity<Emailconfirmation>(entity =>
        {
            _ = entity.HasKey(e => e.ConfirmationId).HasName("emailconfirmation_pkey");

            _ = entity.ToTable("emailconfirmation");

            _ = entity.HasIndex(e => e.UserId, "emailconfirmation_user_id_key").IsUnique();

            _ = entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");
            _ = entity.Property(e => e.ConfirmationToken)
                .HasMaxLength(100)
                .HasColumnName("confirmation_token");
            _ = entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            _ = entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expires_at");
            _ = entity.Property(e => e.UserId).HasColumnName("user_id");

            _ = entity.HasOne(d => d.User).WithOne(p => p.Emailconfirmation)
                .HasForeignKey<Emailconfirmation>(d => d.UserId)
                .HasConstraintName("emailconfirmation_user_id_fkey");
        });

        _ = modelBuilder.Entity<Faculty>(entity =>
        {
            _ = entity.HasKey(e => e.FacultyId).HasName("faculty_pkey");

            _ = entity.ToTable("faculty");

            _ = entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            _ = entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        _ = modelBuilder.Entity<Favorite>(entity =>
        {
            _ = entity.HasKey(e => e.FavoriteId).HasName("favorite_pkey");

            _ = entity.ToTable("favorite");

            _ = entity.HasIndex(e => new { e.UserId, e.BookId }, "unique_favorite").IsUnique();

            _ = entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            _ = entity.Property(e => e.BookId).HasColumnName("book_id");
            _ = entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            _ = entity.Property(e => e.UserId).HasColumnName("user_id");

            _ = entity.HasOne(d => d.Book).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("favorite_book_id_fkey");

            _ = entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorite_user_id_fkey");
        });

        _ = modelBuilder.Entity<Image>(entity =>
        {
            _ = entity.HasKey(e => e.ImageId).HasName("image_pkey");

            _ = entity.ToTable("image");

            _ = entity.Property(e => e.ImageId).HasColumnName("image_id");
            _ = entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            _ = entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploaded_at");
        });

        _ = modelBuilder.Entity<User>(entity =>
        {
            _ = entity.HasKey(e => e.UserId).HasName("User_pkey");

            _ = entity.ToTable("User");

            _ = entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            _ = entity.Property(e => e.UserId).HasColumnName("user_id");
            _ = entity.Property(e => e.AvatarId).HasColumnName("avatar_id");
            _ = entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            _ = entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            _ = entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            _ = entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            _ = entity.Property(e => e.IsEmailConfirmed).HasColumnName("is_email_confirmed");
            _ = entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            _ = entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            _ = entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            _ = entity.HasOne(d => d.Avatar).WithMany(p => p.Users)
                .HasForeignKey(d => d.AvatarId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("User_avatar_id_fkey");

            _ = entity.HasOne(d => d.Faculty).WithMany(p => p.Users)
                .HasForeignKey(d => d.FacultyId)
                .HasConstraintName("User_faculty_id_fkey");
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
