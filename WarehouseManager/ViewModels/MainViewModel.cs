using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Drawing.Interop;
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
        private ObservableCollection<Product> _allProducts;
        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }
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
        public RelayCommand LogoutCommand { get; }
        public RelayCommand NavUsersCommand { get; }
        
        public RelayCommand NavDashboardCommand { get; }

        public int TotalProductsCount => Products?.Count ?? 0;
        public decimal TotalInventoryValue => Products?.Sum(p => p.Price * p.StockLevel) ?? 0;
        public int LowStockCount => Products?.Count(p => p.StockLevel < 5) ?? 0;
        public System.Collections.ObjectModel.ObservableCollection<Product> LowStockProducts =>
            new System.Collections.ObjectModel.ObservableCollection<Product>(Products?.Where(p => p.StockLevel < 5) ?? new System.Collections.Generic.List<Product>());
        

        private ObservableCollection<User> _usersList;
        public ObservableCollection<User> UsersList { get => _usersList; set { _usersList = value; OnPropertyChanged(); } }

        public System.Windows.Visibility AdminVisibility =>
            Helpers.SessionManager.IsAdmin ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        
        public string CurrentUserName => Helpers.SessionManager.CurrentUser?.Username?.ToUpper() ?? "VENDÉG";
        public MainViewModel()
        {
            LoadData();

            CurrentPage = new Views.DashboardPage { DataContext = this };
            NavDashboardCommand = new RelayCommand(o => CurrentPage = new Views.DashboardPage { DataContext = this });
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
            LogoutCommand = new RelayCommand(Logout);
            NavUsersCommand = new RelayCommand(o => { CurrentPage = new UsersPage { DataContext = this }; LoadUsers(); });
        }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                var list = db.Products.Include(p => p.Category).ToList();
                _allProducts = new ObservableCollection<Product>(list);
                ApplyFilter(); 
            }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(_allProducts);
            }
            else
            {
                
                Products = new ObservableCollection<Product>(_allProducts.Where(p =>
                    p.Name.ToLower().Contains(SearchText.ToLower()) ||
                    p.SKU.ToLower().Contains(SearchText.ToLower())));
            }
            OnPropertyChanged(nameof(Products));
            OnPropertyChanged(nameof(TotalProductsCount));
            OnPropertyChanged(nameof(TotalInventoryValue));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(LowStockProducts));
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

        private void Logout(object parameter)
        {
            
            Helpers.SessionManager.CurrentUser = null;

            
            var authWindow = new Views.AuthWindow();
            authWindow.Show();

           
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        public void LoadUsers()
        {
            using (var db = new AppDbContext())
            {
                UsersList = new ObservableCollection<User>(db.Users.ToList());
            }
        }

        private void Checkout()
        {
            if (System.Windows.MessageBox.Show($"Biztosan kifizeted? Összeg: {CartTotal:N0} Ft", "Fizetés", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                using (var db = new Data.AppDbContext())
                {
                   
                    var newOrder = new Models.Order
                    {
                        OrderDate = System.DateTime.Now,
                        TotalAmount = CartTotal
                    };
                    db.Orders.Add(newOrder);

                    
                    var groupedCart = CartItems.GroupBy(p => p.Id)
                                               .Select(g => new { Product = g.First(), Quantity = g.Count() });

                    foreach (var item in groupedCart)
                    {
                        
                        var dbProduct = db.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                        if (dbProduct != null && dbProduct.StockLevel >= item.Quantity)
                        {
                            dbProduct.StockLevel -= item.Quantity;
                        }

                        
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

                    db.SaveChanges(); 
                }

                
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