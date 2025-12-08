using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using KPWrestlingScoreboard.Windows;
using KPWrestlingScoreboard.Data;
using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard
{
    public partial class MainWindow : Window
    {
        // Счет
        private int _redScore = 0;
        private int _blueScore = 0;

        // Штрафы
        private int _redPenalty = 0;
        private int _bluePenalty = 0;

        // Таймеры
        private DispatcherTimer _mainTimer = null!;
        private DispatcherTimer _redTimer = null!;
        private DispatcherTimer _blueTimer = null!;

        // Время (в секундах)
        private int _mainTimeSeconds = 360; // 6:00
        private int _redTimeSeconds = 30;   // 0:30
        private int _blueTimeSeconds = 30;  // 0:30

        // Текущий период
        private int _currentPeriod = 1;

        // Состояние таймера
        private bool _isMainTimerRunning = false;
        private bool _isBreakActive = false;
        private int _savedMainTimeSeconds = 0;

        // Окна
        private ScoreboardWindow? _scoreboardWindow;
        private FlagWindow? _flagWindow;
        private bool _isScoreboardVisible = false;

        // Данные из БД
        private List<Wrestler> _allWrestlers = new();
        private List<WeightCategory> _weightCategories = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimers();
            UpdateAllDisplays();
            SetupUserPermissions();
            LoadDataFromDb();
            
            // Показываем флаг на втором экране при запуске
            ShowFlagOnSecondScreen();
        }

        private void SetupUserPermissions()
        {
            // Показываем кнопку управления пользователями только для админов
            if (CurrentUser.IsAdmin)
            {
                usersButton.Visibility = Visibility.Visible;
            }
            
            // Гости могут только смотреть базу данных, судейство недоступно
            if (CurrentUser.IsGuest)
            {
                judgingRow1.Visibility = Visibility.Collapsed;
                judgingRow2.Visibility = Visibility.Collapsed;
                judgingRow3.Visibility = Visibility.Collapsed;
                judgingRow4.Visibility = Visibility.Collapsed;
                judgingRow5.Visibility = Visibility.Collapsed;
                
                // Показываем сообщение
                System.Windows.MessageBox.Show(
                    "Вы вошли как гость.\nДоступен только просмотр базы данных борцов.",
                    "Режим гостя",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void LoadDataFromDb()
        {
            try
            {
                using var context = new WrestlingDbContext();
                
                // Загружаем весовые категории
                _weightCategories = context.WeightCategories.OrderBy(c => c.CategoryName).ToList();
                
                weightCategoryComboBox.Items.Clear();
                foreach (var cat in _weightCategories)
                {
                    weightCategoryComboBox.Items.Add(new ComboBoxItem { Content = $"{cat.CategoryName} кг", Tag = cat.IdWeightCategory });
                }
                
                // Загружаем борцов
                _allWrestlers = context.Wrestlers
                    .Include(w => w.WeightCategory)
                    .Include(w => w.Region)
                    .OrderBy(w => w.FullName)
                    .ToList();
                
                if (weightCategoryComboBox.Items.Count > 0)
                {
                    weightCategoryComboBox.SelectedIndex = 0;
                    UpdateWrestlersByCategory();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateWrestlersByCategory()
        {
            if (weightCategoryComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            
            int categoryId = (int)selectedItem.Tag!;
            
            // Фильтруем борцов по выбранной категории
            var filteredWrestlers = _allWrestlers
                .Where(w => w.IdWeightCategory == categoryId)
                .ToList();
            
            // Обновляем ComboBox красного угла
            redWrestlerComboBox.Items.Clear();
            foreach (var wrestler in filteredWrestlers)
            {
                redWrestlerComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = wrestler.FullName, 
                    Tag = wrestler.IdWrestler 
                });
            }
            
            // Обновляем ComboBox синего угла
            blueWrestlerComboBox.Items.Clear();
            foreach (var wrestler in filteredWrestlers)
            {
                blueWrestlerComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = wrestler.FullName, 
                    Tag = wrestler.IdWrestler 
                });
            }
            
            // Выбираем первых борцов если есть
            if (redWrestlerComboBox.Items.Count > 0)
                redWrestlerComboBox.SelectedIndex = 0;
            if (blueWrestlerComboBox.Items.Count > 1)
                blueWrestlerComboBox.SelectedIndex = 1;
            else if (blueWrestlerComboBox.Items.Count > 0)
                blueWrestlerComboBox.SelectedIndex = 0;
        }

        private void WeightCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateWrestlersByCategory();
        }

        private void InitializeTimers()
        {
            _mainTimer = new DispatcherTimer();
            _mainTimer.Interval = TimeSpan.FromSeconds(1);
            _mainTimer.Tick += MainTimer_Tick;

            _redTimer = new DispatcherTimer();
            _redTimer.Interval = TimeSpan.FromSeconds(1);
            _redTimer.Tick += RedTimer_Tick;

            _blueTimer = new DispatcherTimer();
            _blueTimer.Interval = TimeSpan.FromSeconds(1);
            _blueTimer.Tick += BlueTimer_Tick;
        }

        private void UpdateAllDisplays()
        {
            UpdateMainTimerDisplay();
            UpdateRedTimerDisplay();
            UpdateBlueTimerDisplay();
            UpdateStartButtonIcon();
        }

        #region Main Timer
        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            if (_mainTimeSeconds > 0)
            {
                _mainTimeSeconds--;
                UpdateMainTimerDisplay();
                UpdateScoreboardTimer();

                // Если активен перерыв, обновляем отображение на табло
                if (_isBreakActive)
                {
                    _scoreboardWindow?.ShowBreak(true, _mainTimeSeconds);
                }
            }
            else
            {
                _mainTimer.Stop();
                _isMainTimerRunning = false;
                UpdateStartButtonIcon();

                if (_isBreakActive)
                {
                    // Перерыв закончился, восстанавливаем время
                    _isBreakActive = false;
                    _mainTimeSeconds = _savedMainTimeSeconds;
                    UpdateMainTimerDisplay();
                    UpdateScoreboardTimer();
                    _scoreboardWindow?.ShowBreak(false);
                    breakButton.Content = "ПЕРЕРЫВ 30 сек";
                }
                else
                {
                    System.Windows.MessageBox.Show("Время периода истекло!", "Конец периода", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void UpdateMainTimerDisplay()
        {
            int minutes = _mainTimeSeconds / 60;
            int seconds = _mainTimeSeconds % 60;
            mainTimerTextBlock.Text = $"{minutes}:{seconds:D2}";
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_isMainTimerRunning)
            {
                _mainTimer.Stop();
                _isMainTimerRunning = false;
            }
            else
            {
                _mainTimer.Start();
                _isMainTimerRunning = true;
            }
            UpdateStartButtonIcon();
        }

        private void UpdateStartButtonIcon()
        {
            startButton.Content = _isMainTimerRunning ? "⏸" : "▶";
            startButton.Background = _isMainTimerRunning 
                ? new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"))
                : new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#22C55E"));
        }

        // +/- изменяют секунды
        private void TimerPlus_Click(object sender, RoutedEventArgs e)
        {
            _mainTimeSeconds += 1;
            UpdateMainTimerDisplay();
            UpdateScoreboardTimer();
        }

        private void TimerMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_mainTimeSeconds > 0)
            {
                _mainTimeSeconds -= 1;
                UpdateMainTimerDisplay();
                UpdateScoreboardTimer();
            }
        }

        // Перерыв 30 секунд на главном таймере
        private void Break_Click(object sender, RoutedEventArgs e)
        {
            if (!_isBreakActive)
            {
                // Сохраняем текущее время и запускаем перерыв
                _savedMainTimeSeconds = _mainTimeSeconds;
                _mainTimeSeconds = 30;
                _isBreakActive = true;
                breakButton.Content = "ОТМЕНА";
                
                _scoreboardWindow?.ShowBreak(true, 30);
                
                // Запускаем таймер если не запущен
                if (!_isMainTimerRunning)
                {
                    _mainTimer.Start();
                    _isMainTimerRunning = true;
                    UpdateStartButtonIcon();
                }
            }
            else
            {
                // Отменяем перерыв
                _mainTimer.Stop();
                _isMainTimerRunning = false;
                _isBreakActive = false;
                _mainTimeSeconds = _savedMainTimeSeconds;
                breakButton.Content = "ПЕРЕРЫВ 30 сек";
                
                _scoreboardWindow?.ShowBreak(false);
                UpdateStartButtonIcon();
            }
            
            UpdateMainTimerDisplay();
            UpdateScoreboardTimer();
        }

        private void Signal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "stop.wav");
                
                if (System.IO.File.Exists(soundPath))
                {
                    var player = new System.Media.SoundPlayer(soundPath);
                    player.Play();
                }
                else
                {
                    // Пробуем загрузить из ресурсов приложения
                    try
                    {
                        var uri = new Uri("pack://application:,,,/Resources/stop.wav", UriKind.Absolute);
                        var stream = System.Windows.Application.GetResourceStream(uri)?.Stream;
                        if (stream != null)
                        {
                            var player = new System.Media.SoundPlayer(stream);
                            player.Play();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Файл stop.wav не найден!\n\nПоместите файл в папку Resources рядом с программой.", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("Файл stop.wav не найден!\n\nПоместите файл в папку Resources рядом с программой.", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Red Score
        private void RedPlus1_Click(object sender, RoutedEventArgs e) => UpdateRedScore(1);
        private void RedPlus2_Click(object sender, RoutedEventArgs e) => UpdateRedScore(2);
        private void RedPlus3_Click(object sender, RoutedEventArgs e) => UpdateRedScore(3);
        private void RedPlus4_Click(object sender, RoutedEventArgs e) => UpdateRedScore(4);
        private void RedPlus5_Click(object sender, RoutedEventArgs e) => UpdateRedScore(5);
        private void RedMinus1_Click(object sender, RoutedEventArgs e) => UpdateRedScore(-1);
        private void RedMinus2_Click(object sender, RoutedEventArgs e) => UpdateRedScore(-2);
        private void RedMinus3_Click(object sender, RoutedEventArgs e) => UpdateRedScore(-3);
        private void RedMinus4_Click(object sender, RoutedEventArgs e) => UpdateRedScore(-4);
        private void RedMinus5_Click(object sender, RoutedEventArgs e) => UpdateRedScore(-5);

        private void UpdateRedScore(int delta)
        {
            _redScore = Math.Max(0, _redScore + delta);
            redScoreTextBlock.Text = _redScore.ToString();
            UpdateScoreboardScore();
        }
        #endregion

        #region Blue Score
        private void BluePlus1_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(1);
        private void BluePlus2_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(2);
        private void BluePlus3_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(3);
        private void BluePlus4_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(4);
        private void BluePlus5_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(5);
        private void BlueMinus1_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(-1);
        private void BlueMinus2_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(-2);
        private void BlueMinus3_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(-3);
        private void BlueMinus4_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(-4);
        private void BlueMinus5_Click(object sender, RoutedEventArgs e) => UpdateBlueScore(-5);

        private void UpdateBlueScore(int delta)
        {
            _blueScore = Math.Max(0, _blueScore + delta);
            blueScoreTextBlock.Text = _blueScore.ToString();
            UpdateScoreboardScore();
        }
        #endregion

        #region Period
        private void Period1_Click(object sender, RoutedEventArgs e)
        {
            _currentPeriod = 1;
            period1Button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
            period2Button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
            periodDisplayTextBlock.Text = "ПЕРИОД 1";
            
            // Первый период - 6 минут
            _mainTimeSeconds = 360;
            UpdateMainTimerDisplay();
            UpdateScoreboardTimer();
            UpdateScoreboardPeriod();
        }

        private void Period2_Click(object sender, RoutedEventArgs e)
        {
            _currentPeriod = 2;
            period1Button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1a1a1a"));
            period2Button.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
            periodDisplayTextBlock.Text = "ПЕРИОД 2";
            
            // Второй период - 3 минуты
            _mainTimeSeconds = 180;
            UpdateMainTimerDisplay();
            UpdateScoreboardTimer();
            UpdateScoreboardPeriod();
        }
        #endregion

        #region Penalties
        private void RedPenalty_Click(object sender, RoutedEventArgs e)
        {
            _redPenalty++;
            if (_redPenalty > 3) _redPenalty = 0;
            redPenaltyComboBox.SelectedIndex = _redPenalty;
        }

        private void BluePenalty_Click(object sender, RoutedEventArgs e)
        {
            _bluePenalty++;
            if (_bluePenalty > 3) _bluePenalty = 0;
            bluePenaltyComboBox.SelectedIndex = _bluePenalty;
        }
        #endregion

        #region Individual Timers
        private void RedTimer_Tick(object? sender, EventArgs e)
        {
            if (_redTimeSeconds > 0)
            {
                _redTimeSeconds--;
                UpdateRedTimerDisplay();
            }
            else
            {
                _redTimer.Stop();
            }
        }

        private void BlueTimer_Tick(object? sender, EventArgs e)
        {
            if (_blueTimeSeconds > 0)
            {
                _blueTimeSeconds--;
                UpdateBlueTimerDisplay();
            }
            else
            {
                _blueTimer.Stop();
            }
        }

        private void UpdateRedTimerDisplay()
        {
            int minutes = _redTimeSeconds / 60;
            int seconds = _redTimeSeconds % 60;
            redTimerTextBlock.Text = $"{minutes}:{seconds:D2}";
        }

        private void UpdateBlueTimerDisplay()
        {
            int minutes = _blueTimeSeconds / 60;
            int seconds = _blueTimeSeconds % 60;
            blueTimerTextBlock.Text = $"{minutes}:{seconds:D2}";
        }

        private void RedTimerStart_Click(object sender, RoutedEventArgs e) => _redTimer.Start();
        private void RedTimerStop_Click(object sender, RoutedEventArgs e) => _redTimer.Stop();
        private void BlueTimerStart_Click(object sender, RoutedEventArgs e) => _blueTimer.Start();
        private void BlueTimerStop_Click(object sender, RoutedEventArgs e) => _blueTimer.Stop();

        // +/- изменяют секунды
        private void RedTimerPlus_Click(object sender, RoutedEventArgs e)
        {
            _redTimeSeconds += 1;
            UpdateRedTimerDisplay();
        }

        private void RedTimerMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_redTimeSeconds > 0)
            {
                _redTimeSeconds -= 1;
                UpdateRedTimerDisplay();
            }
        }

        private void BlueTimerPlus_Click(object sender, RoutedEventArgs e)
        {
            _blueTimeSeconds += 1;
            UpdateBlueTimerDisplay();
        }

        private void BlueTimerMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_blueTimeSeconds > 0)
            {
                _blueTimeSeconds -= 1;
                UpdateBlueTimerDisplay();
            }
        }
        #endregion

        #region Winners
        private void DeclareRedWinner_Click(object sender, RoutedEventArgs e)
        {
            // Показываем победителя на табло
            _scoreboardWindow?.ShowRedWinner(true);
            
            // Останавливаем таймер
            _mainTimer.Stop();
            _isMainTimerRunning = false;
            UpdateStartButtonIcon();
        }

        private void DeclareBlueWinner_Click(object sender, RoutedEventArgs e)
        {
            // Показываем победителя на табло
            _scoreboardWindow?.ShowBlueWinner(true);
            
            // Останавливаем таймер
            _mainTimer.Stop();
            _isMainTimerRunning = false;
            UpdateStartButtonIcon();
        }
        #endregion

        #region Scoreboard
        private void ToggleScoreboard_Click(object sender, RoutedEventArgs e)
        {
            if (_isScoreboardVisible)
            {
                HideScoreboard();
                toggleScoreboardButton.Content = "Показать табло";
            }
            else
            {
                ShowScoreboard();
                toggleScoreboardButton.Content = "Скрыть табло";
            }
        }

        private void ShowScoreboard()
        {
            // Закрываем окно с флагом
            _flagWindow?.Close();
            _flagWindow = null;

            // Создаем и показываем табло
            _scoreboardWindow = new ScoreboardWindow();
            
            // Позиционируем на втором мониторе
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length > 1)
            {
                var secondScreen = screens[1];
                _scoreboardWindow.Left = secondScreen.Bounds.Left;
                _scoreboardWindow.Top = secondScreen.Bounds.Top;
            }
            
            _scoreboardWindow.Show();
            _scoreboardWindow.WindowState = WindowState.Maximized;
            _isScoreboardVisible = true;

            // Обновляем данные на табло
            UpdateScoreboardData();
        }

        private void HideScoreboard()
        {
            _scoreboardWindow?.Close();
            _scoreboardWindow = null;
            _isScoreboardVisible = false;

            // Показываем флаг
            ShowFlagOnSecondScreen();
        }

        private void ShowFlagOnSecondScreen()
        {
            _flagWindow = new FlagWindow();
            
            // Позиционируем на втором мониторе
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length > 1)
            {
                var secondScreen = screens[1];
                _flagWindow.Left = secondScreen.Bounds.Left;
                _flagWindow.Top = secondScreen.Bounds.Top;
            }
            
            _flagWindow.Show();
            _flagWindow.WindowState = WindowState.Maximized;
        }

        private void UpdateScoreboardData()
        {
            if (_scoreboardWindow == null) return;

            _scoreboardWindow.UpdateScore(_redScore, _blueScore);
            _scoreboardWindow.UpdateTimer(mainTimerTextBlock.Text);
            _scoreboardWindow.UpdatePeriod(_currentPeriod);
            _scoreboardWindow.UpdateWrestlers(
                (redWrestlerComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "WRESTLER 1",
                (blueWrestlerComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "WRESTLER 2"
            );
            _scoreboardWindow.UpdateWeightCategory((weightCategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "31");
            _scoreboardWindow.UpdateStage(GetSelectedStage());
        }

        private void UpdateScoreboardScore()
        {
            _scoreboardWindow?.UpdateScore(_redScore, _blueScore);
        }

        private void UpdateScoreboardTimer()
        {
            _scoreboardWindow?.UpdateTimer(mainTimerTextBlock.Text);
        }

        private void UpdateScoreboardPeriod()
        {
            _scoreboardWindow?.UpdatePeriod(_currentPeriod);
        }

        private string GetSelectedStage()
        {
            if (qualificationRadioButton.IsChecked == true) return "QUALIFICATION";
            if (round16RadioButton.IsChecked == true) return "1/16 FINAL";
            if (round8RadioButton.IsChecked == true) return "1/8 FINAL";
            if (quarterFinalRadioButton.IsChecked == true) return "1/4 FINAL";
            if (semiFinalRadioButton.IsChecked == true) return "1/2 FINAL";
            if (repechageRadioButton.IsChecked == true) return "REPECHAGE";
            if (bronzeFinalRadioButton.IsChecked == true) return "FINAL 3-5";
            if (goldFinalRadioButton.IsChecked == true) return "FINAL 1-2";
            return "FINAL 1-2";
        }

        private void Stage_Changed(object sender, RoutedEventArgs e)
        {
            // Обновляем стадию на табло при изменении
            _scoreboardWindow?.UpdateStage(GetSelectedStage());
        }
        #endregion

        #region Menu
        private void Database_Click(object sender, RoutedEventArgs e)
        {
            var databaseWindow = new DatabaseWindow();
            databaseWindow.Owner = this;
            databaseWindow.ShowDialog();
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsAdmin)
            {
                var usersWindow = new UserManagementWindow();
                usersWindow.Owner = this;
                usersWindow.ShowDialog();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _scoreboardWindow?.Close();
            _flagWindow?.Close();
            System.Windows.Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string message = "ПОДСИСТЕМА ДЛЯ СУДЕЙСТВА СОРЕВНОВАНИЙ ПО ГРЕКО-РИМСКОЙ И ВОЛЬНОЙ БОРЬБЕ\n" +
                           "Версия: 1.0.0\n\n" +
                           "Махин Николай, ИСПП-21, 2025";
            
            System.Windows.MessageBox.Show(message, "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _scoreboardWindow?.Close();
            _flagWindow?.Close();
        }
    }
}
