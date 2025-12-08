using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard.Data
{
    public static class CurrentUser
    {
        public static User? User { get; set; }
        public static bool IsGuest { get; set; } = false;
        public static bool IsAdmin => User?.Role?.RoleName == "Администратор";
        public static bool CanEdit => !IsGuest && User != null;
        
        public static void Logout()
        {
            User = null;
            IsGuest = false;
        }
    }
}

