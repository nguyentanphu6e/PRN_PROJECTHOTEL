using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<HotelServiceItem> Services => Set<HotelServiceItem>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(24);
            entity.HasMany(x => x.Bookings).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(x => x.RoomTypeId);
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.BasePrice).HasPrecision(18, 2);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(x => x.RoomId);
            entity.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.RoomNumber).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.HasOne(x => x.RoomType).WithMany(x => x.Rooms).HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.BookingId);
            entity.Property(x => x.BookingCode).HasMaxLength(24).IsRequired();
            entity.HasIndex(x => x.BookingCode).IsUnique();
            entity.HasIndex(x => new { x.RoomId, x.CheckIn, x.CheckOut });
            entity.Property(x => x.NightlyRateAtBooking).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.GuestName).HasMaxLength(120);
            entity.Property(x => x.GuestPhone).HasMaxLength(30);
            entity.Property(x => x.GuestCount);
            entity.HasOne(x => x.Room).WithMany(x => x.Bookings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Invoice).WithOne(x => x.Booking).HasForeignKey<Invoice>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HotelServiceItem>(entity =>
        {
            entity.ToTable("Services");
            entity.HasKey(x => x.ServiceId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(x => x.ServiceOrderId);
            entity.Property(x => x.UnitPriceAtOrder).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ServiceName).HasMaxLength(100);
            entity.Property(x => x.UsedAt);
            entity.HasOne(x => x.Booking).WithMany(x => x.ServiceOrders).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Service).WithMany(x => x.Orders).HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(x => x.InvoiceId);
            entity.Property(x => x.RoomTotal).HasPrecision(18, 2);
            entity.Property(x => x.ServiceTotal).HasPrecision(18, 2);
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(24);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Method).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
