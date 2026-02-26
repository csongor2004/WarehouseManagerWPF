using WarehouseManager.Models;

namespace WarehouseManager.Helpers
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }

        
        public static bool IsAdmin => CurrentUser != null && CurrentUser.Role == "Admin";
    }
}