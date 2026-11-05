using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects;
public sealed class ServiceOrder
{
    public int ServiceOrderId { get; set; }
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtOrder { get; set; }
    public DateTime? UsedAt { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public string? ServiceName { get; set; }
    [NotMapped]
    public decimal TotalPrice => Quantity * UnitPriceAtOrder;
    public Booking? Booking { get; set; }
    public HotelServiceItem? Service { get; set; }
}
