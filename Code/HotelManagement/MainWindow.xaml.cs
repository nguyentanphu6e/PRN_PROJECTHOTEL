using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using HotelManagement.Pages;
using Services;

namespace HotelManagement;

public partial class MainWindow : Window
{
    private readonly User _currentUser;
    private readonly LoginWindow _loginWindow;
    private readonly Button[] _navigationButtons;

    public MainWindow(User user, LoginWindow loginWindow)
    {
        InitializeComponent();
        _currentUser = user;
        _loginWindow = loginWindow;
        _navigationButtons = [RoomsButton, BookingButton, ServicesButton, ReceptionButton, StaffServicesButton, AdminButton];
        UserNameText.Text = user.FullName;
        UserRoleText.Text = RoleLabel(user.Role);
        AvatarText.Text = Initials(user.FullName);
        RoleHeader.Text = RoleLabel(user.Role).ToUpperInvariant();
        ConfigureRoleNavigation(user.Role);
        switch (user.Role)
        {
            case UserRole.Customer: ShowPage(new RoomsPage(), RoomsButton); break;
            case UserRole.Receptionist: ShowPage(new ReceptionPage(), ReceptionButton); break;
            case UserRole.Administrator: ShowPage(new AdminPage(_currentUser), AdminButton); break;
        }
        Closed += (_, _) => _loginWindow.Show();
    }

    private void ConfigureRoleNavigation(UserRole role)
    {
        var customer = role == UserRole.Customer;
        var receptionist = role == UserRole.Receptionist;
        var administrator = role == UserRole.Administrator;
        CustomerGroupLabel.Visibility = customer ? Visibility.Visible : Visibility.Collapsed;
        RoomsButton.Visibility = customer ? Visibility.Visible : Visibility.Collapsed;
        BookingButton.Visibility = customer ? Visibility.Visible : Visibility.Collapsed;
        ServicesButton.Visibility = customer ? Visibility.Visible : Visibility.Collapsed;
        StaffGroupLabel.Visibility = receptionist ? Visibility.Visible : Visibility.Collapsed;
        ReceptionButton.Visibility = receptionist ? Visibility.Visible : Visibility.Collapsed;
        StaffServicesButton.Visibility = receptionist ? Visibility.Visible : Visibility.Collapsed;
        AdminGroupLabel.Visibility = administrator ? Visibility.Visible : Visibility.Collapsed;
        AdminButton.Visibility = administrator ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Rooms_Click(object sender, RoutedEventArgs e) => ShowPage(new RoomsPage(), RoomsButton);

    private void Booking_Click(object sender, RoutedEventArgs e) =>
        ShowPage(new MyBookingsPage(_currentUser, () => ShowPage(new RoomsPage(), RoomsButton)), BookingButton);

    private void Services_Click(object sender, RoutedEventArgs e) => ShowPage(new ServicesPage(_currentUser), (Button)sender);
    private void Reception_Click(object sender, RoutedEventArgs e) => ShowPage(new ReceptionPage(), ReceptionButton);
    private void Admin_Click(object sender, RoutedEventArgs e) => ShowPage(new AdminPage(_currentUser), AdminButton);
    public void OpenBooking(Room room, DateTime checkIn, DateTime checkOut, int guestCount) => ShowPage(new BookingPage(_currentUser, room, checkIn, checkOut, guestCount), BookingButton);
    private void Logout_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowPage(Page page, Button activeButton)
    {
        ContentFrame.Content = page;
        foreach (var button in _navigationButtons) button.Tag = button == activeButton ? "Selected" : null;
    }

    private static string RoleLabel(UserRole role) => role switch { UserRole.Customer => "Khách hàng", UserRole.Receptionist => "Lễ tân", _ => "Quản trị viên" };
    private static string Initials(string name) => string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).TakeLast(2).Select(x => x[0])).ToUpperInvariant();
}


