using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class StaffBookingWindow : Window
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private DateTime? _searchedCheckIn;
    private DateTime? _searchedCheckOut;
    private int _searchedGuestCount;
    private bool _ready;

    public string? CreatedBookingCode { get; private set; }

    public StaffBookingWindow()
    {
        InitializeComponent();
        CheckInPicker.SelectedDate = DateTime.Today.AddDays(5);
        CheckOutPicker.SelectedDate = DateTime.Today.AddDays(7);
        CustomerCombo.ItemsSource = _hotelService.GetActiveCustomers();
        if (CustomerCombo.Items.Count > 0)
        {
            CustomerCombo.SelectedIndex = 0;
            GuestNameBox.Text = ((User)CustomerCombo.SelectedItem).FullName;
        }
        _ready = true;
    }

    private void Customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        if (CustomerCombo.SelectedItem is User customer) GuestNameBox.Text = customer.FullName;
    }

    private void StayCriteria_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        _searchedCheckIn = null;
        _searchedCheckOut = null;
        RoomCombo.ItemsSource = null;
        CreateBookingButton.IsEnabled = false;
        PriceSummaryText.Text = "Thông tin lưu trú đã thay đổi. Hãy tìm phòng lại.";
    }

    private async void SearchRooms_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerCombo.SelectedItem is not User)
        {
            MessageBox.Show("Chưa có khách hàng đang hoạt động để tạo booking.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (CheckInPicker.SelectedDate is not DateTime checkIn || CheckOutPicker.SelectedDate is not DateTime checkOut)
        {
            MessageBox.Show("Vui lòng chọn ngày nhận phòng và ngày trả phòng.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (checkIn.Date < DateTime.Today)
        {
            MessageBox.Show("Ngày nhận phòng không được ở quá khứ.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (checkOut.Date <= checkIn.Date)
        {
            MessageBox.Show("Ngày trả phòng phải sau ngày nhận phòng.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (GuestsCombo.SelectedItem is not ComboBoxItem { Tag: string guestTag }
            || !int.TryParse(guestTag, out var guestCount) || guestCount < 1)
        {
            MessageBox.Show("Số khách phải lớn hơn 0.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SearchRoomsButton.IsEnabled = false;
        try
        {
            var rooms = await Task.Run(() => _hotelService.SearchAvailableRooms(checkIn, checkOut, guestCount));
            RoomCombo.ItemsSource = rooms.Select(room => new RoomOption(room,
                $"Phòng {room.RoomNumber} · {room.RoomType?.Name} · tối đa {room.RoomType?.Capacity} khách · {room.RoomType?.BasePrice:N0} ₫/đêm")).ToList();
            RoomCombo.SelectedIndex = rooms.Count > 0 ? 0 : -1;
            _searchedCheckIn = checkIn.Date;
            _searchedCheckOut = checkOut.Date;
            _searchedGuestCount = guestCount;
            if (rooms.Count == 0)
            {
                PriceSummaryText.Text = "Không tìm thấy phòng phù hợp với thời gian và số khách đã chọn.";
                CreateBookingButton.IsEnabled = false;
            }
            else
            {
                UpdatePriceSummary();
            }
        }
        catch
        {
            MessageBox.Show("Không thể tải phòng trống. Vui lòng thử lại.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SearchRoomsButton.IsEnabled = true;
        }
    }

    private void Room_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_ready) UpdatePriceSummary();
    }

    private void UpdatePriceSummary()
    {
        if (RoomCombo.SelectedItem is not RoomOption option || _searchedCheckIn is null || _searchedCheckOut is null)
        {
            CreateBookingButton.IsEnabled = false;
            return;
        }

        var nights = (_searchedCheckOut.Value - _searchedCheckIn.Value).Days;
        var roomTotal = option.Room.RoomType!.BasePrice * nights;
        PriceSummaryText.Text = $"{option.Room.RoomType.Name}: {option.Room.RoomType.BasePrice:N0} ₫/đêm × {nights} đêm = {roomTotal:N0} ₫. Booking mới sẽ ở trạng thái Pending.";
        CreateBookingButton.IsEnabled = true;
    }

    private void CreateBooking_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerCombo.SelectedItem is not User customer || RoomCombo.SelectedItem is not RoomOption option
            || _searchedCheckIn is null || _searchedCheckOut is null)
        {
            MessageBox.Show("Hãy chọn khách hàng, ngày lưu trú và phòng còn trống.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (CheckInPicker.SelectedDate?.Date != _searchedCheckIn || CheckOutPicker.SelectedDate?.Date != _searchedCheckOut
            || GuestsCombo.SelectedItem is not ComboBoxItem { Tag: string guestTag }
            || !int.TryParse(guestTag, out var guestCount) || guestCount != _searchedGuestCount)
        {
            MessageBox.Show("Ngày hoặc số khách đã thay đổi. Vui lòng tìm phòng lại.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var phoneDigits = new string(GuestPhoneBox.Text.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(GuestPhoneBox.Text) && phoneDigits.Length is < 9 or > 12)
        {
            MessageBox.Show("Số điện thoại cần có từ 9 đến 12 chữ số.", "Tạo booking", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var booking = _hotelService.CreateBooking(customer.UserId, option.Room.RoomId, _searchedCheckIn.Value,
                _searchedCheckOut.Value, GuestNameBox.Text, _searchedGuestCount, GuestPhoneBox.Text);
            CreatedBookingCode = booking.BookingCode;
            DialogResult = true;
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Không thể tạo booking", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(exception.Message, "Thông tin booking", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    public sealed record RoomOption(Room Room, string Label);
}
