using System.Windows;
using WarehouseManager.ViewModels;

namespace WarehouseManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(); // Itt példányosítjuk egyszer!
        }
    }
}