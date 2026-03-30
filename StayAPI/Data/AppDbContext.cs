using Microsoft.EntityFrameworkCore;
using StayAPI.Models;

namespace StayAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.HostUser)
                  .WithMany()
                  .HasForeignKey(e => e.HostUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NamesOfPeople).IsRequired();
            entity.HasOne(e => e.Listing)
                  .WithMany(l => l.Bookings)
                  .HasForeignKey(e => e.ListingId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.GuestUser)
                  .WithMany()
                  .HasForeignKey(e => e.GuestUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.HasIndex(e => e.BookingId).IsUnique();
            entity.HasOne(e => e.Booking)
                  .WithOne(b => b.Review)
                  .HasForeignKey<Review>(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Listing)
                  .WithMany(l => l.Reviews)
                  .HasForeignKey(e => e.ListingId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.GuestUser)
                  .WithMany()
                  .HasForeignKey(e => e.GuestUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
