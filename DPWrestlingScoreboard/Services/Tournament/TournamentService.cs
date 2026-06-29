using DPWrestlingScoreboard.Models;

namespace DPWrestlingScoreboard.Services.Tournament
{
    public class TournamentService
    {
        public const int RoundRobinMaxParticipants = 5;
        public const int OlympicMinParticipants = 6;

        private readonly Random _random;

        public TournamentService() : this(Random.Shared) { }

        public TournamentService(Random random)
        {
            _random = random;
        }

        public TournamentTableResult BuildTable(
            IReadOnlyList<Wrestler> wrestlers,
            int weightCategoryKg,
            int weightCategoryId,
            TournamentCategoryState? state = null)
        {
            state ??= TournamentStateService.Current.GetOrCreate(weightCategoryId);

            var participants = wrestlers
                .Select(w => CreateParticipant(w, state))
                .ToList();

            var result = new TournamentTableResult
            {
                WeightCategoryKg = weightCategoryKg,
                WeightCategoryId = weightCategoryId
            };

            if (participants.Count == 0)
            {
                result.SystemType = TournamentSystemType.RoundRobin;
                result.SystemName = "—";
                result.Participants = participants;
                return result;
            }

            bool useOlympic = participants.Count > RoundRobinMaxParticipants;

            if (useOlympic)
            {
                if (!state.OlympicFirstBracketGenerated)
                    Shuffle(participants);
                else
                {
                    participants = participants
                        .OrderByDescending(p => p.VictoryPoints)
                        .ThenBy(p => p.WrestlerId)
                        .ToList();
                }
            }
            else
            {
                Shuffle(participants);
            }

            if (!useOlympic)
            {
                for (int i = 0; i < participants.Count; i++)
                    participants[i].Number = i + 1;

                result.SystemType = TournamentSystemType.RoundRobin;
                result.SystemName = "Круговая система";
                result.RoundRobinMatches = BuildRoundRobinMatches(participants, state);
                var (paired, bye) = OrderParticipantsForRoundRobinDisplay(
                    participants, result.RoundRobinMatches);
                result.Participants = paired;
                result.RoundRobinByeParticipants = bye;
                return result;
            }

            var (rounds, ordered) = BuildOlympicBracket(participants, state);
            result.Participants = ordered;
            result.OlympicRounds = rounds;
            result.SystemType = TournamentSystemType.Olympic;
            result.SystemName = "Олимпийская система";
            return result;
        }

        private static TournamentParticipant CreateParticipant(Wrestler wrestler, TournamentCategoryState state)
        {
            var (nameLine1, nameLine2) = ParticipantNameFormatter.FormatName(wrestler.FullName);
            var (regionLine1, regionLine2) = ParticipantNameFormatter.FormatRegion(wrestler.Region?.RegionName);

            return new TournamentParticipant
            {
                WrestlerId = wrestler.IdWrestler,
                FullName = wrestler.FullName,
                NameLine1 = nameLine1,
                NameLine2 = nameLine2,
                BirthDateText = ParticipantNameFormatter.FormatBirthDate(wrestler.BirthDate),
                BirthDatePrint = wrestler.BirthDate.ToString("dd.MM.yyyy"),
                RegionLine1 = regionLine1,
                RegionLine2 = regionLine2,
                VictoryPoints = state.GetVictoryPoints(wrestler.IdWrestler)
            };
        }

