namespace BusinessObjects;
public sealed class Room
{
    public int RoomId { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }
    public int Floor { get; set; }
    public RoomType? RoomType { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
