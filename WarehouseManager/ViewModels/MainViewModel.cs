using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Helpers;
using WarehouseManager.Models;
using WarehouseManager.Data;
using WarehouseManager.Views;
using System.IO;

namespace WarehouseManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentPage;
        private Product _selectedProduct;

        public object CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
        public ObservableCollection<Product> Products { get; set; }
        public Product SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged(); } }

        public RelayCommand NavInventoryCommand { get; }
        public RelayCommand AddProductCommand { get; }
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand ExportCsvCommand { get; }
        public RelayCommand EditProductCommand { get; }
        public RelayCommand NavStatsCommand { get; }

        public MainViewModel()
        {
            LoadData();
            
            CurrentPage = new InventoryPage { DataContext = this };

            NavInventoryCommand = new RelayCommand(o => CurrentPage = new InventoryPage { DataContext = this });
            AddProductCommand = new RelayCommand(o => OpenAddDialog());
            DeleteProductCommand = new RelayCommand(o => Delete(), o => SelectedProduct != null);
            ExportCsvCommand = new RelayCommand(o => ExportToCsv());
            EditProductCommand = new RelayCommand(o => EditProduct(), o => SelectedProduct != null);
            NavStatsCommand = new RelayCommand(o => CurrentPage = new StatsPage { DataContext = this });
        }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                var list = db.Products.Include(p => p.Category).ToList();
                Products = new ObservableCollection<Product>(list);
                OnPropertyChanged(nameof(Products));
            }
        }

        private void OpenAddDialog()
        {
            var dialog = new AddProductWindow();
            if (dialog.ShowDialog() == true) LoadData();
        }

        private void Delete()
        {
            if (SelectedProduct == null) return;
            if (MessageBox.Show($"Biztosan törlöd a(z) {SelectedProduct.Name} terméket?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    db.Products.Remove(SelectedProduct);
                    db.SaveChanges();
                }
                LoadData();
            }
        }

        private void ExportToCsv()
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "RaktarExport.csv");
            var lines = Products.Select(p => $"{p.SKU};{p.Name};{p.StockLevel};{p.Price}");
            File.WriteAllLines(path, lines);
            MessageBox.Show("Exportálva az asztalra!");
        }
        private void EditProduct()
        {
            if (SelectedProduct == null) return;

           
            var dialog = new EditProductWindow(SelectedProduct);
            if (dialog.ShowDialog() == true)
            {
                LoadData(); 
            }
        }
    }
}