        /// <summary>
        /// Формирует следующий тур круговой системы без повторяющихся пар.
        /// </summary>
        public List<RoundRobinMatch> BuildRoundRobinMatches(
            IReadOnlyList<TournamentParticipant> participants,
            TournamentCategoryState state)
        {
            var remaining = new List<(int Id1, int Id2)>();
            for (int i = 0; i < participants.Count; i++)
            {
                for (int j = i + 1; j < participants.Count; j++)
                {
                    int id1 = participants[i].WrestlerId;
                    int id2 = participants[j].WrestlerId;
                    if (!state.IsPairPlayed(id1, id2))
                        remaining.Add((id1, id2));
                }
            }

            var roundPairs = SelectNonOverlappingPairs(remaining);
            var byId = participants.ToDictionary(p => p.WrestlerId);

            var matches = new List<RoundRobinMatch>();
            int number = 1;
            foreach (var (id1, id2) in roundPairs)
            {
                state.MarkPairPlayed(id1, id2);
                matches.Add(new RoundRobinMatch
                {
                    Number = number++,
                    Participant1 = ParticipantNameFormatter.FormatDisplayName(byId[id1].FullName),
                    Participant2 = ParticipantNameFormatter.FormatDisplayName(byId[id2].FullName)
                });
            }

            return matches;
        }

        /// <summary>
        /// Участники для печати/экрана: пары текущего тура (1–2, 3–4…) и свободные ниже.
        /// </summary>
        public static (List<TournamentParticipant> Paired, List<TournamentParticipant> Bye)
            OrderParticipantsForRoundRobinDisplay(
                IReadOnlyList<TournamentParticipant> allParticipants,
                IReadOnlyList<RoundRobinMatch> matches)
        {
            if (matches.Count == 0)
            {
                if (allParticipants.Count == 1)
                    return (new List<TournamentParticipant> { CloneParticipant(allParticipants[0], 1) }, new());
                return (new List<TournamentParticipant>(), new List<TournamentParticipant>());
            }

            var rows = new List<TournamentParticipant>(matches.Count * 2);
            var playingIds = new HashSet<int>();
            int number = 1;

            foreach (var match in matches)
            {
                if (TryFindByDisplayName(allParticipants, match.Participant1) is { } p1)
                {
                    rows.Add(CloneParticipant(p1, number++));
                    playingIds.Add(p1.WrestlerId);
                }

                if (TryFindByDisplayName(allParticipants, match.Participant2) is { } p2)
                {
                    rows.Add(CloneParticipant(p2, number++));
                    playingIds.Add(p2.WrestlerId);
                }
            }

            var bye = new List<TournamentParticipant>();
            foreach (var p in allParticipants)
            {
                if (p.WrestlerId == 0 || playingIds.Contains(p.WrestlerId))
                    continue;
                bye.Add(CloneParticipant(p, number++));
            }

            return (rows, bye);
        }

        /// <summary>
        /// Единый вид таблицы: блоки по парам тура (1–2 борца), разделитель, свободные — отдельной строкой ниже.
        /// </summary>
        public static List<TournamentDisplayLine> BuildDisplayLines(TournamentTableResult table)
        {
            var lines = new List<TournamentDisplayLine>();
            var allReal = CollectRealParticipants(table);

            IReadOnlyList<(string Name1, string Name2)> pairs = table.SystemType == TournamentSystemType.Olympic
                ? (table.OlympicRounds.FirstOrDefault()?.Matches ?? new List<OlympicMatch>())
                    .Select(m => (m.Participant1, m.Participant2)).ToList()
                : table.RoundRobinMatches.Select(m => (m.Participant1, m.Participant2)).ToList();

            int number = 1;
            for (int matchIndex = 0; matchIndex < pairs.Count; matchIndex++)
            {
                var (name1, name2) = pairs[matchIndex];
                var block = new List<TournamentParticipant>();

                if (!IsByeName(name1) && TryFindByDisplayName(allReal, name1) is { } p1)
                    block.Add(CloneParticipant(p1, number++));

                if (!IsByeName(name2) && TryFindByDisplayName(allReal, name2) is { } p2)
                    block.Add(CloneParticipant(p2, number++));

                bool hasMoreBlocks = HasMoreDisplayBlocks(pairs, matchIndex);
                bool hasByeAfter = table.SystemType == TournamentSystemType.RoundRobin
                    && table.RoundRobinByeParticipants.Count > 0;

                for (int i = 0; i < block.Count; i++)
                {
                    bool gapAfter = i == block.Count - 1 && (hasMoreBlocks || hasByeAfter);
                    lines.Add(new TournamentDisplayLine
                    {
                        Participant = block[i],
                        GapAfter = gapAfter
                    });
                }
            }

            if (table.SystemType == TournamentSystemType.RoundRobin)
            {
                for (int i = 0; i < table.RoundRobinByeParticipants.Count; i++)
                {
                    bool gapAfter = i < table.RoundRobinByeParticipants.Count - 1;
                    lines.Add(new TournamentDisplayLine
                    {
                        Participant = CloneParticipant(table.RoundRobinByeParticipants[i], number++),
                        GapAfter = gapAfter
                    });
                }
            }

            return lines;
        }

