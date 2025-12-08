using System.Windows;
using System.Windows.Controls;
using KPWrestlingScoreboard.Data;
using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard.Windows
{
    public partial class WrestlerEditWindow : Window
    {
        private readonly Wrestler? _wrestler;
        private readonly List<WeightCategory> _categories;
        private readonly List<Models.Region> _regions;

        public WrestlerEditWindow(Wrestler? wrestler, List<WeightCategory> categories, List<Models.Region> regions)
        {
            InitializeComponent();
            _wrestler = wrestler;
            _categories = categories;
            _regions = regions;
            
            LoadCategories();
            LoadRegions();
            
            if (wrestler != null)
            {
                titleTextBlock.Text = "Редактировать борца";
                LoadWrestlerData();
            }
        }

        private void LoadCategories()
        {
            weightCategoryComboBox.Items.Clear();
            foreach (var cat in _categories)
            {
                weightCategoryComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = $"{cat.CategoryName} кг", 
                    Tag = cat.IdWeightCategory 
                });
            }
            
            if (weightCategoryComboBox.Items.Count > 0)
                weightCategoryComboBox.SelectedIndex = 0;
        }

        private void LoadRegions()
        {
            regionComboBox.Items.Clear();
            foreach (var region in _regions)
            {
                regionComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = region.RegionName, 
                    Tag = region.IdRegion 
                });
            }
            
            if (regionComboBox.Items.Count > 0)
                regionComboBox.SelectedIndex = 0;
        }

        private void LoadWrestlerData()
        {
            if (_wrestler == null) return;
            
            fullNameTextBox.Text = _wrestler.FullName;
            birthDatePicker.SelectedDate = _wrestler.BirthDate;
            
            // Выбираем регион
            for (int i = 0; i < regionComboBox.Items.Count; i++)
            {
                if (regionComboBox.Items[i] is ComboBoxItem item && 
                    item.Tag is int regionId && regionId == _wrestler.IdRegion)
                {
                    regionComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Выбираем категорию
            for (int i = 0; i < weightCategoryComboBox.Items.Count; i++)
            {
                if (weightCategoryComboBox.Items[i] is ComboBoxItem item && 
                    item.Tag is int catId && catId == _wrestler.IdWeightCategory)
                {
                    weightCategoryComboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(fullNameTextBox.Text))
            {
                System.Windows.MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (birthDatePicker.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("Выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (regionComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите регион", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (weightCategoryComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Выберите весовую категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();
                
                Wrestler wrestler;
                if (_wrestler != null)
                {
                    // Редактирование
                    wrestler = context.Wrestlers.Find(_wrestler.IdWrestler)!;
                }
                else
                {
                    // Добавление
                    wrestler = new Wrestler();
                    context.Wrestlers.Add(wrestler);
                }
                
                wrestler.FullName = fullNameTextBox.Text.Trim();
                wrestler.BirthDate = birthDatePicker.SelectedDate!.Value;
                wrestler.IdRegion = (int)((ComboBoxItem)regionComboBox.SelectedItem).Tag!;
                wrestler.IdWeightCategory = (int)((ComboBoxItem)weightCategoryComboBox.SelectedItem).Tag!;
                
                context.SaveChanges();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
