using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace HotelManagement.Pages;

public partial class BookingPage : Page
{
    private readonly HotelManagementService _hotelService = HotelServicesFactory.CreateHotelManagementService();
    private readonly User _customer;
    private readonly Room _room;
    private readonly DateTime _checkIn;
    private readonly DateTime _checkOut;
    private readonly int _nights;
    private readonly int _guestCount;
    private readonly decimal _roomTotal;

    public BookingPage(User customer, Room room, DateTime checkIn, DateTime checkOut, int guestCount)
    {
        InitializeComponent();
        _customer = customer;
        _room = room;
        _checkIn = checkIn;
        _checkOut = checkOut;
        _guestCount = guestCount;
        _nights = (checkOut.Date - checkIn.Date).Days;
        _roomTotal = room.RoomType!.BasePrice * _nights;
        GuestNameBox.Text = customer.FullName;
        EmailBox.Text = customer.Email;
        SelectedRoomTypeText.Text = $"{room.RoomType.Name} · Phòng {room.RoomNumber}";
        StayDetailsText.Text = $"{_guestCount} khách · {_nights} đêm · {checkIn:dd/MM/yyyy}–{checkOut:dd/MM/yyyy}";
        RoomSubtotalLabel.Text = $"{room.RoomType.Name} × {_nights} đêm";
        RoomSubtotalText.Text = $"{_roomTotal:N0} ₫";
        GrandTotalText.Text = $"{_roomTotal:N0} ₫";
    }

    private void SubmitBooking_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GuestNameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên và email.", "Đặt phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!IsValidEmail(EmailBox.Text.Trim()))
        {
            MessageBox.Show("Email chưa đúng định dạng.", "Đặt phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var phoneDigits = new string(PhoneBox.Text.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrWhiteSpace(PhoneBox.Text) && phoneDigits.Length is < 9 or > 12)
        {
            MessageBox.Show("Số điện thoại cần có từ 9 đến 12 chữ số.", "Đặt phòng", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var booking = _hotelService.CreateBooking(_customer.UserId, _room.RoomId, _checkIn, _checkOut, GuestNameBox.Text, _guestCount, PhoneBox.Text);
            SubmitBookingButton.IsEnabled = false;
            MessageBox.Show($"Đã tạo yêu cầu đặt phòng {booking.BookingCode} cho {_room.RoomType!.Name}, phòng {_room.RoomNumber}.", "Đặt phòng thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Không thể đặt phòng", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(exception.Message, "Thông tin đặt phòng", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new System.Net.Mail.MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

