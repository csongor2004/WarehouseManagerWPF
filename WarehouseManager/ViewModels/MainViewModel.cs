using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Data;

using WarehouseManager.Models;
using WarehouseManager.Helpers;
namespace WarehouseManager.ViewModels
{
    public class MainViewModel : Helpers.ViewModelBase
    {
        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                
                Products = new ObservableCollection<Product>(db.Products.Include(p => p.Category).ToList());
            }
        }
    }
}