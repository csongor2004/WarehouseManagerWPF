using System.Windows;
using WarehouseManager.Data;

namespace WarehouseManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var db = new Data.AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            
            var authWindow = new Views.AuthWindow();
            authWindow.Show();
        }
    }
}