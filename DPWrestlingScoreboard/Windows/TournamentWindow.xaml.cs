using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using DPWrestlingScoreboard.Data;
using DPWrestlingScoreboard.Models;
using DPWrestlingScoreboard.Services.Tournament;

namespace DPWrestlingScoreboard.Windows
{
    public partial class TournamentWindow : Window
    {
        private static readonly string[] DefaultStages =
        {
            "QUALIFICATION",
            "1/16 FINAL",
            "1/8 FINAL",
            "1/4 FINAL",
            "1/2 FINAL",
            "REPECHAGE",
            "FINAL 3-5",
            "FINAL 1-2"
        };

        private readonly TournamentService _tournamentService = new();
        private List<WeightCategoryItem> _categories = new();
        private TournamentTableResult? _currentTable;

        public TournamentWindow()
        {
            InitializeComponent();
            FillCompetitionStages();
            LoadWeightCategories();
        }

        private void FillCompetitionStages()
        {
            competitionStageComboBox.Items.Clear();
            foreach (var s in DefaultStages)
                competitionStageComboBox.Items.Add(s);
            if (competitionStageComboBox.Items.Count > 0)
                competitionStageComboBox.SelectedIndex = competitionStageComboBox.Items.Count - 1;
        }

        private void LoadWeightCategories()
        {
            try
            {
                using var context = new WrestlingDbContext();
                _categories = context.WeightCategories
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new WeightCategoryItem
                    {
                        Id = c.IdWeightCategory,
                        DisplayName = $"{c.CategoryName} кг",
                        WeightKg = c.CategoryName
                    })
                    .ToList();

                weightCategoryComboBox.ItemsSource = _categories;
                if (_categories.Count > 0)
                    weightCategoryComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка загрузки категорий: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetSelectedStageText() =>
            competitionStageComboBox.SelectedItem?.ToString()?.Trim() ?? string.Empty;

        private void CompetitionStageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentTable == null) return;
            _currentTable.CompetitionStage = GetSelectedStageText();
            RenderPrintSheet(_currentTable);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (weightCategoryComboBox.SelectedItem is not WeightCategoryItem category)
            {
                System.Windows.MessageBox.Show(
                    "Выберите весовую категорию.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new WrestlingDbContext();
                var wrestlers = context.Wrestlers
                    .Include(w => w.Region)
                    .Include(w => w.WeightCategory)
                    .Where(w => w.IdWeightCategory == category.Id)
                    .ToList();

                var state = TournamentStateService.Current.GetOrCreate(category.Id);
                _currentTable = _tournamentService.BuildTable(wrestlers, category.WeightKg, category.Id, state);
                _currentTable.CompetitionStage = GetSelectedStageText();
                RenderPrintSheet(_currentTable);
                savePrintButton.IsEnabled = true;
                emptyHintTextBlock.Visibility = Visibility.Collapsed;
                tableScrollViewer.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка формирования таблицы: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RenderPrintSheet(TournamentTableResult table)
        {
            printRootPanel.Children.Clear();

            var titleFont = new FontFamily("Times New Roman");
            printRootPanel.Children.Add(new TextBlock
            {
                Text = $"Весовая категория {table.WeightCategoryKg}кг",
                FontFamily = titleFont,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 10)
            });

            if (!string.IsNullOrWhiteSpace(table.CompetitionStage))
            {
                printRootPanel.Children.Add(new TextBlock
                {
                    Text = table.CompetitionStage,
                    FontFamily = titleFont,
                    FontSize = 14,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10)
                });
            }

            if (table.SystemType == TournamentSystemType.RoundRobin
                && table.RoundRobinMatches.Count == 0)
            {
                printRootPanel.Children.Add(new TextBlock
                {
                    Text = "Все пары в этой категории уже сыграны. Нажмите «Сбросить» для нового зачёта.",
                    FontFamily = titleFont,
                    FontSize = 12,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.DimGray,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 0)
                });
                return;
            }

            var displayLines = TournamentService.BuildDisplayLines(table);
            if (displayLines.Count == 0)
                return;

