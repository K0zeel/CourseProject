using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using KPWrestlingScoreboard.Data;

namespace KPWrestlingScoreboard.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            loginTextBox.Focus();
            
            // Позволяем перетаскивать окно
            this.MouseLeftButtonDown += (s, e) => 
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text.Trim();
            string password = passwordPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();

                // Ищем пользователя
                var user = context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user != null)
                {
                    CurrentUser.User = user;
                    CurrentUser.IsGuest = false;
                    
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    passwordPasswordBox.Clear();
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                ShowError($"Ошибка SQL Server: {sqlEx.Message}\n\nПроверьте:\n1. SQL Server запущен\n2. Логин/пароль БД верны\n3. Разрешена SQL аутентификация");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void GuestLogin_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.User = null;
            CurrentUser.IsGuest = true;
            
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ShowError(string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