        private static List<TournamentParticipant> CollectRealParticipants(TournamentTableResult table)
        {
            var list = new List<TournamentParticipant>();
            foreach (var p in table.Participants)
            {
                if (p.WrestlerId > 0)
                    list.Add(p);
            }

            foreach (var p in table.RoundRobinByeParticipants)
            {
                if (p.WrestlerId > 0 && list.All(x => x.WrestlerId != p.WrestlerId))
                    list.Add(p);
            }

            return list;
        }

        private static bool IsByeName(string name) =>
            string.IsNullOrWhiteSpace(name) || name == "—" || name == "________________";

        private static bool HasMoreDisplayBlocks(
            IReadOnlyList<(string Name1, string Name2)> pairs, int currentMatchIndex)
        {
            for (int j = currentMatchIndex + 1; j < pairs.Count; j++)
            {
                var (name1, name2) = pairs[j];
                if (!IsByeName(name1) || !IsByeName(name2))
                    return true;
            }

            return false;
        }

        private static TournamentParticipant? TryFindByDisplayName(
            IReadOnlyList<TournamentParticipant> participants, string displayName)
        {
            foreach (var p in participants)
            {
                if (ParticipantNameFormatter.FormatDisplayName(p.FullName) == displayName)
                    return p;
            }

            return null;
        }

        private static TournamentParticipant CloneParticipant(TournamentParticipant source, int number) =>
            new()
            {
                WrestlerId = source.WrestlerId,
                Number = number,
                FullName = source.FullName,
                NameLine1 = source.NameLine1,
                NameLine2 = source.NameLine2,
                BirthDateText = source.BirthDateText,
                BirthDatePrint = source.BirthDatePrint,
                RegionLine1 = source.RegionLine1,
                RegionLine2 = source.RegionLine2,
                VictoryPoints = source.VictoryPoints
            };

        public static List<(int Id1, int Id2)> SelectNonOverlappingPairs(List<(int Id1, int Id2)> pairs)
        {
            var selected = new List<(int Id1, int Id2)>();
            var used = new HashSet<int>();

            foreach (var pair in pairs)
            {
                if (used.Contains(pair.Id1) || used.Contains(pair.Id2))
                    continue;

                selected.Add(pair);
                used.Add(pair.Id1);
                used.Add(pair.Id2);
            }

            return selected;
        }

        public (List<OlympicRound> Rounds, List<TournamentParticipant> OrderedParticipants) BuildOlympicBracket(
            IReadOnlyList<TournamentParticipant> participants,
            TournamentCategoryState state)
        {
            int bracketSize = NextPowerOfTwo(participants.Count);
            var slots = new TournamentParticipant?[bracketSize];

            if (!state.OlympicFirstBracketGenerated)
            {
                PlaceRandomly(slots, participants);
                state.OlympicFirstBracketGenerated = true;
            }
            else
            {
                PlaceByVictoryPoints(slots, participants, bracketSize);
            }

            var firstRound = new List<OlympicMatch>();
            int matchNumber = 1;

            for (int i = 0; i < bracketSize; i += 2)
            {
                firstRound.Add(new OlympicMatch
                {
                    Number = matchNumber++,
                    Participant1 = FormatSlot(slots[i]),
                    Participant2 = FormatSlot(slots[i + 1])
                });
            }

            var rounds = new List<OlympicRound>
            {
                new()
                {
                    Name = GetRoundName(bracketSize / 2),
                    Matches = firstRound
                }
            };

            int matchesInRound = firstRound.Count;
            while (matchesInRound > 1)
            {
                matchesInRound /= 2;
                var roundMatches = new List<OlympicMatch>();
                for (int i = 0; i < matchesInRound; i++)
                {
                    roundMatches.Add(new OlympicMatch
                    {
                        Number = i + 1,
                        Participant1 = "________________",
                        Participant2 = "________________"
                    });
                }

                rounds.Add(new OlympicRound
                {
                    Name = GetRoundName(matchesInRound),
                    Matches = roundMatches
                });
            }

            return (rounds, BuildParticipantListFromSlots(slots));
        }

