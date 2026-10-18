using Microsoft.EntityFrameworkCore;
using Pickleball.Api.Models;

namespace Pickleball.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceBlock> MaintenanceBlocks => Set<MaintenanceBlock>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<RefreshToken>(e =>
        {
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Court>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.Property(x => x.SurfaceType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.HourlyRate).HasPrecision(10, 2);
        });

        b.Entity<Reservation>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalPrice).HasPrecision(10, 2);
            // Overlap prevention is enforced in BookingService with a Serializable transaction;
            // this index makes the overlap range scan cheap and lets it take narrow range locks.
            e.HasIndex(x => new { x.CourtId, x.StartTime, x.EndTime });
            e.HasOne(x => x.User).WithMany(u => u.Reservations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Court).WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(10, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Provider).HasMaxLength(50);
            e.Property(x => x.ProviderReference).HasMaxLength(100);
        });

        b.Entity<MaintenanceBlock>(e =>
        {
            e.Property(x => x.Reason).HasMaxLength(200);
            e.HasIndex(x => new { x.CourtId, x.StartTime, x.EndTime });
            e.HasOne(x => x.Court).WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
