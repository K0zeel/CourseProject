namespace DPWrestlingScoreboard.Services.Tournament

{

    /// <summary>

    /// Строка таблицы для печати с разделителем после блока пары.

    /// </summary>

    public class TournamentDisplayLine

    {

        public TournamentParticipant Participant { get; init; } = null!;

        public bool GapAfter { get; init; }

    }

}


