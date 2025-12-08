using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using KPWrestlingScoreboard.Data;
using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard.Windows
{
    public partial class UserManagementWindow : Window
    {
        private List<Role> _roles = new();

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var context = new WrestlingDbContext();
                
                // Загружаем роли
                _roles = context.Roles.ToList();
                roleComboBox.Items.Clear();
                foreach (var role in _roles)
                {
                    roleComboBox.Items.Add(new ComboBoxItem { Content = role.RoleName, Tag = role.IdRole });
                }
                if (roleComboBox.Items.Count > 0)
                    roleComboBox.SelectedIndex = 0;
                
                // Загружаем пользователей
                var users = context.Users.Include(u => u.Role).ToList();
                usersDataGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string login = usernameTextBox.Text.Trim();
            string password = passwordBox.Password;
            
            if (string.IsNullOrEmpty(login))
            {
                System.Windows.MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                System.Windows.MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (roleComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();
                
                // Проверяем уникальность логина
                if (context.Users.Any(u => u.Login == login))
                {
                    System.Windows.MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var user = new User
                {
                    Login = login,
                    Password = password,
                    IdRole = (int)((ComboBoxItem)roleComboBox.SelectedItem).Tag!
                };
                
                context.Users.Add(user);
                context.SaveChanges();
                
                usernameTextBox.Clear();
                passwordBox.Clear();
                
                LoadData();
                
                System.Windows.MessageBox.Show("Пользователь успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedItem is User user)
            {
                // Нельзя удалить себя
                if (user.IdUser == CurrentUser.User?.IdUser)
                {
                    System.Windows.MessageBox.Show("Нельзя удалить текущего пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var result = System.Windows.MessageBox.Show(
                    $"Удалить пользователя {user.Login}?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = new WrestlingDbContext();
                        var toDelete = context.Users.Find(user.IdUser);
                        if (toDelete != null)
                        {
                            context.Users.Remove(toDelete);
                            context.SaveChanges();
                            LoadData();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Выберите пользователя для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
