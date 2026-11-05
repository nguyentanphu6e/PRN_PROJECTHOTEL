using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class AdminPage : Page
{
    private readonly User _administrator;
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();

    public AdminPage(User administrator)
    {
        InitializeComponent();
        _administrator = administrator;
        RefreshSummary();
    }

    private void RefreshSummary()
    {
        var users = _hotelService.GetAllUsers();
        var rooms = _hotelService.GetRooms();
        var roomTypes = _hotelService.GetRoomTypes();
        AccountCountText.Text = users.Count.ToString();
        AccountSummaryText.Text = $"{users.Count(user => user.IsActive)} tài khoản đang hoạt động";
        RoomCountText.Text = rooms.Count.ToString();
        RoomSummaryText.Text = $"Phòng thuộc {roomTypes.Count} loại phòng";
    }

    private void OpenManagement(AdminManagementSection section)
    {
        new AdminManagementWindow(section, _administrator.UserId)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
        RefreshSummary();
    }

    private void Accounts_Click(object sender, RoutedEventArgs e) => OpenManagement(AdminManagementSection.Accounts);
    private void RoomTypes_Click(object sender, RoutedEventArgs e) => OpenManagement(AdminManagementSection.RoomTypes);
    private void Rooms_Click(object sender, RoutedEventArgs e) => OpenManagement(AdminManagementSection.Rooms);
}
