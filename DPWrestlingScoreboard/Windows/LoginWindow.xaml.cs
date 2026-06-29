using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using DPWrestlingScoreboard.Data;
using DPWrestlingScoreboard.Services;

namespace DPWrestlingScoreboard.Windows
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
                var user = AuthService.Authenticate(login, password);

                if (user != null)
                {
                    CurrentUser.User = user;
                    CurrentUser.IsGuest = false;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    passwordPasswordBox.Clear();
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                ShowError("Не удалось подключиться к базе данных. Проверьте SQL Server и файл appsettings.json.");
            }
            catch (Exception)
            {
                ShowError("Ошибка входа. Обратитесь к администратору.");
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
