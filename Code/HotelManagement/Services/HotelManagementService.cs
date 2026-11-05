using BusinessObjects;
using Repositories;

namespace Services;

public sealed class HotelManagementService(IRoomRepository rooms, IBookingRepository bookings, IHotelServiceRepository services, IInvoiceRepository invoices, IUserRepository users)
{
    public IReadOnlyList<Room> SearchAvailableRooms(DateTime checkIn, DateTime checkOut, int guests)
    {
        if (checkIn.Date < DateTime.Today) throw new ArgumentException("Ngày nhận phòng không được ở quá khứ.");
        if (checkOut.Date <= checkIn.Date) throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng.");
        if (guests < 1) throw new ArgumentOutOfRangeException(nameof(guests));
        return rooms.GetAvailable(checkIn, checkOut, guests);
    }

    public IReadOnlyList<RoomType> GetRoomTypes() => rooms.GetRoomTypes();

    public RoomType SaveRoomType(int? roomTypeId, string name, int capacity, decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 80) throw new ArgumentException("Tên loại phòng không được để trống và tối đa 80 ký tự.");
        if (capacity < 1) throw new ArgumentException("Sức chứa phải từ 1 khách trở lên.");
        if (basePrice < 0) throw new ArgumentException("Giá phòng không được âm.");
        return rooms.SaveRoomType(roomTypeId, name, capacity, basePrice);
    }

    public Room SaveRoom(int? roomId, string roomNumber, int roomTypeId, int floor, RoomStatus status)
    {
        if (string.IsNullOrWhiteSpace(roomNumber) || roomNumber.Trim().Length > 20) throw new ArgumentException("Số phòng không được để trống và tối đa 20 ký tự.");
        if (floor < 0) throw new ArgumentException("Tầng không được âm.");
        return rooms.SaveRoom(roomId, roomNumber, roomTypeId, floor, status);
    }

    public IReadOnlyList<Room> FilterRooms(IEnumerable<Room> availableRooms, int? roomTypeId, RoomPriceBand priceBand, RoomPriceSort sort)
    {
        IEnumerable<Room> filtered = availableRooms;
        if (roomTypeId.HasValue)
            filtered = filtered.Where(room => room.RoomTypeId == roomTypeId.Value);

        filtered = priceBand switch
        {
            RoomPriceBand.Under500000 => filtered.Where(room => room.RoomType!.BasePrice < 500000m),
            RoomPriceBand.From500000To1000000 => filtered.Where(room => room.RoomType!.BasePrice >= 500000m && room.RoomType.BasePrice <= 1000000m),
            RoomPriceBand.From1000000To2000000 => filtered.Where(room => room.RoomType!.BasePrice > 1000000m && room.RoomType.BasePrice <= 2000000m),
            RoomPriceBand.Over2000000 => filtered.Where(room => room.RoomType!.BasePrice > 2000000m),
            _ => filtered
        };

        filtered = sort switch
        {
            RoomPriceSort.LowToHigh => filtered.OrderBy(room => room.RoomType!.BasePrice).ThenBy(room => room.RoomNumber),
            RoomPriceSort.HighToLow => filtered.OrderByDescending(room => room.RoomType!.BasePrice).ThenBy(room => room.RoomNumber),
            _ => filtered.OrderBy(room => room.RoomNumber)
        };
        return filtered.ToList();
    }

    public bool IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut, int guests) =>
        SearchAvailableRooms(checkIn, checkOut, guests).Any(room => room.RoomId == roomId);

    public Booking CreateBooking(int customerId, int roomId, DateTime checkIn, DateTime checkOut, string guestName, int guestCount, string? guestPhone = null)
    {
        if (string.IsNullOrWhiteSpace(guestName)) throw new ArgumentException("Vui lòng nhập họ tên khách.", nameof(guestName));
        if (guestCount < 1) throw new ArgumentOutOfRangeException(nameof(guestCount), "Số khách phải lớn hơn 0.");
        if (!IsRoomAvailable(roomId, checkIn, checkOut, guestCount))
            throw new InvalidOperationException("Phòng không còn phù hợp hoặc không đủ sức chứa cho số khách đã chọn.");
        var room = rooms.Find(roomId) ?? throw new InvalidOperationException("Không tìm thấy phòng.");
        var booking = bookings.CreateIfAvailable(new Booking
        {
            CustomerId = customerId,
            GuestName = guestName.Trim(),
            GuestPhone = string.IsNullOrWhiteSpace(guestPhone) ? null : guestPhone.Trim(),
            RoomId = roomId,
            GuestCount = guestCount,
            CheckIn = checkIn,
            CheckOut = checkOut,
            NightlyRateAtBooking = room.RoomType!.BasePrice,
            Status = BookingStatus.Pending
        });
        return booking ?? throw new InvalidOperationException("Phòng không còn trống trong thời gian đã chọn.");
    }

    public bool CheckInBooking(int bookingId) => bookings.ChangeStatus(bookingId, BookingStatus.CheckedIn);

    public bool CancelBooking(int bookingId) => bookings.Cancel(bookingId);

    public Invoice CheckOutBooking(int bookingId)
    {
        var booking = bookings.GetAll().SingleOrDefault(x => x.BookingId == bookingId)
            ?? throw new InvalidOperationException("Không tìm thấy đặt phòng.");
        if (booking.Status == BookingStatus.CheckedOut)
            return CreateInvoice(bookingId);
        if (booking.Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Chỉ có thể trả phòng sau khi khách đã check-in.");
        services.CompleteOpenOrders(bookingId);
        if (!bookings.ChangeStatus(bookingId, BookingStatus.CheckedOut))
            throw new InvalidOperationException("Không thể cập nhật trạng thái trả phòng.");
        return CreateInvoice(bookingId);
    }
    public IReadOnlyList<Booking> GetBookings() => bookings.GetAll();

    public IReadOnlyList<User> GetActiveCustomers() => users.GetActiveCustomers();

    public IReadOnlyList<User> GetAllUsers() => users.GetAll();

    public bool SetUserActive(int userId, bool isActive) => users.SetActive(userId, isActive);

    public IReadOnlyList<Room> GetRooms() => rooms.GetAll();

    public IReadOnlyList<Booking> GetCheckedInBookings() => bookings.GetCheckedIn();

    public IReadOnlyList<Booking> GetBookingsForCustomer(int customerId) => bookings.GetForCustomer(customerId);

    public Booking? GetCurrentBookingForCustomer(int customerId) => bookings.GetForCustomer(customerId).FirstOrDefault(x => x.Status == BookingStatus.CheckedIn);

    public IReadOnlyList<HotelServiceItem> GetAvailableServices() => services.GetActive();

    public decimal GetServiceTotal(int bookingId) => services.GetTotalForBooking(bookingId);

    public IReadOnlyList<ServiceOrder> GetServiceOrdersForBooking(int bookingId) => services.GetOrdersForBooking(bookingId);

    public bool CanManageServiceOrders(int bookingId) => services.CanManageOrders(bookingId);

    public bool RemoveServiceOrder(int bookingId, int serviceOrderId) => services.RemoveOrder(bookingId, serviceOrderId);

    public ServiceOrder RequestService(int bookingId, int serviceId, int quantity)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        var item = services.GetActive().SingleOrDefault(x => x.ServiceId == serviceId)
            ?? throw new InvalidOperationException("Dịch vụ hiện không khả dụng.");
        var order = services.CreateOrder(new ServiceOrder
        {
            BookingId = bookingId,
            ServiceId = item.ServiceId,
            Quantity = quantity,
            UnitPriceAtOrder = item.UnitPrice,
            UsedAt = DateTime.Now,
            Status = ServiceOrderStatus.Pending
        });
        return order ?? throw new InvalidOperationException("Chỉ có thể gọi dịch vụ khi đang lưu trú.");
    }

    public Invoice CreateInvoice(int bookingId)
    {
        var booking = bookings.GetAll().SingleOrDefault(x => x.BookingId == bookingId)
            ?? throw new InvalidOperationException("Không tìm thấy đặt phòng.");
        var nights = Math.Max(1, (booking.CheckOut.Date - booking.CheckIn.Date).Days);
        return invoices.Create(new Invoice
        {
            BookingId = bookingId,
            RoomTotal = booking.NightlyRateAtBooking * nights,
            ServiceTotal = services.GetTotalForBooking(bookingId),
            PaymentStatus = PaymentStatus.Unpaid
        });
    }
}





