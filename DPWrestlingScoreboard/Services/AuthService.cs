using DPWrestlingScoreboard.Data;
using DPWrestlingScoreboard.Models;
using Microsoft.EntityFrameworkCore;

namespace DPWrestlingScoreboard.Services
{
    public static class AuthService
    {
        /// <summary>
        /// Проверка логина и пароля. Старые пароли в открытом виде при успешном входе переводятся в хеш.
        /// </summary>
        public static User? Authenticate(string login, string password)
        {
            login = login.Trim();
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return null;

            using var context = new WrestlingDbContext();
            var user = context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login);

            if (user == null)
                return null;

            if (PasswordHasher.IsHashed(user.Password))
            {
                return PasswordHasher.Verify(password, user.Password) ? user : null;
            }

            // Миграция с открытого пароля (старые записи в БД)
            if (!string.Equals(user.Password, password, StringComparison.Ordinal))
                return null;

            try
            {
                user.Password = PasswordHasher.Hash(password);
                context.SaveChanges();
            }
            catch
            {
                // Вход не блокируем, если не удалось сохранить хеш (например, узкая колонка Password в БД).
            }

            return user;
        }
    }
}
