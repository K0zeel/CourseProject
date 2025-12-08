using KPWrestlingScoreboard.Services;

namespace KPWrestlingScoreboard.Tests
{
    /// <summary>
    /// Юнит-тесты для ScoreboardService
    /// </summary>
    public class ScoreboardServiceTests
    {
        /// <summary>
        /// Тест: Добавление очков увеличивает счёт
        /// </summary>
        [Fact]
        public void UpdateScore_AddsPoints_ScoreIncreases()
        {
            // Arrange - подготовка
            var service = new ScoreboardService();

            // Act - действие
            service.UpdateRedScore(3);
            service.UpdateBlueScore(2);

            // Assert - проверка
            Assert.Equal(3, service.RedScore);
            Assert.Equal(2, service.BlueScore);
        }

        /// <summary>
        /// Тест: Форматирование времени в MM:SS
        /// </summary>
        [Fact]
        public void FormatTime_ReturnsCorrectFormat()
        {
            // Arrange
            var service = new ScoreboardService();

            // Act & Assert
            Assert.Equal("6:00", service.FormatTime(360));
            Assert.Equal("3:00", service.FormatTime(180));
            Assert.Equal("1:30", service.FormatTime(90));
            Assert.Equal("0:00", service.FormatTime(0));
        }

        /// <summary>
        /// Тест: Установка периода меняет время матча
        /// </summary>
        [Fact]
        public void SetPeriod_ChangesMatchTime()
        {
            // Arrange
            var service = new ScoreboardService();

            // Act - Period 1
            service.SetPeriod(1);
            Assert.Equal(360, service.TimeSeconds); // 6 минут

            // Act - Period 2
            service.SetPeriod(2);
            Assert.Equal(180, service.TimeSeconds); // 3 минуты
        }

        /// <summary>
        /// Тест: Определение победителя по счёту
        /// </summary>
        [Fact]
        public void GetWinnerByScore_DeterminesWinner()
        {
            // Arrange
            var service = new ScoreboardService();

            // Красный ведёт
            service.UpdateRedScore(5);
            service.UpdateBlueScore(3);
            Assert.Equal("RED", service.GetWinnerByScore());

            // Сброс и синий ведёт
            service.Reset();
            service.UpdateRedScore(2);
            service.UpdateBlueScore(7);
            Assert.Equal("BLUE", service.GetWinnerByScore());
        }

        /// <summary>
        /// Тест: Сброс очищает все данные матча
        /// </summary>
        [Fact]
        public void Reset_ClearsAllMatchData()
        {
            // Arrange - заполняем данные
            var service = new ScoreboardService();
            service.UpdateRedScore(10);
            service.UpdateBlueScore(8);
            service.SetPeriod(2);
            service.DeclareRedWinner();

            // Act - сброс
            service.Reset();

            // Assert - проверяем что всё очищено
            Assert.Equal(0, service.RedScore);
            Assert.Equal(0, service.BlueScore);
            Assert.Equal(1, service.Period);
            Assert.Equal(360, service.TimeSeconds);
            Assert.False(service.IsRedWinner);
            Assert.False(service.IsBlueWinner);
        }
    }
}
