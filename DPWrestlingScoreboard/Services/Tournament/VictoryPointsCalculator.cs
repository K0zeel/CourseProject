namespace DPWrestlingScoreboard.Services.Tournament
{
    /// <summary>
    /// Классификационные очки победы (упрощённые правила UWW по разнице в технических очках).
    /// </summary>
    public static class VictoryPointsCalculator
    {
        public static int Calculate(int winnerScore, int loserScore)
        {
            winnerScore = Math.Max(0, winnerScore);
            loserScore = Math.Max(0, loserScore);

            if (loserScore == 0 && winnerScore >= 8)
                return 5;

            int diff = winnerScore - loserScore;
            if (diff >= 8)
                return 4;
            if (diff >= 5)
                return 2;

            return 1;
        }
    }
}
