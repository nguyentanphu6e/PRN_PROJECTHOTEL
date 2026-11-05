namespace BusinessObjects;
public sealed class RoomType
{
    public int RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
