using System.Windows;
using WarehouseManager.Data;
using WarehouseManager.Models;

namespace WarehouseManager.Views
{
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var p = new Product
                    {
                        Name = txtName.Text,
                        SKU = txtSKU.Text,
                        StockLevel = int.Parse(txtStock.Text),
                        Price = decimal.Parse(txtPrice.Text),
                        CategoryId = 1 // Teszt kategória
                    };
                    db.Products.Add(p);
                    db.SaveChanges();
                }
                this.DialogResult = true;
            }
            catch { MessageBox.Show("Hibás adatok!"); }
        }
    }
}