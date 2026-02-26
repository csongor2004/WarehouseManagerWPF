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

            // Amikor megnyílik az ablak, beírtjuk a meglévő adatokat a TextBox-okba
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
                    // Megkeressük az adatbázisban az adott terméket, és felülírjuk az adatait
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
                this.DialogResult = true; // Bezárja az ablakot, és jelzi, hogy sikeres volt
            }
            catch { MessageBox.Show("Hibás adatformátum! Kérjük, számokat adj meg a készlethez és az árhoz."); }
        }
    }
}