using System.ComponentModel.DataAnnotations.Schema;
namespace BusinessObjects;
public sealed class Invoice
{
    public int InvoiceId { get; set; }
    public int BookingId { get; set; }
    public decimal RoomTotal { get; set; }
    public decimal ServiceTotal { get; set; }
    [NotMapped]
    public decimal GrandTotal => RoomTotal + ServiceTotal;
    public PaymentStatus PaymentStatus { get; set; }
    public Booking? Booking { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
