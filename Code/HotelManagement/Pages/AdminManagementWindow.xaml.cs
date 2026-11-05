using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public enum AdminManagementSection
{
    Accounts,
    RoomTypes,
    Rooms
}

public partial class AdminManagementWindow : Window
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private readonly int _administratorId;
    private int? _selectedRoomTypeId;
    private int? _selectedRoomId;
    private bool _loading;

    public AdminManagementWindow(AdminManagementSection section, int administratorId)
    {
        InitializeComponent();
        _administratorId = administratorId;
        RoomStatusCombo.ItemsSource = new[]
        {
            new RoomStatusOption(RoomStatus.Available, "Trống"),
            new RoomStatusOption(RoomStatus.Occupied, "Đang sử dụng"),
            new RoomStatusOption(RoomStatus.Cleaning, "Đang dọn"),
            new RoomStatusOption(RoomStatus.Maintenance, "Bảo trì")
        };
        RoomStatusCombo.DisplayMemberPath = nameof(RoomStatusOption.Label);
        RoomStatusCombo.SelectedValuePath = nameof(RoomStatusOption.Status);
        ManagementTabs.SelectedIndex = (int)section;
        RefreshAll();
        SetSectionText(section);
    }

    private void SetSectionText(AdminManagementSection section)
    {
        (WindowSectionTitle.Text, WindowSectionDescription.Text) = section switch
        {
            AdminManagementSection.Accounts => ("Tài khoản", "Xem tài khoản và khóa hoặc mở khóa tài khoản đăng nhập."),
            AdminManagementSection.RoomTypes => ("Loại phòng và giá", "Chọn loại phòng để cập nhật, hoặc tạo loại phòng mới."),
            _ => ("Phòng", "Chọn phòng để cập nhật, hoặc thêm phòng mới vào hệ thống.")
        };
    }

    private void RefreshAll()
    {
        _loading = true;
        UsersGrid.ItemsSource = _hotelService.GetAllUsers().Select(user => new AccountRow(user)).ToList();
        RoomTypesGrid.ItemsSource = _hotelService.GetRoomTypes();
        RoomTypeCombo.ItemsSource = _hotelService.GetRoomTypes();
        RoomTypeCombo.SelectedValuePath = nameof(RoomType.RoomTypeId);
        RoomsGrid.ItemsSource = _hotelService.GetRooms().Select(room => new RoomRow(room)).ToList();
        _loading = false;
        ToggleUserButton.IsEnabled = false;
        RoomTypesGrid.SelectedItem = null;
        RoomsGrid.SelectedItem = null;
        ResetRoomTypeForm();
        ResetRoomForm();
    }

    private void ManagementTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender != ManagementTabs || ManagementTabs.SelectedIndex < 0) return;
        SetSectionText((AdminManagementSection)ManagementTabs.SelectedIndex);
    }

    private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not AccountRow row)
        {
            ToggleUserButton.IsEnabled = false;
            return;
        }

        ToggleUserButton.Content = row.User.IsActive ? "Khóa tài khoản" : "Mở khóa tài khoản";
        ToggleUserButton.IsEnabled = row.User.UserId != _administratorId;
    }

    private void ToggleUser_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not AccountRow row) return;
        if (row.User.IsActive && row.User.Role == UserRole.Administrator
            && _hotelService.GetAllUsers().Count(user => user.Role == UserRole.Administrator && user.IsActive) <= 1)
        {
            MessageBox.Show("Hệ thống cần ít nhất một tài khoản quản trị đang hoạt động.", "Không thể khóa", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!_hotelService.SetUserActive(row.User.UserId, !row.User.IsActive)) return;
        RefreshAll();
    }

    private void RoomTypesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || RoomTypesGrid.SelectedItem is not RoomType roomType) return;
        _selectedRoomTypeId = roomType.RoomTypeId;
        RoomTypeNameBox.Text = roomType.Name;
        RoomTypeCapacityBox.Text = roomType.Capacity.ToString(CultureInfo.CurrentCulture);
        RoomTypePriceBox.Text = roomType.BasePrice.ToString("0.##", CultureInfo.CurrentCulture);
    }

    private void SaveRoomType_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RoomTypeCapacityBox.Text.Trim(), out var capacity)
            || !decimal.TryParse(RoomTypePriceBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
        {
            MessageBox.Show("Sức chứa phải là số nguyên và giá phòng phải là số hợp lệ.", "Thông tin loại phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saved = _hotelService.SaveRoomType(_selectedRoomTypeId, RoomTypeNameBox.Text, capacity, price);
            RefreshAll();
            MessageBox.Show($"Đã lưu loại phòng {saved.Name}.", "Loại phòng", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Không thể lưu loại phòng", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void NewRoomType_Click(object sender, RoutedEventArgs e)
    {
        RoomTypesGrid.SelectedItem = null;
        ResetRoomTypeForm();
    }

    private void ResetRoomTypeForm()
    {
        _selectedRoomTypeId = null;
        RoomTypeNameBox.Text = string.Empty;
        RoomTypeCapacityBox.Text = "2";
        RoomTypePriceBox.Text = "0";
    }

    private void RoomsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || RoomsGrid.SelectedItem is not RoomRow row) return;
        _selectedRoomId = row.Room.RoomId;
        RoomNumberBox.Text = row.Room.RoomNumber;
        FloorBox.Text = row.Room.Floor.ToString(CultureInfo.CurrentCulture);
        RoomTypeCombo.SelectedValue = row.Room.RoomTypeId;
        RoomStatusCombo.SelectedValue = row.Room.Status;
    }

    private void SaveRoom_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(FloorBox.Text.Trim(), out var floor)
            || RoomTypeCombo.SelectedValue is not int roomTypeId
            || RoomStatusCombo.SelectedValue is not RoomStatus status)
        {
            MessageBox.Show("Vui lòng nhập tầng và chọn loại phòng, tình trạng hợp lệ.", "Thông tin phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saved = _hotelService.SaveRoom(_selectedRoomId, RoomNumberBox.Text, roomTypeId, floor, status);
            RefreshAll();
            MessageBox.Show($"Đã lưu phòng {saved.RoomNumber}.", "Phòng", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Không thể lưu phòng", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void NewRoom_Click(object sender, RoutedEventArgs e)
    {
        RoomsGrid.SelectedItem = null;
        ResetRoomForm();
    }

    private void ResetRoomForm()
    {
        _selectedRoomId = null;
        RoomNumberBox.Text = string.Empty;
        FloorBox.Text = "1";
        if (RoomTypeCombo.Items.Count > 0) RoomTypeCombo.SelectedIndex = 0;
        RoomStatusCombo.SelectedValue = RoomStatus.Available;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private sealed record AccountRow(User User)
    {
        public string FullName => User.FullName;
        public string Email => User.Email;
        public string RoleLabel => User.Role switch
        {
            UserRole.Administrator => "Quản trị viên",
            UserRole.Receptionist => "Lễ tân",
            _ => "Khách hàng"
        };
        public string StatusLabel => User.IsActive ? "Đang hoạt động" : "Đã khóa";
    }

    private sealed record RoomRow(Room Room)
    {
        public string RoomNumber => Room.RoomNumber;
        public string RoomTypeName => Room.RoomType?.Name ?? string.Empty;
        public int Floor => Room.Floor;
        public string StatusLabel => Room.Status switch
        {
            RoomStatus.Available => "Trống",
            RoomStatus.Occupied => "Đang sử dụng",
            RoomStatus.Cleaning => "Đang dọn",
            _ => "Bảo trì"
        };
    }

    private sealed record RoomStatusOption(RoomStatus Status, string Label);
}
