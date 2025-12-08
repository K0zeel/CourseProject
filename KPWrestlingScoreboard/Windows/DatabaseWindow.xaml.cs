using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using KPWrestlingScoreboard.Data;
using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard.Windows
{
    public partial class DatabaseWindow : Window
    {
        private List<Wrestler> _allWrestlers = new();
        private List<WeightCategory> _weightCategories = new();
        private List<Models.Region> _regions = new();
        private bool _isLoading = false;

        public DatabaseWindow()
        {
            InitializeComponent();
            LoadData();
            SetupPermissions();
        }

        private void SetupPermissions()
        {
            if (CurrentUser.IsGuest)
            {
                userRoleTextBlock.Text = "(Гость - только просмотр)";
                crudButtonsPanel.Visibility = Visibility.Collapsed;
            }
            else if (CurrentUser.IsAdmin)
            {
                userRoleTextBlock.Text = $"(Администратор: {CurrentUser.User?.Login})";
            }
            else
            {
                userRoleTextBlock.Text = $"(Пользователь: {CurrentUser.User?.Login})";
            }
        }

        private void LoadData()
        {
            try
            {
                _isLoading = true;
                
                using var context = new WrestlingDbContext();
                
                // Загружаем борцов
                _allWrestlers = context.Wrestlers
                    .Include(w => w.WeightCategory)
                    .Include(w => w.Region)
                    .OrderBy(w => w.FullName)
                    .ToList();
                
                // Загружаем весовые категории
                _weightCategories = context.WeightCategories.OrderBy(wc => wc.CategoryName).ToList();
                
                // Загружаем регионы
                _regions = context.Regions.OrderBy(r => r.RegionName).ToList();
                
                // Заполняем фильтр категорий
                filterCategoryComboBox.Items.Clear();
                filterCategoryComboBox.Items.Add(new ComboBoxItem { Content = "Все", Tag = 0 });
                foreach (var cat in _weightCategories)
                {
                    filterCategoryComboBox.Items.Add(new ComboBoxItem { Content = $"{cat.CategoryName}", Tag = cat.IdWeightCategory });
                }
                filterCategoryComboBox.SelectedIndex = 0;
                
                // Заполняем фильтр регионов
                filterRegionComboBox.Items.Clear();
                filterRegionComboBox.Items.Add(new ComboBoxItem { Content = "Все", Tag = 0 });
                foreach (var region in _regions)
                {
                    filterRegionComboBox.Items.Add(new ComboBoxItem { Content = region.RegionName, Tag = region.IdRegion });
                }
                filterRegionComboBox.SelectedIndex = 0;
                
                _isLoading = false;
                ApplyFilters();
                
                statusTextBlock.Text = $"Загружено борцов: {_allWrestlers.Count}";
            }
            catch (Exception ex)
            {
                _isLoading = false;
                System.Windows.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            if (_isLoading) return;
            
            var filtered = _allWrestlers.AsEnumerable();
            
            // Фильтр по ФИО
            string nameFilter = filterNameTextBox?.Text?.ToLower().Trim() ?? "";
            if (!string.IsNullOrEmpty(nameFilter))
            {
                filtered = filtered.Where(w => w.FullName.ToLower().Contains(nameFilter));
            }
            
            // Фильтр по дате рождения
            string birthDateFilter = filterBirthDateTextBox?.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(birthDateFilter))
            {
                filtered = filtered.Where(w => w.BirthDate.ToString("dd.MM.yyyy").Contains(birthDateFilter));
            }
            
            // Фильтр по региону
            if (filterRegionComboBox?.SelectedItem is ComboBoxItem regionItem && regionItem.Tag is int regionId && regionId > 0)
            {
                filtered = filtered.Where(w => w.IdRegion == regionId);
            }
            
            // Фильтр по категории
            if (filterCategoryComboBox?.SelectedItem is ComboBoxItem catItem && catItem.Tag is int categoryId && categoryId > 0)
            {
                filtered = filtered.Where(w => w.IdWeightCategory == categoryId);
            }
            
            var result = filtered.ToList();
            wrestlersDataGrid.ItemsSource = result;
            statusTextBlock.Text = $"Показано: {result.Count} из {_allWrestlers.Count}";
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            _isLoading = true;
            
            filterNameTextBox.Text = "";
            filterBirthDateTextBox.Text = "";
            
            if (filterRegionComboBox.Items.Count > 0)
                filterRegionComboBox.SelectedIndex = 0;
            
            if (filterCategoryComboBox.Items.Count > 0)
                filterCategoryComboBox.SelectedIndex = 0;
            
            _isLoading = false;
            ApplyFilters();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new WrestlerEditWindow(null, _weightCategories, _regions);
            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (wrestlersDataGrid.SelectedItem is Wrestler wrestler)
            {
                var editWindow = new WrestlerEditWindow(wrestler, _weightCategories, _regions);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Выберите борца для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (wrestlersDataGrid.SelectedItem is Wrestler wrestler)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Удалить борца {wrestler.FullName}?", 
                    "Подтверждение удаления", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = new WrestlingDbContext();
                        var toDelete = context.Wrestlers.Find(wrestler.IdWrestler);
                        if (toDelete != null)
                        {
                            context.Wrestlers.Remove(toDelete);
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
                System.Windows.MessageBox.Show("Выберите борца для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"Борцы_{DateTime.Now:yyyy-MM-dd}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Борцы");

                    // Заголовки
                    worksheet.Cell(1, 1).Value = "ФИО";
                    worksheet.Cell(1, 2).Value = "Дата рождения";
                    worksheet.Cell(1, 3).Value = "Регион";
                    worksheet.Cell(1, 4).Value = "Весовая категория (кг)";

                    // Стиль заголовков
                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                    headerRange.Style.Font.FontColor = XLColor.White;

                    // Данные (экспортируем отфильтрованные)
                    var dataToExport = wrestlersDataGrid.ItemsSource as List<Wrestler> ?? _allWrestlers;
                    int row = 2;
                    foreach (var wrestler in dataToExport)
                    {
                        worksheet.Cell(row, 1).Value = wrestler.FullName;
                        worksheet.Cell(row, 2).Value = wrestler.BirthDate.ToString("dd.MM.yyyy");
                        worksheet.Cell(row, 3).Value = wrestler.Region?.RegionName ?? "";
                        worksheet.Cell(row, 4).Value = wrestler.WeightCategory?.CategoryName ?? 0;
                        row++;
                    }

                    // Авто-ширина колонок
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(saveDialog.FileName);

                    System.Windows.MessageBox.Show(
                        $"Экспортировано {dataToExport.Count} борцов в файл:\n{saveDialog.FileName}",
                        "Экспорт завершён",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Показываем инструкцию по формату файла
            string formatInfo = 
                "ФОРМАТ EXCEL-ФАЙЛА ДЛЯ ИМПОРТА:\n\n" +
                "Файл должен содержать следующие столбцы (в порядке):\n\n" +
                "1. ФИО — полное имя борца (текст)\n" +
                "2. Дата рождения — в формате ДД.ММ.ГГГГ\n" +
                "3. Регион — название региона (должен существовать в БД)\n" +
                "4. Весовая категория — вес в кг (должен существовать в БД)\n\n" +
                "Первая строка файла должна содержать заголовки.\n" +
                "Данные начинаются со второй строки.\n\n" +
                "Доступные регионы: " + string.Join(", ", _regions.Select(r => r.RegionName)) + "\n\n" +
                "Доступные весовые категории (кг): " + string.Join(", ", _weightCategories.Select(c => c.CategoryName)) + "\n\n" +
                "Продолжить импорт?";

            var result = System.Windows.MessageBox.Show(
                formatInfo,
                "Формат файла для импорта",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes) return;

            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
                DefaultExt = "xlsx"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    int imported = 0;
                    int skipped = 0;
                    var errors = new List<string>();

                    using var workbook = new XLWorkbook(openDialog.FileName);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

                    using var context = new WrestlingDbContext();

                    foreach (var row in rows)
                    {
                        try
                        {
                            string fullName = row.Cell(1).GetString().Trim();
                            string birthDateStr = row.Cell(2).GetString().Trim();
                            string regionName = row.Cell(3).GetString().Trim();
                            string weightStr = row.Cell(4).GetString().Trim();

                            if (string.IsNullOrEmpty(fullName))
                            {
                                skipped++;
                                continue;
                            }

                            // Парсим дату
                            if (!DateTime.TryParse(birthDateStr, out DateTime birthDate))
                            {
                                errors.Add($"Строка {row.RowNumber()}: неверный формат даты '{birthDateStr}'");
                                skipped++;
                                continue;
                            }

                            // Ищем регион
                            var region = _regions.FirstOrDefault(r => 
                                r.RegionName.Equals(regionName, StringComparison.OrdinalIgnoreCase));
                            if (region == null)
                            {
                                errors.Add($"Строка {row.RowNumber()}: регион '{regionName}' не найден");
                                skipped++;
                                continue;
                            }

                            // Ищем весовую категорию
                            if (!int.TryParse(weightStr, out int weight))
                            {
                                errors.Add($"Строка {row.RowNumber()}: неверный формат веса '{weightStr}'");
                                skipped++;
                                continue;
                            }

                            var weightCategory = _weightCategories.FirstOrDefault(c => c.CategoryName == weight);
                            if (weightCategory == null)
                            {
                                errors.Add($"Строка {row.RowNumber()}: весовая категория '{weight}' не найдена");
                                skipped++;
                                continue;
                            }

                            // Создаём борца
                            var wrestler = new Wrestler
                            {
                                FullName = fullName,
                                BirthDate = birthDate,
                                IdRegion = region.IdRegion,
                                IdWeightCategory = weightCategory.IdWeightCategory
                            };

                            context.Wrestlers.Add(wrestler);
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Строка {row.RowNumber()}: {ex.Message}");
                            skipped++;
                        }
                    }

                    context.SaveChanges();
                    LoadData();

                    string message = $"Импорт завершён!\n\nИмпортировано: {imported}\nПропущено: {skipped}";
                    if (errors.Count > 0)
                    {
                        message += $"\n\nОшибки (первые 10):\n" + string.Join("\n", errors.Take(10));
                    }

                    System.Windows.MessageBox.Show(message, "Результат импорта", MessageBoxButton.OK, 
                        errors.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