        private static List<TournamentParticipant> BuildParticipantListFromSlots(TournamentParticipant?[] slots)
        {
            var list = new List<TournamentParticipant>(slots.Length);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    var p = slots[i]!;
                    p.Number = i + 1;
                    list.Add(p);
                }
                else
                {
                    list.Add(new TournamentParticipant
                    {
                        Number = i + 1,
                        WrestlerId = 0,
                        FullName = "—",
                        NameLine1 = "—",
                        NameLine2 = string.Empty,
                        BirthDateText = string.Empty,
                        BirthDatePrint = string.Empty,
                        RegionLine1 = "—",
                        RegionLine2 = string.Empty
                    });
                }
            }

            return list;
        }

        private void PlaceRandomly(TournamentParticipant?[] slots, IReadOnlyList<TournamentParticipant> participants)
        {
            for (int i = 0; i < participants.Count; i++)
                slots[i] = participants[i];
        }

        private void PlaceByVictoryPoints(
            TournamentParticipant?[] slots,
            IReadOnlyList<TournamentParticipant> participants,
            int bracketSize)
        {
            var ranked = participants
                .OrderByDescending(p => p.VictoryPoints)
                .ThenBy(p => p.WrestlerId)
                .ToList();

            var seedNumbersBySlot = GetSeedNumbersByBracketSlot(bracketSize);
            for (int slot = 0; slot < seedNumbersBySlot.Count && slot < slots.Length; slot++)
            {
                int rankIndex = seedNumbersBySlot[slot] - 1;
                if (rankIndex < ranked.Count)
                    slots[slot] = ranked[rankIndex];
            }
        }

        /// <summary>
        /// Номера сидов (1..N) по позициям сетки: 1 — лидер, N — слабейший.
        /// </summary>
        public static List<int> GetSeedNumbersByBracketSlot(int bracketSize)
        {
            if (bracketSize == 2)
                return new List<int> { 1, 2 };

            var half = GetSeedNumbersByBracketSlot(bracketSize / 2);
            var result = new List<int>(bracketSize);
            foreach (int seed in half)
            {
                result.Add(seed);
                result.Add(bracketSize + 1 - seed);
            }

            return result;
        }

        private static string FormatSlot(TournamentParticipant? participant) =>
            participant == null
                ? "—"
                : ParticipantNameFormatter.FormatDisplayName(participant.FullName);

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static int NextPowerOfTwo(int value)
        {
            int size = 2;
            while (size < value)
                size *= 2;
            return size;
        }

        public static List<int> GetSeedOrder(int bracketSize)
        {
            if (bracketSize == 2)
                return new List<int> { 0, 1 };

            var half = GetSeedOrder(bracketSize / 2);
            var result = new List<int>(bracketSize);
            foreach (var seed in half)
            {
                result.Add(seed);
                result.Add(bracketSize - 1 - seed);
            }

            return result;
        }

        internal static string GetRoundName(int matchesInRound) => matchesInRound switch
        {
            8 => "1/8 финала",
            4 => "1/4 финала",
            2 => "1/2 финала",
            1 => "Финал",
            _ => $"Раунд ({matchesInRound} пар)"
        };
    }
}
