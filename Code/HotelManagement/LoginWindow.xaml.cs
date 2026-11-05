using System.Windows;
using System.Windows.Controls;
using Services;

namespace HotelManagement;

public partial class LoginWindow : Window
{
    private readonly AuthenticationService _authentication = HotelServicesFactory.CreateAuthenticationService();
    private static readonly string[] DemoEmails = ["khachhang@ks.com", "letan@ks.com", "admin@ks.com"];

    public LoginWindow() { InitializeComponent(); SetDemoAccount(0); }

    private void RoleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EmailBox is null || PasswordInput is null || RoleCombo.SelectedIndex < 0) return;
        SetDemoAccount(RoleCombo.SelectedIndex);
    }

    private void SetDemoAccount(int index)
    {
        EmailBox.Text = DemoEmails[index];
        PasswordInput.Password = "123456";
        ErrorText.Text = "";
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var user = _authentication.Login(EmailBox.Text, PasswordInput.Password);
        if (user is null) { ErrorText.Text = "Email hoặc mật khẩu không chính xác."; return; }
        var main = new MainWindow(user, this);
        Hide();
        main.Show();
    }
}
