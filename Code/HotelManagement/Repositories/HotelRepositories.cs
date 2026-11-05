using BusinessObjects;
using DataAccessObjects;

namespace Repositories;

public interface IRoomRepository
{
    IReadOnlyList<Room> GetAll();
    IReadOnlyList<RoomType> GetRoomTypes();
    IReadOnlyList<Room> GetAvailable(DateTime checkIn, DateTime checkOut, int guests);
    Room? Find(int id);
    RoomType SaveRoomType(int? id, string name, int capacity, decimal basePrice);
    Room SaveRoom(int? id, string roomNumber, int roomTypeId, int floor, RoomStatus status);
}

public interface IBookingRepository
{
    IReadOnlyList<Booking> GetAll();
    IReadOnlyList<Booking> GetCheckedIn();
    IReadOnlyList<Booking> GetForCustomer(int customerId);
    Booking? CreateIfAvailable(Booking booking);
    bool ChangeStatus(int id, BookingStatus status);
    bool Cancel(int id);
}

public interface IHotelServiceRepository
{
    IReadOnlyList<HotelServiceItem> GetActive();
    IReadOnlyList<ServiceOrder> GetOrdersForBooking(int bookingId);
    ServiceOrder? CreateOrder(ServiceOrder order);
    bool CanManageOrders(int bookingId);
    bool RemoveOrder(int bookingId, int serviceOrderId);
    void CompleteOpenOrders(int bookingId);
    decimal GetTotalForBooking(int bookingId);
}

public interface IInvoiceRepository
{
    Invoice Create(Invoice invoice);
    Invoice? FindByBooking(int bookingId);
}

public interface IUserRepository
{
    User? Authenticate(string email, string password);
    IReadOnlyList<User> GetActiveCustomers();
    IReadOnlyList<User> GetAll();
    bool SetActive(int userId, bool isActive);
}

public sealed class RoomRepository(RoomDAO dao) : IRoomRepository
{
    public IReadOnlyList<Room> GetAll() => dao.GetAll();
    public IReadOnlyList<RoomType> GetRoomTypes() => dao.GetRoomTypes();
    public IReadOnlyList<Room> GetAvailable(DateTime checkIn, DateTime checkOut, int guests) => dao.GetAvailable(checkIn, checkOut, guests);
    public Room? Find(int id) => dao.Find(id);
    public RoomType SaveRoomType(int? id, string name, int capacity, decimal basePrice) => dao.SaveRoomType(id, name, capacity, basePrice);
    public Room SaveRoom(int? id, string roomNumber, int roomTypeId, int floor, RoomStatus status) => dao.SaveRoom(id, roomNumber, roomTypeId, floor, status);
}

public sealed class BookingRepository(BookingDAO dao) : IBookingRepository
{
    public IReadOnlyList<Booking> GetAll() => dao.GetAll();
    public IReadOnlyList<Booking> GetCheckedIn() => dao.GetCheckedIn();
    public IReadOnlyList<Booking> GetForCustomer(int customerId) => dao.GetForCustomer(customerId);
    public Booking? CreateIfAvailable(Booking booking) => dao.CreateIfAvailable(booking);
    public bool ChangeStatus(int id, BookingStatus status) => dao.ChangeStatus(id, status);
    public bool Cancel(int id) => dao.Cancel(id);
}

public sealed class HotelServiceRepository(ServiceDAO dao) : IHotelServiceRepository
{
    public IReadOnlyList<HotelServiceItem> GetActive() => dao.GetActive();
    public IReadOnlyList<ServiceOrder> GetOrdersForBooking(int bookingId) => dao.GetOrdersForBooking(bookingId);
    public ServiceOrder? CreateOrder(ServiceOrder order) => dao.AddOrder(order);
    public bool CanManageOrders(int bookingId) => dao.CanManageOrders(bookingId);
    public bool RemoveOrder(int bookingId, int serviceOrderId) => dao.RemoveOrder(bookingId, serviceOrderId);
    public void CompleteOpenOrders(int bookingId) => dao.CompleteOpenOrders(bookingId);
    public decimal GetTotalForBooking(int bookingId) => dao.GetTotalForBooking(bookingId);
}

public sealed class InvoiceRepository(InvoiceDAO dao) : IInvoiceRepository
{
    public Invoice Create(Invoice invoice) => dao.Create(invoice);
    public Invoice? FindByBooking(int bookingId) => dao.FindByBooking(bookingId);
}

public sealed class UserRepository(UserDAO dao) : IUserRepository
{
    public User? Authenticate(string email, string password) => dao.Authenticate(email, password);
    public IReadOnlyList<User> GetActiveCustomers() => dao.GetActiveCustomers();
    public IReadOnlyList<User> GetAll() => dao.GetAll();
    public bool SetActive(int userId, bool isActive) => dao.SetActive(userId, isActive);
}
