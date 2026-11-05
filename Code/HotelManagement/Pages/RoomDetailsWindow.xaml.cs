using System.Windows;

namespace HotelManagement.Pages;

public partial class RoomDetailsWindow : Window
{
    public RoomDetailsWindow(RoomSearchItem item)
    {
        InitializeComponent();
        RoomTypeNameText.Text = item.RoomTypeName;
        RoomNumberText.Text = $"Phòng {item.RoomNumber} · {item.GuestCount} khách · {item.Nights} đêm";
        RoomNumberValue.Text = item.RoomNumber;
        RoomTypeValue.Text = item.RoomTypeName;
        FloorValue.Text = item.Floor.ToString();
        CapacityValue.Text = $"{item.Room.RoomType?.Capacity ?? 0} khách";
        PriceValue.Text = item.NightlyPrice.ToString("N0") + " ₫ / đêm";
        EstimatedTotalValue.Text = item.EstimatedTotal.ToString("N0") + " ₫";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ChooseRoom_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
