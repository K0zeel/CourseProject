namespace DPWrestlingScoreboard.Services.Tournament
{
    /// <summary>
    /// Состояние турнира в памяти для одной весовой категории.
    /// </summary>
    public class TournamentCategoryState
    {
        public int WeightCategoryId { get; init; }

        /// <summary>Очки победы по IdWrestler (накопленные при объявлении победителя).</summary>
        public Dictionary<int, int> VictoryPointsByWrestlerId { get; } = new();

        /// <summary>Уже сыгранные пары (ключ: minId_maxId) для круговой системы.</summary>
        public HashSet<string> PlayedPairs { get; } = new(StringComparer.Ordinal);

        /// <summary>Первая олимпийская сетка уже была сформирована (случайная расстановка).</summary>
        public bool OlympicFirstBracketGenerated { get; set; }

        public int GetVictoryPoints(int wrestlerId) =>
            VictoryPointsByWrestlerId.GetValueOrDefault(wrestlerId, 0);

        public void AddVictoryPoints(int wrestlerId, int points)
        {
            VictoryPointsByWrestlerId.TryGetValue(wrestlerId, out int current);
            VictoryPointsByWrestlerId[wrestlerId] = current + points;
        }

        public static string PairKey(int wrestlerId1, int wrestlerId2)
        {
            int a = Math.Min(wrestlerId1, wrestlerId2);
            int b = Math.Max(wrestlerId1, wrestlerId2);
            return $"{a}_{b}";
        }

        public void MarkPairPlayed(int wrestlerId1, int wrestlerId2) =>
            PlayedPairs.Add(PairKey(wrestlerId1, wrestlerId2));

        public bool IsPairPlayed(int wrestlerId1, int wrestlerId2) =>
            PlayedPairs.Contains(PairKey(wrestlerId1, wrestlerId2));
    }
}
