namespace KPWrestlingScoreboard.Services
{
    /// <summary>
    /// Сервис для управления данными табло
    /// </summary>
    public class ScoreboardService
    {
        public int RedScore { get; private set; }
        public int BlueScore { get; private set; }
        public int Period { get; private set; } = 1;
        public int TimeSeconds { get; private set; }
        public string RedWrestler { get; private set; } = "";
        public string BlueWrestler { get; private set; } = "";
        public string WeightCategory { get; private set; } = "";
        public string Stage { get; private set; } = "";
        public string Style { get; private set; } = "FS";
        public bool IsBreakActive { get; private set; }
        public bool IsRedWinner { get; private set; }
        public bool IsBlueWinner { get; private set; }

        /// <summary>
        /// Обновляет счёт красного угла
        /// </summary>
        public void UpdateRedScore(int delta)
        {
            RedScore = Math.Max(0, RedScore + delta);
        }

        /// <summary>
        /// Обновляет счёт синего угла
        /// </summary>
        public void UpdateBlueScore(int delta)
        {
            BlueScore = Math.Max(0, BlueScore + delta);
        }

        /// <summary>
        /// Устанавливает период матча
        /// </summary>
        public void SetPeriod(int period)
        {
            if (period >= 1 && period <= 2)
            {
                Period = period;
                // Первый период - 6 минут, второй - 3 минуты
                TimeSeconds = period == 1 ? 360 : 180;
            }
        }

        /// <summary>
        /// Форматирует время в строку MM:SS
        /// </summary>
        public string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes}:{secs:D2}";
        }

        /// <summary>
        /// Устанавливает время в секундах
        /// </summary>
        public void SetTime(int seconds)
        {
            TimeSeconds = Math.Max(0, seconds);
        }

        /// <summary>
        /// Уменьшает время на 1 секунду
        /// </summary>
        public bool Tick()
        {
            if (TimeSeconds > 0)
            {
                TimeSeconds--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Устанавливает борцов
        /// </summary>
        public void SetWrestlers(string red, string blue)
        {
            RedWrestler = red ?? "";
            BlueWrestler = blue ?? "";
        }

        /// <summary>
        /// Устанавливает весовую категорию
        /// </summary>
        public void SetWeightCategory(string weight)
        {
            WeightCategory = weight ?? "";
        }

        /// <summary>
        /// Устанавливает стадию соревнований
        /// </summary>
        public void SetStage(string stage)
        {
            Stage = stage ?? "";
        }

        /// <summary>
        /// Активирует перерыв
        /// </summary>
        public void StartBreak(int seconds = 30)
        {
            IsBreakActive = true;
            TimeSeconds = seconds;
        }

        /// <summary>
        /// Отменяет перерыв
        /// </summary>
        public void EndBreak()
        {
            IsBreakActive = false;
        }

        /// <summary>
        /// Объявляет победителя красного угла
        /// </summary>
        public void DeclareRedWinner()
        {
            IsRedWinner = true;
            IsBlueWinner = false;
        }

        /// <summary>
        /// Объявляет победителя синего угла
        /// </summary>
        public void DeclareBlueWinner()
        {
            IsBlueWinner = true;
            IsRedWinner = false;
        }

        /// <summary>
        /// Определяет победителя по текущему счёту
        /// </summary>
        public string? GetWinnerByScore()
        {
            if (RedScore > BlueScore) return "RED";
            if (BlueScore > RedScore) return "BLUE";
            return null; // Ничья
        }

        /// <summary>
        /// Сбрасывает состояние матча
        /// </summary>
        public void Reset()
        {
            RedScore = 0;
            BlueScore = 0;
            Period = 1;
            TimeSeconds = 360;
            IsBreakActive = false;
            IsRedWinner = false;
            IsBlueWinner = false;
        }
    }
}

