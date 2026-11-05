using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class MyBookingsPage : Page
{
    private readonly User _customer;
    private readonly Action _searchRooms;
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();

    public MyBookingsPage(User customer, Action searchRooms)
    {
        InitializeComponent();
        _customer = customer;
        _searchRooms = searchRooms;
        RefreshBookings();
    }

    private void RefreshBookings()
    {
        var rows = _hotelService.GetBookingsForCustomer(_customer.UserId)
            .Where(booking => booking.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn)
            .Select(booking => new CustomerBookingRow(booking))
            .ToList();
        BookingsGrid.ItemsSource = rows;
        BookingCountText.Text = $"{rows.Count} đặt phòng đang hoạt động";
        BookingsGrid.Visibility = rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        EmptyStatePanel.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CancelBooking_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int bookingId }) return;
        var row = BookingsGrid.Items.OfType<CustomerBookingRow>().SingleOrDefault(item => item.BookingId == bookingId);
        if (row is null) return;

        if (MessageBox.Show($"Bạn muốn hủy đặt phòng {row.Code}? Sau khi hủy, booking sẽ được gỡ khỏi danh sách đặt phòng đang hoạt động.",
                "Xác nhận hủy đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        if (!_hotelService.CancelBooking(bookingId))
        {
            MessageBox.Show("Không thể hủy booking này. Booking đã có dịch vụ hoặc hóa đơn cần được xử lý tại quầy lễ tân.",
                "Không thể hủy", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RefreshBookings();
        MessageBox.Show("Đặt phòng đã được hủy. Tài khoản hiện không còn đặt phòng đang hoạt động.",
            "Đã hủy đặt phòng", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SearchRooms_Click(object sender, RoutedEventArgs e) => _searchRooms();

    private sealed record CustomerBookingRow(Booking Booking)
    {
        public int BookingId => Booking.BookingId;
        public string Code => Booking.BookingCode;
        public string Room => $"{Booking.Room?.RoomNumber} · {Booking.Room?.RoomType?.Name}";
        public string CheckIn => Booking.CheckIn.ToString("dd/MM/yyyy");
        public string CheckOut => Booking.CheckOut.ToString("dd/MM/yyyy");
        public string GuestCount => Booking.GuestCount?.ToString() ?? "—";
        public string TotalLabel => $"{Booking.NightlyRateAtBooking * Math.Max(1, (Booking.CheckOut.Date - Booking.CheckIn.Date).Days):N0} ₫";
        public string StatusLabel => Booking.Status switch
        {
            BookingStatus.Pending => "Chờ xác nhận",
            BookingStatus.Confirmed => "Đã xác nhận",
            _ => "Đang lưu trú"
        };
        public bool CanCancel => Booking.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn;
    }
}
