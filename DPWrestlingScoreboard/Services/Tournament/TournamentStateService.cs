namespace DPWrestlingScoreboard.Services.Tournament
{
    /// <summary>
    /// Глобальное хранилище турнирного состояния в памяти (очки победы, сыгранные пары).
    /// </summary>
    public sealed class TournamentStateService
    {
        private static readonly Lazy<TournamentStateService> Instance = new(() => new());
        public static TournamentStateService Current => Instance.Value;

        private readonly Dictionary<int, TournamentCategoryState> _byCategory = new();

        public TournamentCategoryState GetOrCreate(int weightCategoryId)
        {
            if (!_byCategory.TryGetValue(weightCategoryId, out var state))
            {
                state = new TournamentCategoryState { WeightCategoryId = weightCategoryId };
                _byCategory[weightCategoryId] = state;
            }
            return state;
        }

        public void RecordMatchResult(
            int weightCategoryId,
            int winnerWrestlerId,
            int loserWrestlerId,
            int victoryPoints)
        {
            var state = GetOrCreate(weightCategoryId);
            state.AddVictoryPoints(winnerWrestlerId, victoryPoints);
            state.MarkPairPlayed(winnerWrestlerId, loserWrestlerId);
        }

        public void ResetCategory(int weightCategoryId) => _byCategory.Remove(weightCategoryId);

        public void ResetAll() => _byCategory.Clear();
    }
}
