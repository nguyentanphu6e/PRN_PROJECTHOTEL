using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class InvoiceDAO(HotelDbContextFactory factory)
{
    public Invoice Create(Invoice invoice)
    {
        using var db = factory.CreateDbContext();
        var existing = db.Invoices.SingleOrDefault(x => x.BookingId == invoice.BookingId);
        if (existing is not null) return existing;
        db.Invoices.Add(invoice);
        db.SaveChanges();
        return invoice;
    }

    public Invoice? FindByBooking(int bookingId)
    {
        using var db = factory.CreateDbContext();
        return db.Invoices.Include(x => x.Payments).SingleOrDefault(x => x.BookingId == bookingId);
    }
}
