namespace BusinessObjects;

public static class BookingStatusRules
{
    public static readonly BookingStatus[] RoomHoldingStatuses =
    [
        BookingStatus.Pending,
        BookingStatus.Confirmed,
        BookingStatus.CheckedIn
    ];
}
