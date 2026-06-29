namespace DPWrestlingScoreboard.Services.Tournament
{
    public enum TournamentSystemType
    {
        RoundRobin,
        Olympic
    }

    public class TournamentParticipant
    {
        public int WrestlerId { get; set; }
        public int Number { get; set; }
        public string FullName { get; set; } = string.Empty;
        /// <summary>Накопленные очки победы (для отображения при посеве).</summary>
        public int VictoryPoints { get; set; }
        public string NameLine1 { get; set; } = string.Empty;
        public string NameLine2 { get; set; } = string.Empty;
        public string BirthDateText { get; set; } = string.Empty;
        /// <summary>Дата для печати (дд.мм.гггг).</summary>
        public string BirthDatePrint { get; set; } = string.Empty;
        public string RegionLine1 { get; set; } = string.Empty;
        public string RegionLine2 { get; set; } = string.Empty;
    }

    public class RoundRobinMatch
    {
        public int Number { get; set; }
        public string Participant1 { get; set; } = string.Empty;
        public string Participant2 { get; set; } = string.Empty;
    }

    public class OlympicMatch
    {
        public int Number { get; set; }
        public string Participant1 { get; set; } = string.Empty;
        public string Participant2 { get; set; } = string.Empty;
    }

    public class OlympicRound
    {
        public string Name { get; set; } = string.Empty;
        public List<OlympicMatch> Matches { get; set; } = new();
    }

    public class TournamentTableResult
    {
        public TournamentSystemType SystemType { get; set; }
        public int WeightCategoryId { get; set; }
        public int WeightCategoryKg { get; set; }
        public string StyleName { get; set; } = "Греко-римская борьба";
        public string SystemName { get; set; } = string.Empty;
        /// <summary>Этап соревнований (для печати), задаётся вручную.</summary>
        public string CompetitionStage { get; set; } = string.Empty;
        public List<TournamentParticipant> Participants { get; set; } = new();
        /// <summary>Круговая система: участники без пары в текущем туре.</summary>
        public List<TournamentParticipant> RoundRobinByeParticipants { get; set; } = new();
        public List<RoundRobinMatch> RoundRobinMatches { get; set; } = new();
        public List<OlympicRound> OlympicRounds { get; set; } = new();
    }
}
