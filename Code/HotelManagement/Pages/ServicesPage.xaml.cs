using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class ServicesPage : Page
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private readonly bool _isStaff;
    private readonly List<BookingSelectionRow> _checkedInBookings = [];
    private Booking? _currentBooking;
    private bool _canManageOrders;
    private bool _updatingBookingRows;
    private bool _requiresBookingSelection;

    public ServicesPage(User currentUser)
    {
        InitializeComponent();
        _isStaff = currentUser.Role != UserRole.Customer;
        AudienceHeaderText.Text = _isStaff ? "LỄ TÂN  ›  DỊCH VỤ LƯU TRÚ" : "KHÁCH HÀNG  ›  DỊCH VỤ LƯU TRÚ";
        ServiceCombo.ItemsSource = _hotelService.GetAvailableServices();
        if (ServiceCombo.Items.Count > 0) ServiceCombo.SelectedIndex = 0;
        UpdateServiceAmount();

        if (_isStaff)
        {
            BookingSelectorPanel.Visibility = Visibility.Visible;
            BookingSelectorTitleText.Text = "Booking đang lưu trú";
            _checkedInBookings.AddRange(_hotelService.GetCheckedInBookings().Select(booking => new BookingSelectionRow(booking)));
            ApplyBookingSearch();
        }
        else
        {
            var customerBookings = _hotelService.GetBookingsForCustomer(currentUser.UserId)
                .Where(booking => booking.Status == BookingStatus.CheckedIn)
                .ToList();
            if (customerBookings.Count > 1)
            {
                _requiresBookingSelection = true;
                BookingSelectorPanel.Visibility = Visibility.Visible;
                BookingSelectorTitleText.Text = "Chọn phòng đang lưu trú";
                _checkedInBookings.AddRange(customerBookings.Select(booking => new BookingSelectionRow(booking)));
                ApplyBookingSearch();
            }
            else
            {
                UpdateSelectedBooking(customerBookings.SingleOrDefault());
            }
        }
    }

    private void BookingSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyBookingSearch();

    private void ApplyBookingSearch()
    {
        if (CheckedInBookingsGrid is null) return;
        var query = BookingSearchBox.Text.Trim();
        var rows = _checkedInBookings.Where(row => query.Length == 0
                || row.Code.Contains(query, StringComparison.OrdinalIgnoreCase)
                || row.GuestName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || row.RoomNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _updatingBookingRows = true;
        CheckedInBookingsGrid.ItemsSource = rows;
        CheckedInBookingsGrid.SelectedItem = null;
        _updatingBookingRows = false;
        NoBookingsText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        UpdateSelectedBooking(null);
    }

    private void CheckedInBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingBookingRows) return;
        var row = (sender as DataGrid)?.SelectedItem as BookingSelectionRow;
        UpdateSelectedBooking(row?.Booking);
    }

    private void UpdateSelectedBooking(Booking? booking)
    {
        _currentBooking = booking;
        if (_currentBooking is null)
        {
            BookingCodeText.Text = _requiresBookingSelection ? "Chưa chọn phòng" : _isStaff ? "Chưa chọn booking" : "Chưa có đặt phòng đang lưu trú";
            RoomStayText.Text = _requiresBookingSelection
                ? "Chọn phòng đang lưu trú để xem và gọi dịch vụ."
                : _isStaff
                    ? "Chọn một booking CheckedIn trong danh sách để xem và thêm dịch vụ."
                    : "Dịch vụ chỉ khả dụng sau khi lễ tân check-in.";
            StayStatusBadge.Visibility = Visibility.Collapsed;
            _canManageOrders = false;
            ServiceOrdersGrid.ItemsSource = null;
            ServiceTotalText.Text = "0 ₫";
            NoServiceOrdersText.Visibility = Visibility.Collapsed;
            ServiceMessageText.Text = _requiresBookingSelection
                ? "Chọn phòng trước khi gọi dịch vụ."
                : _isStaff ? "Chọn một booking CheckedIn để bắt đầu." : "Chưa có booking đang lưu trú.";
            AddServiceButton.IsEnabled = false;
            return;
        }

        BookingCodeText.Text = $"BOOKING {_currentBooking.BookingCode}";
        RoomStayText.Text = $"Khách {_currentBooking.GuestName ?? _currentBooking.Customer?.FullName} · Phòng {_currentBooking.Room?.RoomNumber} · {_currentBooking.CheckIn:dd/MM/yyyy}–{_currentBooking.CheckOut:dd/MM/yyyy}";
        StayStatusBadge.Visibility = Visibility.Visible;
        _canManageOrders = _hotelService.CanManageServiceOrders(_currentBooking.BookingId);
        ServiceMessageText.Text = _canManageOrders
            ? "Chỉ booking CheckedIn mới được thêm dịch vụ."
            : "Booking này đã khóa dịch vụ hoặc hoàn tất thanh toán.";
        UpdateServiceActionState();
        RefreshServiceOrders();
    }

    private void RefreshServiceOrders()
    {
        if (_currentBooking is null) return;
        var orders = _hotelService.GetServiceOrdersForBooking(_currentBooking.BookingId);
        ServiceOrdersGrid.ItemsSource = orders.Select(order => new ServiceOrderRow(
            order.ServiceOrderId,
            order.ServiceName ?? order.Service?.Name ?? string.Empty,
            order.Quantity,
            order.UnitPriceAtOrder,
            order.TotalPrice,
            order.UsedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—",
            order.Status.ToString(),
            _isStaff && _canManageOrders && order.Status is ServiceOrderStatus.Pending or ServiceOrderStatus.Processing)).ToList();
        ServiceTotalText.Text = $"{_hotelService.GetServiceTotal(_currentBooking.BookingId):N0} ₫";
        NoServiceOrdersText.Visibility = orders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ServiceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateServiceAmount();

    private void Quantity_TextChanged(object sender, TextChangedEventArgs e) => UpdateServiceAmount();

    private void UpdateServiceAmount()
    {
        if (UnitPriceText is null || LineTotalText is null) return;
        if (ServiceCombo?.SelectedItem is not HotelServiceItem service || !int.TryParse(QuantityBox?.Text, out var quantity) || quantity < 1)
        {
            UnitPriceText.Text = ServiceCombo?.SelectedItem is HotelServiceItem selectedService
                ? $"{selectedService.UnitPrice:N0} ₫"
                : "—";
            LineTotalText.Text = "—";
            UpdateServiceActionState();
            return;
        }

        UnitPriceText.Text = $"{service.UnitPrice:N0} ₫";
        LineTotalText.Text = $"{service.UnitPrice * quantity:N0} ₫";
        UpdateServiceActionState();
    }

    private void UpdateServiceActionState()
    {
        if (AddServiceButton is null) return;
        AddServiceButton.IsEnabled = _currentBooking is not null
            && _canManageOrders
            && ServiceCombo.SelectedItem is HotelServiceItem
            && int.TryParse(QuantityBox.Text, out var quantity)
            && quantity > 0;
    }

    private void AddService_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBooking is null)
        {
            MessageBox.Show("Vui lòng chọn booking đang lưu trú trước.", "Dịch vụ lưu trú", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (ServiceCombo.SelectedItem is not HotelServiceItem service)
        {
            MessageBox.Show("Vui lòng chọn một dịch vụ đang hoạt động.", "Dịch vụ lưu trú", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!int.TryParse(QuantityBox.Text, out var quantity) || quantity < 1)
        {
            MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0.", "Dịch vụ lưu trú", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            _hotelService.RequestService(_currentBooking.BookingId, service.ServiceId, quantity);
            RefreshServiceOrders();
            ServiceMessageText.Text = $"Đã thêm {quantity} × {service.Name} vào booking {_currentBooking.BookingCode}.";
        }
        catch (InvalidOperationException exception)
        {
            AddServiceButton.IsEnabled = false;
            MessageBox.Show(exception.Message, "Không thể thêm dịch vụ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RemoveService_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBooking is null || sender is not Button { Tag: int serviceOrderId }) return;
        if (MessageBox.Show("Xóa dịch vụ này khỏi booking?", "Xóa dịch vụ", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        if (!_hotelService.RemoveServiceOrder(_currentBooking.BookingId, serviceOrderId))
        {
            MessageBox.Show("Dịch vụ đã hoàn tất, booking đã khóa hoặc không còn được phép chỉnh sửa.", "Không thể xóa dịch vụ", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateSelectedBooking(_hotelService.GetCheckedInBookings().SingleOrDefault(x => x.BookingId == _currentBooking.BookingId));
            return;
        }

        RefreshServiceOrders();
        ServiceMessageText.Text = "Đã xóa dịch vụ khỏi booking.";
    }

    private sealed record BookingSelectionRow(Booking Booking)
    {
        public string Code => Booking.BookingCode;
        public string GuestName => Booking.GuestName ?? Booking.Customer?.FullName ?? string.Empty;
        public string RoomNumber => Booking.Room?.RoomNumber ?? string.Empty;
        public string CheckIn => Booking.CheckIn.ToString("dd/MM/yyyy");
        public string CheckOut => Booking.CheckOut.ToString("dd/MM/yyyy");
    }

    private sealed record ServiceOrderRow(int ServiceOrderId, string Name, int Quantity, decimal UnitPrice, decimal TotalPrice, string UsedAt, string Status, bool CanRemove);
}
