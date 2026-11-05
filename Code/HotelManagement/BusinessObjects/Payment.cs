namespace BusinessObjects;
public sealed class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
    public Invoice? Invoice { get; set; }
}
