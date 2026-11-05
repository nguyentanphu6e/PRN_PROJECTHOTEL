using System.Windows;
using DataAccessObjects;

namespace HotelManagement;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            HotelDatabaseInitializer.Initialize(new HotelDbContextFactory());
            new LoginWindow().Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Không thể kết nối cơ sở dữ liệu khách sạn.\n{exception.Message}", "Khách sạn tình yêu", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
