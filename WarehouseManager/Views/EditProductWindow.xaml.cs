using System.Linq;
using System.Windows;
using WarehouseManager.Data;
using WarehouseManager.Models;

namespace WarehouseManager.Views
{
    public partial class EditProductWindow : Window
    {
        private Product _productToEdit;

        public EditProductWindow(Product product)
        {
            InitializeComponent();
            _productToEdit = product;

            // Mezők feltöltése a kiválasztott termék adataival
            txtName.Text = product.Name;
            txtSKU.Text = product.SKU;
            txtStock.Text = product.StockLevel.ToString();
            txtPrice.Text = product.Price.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Megkeressük az adatbázisban a módosítandó sort
                    var p = db.Products.FirstOrDefault(x => x.Id == _productToEdit.Id);
                    if (p != null)
                    {
                        p.Name = txtName.Text;
                        p.SKU = txtSKU.Text;
                        p.StockLevel = int.Parse(txtStock.Text);
                        p.Price = decimal.Parse(txtPrice.Text);
                        db.SaveChanges();
                    }
                }
                this.DialogResult = true; // Bezárja és jelzi a sikert
            }
            catch { MessageBox.Show("Kérlek, ellenőrizd a számformátumokat!"); }
        }
    }
}