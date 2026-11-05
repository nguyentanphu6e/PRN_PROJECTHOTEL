using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class RoomsPage : Page
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private readonly List<RoomSearchItem> _searchResults = [];
    private DateTime _checkIn;
    private DateTime _checkOut;
    private int _guestCount = 1;
    private bool _loaded;
    private bool _filtersReady;

    public RoomsPage()
    {
        InitializeComponent();
        CheckInBox.Text = DateTime.Today.AddDays(5).ToString("dd/MM/yyyy");
        CheckOutBox.Text = DateTime.Today.AddDays(7).ToString("dd/MM/yyyy");
    }

    private async void RoomsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        _loaded = true;
        LoadingText.Visibility = Visibility.Visible;
        try
        {
            var roomTypes = await Task.Run(_hotelService.GetRoomTypes);
            RoomTypeFilter.ItemsSource = new[] { new RoomTypeFilterOption("Tất cả loại phòng", null) }
                .Concat(roomTypes.Select(type => new RoomTypeFilterOption(type.Name, type.RoomTypeId)))
                .ToList();
            RoomTypeFilter.SelectedIndex = 0;
            _filtersReady = true;
            await SearchRoomsAsync();
        }
        catch
        {
            LoadingText.Visibility = Visibility.Collapsed;
            MessageBox.Show("Không thể tải loại phòng. Vui lòng kiểm tra kết nối cơ sở dữ liệu.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SearchRooms_Click(object sender, RoutedEventArgs e) => await SearchRoomsAsync();

    private async Task SearchRoomsAsync()
    {
        if (!DateTime.TryParseExact(CheckInBox.Text.Trim(), "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out _checkIn))
        {
            MessageBox.Show("Ngày nhận phòng chưa đúng. Vui lòng nhập theo định dạng dd/MM/yyyy.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!DateTime.TryParseExact(CheckOutBox.Text.Trim(), "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out _checkOut))
        {
            MessageBox.Show("Ngày trả phòng chưa đúng. Vui lòng nhập theo định dạng dd/MM/yyyy.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_checkIn.Date < DateTime.Today)
        {
            MessageBox.Show("Ngày nhận phòng không được ở quá khứ.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_checkOut.Date <= _checkIn.Date)
        {
            MessageBox.Show("Ngày trả phòng phải sau ngày nhận phòng.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (GuestsCombo.SelectedItem is not ComboBoxItem { Tag: string guestTag }
            || !int.TryParse(guestTag, out _guestCount) || _guestCount < 1)
        {
            MessageBox.Show("Số khách phải lớn hơn 0.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SearchRoomsButton.IsEnabled = false;
        LoadingText.Visibility = Visibility.Visible;
        EmptyResultsText.Visibility = Visibility.Collapsed;
        AvailableRoomsItems.ItemsSource = null;
        ResultCountText.Text = string.Empty;
        try
        {
            var rooms = await Task.Run(() => _hotelService.SearchAvailableRooms(_checkIn, _checkOut, _guestCount));
            var nights = (_checkOut.Date - _checkIn.Date).Days;
            _searchResults.Clear();
            _searchResults.AddRange(rooms.Select(room => new RoomSearchItem(room, nights, _guestCount)));
            SearchSummaryText.Text = $"{_checkIn:dd/MM/yyyy}–{_checkOut:dd/MM/yyyy} · {nights} đêm · {_guestCount} người lớn";
            ApplyRoomFilters();
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(exception.Message, "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch
        {
            MessageBox.Show("Không thể tải danh sách phòng. Vui lòng thử lại.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingText.Visibility = Visibility.Collapsed;
            SearchRoomsButton.IsEnabled = true;
        }
    }

    private void RoomFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_filtersReady) ApplyRoomFilters();
    }

    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        RoomTypeFilter.SelectedIndex = 0;
        PriceFilter.SelectedIndex = 0;
        SortFilter.SelectedIndex = 0;
        ApplyRoomFilters();
    }

    private void ApplyRoomFilters()
    {
        var roomTypeId = (RoomTypeFilter.SelectedItem as RoomTypeFilterOption)?.RoomTypeId;
        var priceBand = (PriceFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() switch
        {
            "Under500" => RoomPriceBand.Under500000,
            "500To1M" => RoomPriceBand.From500000To1000000,
            "1MTo2M" => RoomPriceBand.From1000000To2000000,
            "Over2M" => RoomPriceBand.Over2000000,
            _ => RoomPriceBand.Any
        };
        var sort = (SortFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() switch
        {
            "Ascending" => RoomPriceSort.LowToHigh,
            "Descending" => RoomPriceSort.HighToLow,
            _ => RoomPriceSort.Default
        };

        var filteredRooms = _hotelService.FilterRooms(_searchResults.Select(item => item.Room), roomTypeId, priceBand, sort);
        var itemByRoomId = _searchResults.ToDictionary(item => item.Room.RoomId);
        var results = filteredRooms.Select(room => itemByRoomId[room.RoomId]).ToList();
        AvailableRoomsItems.ItemsSource = results;
        ResultCountText.Text = $"{results.Count} phòng phù hợp";
        EmptyResultsText.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void ChooseRoom_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: RoomSearchItem item }) await ChooseRoomAsync(item);
    }

    private async void RoomDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: RoomSearchItem item }) return;
        var dialog = new RoomDetailsWindow(item) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true) await ChooseRoomAsync(item);
    }

    private async Task ChooseRoomAsync(RoomSearchItem item)
    {
        if (!DateTime.TryParseExact(CheckInBox.Text.Trim(), "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var enteredCheckIn)
            || !DateTime.TryParseExact(CheckOutBox.Text.Trim(), "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var enteredCheckOut)
            || enteredCheckIn.Date != _checkIn.Date || enteredCheckOut.Date != _checkOut.Date
            || GuestsCombo.SelectedItem is not ComboBoxItem { Tag: string guestTag }
            || !int.TryParse(guestTag, out var enteredGuestCount) || enteredGuestCount != _guestCount)
        {
            MessageBox.Show("Ngày lưu trú hoặc số khách đã thay đổi. Vui lòng tìm phòng lại trước khi chọn.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var stillAvailable = await Task.Run(() => _hotelService.IsRoomAvailable(item.Room.RoomId, _checkIn, _checkOut, _guestCount));
            if (!stillAvailable)
            {
                MessageBox.Show("Phòng vừa được đặt hoặc không còn phù hợp. Danh sách phòng sẽ được cập nhật.", "Phòng không còn trống", MessageBoxButton.OK, MessageBoxImage.Information);
                await SearchRoomsAsync();
                return;
            }

            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.OpenBooking(item.Room, _checkIn, _checkOut, _guestCount);
        }
        catch
        {
            MessageBox.Show("Không thể kiểm tra tình trạng phòng. Vui lòng thử lại.", "Tìm phòng", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public sealed record RoomTypeFilterOption(string Name, int? RoomTypeId);
}

public sealed class RoomSearchItem(Room room, int nights, int guestCount)
{
    public Room Room { get; } = room;
    public string RoomTypeName => Room.RoomType?.Name ?? string.Empty;
    public string RoomNumber => Room.RoomNumber;
    public string RoomLabel => $"PHÒNG {RoomNumber}";
    public int Floor => Room.Floor;
    public string CapacityText => $"Tối đa {Room.RoomType?.Capacity ?? 0} khách";
    public decimal NightlyPrice => Room.RoomType?.BasePrice ?? 0m;
    public int Nights => nights;
    public int GuestCount => guestCount;
    public string StayText => $"{guestCount} khách · {nights} đêm";
    public string NightlyPriceText => $"{NightlyPrice:N0} ₫ / đêm";
    public string EstimatedTotalText => $"Dự kiến {NightlyPrice * nights:N0} ₫ cho {nights} đêm";
    public decimal EstimatedTotal => NightlyPrice * nights;
}
