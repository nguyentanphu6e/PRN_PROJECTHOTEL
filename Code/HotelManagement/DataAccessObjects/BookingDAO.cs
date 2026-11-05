using System.Data;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class BookingDAO(HotelDbContextFactory factory)
{
    public IReadOnlyList<Booking> GetAll()
    {
        using var db = factory.CreateDbContext();
        return db.Bookings.Include(x => x.Customer).Include(x => x.Room).ThenInclude(x => x!.RoomType)
            .OrderBy(x => x.CheckIn).ToList();
    }

    public IReadOnlyList<Booking> GetCheckedIn()
    {
        using var db = factory.CreateDbContext();
        return db.Bookings.Where(x => x.Status == BookingStatus.CheckedIn)
            .Include(x => x.Customer)
            .Include(x => x.Room).ThenInclude(x => x!.RoomType)
            .OrderBy(x => x.BookingCode)
            .ToList();
    }

    public IReadOnlyList<Booking> GetForCustomer(int customerId)
    {
        using var db = factory.CreateDbContext();
        return db.Bookings.Include(x => x.Room).ThenInclude(x => x!.RoomType)
            .Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CheckIn).ToList();
    }

    public Booking? CreateIfAvailable(Booking booking)
    {
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var roomHoldingStatuses = BookingStatusRules.RoomHoldingStatuses;
        var overlaps = db.Bookings.Any(existing => existing.RoomId == booking.RoomId
            && roomHoldingStatuses.Contains(existing.Status)
            && booking.CheckIn < existing.CheckOut
            && booking.CheckOut > existing.CheckIn);
        var room = db.Rooms.Include(x => x.RoomType).SingleOrDefault(x => x.RoomId == booking.RoomId);
        if (overlaps || room is null || room.Status == RoomStatus.Maintenance
            || booking.GuestCount is null || room.RoomType!.Capacity < booking.GuestCount.Value) return null;
        booking.BookingCode = $"TY-{DateTime.Now:yyMMddHHmmss}-{Random.Shared.Next(100, 1000)}";
        booking.NightlyRateAtBooking = room.RoomType!.BasePrice;
        db.Bookings.Add(booking);
        db.SaveChanges();
        transaction.Commit();
        return booking;
    }

    public bool ChangeStatus(int id, BookingStatus status)
    {
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction();
        var booking = db.Bookings.Include(x => x.Room).SingleOrDefault(x => x.BookingId == id);
        if (booking is null) return false;
        if (status == BookingStatus.CheckedIn && booking.Status is not (BookingStatus.Confirmed or BookingStatus.Pending)) return false;
        if (status == BookingStatus.CheckedOut && booking.Status != BookingStatus.CheckedIn) return false;
        booking.Status = status;
        if (booking.Room is not null)
            booking.Room.Status = status == BookingStatus.CheckedIn ? RoomStatus.Occupied : RoomStatus.Cleaning;
        db.SaveChanges();
        transaction.Commit();
        return true;
    }

    public bool Cancel(int id)
    {
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var booking = db.Bookings.Include(x => x.Room).SingleOrDefault(x => x.BookingId == id);
        if (booking is null || booking.Status is BookingStatus.Cancelled or BookingStatus.CheckedOut) return false;
        var hasBillableServices = db.ServiceOrders.Any(x => x.BookingId == id && x.Status != ServiceOrderStatus.Cancelled);
        var hasInvoice = db.Invoices.Any(x => x.BookingId == id);
        if (hasBillableServices || hasInvoice) return false;
        if (booking.Status == BookingStatus.CheckedIn && booking.Room is not null && booking.Room.Status != RoomStatus.Maintenance)
            booking.Room.Status = RoomStatus.Available;
        booking.Status = BookingStatus.Cancelled;
        db.SaveChanges();
        transaction.Commit();
        return true;
    }
}