            printRootPanel.Children.Add(CreateParticipantsPrintTable(displayLines));
        }

        private static Border CreateParticipantsPrintTable(IReadOnlyList<TournamentDisplayLine> lines)
        {
            var borderBrush = Brushes.Black;
            var outer = new Border
            {
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };

            var grid = new Grid { ClipToBounds = false };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            var titleFont = new FontFamily("Times New Roman");
            int displayRow = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var p = lines[i].Participant;
                bool pairSeparatorBelow = lines[i].GapAfter;

                AddPrintCell(grid, 0, displayRow, p.Number.ToString(), borderBrush, titleFont, center: true, pairSeparatorBelow);
                var nameText = ParticipantNameFormatter.CombineNameLines(p.NameLine1, p.NameLine2);
                AddPrintCell(grid, 1, displayRow, nameText, borderBrush, titleFont, pairSeparatorBelow: pairSeparatorBelow);
                var birth = string.IsNullOrEmpty(p.BirthDatePrint) ? p.BirthDateText : p.BirthDatePrint;
                AddPrintCell(grid, 2, displayRow, birth, borderBrush, titleFont, center: false, pairSeparatorBelow: pairSeparatorBelow);
                var region = ParticipantNameFormatter.CombineRegionLines(p.RegionLine1, p.RegionLine2);
                AddPrintCell(grid, 3, displayRow, region, borderBrush, titleFont, pairSeparatorBelow: pairSeparatorBelow);

                displayRow++;

                if (pairSeparatorBelow)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
                    AddPairGapRow(grid, displayRow, borderBrush);
                    displayRow++;
                }
            }

            outer.Child = grid;
            return outer;
        }

        private static void AddPairGapRow(Grid grid, int row, System.Windows.Media.Brush borderBrush)
        {
            var gap = new Border
            {
                Background = Brushes.White,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 0, 0, 2)
            };
            Grid.SetRow(gap, row);
            Grid.SetColumn(gap, 0);
            Grid.SetColumnSpan(gap, 4);
            grid.Children.Add(gap);
        }

        private static void AddPrintCell(
            Grid grid, int col, int row, string text, System.Windows.Media.Brush borderBrush,
            FontFamily font, bool center = false, bool pairSeparatorBelow = false)
        {
            var bottomThickness = pairSeparatorBelow ? 2.0 : 1.0;
            var border = new Border
            {
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 0, 1, bottomThickness),
                Padding = new Thickness(6, 6, 6, 6),
                SnapsToDevicePixels = true,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.Black,
                    FontSize = 12,
                    FontFamily = font,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 16,
                    HorizontalAlignment = center
                        ? System.Windows.HorizontalAlignment.Stretch
                        : System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = center ? TextAlignment.Center : TextAlignment.Left
                }
            };
            Grid.SetColumn(border, col);
            Grid.SetRow(border, row);
            grid.Children.Add(border);
        }

        private void SavePrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null) return;

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "HTML для печати (*.html)|*.html",
                DefaultExt = "html",
                FileName = $"Вес_{_currentTable.WeightCategoryKg}кг_{DateTime.Now:yyyy-MM-dd}"
            };

            if (saveDialog.ShowDialog() != true) return;

            try
            {
                _currentTable.CompetitionStage = GetSelectedStageText();
                var html = TournamentHtmlExporter.Export(_currentTable);
                File.WriteAllText(saveDialog.FileName, html, System.Text.Encoding.UTF8);

                var open = System.Windows.MessageBox.Show(
                    "Файл сохранён. Открыть для печати?",
                    "Готово",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (open == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка сохранения: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (weightCategoryComboBox.SelectedItem is WeightCategoryItem category)
                TournamentStateService.Current.ResetCategory(category.Id);

            _currentTable = null;
            savePrintButton.IsEnabled = false;
            printRootPanel.Children.Clear();
            tableScrollViewer.Visibility = Visibility.Collapsed;
            emptyHintTextBlock.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private sealed class WeightCategoryItem
        {
            public int Id { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public int WeightKg { get; set; }
        }
    }
}
