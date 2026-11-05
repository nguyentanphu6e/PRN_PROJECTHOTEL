using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class RoomDAO(HotelDbContextFactory factory)
{
    public IReadOnlyList<Room> GetAll()
    {
        using var db = factory.CreateDbContext();
        return db.Rooms.Include(x => x.RoomType).OrderBy(x => x.RoomNumber).ToList();
    }

    public IReadOnlyList<RoomType> GetRoomTypes()
    {
        using var db = factory.CreateDbContext();
        return db.RoomTypes.OrderBy(x => x.Name).ToList();
    }

    public RoomType SaveRoomType(int? roomTypeId, string name, int capacity, decimal basePrice)
    {
        using var db = factory.CreateDbContext();
        var normalizedName = name.Trim();
        if (db.RoomTypes.Any(x => x.Name == normalizedName && (!roomTypeId.HasValue || x.RoomTypeId != roomTypeId.Value)))
            throw new InvalidOperationException("Tên loại phòng đã tồn tại.");

        var roomType = roomTypeId.HasValue
            ? db.RoomTypes.SingleOrDefault(x => x.RoomTypeId == roomTypeId.Value)
                ?? throw new InvalidOperationException("Không tìm thấy loại phòng.")
            : new RoomType();
        roomType.Name = normalizedName;
        roomType.Capacity = capacity;
        roomType.BasePrice = basePrice;
        if (!roomTypeId.HasValue) db.RoomTypes.Add(roomType);
        db.SaveChanges();
        return roomType;
    }

    public Room SaveRoom(int? roomId, string roomNumber, int roomTypeId, int floor, RoomStatus status)
    {
        using var db = factory.CreateDbContext();
        var normalizedNumber = roomNumber.Trim();
        if (db.Rooms.Any(x => x.RoomNumber == normalizedNumber && (!roomId.HasValue || x.RoomId != roomId.Value)))
            throw new InvalidOperationException("Số phòng đã tồn tại.");
        if (!db.RoomTypes.Any(x => x.RoomTypeId == roomTypeId))
            throw new InvalidOperationException("Vui lòng chọn loại phòng hợp lệ.");

        var room = roomId.HasValue
            ? db.Rooms.SingleOrDefault(x => x.RoomId == roomId.Value)
                ?? throw new InvalidOperationException("Không tìm thấy phòng.")
            : new Room();
        var hasCheckedInGuest = roomId.HasValue && db.Bookings.Any(x => x.RoomId == roomId.Value && x.Status == BookingStatus.CheckedIn);
        if (hasCheckedInGuest && status != RoomStatus.Occupied)
            throw new InvalidOperationException("Phòng đang có khách lưu trú nên không thể đổi tình trạng.");

        room.RoomNumber = normalizedNumber;
        room.RoomTypeId = roomTypeId;
        room.Floor = floor;
        room.Status = hasCheckedInGuest ? RoomStatus.Occupied : status;
        if (!roomId.HasValue) db.Rooms.Add(room);
        db.SaveChanges();
        return room;
    }

    public Room? Find(int id)
    {
        using var db = factory.CreateDbContext();
        return db.Rooms.Include(x => x.RoomType).SingleOrDefault(x => x.RoomId == id);
    }

    public IReadOnlyList<Room> GetAvailable(DateTime checkIn, DateTime checkOut, int guests)
    {
        using var db = factory.CreateDbContext();
        var roomHoldingStatuses = BookingStatusRules.RoomHoldingStatuses;
        return db.Rooms.Include(x => x.RoomType)
            .Where(room => room.Status != RoomStatus.Maintenance && room.RoomType!.Capacity >= guests)
            .Where(room => !db.Bookings.Any(booking => booking.RoomId == room.RoomId
                && roomHoldingStatuses.Contains(booking.Status)
                && checkIn < booking.CheckOut
                && checkOut > booking.CheckIn))
            .OrderBy(room => room.RoomNumber)
            .ToList();
    }
}
