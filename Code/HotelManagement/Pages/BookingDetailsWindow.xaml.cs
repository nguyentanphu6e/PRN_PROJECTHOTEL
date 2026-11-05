using System.Windows;
using BusinessObjects;

namespace HotelManagement.Pages;

public partial class BookingDetailsWindow : Window
{
    public BookingDetailsWindow(Booking booking)
    {
        InitializeComponent();
        var nights = Math.Max(1, (booking.CheckOut.Date - booking.CheckIn.Date).Days);
        BookingCodeText.Text = booking.BookingCode;
        GuestNameText.Text = booking.GuestName ?? booking.Customer?.FullName ?? "—";
        GuestPhoneText.Text = string.IsNullOrWhiteSpace(booking.GuestPhone) ? "—" : booking.GuestPhone;
        EmailText.Text = booking.Customer?.Email ?? "—";
        RoomText.Text = $"{booking.Room?.RoomNumber} · {booking.Room?.RoomType?.Name}";
        CheckInText.Text = booking.CheckIn.ToString("dd/MM/yyyy");
        CheckOutText.Text = booking.CheckOut.ToString("dd/MM/yyyy");
        GuestCountText.Text = booking.GuestCount?.ToString() ?? "—";
        StatusText.Text = booking.Status switch
        {
            BookingStatus.Pending => "Chờ xác nhận",
            BookingStatus.Confirmed => "Đã xác nhận",
            BookingStatus.CheckedIn => "Đang lưu trú",
            BookingStatus.CheckedOut => "Đã trả phòng",
            _ => "Đã hủy"
        };
        NightlyRateText.Text = $"{booking.NightlyRateAtBooking:N0} ₫";
        NightsText.Text = nights.ToString();
        RoomTotalText.Text = $"{booking.NightlyRateAtBooking * nights:N0} ₫";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
