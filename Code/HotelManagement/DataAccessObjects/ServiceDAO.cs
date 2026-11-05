using System.Data;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class ServiceDAO(HotelDbContextFactory factory)
{
    public IReadOnlyList<HotelServiceItem> GetActive()
    {
        using var db = factory.CreateDbContext();
        return db.Services.Where(x => x.IsActive).OrderBy(x => x.Name).ToList();
    }

    public IReadOnlyList<ServiceOrder> GetOrdersForBooking(int bookingId)
    {
        using var db = factory.CreateDbContext();
        return db.ServiceOrders.Include(x => x.Service)
            .Where(x => x.BookingId == bookingId)
            .OrderByDescending(x => x.UsedAt)
            .ThenByDescending(x => x.ServiceOrderId)
            .ToList();
    }

    public ServiceOrder? AddOrder(ServiceOrder order)
    {
        using var db = factory.CreateDbContext();
        if (order.Quantity < 1) return null;
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var service = db.Services.SingleOrDefault(x => x.ServiceId == order.ServiceId && x.IsActive);
        var bookingExists = db.Bookings.Any(x => x.BookingId == order.BookingId && x.Status == BookingStatus.CheckedIn);
        var invoiceIsPaid = db.Invoices.Any(x => x.BookingId == order.BookingId && x.PaymentStatus == PaymentStatus.Paid);
        if (service is null || !bookingExists || invoiceIsPaid) return null;
        order.ServiceName = service.Name;
        order.UnitPriceAtOrder = service.UnitPrice;
        order.UsedAt = DateTime.Now;
        db.ServiceOrders.Add(order);
        db.SaveChanges();
        transaction.Commit();
        return order;
    }

    public bool CanManageOrders(int bookingId)
    {
        using var db = factory.CreateDbContext();
        return db.Bookings.Any(x => x.BookingId == bookingId && x.Status == BookingStatus.CheckedIn)
            && !db.Invoices.Any(x => x.BookingId == bookingId && x.PaymentStatus == PaymentStatus.Paid);
    }

    public bool RemoveOrder(int bookingId, int serviceOrderId)
    {
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var order = db.ServiceOrders.SingleOrDefault(x => x.ServiceOrderId == serviceOrderId && x.BookingId == bookingId);
        var bookingIsActive = db.Bookings.Any(x => x.BookingId == bookingId && x.Status == BookingStatus.CheckedIn);
        var isPaid = db.Invoices.Any(x => x.BookingId == bookingId && x.PaymentStatus == PaymentStatus.Paid);
        if (order is null || !bookingIsActive || isPaid
            || order.Status is not (ServiceOrderStatus.Pending or ServiceOrderStatus.Processing)) return false;
        db.ServiceOrders.Remove(order);
        db.SaveChanges();
        transaction.Commit();
        return true;
    }

    public void CompleteOpenOrders(int bookingId)
    {
        using var db = factory.CreateDbContext();
        var orders = db.ServiceOrders.Where(x => x.BookingId == bookingId
            && (x.Status == ServiceOrderStatus.Pending || x.Status == ServiceOrderStatus.Processing)).ToList();
        foreach (var order in orders) order.Status = ServiceOrderStatus.Completed;
        db.SaveChanges();
    }

    public decimal GetTotalForBooking(int bookingId)
    {
        using var db = factory.CreateDbContext();
        return db.ServiceOrders.Where(x => x.BookingId == bookingId && x.Status != ServiceOrderStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceAtOrder).Sum();
    }
}
