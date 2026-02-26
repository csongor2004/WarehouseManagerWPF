using System.Windows;

namespace WarehouseManager.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            
            AuthFrame.Navigate(new LoginPage());
        }
    }
}