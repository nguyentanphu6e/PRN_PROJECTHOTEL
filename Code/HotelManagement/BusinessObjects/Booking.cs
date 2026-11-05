namespace BusinessObjects;
public sealed class Booking
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public int? GuestCount { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal NightlyRateAtBooking { get; set; }
    public BookingStatus Status { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public User? Customer { get; set; }
    public Room? Room { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    public Invoice? Invoice { get; set; }
}
