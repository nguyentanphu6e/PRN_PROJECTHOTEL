using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class ReceptionPage : Page
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private readonly List<ReservationRow> _allRows = [];
    private bool _pageReady;

    public ReceptionPage()
    {
        InitializeComponent();
        TodayText.Text = DateTime.Now.ToString("dddd, dd 'tháng' MM 'năm' yyyy").ToUpperInvariant();
        BookingStatusFilter.ItemsSource = new[]
        {
            new BookingStatusFilterOption("Tất cả trạng thái", null),
            new BookingStatusFilterOption("Chờ xác nhận", BookingStatus.Pending),
            new BookingStatusFilterOption("Đã xác nhận", BookingStatus.Confirmed),
            new BookingStatusFilterOption("Đang lưu trú", BookingStatus.CheckedIn),
            new BookingStatusFilterOption("Đã trả phòng", BookingStatus.CheckedOut),
            new BookingStatusFilterOption("Đã hủy", BookingStatus.Cancelled)
        };
        BookingStatusFilter.SelectedIndex = 0;
        _pageReady = true;
        RefreshReservations();
    }

    private void RefreshReservations()
    {
        var bookings = _hotelService.GetBookings();
        _allRows.Clear();
        _allRows.AddRange(bookings.Select(booking => new ReservationRow(booking)));
        ArrivalsTodayText.Text = bookings.Count(booking => booking.CheckIn.Date == DateTime.Today
            && booking.Status is BookingStatus.Pending or BookingStatus.Confirmed).ToString("00");
        DeparturesTodayText.Text = bookings.Count(booking => booking.CheckOut.Date == DateTime.Today
            && booking.Status == BookingStatus.CheckedIn).ToString("00");
        CheckedInCountText.Text = bookings.Count(booking => booking.Status == BookingStatus.CheckedIn).ToString("00");
        PendingCountText.Text = bookings.Count(booking => booking.Status == BookingStatus.Pending).ToString("00");
        ApplyFilters();
    }

    private void BookingSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_pageReady) ApplyFilters();
    }

    private void BookingStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_pageReady) ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = BookingSearchBox.Text.Trim();
        var status = (BookingStatusFilter.SelectedItem as BookingStatusFilterOption)?.Status;
        var rows = _allRows.Where(row => (!status.HasValue || row.Booking.Status == status.Value)
                && (query.Length == 0
                    || row.Code.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || row.Guest.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || row.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        ReservationsGrid.ItemsSource = rows;
        BookingSummaryText.Text = $"Hiển thị {rows.Count} trong { _allRows.Count} booking";
    }

    private void CreateBooking_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new StaffBookingWindow { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true) RefreshReservations();
    }

    private void Details_Click(object sender, RoutedEventArgs e)
    {
        if (ReservationsGrid.SelectedItem is not ReservationRow row)
        {
            MessageBox.Show("Chọn một booking để xem chi tiết.", "Chi tiết booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        new BookingDetailsWindow(row.Booking) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void CheckIn_Click(object sender, RoutedEventArgs e)
    {
        if (ReservationsGrid.SelectedItem is not ReservationRow row)
        {
            MessageBox.Show("Chọn một booking trước khi check-in.", "Check-in", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!_hotelService.CheckInBooking(row.BookingId))
        {
            MessageBox.Show("Chỉ booking Pending hoặc Confirmed mới được check-in.", "Không thể check-in", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        RefreshReservations();
    }

    private void CancelBooking_Click(object sender, RoutedEventArgs e)
    {
        if (ReservationsGrid.SelectedItem is not ReservationRow row)
        {
            MessageBox.Show("Chọn một booking trước khi hủy.", "Hủy booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show($"Hủy booking {row.Code} của {row.Guest}?", "Xác nhận hủy booking", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        if (!_hotelService.CancelBooking(row.BookingId))
        {
            MessageBox.Show("Booking đã trả phòng, có dịch vụ chưa hủy hoặc đã có hóa đơn nên không thể hủy.", "Không thể hủy", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RefreshReservations();
    }

    private void CheckOut_Click(object sender, RoutedEventArgs e)
    {
        if (ReservationsGrid.SelectedItem is not ReservationRow row)
        {
            MessageBox.Show("Chọn một booking trước khi check-out.", "Check-out", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            var invoice = _hotelService.CheckOutBooking(row.BookingId);
            MessageBox.Show($"Đã lập hóa đơn #{invoice.InvoiceId}.\nTiền phòng: {invoice.RoomTotal:N0} ₫\nDịch vụ: {invoice.ServiceTotal:N0} ₫\nTổng thanh toán: {invoice.GrandTotal:N0} ₫.", "Check-out hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshReservations();
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Không thể check-out", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public sealed record BookingStatusFilterOption(string Label, BookingStatus? Status);

    public sealed record ReservationRow(Booking Booking)
    {
        public int BookingId => Booking.BookingId;
        public string Code => Booking.BookingCode;
        public string Guest => Booking.GuestName ?? Booking.Customer?.FullName ?? string.Empty;
        public string Phone => string.IsNullOrWhiteSpace(Booking.GuestPhone) ? "—" : Booking.GuestPhone;
        public string Room => $"{Booking.Room?.RoomNumber} · {Booking.Room?.RoomType?.Name}";
        public string CheckIn => Booking.CheckIn.ToString("dd/MM/yyyy");
        public string CheckOut => Booking.CheckOut.ToString("dd/MM/yyyy");
        public string Guests => Booking.GuestCount?.ToString() ?? "—";
        public decimal RoomTotal => Booking.NightlyRateAtBooking * Math.Max(1, (Booking.CheckOut.Date - Booking.CheckIn.Date).Days);
        public string RoomTotalLabel => $"{RoomTotal:N0} ₫";
        public string StatusLabel => Booking.Status switch
        {
            BookingStatus.Pending => "Chờ xác nhận",
            BookingStatus.Confirmed => "Đã xác nhận",
            BookingStatus.CheckedIn => "Đang lưu trú",
            BookingStatus.CheckedOut => "Đã trả phòng",
            _ => "Đã hủy"
        };
    }
}
