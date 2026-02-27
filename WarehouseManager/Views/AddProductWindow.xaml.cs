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
                using (var db = new Data.AppDbContext())
                {
                    
                    var category = db.Categories.FirstOrDefault();
                    if (category == null)
                    {
                        category = new Models.Category { Name = "Általános" };
                        db.Categories.Add(category);
                        db.SaveChanges(); 
                    }

                    var p = new Models.Product
                    {
                        Name = txtName.Text,
                        SKU = txtSKU.Text,
                        StockLevel = int.Parse(txtStock.Text),
                        Price = decimal.Parse(txtPrice.Text),
                        CategoryId = category.Id 
                    };
                    db.Products.Add(p);
                    db.SaveChanges();
                }
                this.DialogResult = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Mentési hiba történt: {ex.Message}\nEllenőrizd a számformátumokat!");
            }
        }
    }
}