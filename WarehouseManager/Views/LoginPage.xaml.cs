using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WarehouseManager.Data;
using WarehouseManager.Helpers;

namespace WarehouseManager.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage() { InitializeComponent(); }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text;
            var passwordHash = SecurityHelper.HashPassword(txtPassword.Password);

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);

                if (user != null)
                {
                    
                    SessionManager.CurrentUser = user;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("Hibás felhasználónév vagy jelszó!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            
            NavigationService.Navigate(new RegisterPage());
        }
    }
}