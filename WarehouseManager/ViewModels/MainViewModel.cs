using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WarehouseManager.Data;
using WarehouseManager.Helpers;
using WarehouseManager.Models;
using WarehouseManager.Services;
using WarehouseManager.Views;

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
        private ObservableCollection<Product> _cartItems = new ObservableCollection<Product>();
        public ObservableCollection<Product> CartItems { get => _cartItems; set { _cartItems = value; OnPropertyChanged(); } }

        private decimal _cartTotal;
        public decimal CartTotal { get => _cartTotal; set { _cartTotal = value; OnPropertyChanged(); } }

        private ObservableCollection<Order> _ordersHistory;
        public ObservableCollection<Order> OrdersHistory { get => _ordersHistory; set { _ordersHistory = value; OnPropertyChanged(); } }

        public RelayCommand NavHistoryCommand { get; }
        public RelayCommand NavSalesCommand { get; }
        public RelayCommand AddToCartCommand { get; }
        public RelayCommand CheckoutCommand { get; }
        public RelayCommand ClearCartCommand { get; }
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
            NavSalesCommand = new RelayCommand(o => CurrentPage = new SalesPage { DataContext = this });
            AddToCartCommand = new RelayCommand(AddToCart);
            CheckoutCommand = new RelayCommand(o => Checkout(), o => CartItems.Count > 0);
            ClearCartCommand = new RelayCommand(o => { CartItems.Clear(); CalculateTotal(); });
            NavHistoryCommand = new RelayCommand(o => CurrentPage = new HistoryPage { DataContext = this });
        }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                // Meglévő termék betöltés...
                var list = db.Products.Include(p => p.Category).ToList();
                Products = new ObservableCollection<Product>(list);
                OnPropertyChanged(nameof(Products));

                // ÚJ: Rendelések betöltése (Legújabb elöl)
                var orderList = db.Orders.OrderByDescending(o => o.OrderDate).ToList();
                OrdersHistory = new ObservableCollection<Order>(orderList);
                OnPropertyChanged(nameof(OrdersHistory));
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
        private void AddToCart(object param)
        {
            if (param is Product product)
            {
                if (product.StockLevel > 0)
                {
                    CartItems.Add(product);
                    CalculateTotal();
                }
                else
                {
                    MessageBox.Show("Nincs elég készlet ebből a termékből!");
                }
            }
        }

        private void CalculateTotal()
        {
            CartTotal = CartItems.Sum(p => p.Price);
        }



        private void Checkout()
        {
            if (System.Windows.MessageBox.Show($"Biztosan kifizeted? Összeg: {CartTotal:N0} Ft", "Fizetés", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                using (var db = new Data.AppDbContext())
                {
                    // 1. Létrehozzuk az új rendelést
                    var newOrder = new Models.Order
                    {
                        OrderDate = System.DateTime.Now,
                        TotalAmount = CartTotal
                    };
                    db.Orders.Add(newOrder);

                    // A kosár tartalmának csoportosítása (ha egy termékből többet is beletett)
                    var groupedCart = CartItems.GroupBy(p => p.Id)
                                               .Select(g => new { Product = g.First(), Quantity = g.Count() });

                    foreach (var item in groupedCart)
                    {
                        // 2. Készlet levonása
                        var dbProduct = db.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                        if (dbProduct != null && dbProduct.StockLevel >= item.Quantity)
                        {
                            dbProduct.StockLevel -= item.Quantity;
                        }

                        // 3. Rendelési tétel (OrderItem) mentése
                        var orderItem = new Models.OrderItem
                        {
                            Order = newOrder,
                            ProductId = item.Product.Id,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price
                        };
                        db.OrderItems.Add(orderItem);
                    }

                    db.SaveChanges(); // Mentés az adatbázisba
                }

                // Itt hívod meg a korábbi PDF generálást (ReceiptService.GeneratePdfReceipt...)
                try
                {
                    Services.ReceiptService.GeneratePdfReceipt(CartItems, CartTotal);
                    System.Windows.MessageBox.Show("Sikeres vásárlás! Tranzakció mentve, nyugta generálva.");
                }
                catch { System.Windows.MessageBox.Show("Vásárlás mentve, de a PDF generálás sikertelen."); }

                CartItems.Clear();
                CalculateTotal();
                LoadData();
            }
        }
    }
}