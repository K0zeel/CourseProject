using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using DPWrestlingScoreboard.Data;
using DPWrestlingScoreboard.Models;

namespace DPWrestlingScoreboard.Windows
{
    public partial class WeightCategoryManagementWindow : Window
    {
        public bool DataChanged { get; private set; }

        private sealed class WeightCategoryRow
        {
            public int IdWeightCategory { get; init; }
            public int CategoryName { get; init; }
            public int WrestlerCount { get; init; }
        }

        public WeightCategoryManagementWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var context = new WrestlingDbContext();

                var counts = context.Wrestlers
                    .GroupBy(w => w.IdWeightCategory)
                    .Select(g => new { Id = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Id, x => x.Count);

                var rows = context.WeightCategories
                    .OrderBy(c => c.CategoryName)
                    .AsEnumerable()
                    .Select(c => new WeightCategoryRow
                    {
                        IdWeightCategory = c.IdWeightCategory,
                        CategoryName = c.CategoryName,
                        WrestlerCount = counts.GetValueOrDefault(c.IdWeightCategory, 0)
                    })
                    .ToList();

                categoriesDataGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseWeight(out int weightKg)
        {
            weightKg = 0;
            if (!int.TryParse(weightTextBox.Text.Trim(), out weightKg) || weightKg <= 0)
            {
                System.Windows.MessageBox.Show("Введите корректный вес в килограммах (целое число больше 0).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool IsDuplicateWeight(int weightKg, int? excludeId = null)
        {
            using var context = new WrestlingDbContext();
            return context.WeightCategories.Any(c =>
                c.CategoryName == weightKg &&
                (!excludeId.HasValue || c.IdWeightCategory != excludeId.Value));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseWeight(out int weightKg)) return;

            if (IsDuplicateWeight(weightKg))
            {
                System.Windows.MessageBox.Show($"Категория {weightKg} кг уже существует.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();
                context.WeightCategories.Add(new WeightCategory { CategoryName = weightKg });
                context.SaveChanges();
                DataChanged = true;
                weightTextBox.Clear();
                LoadData();
                System.Windows.MessageBox.Show($"Категория {weightKg} кг добавлена.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesDataGrid.SelectedItem is not WeightCategoryRow row)
            {
                System.Windows.MessageBox.Show("Выберите категорию для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseWeight(out int weightKg)) return;

            if (IsDuplicateWeight(weightKg, row.IdWeightCategory))
            {
                System.Windows.MessageBox.Show($"Категория {weightKg} кг уже существует.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();
                var entity = context.WeightCategories.Find(row.IdWeightCategory);
                if (entity == null)
                {
                    System.Windows.MessageBox.Show("Категория не найдена. Обновите список.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadData();
                    return;
                }

                entity.CategoryName = weightKg;
                context.SaveChanges();
                DataChanged = true;
                LoadData();
                System.Windows.MessageBox.Show($"Категория обновлена: {weightKg} кг.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesDataGrid.SelectedItem is not WeightCategoryRow row)
            {
                System.Windows.MessageBox.Show("Выберите категорию для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (row.WrestlerCount > 0)
            {
                System.Windows.MessageBox.Show(
                    $"Нельзя удалить категорию {row.CategoryName} кг: к ней привязано борцов: {row.WrestlerCount}.\n" +
                    "Сначала переведите или удалите этих борцов.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Удалить весовую категорию {row.CategoryName} кг?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var context = new WrestlingDbContext();
                var entity = context.WeightCategories.Find(row.IdWeightCategory);
                if (entity != null)
                {
                    context.WeightCategories.Remove(entity);
                    context.SaveChanges();
                }

                DataChanged = true;
                weightTextBox.Clear();
                LoadData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoriesDataGrid.SelectedItem is WeightCategoryRow row)
                weightTextBox.Text = row.CategoryName.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
