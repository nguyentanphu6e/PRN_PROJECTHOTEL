using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public static class HotelDatabaseInitializer
{
    public static void Initialize(HotelDbContextFactory factory)
    {
        using var db = factory.CreateDbContext();
        db.Database.Migrate();

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User { FullName = "Nguyễn Minh Anh", Email = "khachhang@ks.com", PasswordHash = PasswordHashing.Hash("123456"), Role = UserRole.Customer },
                new User { FullName = "Lê Thu Hà", Email = "letan@ks.com", PasswordHash = PasswordHashing.Hash("123456"), Role = UserRole.Receptionist },
                new User { FullName = "Quản trị viên", Email = "admin@ks.com", PasswordHash = PasswordHashing.Hash("123456"), Role = UserRole.Administrator });
        }

        if (!db.RoomTypes.Any())
        {
            db.RoomTypes.AddRange(
                new RoomType { Name = "Superior Double", Capacity = 2, BasePrice = 1250000m },
                new RoomType { Name = "Deluxe Twin", Capacity = 2, BasePrice = 1680000m },
                new RoomType { Name = "Family Suite", Capacity = 4, BasePrice = 2450000m });
        }

        if (!db.Services.Any())
        {
            db.Services.AddRange(
                new HotelServiceItem { Name = "Giặt ủi thường", UnitPrice = 35000m },
                new HotelServiceItem { Name = "Ăn sáng tại phòng", UnitPrice = 120000m },
                new HotelServiceItem { Name = "Nước suối", UnitPrice = 15000m });
        }
        db.SaveChanges();

        if (!db.Rooms.Any())
        {
            var roomTypes = db.RoomTypes.ToDictionary(x => x.Name, x => x.RoomTypeId);
            db.Rooms.AddRange(
                new Room { RoomNumber = "108", Floor = 1, RoomTypeId = roomTypes["Superior Double"], Status = RoomStatus.Available },
                new Room { RoomNumber = "212", Floor = 2, RoomTypeId = roomTypes["Superior Double"], Status = RoomStatus.Available },
                new Room { RoomNumber = "305", Floor = 3, RoomTypeId = roomTypes["Deluxe Twin"], Status = RoomStatus.Available },
                new Room { RoomNumber = "401", Floor = 4, RoomTypeId = roomTypes["Family Suite"], Status = RoomStatus.Maintenance });
            db.SaveChanges();
        }

        if (db.Bookings.Any()) return;
        var customer = db.Users.First(x => x.Role == UserRole.Customer);
        var roomsByNumber = db.Rooms.ToDictionary(x => x.RoomNumber, x => x.RoomId);
        var servicesByName = db.Services.ToDictionary(x => x.Name, x => x.ServiceId);
        var today = DateTime.Today;
        db.Bookings.AddRange(
            new Booking { BookingCode = "TY-DEMO-001", CustomerId = customer.UserId, RoomId = roomsByNumber["305"], GuestName = "Nguyễn Minh Anh", CheckIn = today.AddDays(2), CheckOut = today.AddDays(4), NightlyRateAtBooking = 1680000m, Status = BookingStatus.Confirmed },
            new Booking { BookingCode = "TY-DEMO-002", CustomerId = customer.UserId, RoomId = roomsByNumber["212"], GuestName = "Trần Quốc Bảo", CheckIn = today.AddDays(-1), CheckOut = today.AddDays(2), NightlyRateAtBooking = 1250000m, Status = BookingStatus.CheckedIn },
            new Booking { BookingCode = "TY-DEMO-003", CustomerId = customer.UserId, RoomId = roomsByNumber["108"], GuestName = "Lê Thu Hà", CheckIn = today.AddDays(1), CheckOut = today.AddDays(2), NightlyRateAtBooking = 1250000m, Status = BookingStatus.Pending });
        db.SaveChanges();

        var activeBookingId = db.Bookings.Single(x => x.BookingCode == "TY-DEMO-002").BookingId;
        db.ServiceOrders.AddRange(
            new ServiceOrder { BookingId = activeBookingId, ServiceId = servicesByName["Giặt ủi thường"], ServiceName = "Giặt ủi thường", Quantity = 2, UnitPriceAtOrder = 35000m, Status = ServiceOrderStatus.Processing },
            new ServiceOrder { BookingId = activeBookingId, ServiceId = servicesByName["Ăn sáng tại phòng"], ServiceName = "Ăn sáng tại phòng", Quantity = 2, UnitPriceAtOrder = 120000m, Status = ServiceOrderStatus.Completed });
        db.SaveChanges();
    }
}
