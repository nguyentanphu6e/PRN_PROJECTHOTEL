namespace BusinessObjects;
public sealed class HotelServiceItem
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ServiceOrder> Orders { get; set; } = new List<ServiceOrder>();
}
