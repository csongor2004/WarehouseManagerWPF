using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WarehouseManager.Data;
using WarehouseManager.Models;
using WarehouseManager.Helpers;

namespace WarehouseManager.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage() { InitializeComponent(); }

        private void RegisterSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRegUsername.Text) || string.IsNullOrWhiteSpace(txtRegPassword.Password))
            {
                MessageBox.Show("Minden mezőt ki kell tölteni!");
                return;
            }

            using (var db = new Data.AppDbContext())
            {
                if (db.Users.Any(u => u.Username == txtRegUsername.Text))
                {
                    MessageBox.Show("Ez a felhasználónév már foglalt!");
                    return;
                }

                
                bool isFirstUser = !db.Users.Any();

                var newUser = new Models.User
                {
                    Username = txtRegUsername.Text,
                    PasswordHash = Helpers.SecurityHelper.HashPassword(txtRegPassword.Password),

                    
                    Role = isFirstUser ? "Admin" : "User"
                };

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            MessageBox.Show("Sikeres regisztráció! Most már bejelentkezhetsz.");
            
            NavigationService.GoBack();
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}