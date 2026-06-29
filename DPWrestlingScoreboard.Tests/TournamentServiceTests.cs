using DPWrestlingScoreboard.Services.Tournament;



namespace DPWrestlingScoreboard.Tests

{

    public class TournamentServiceTests

    {

        [Fact]

        public void BuildTable_FiveParticipants_UsesRoundRobin()

        {

            var wrestlers = CreateWrestlers(5);

            var service = new TournamentService(new Random(42));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            var table = service.BuildTable(wrestlers, 57, 1, state);



            Assert.Equal(TournamentSystemType.RoundRobin, table.SystemType);

            Assert.Equal("Круговая система", table.SystemName);

            Assert.NotEmpty(table.RoundRobinMatches);

            Assert.True(table.RoundRobinMatches.Count <= 2);

        }



        [Fact]

        public void BuildTable_SixParticipants_UsesOlympic()

        {

            var wrestlers = CreateWrestlers(6);

            var service = new TournamentService(new Random(42));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            var table = service.BuildTable(wrestlers, 65, 1, state);



            Assert.Equal(TournamentSystemType.Olympic, table.SystemType);

            Assert.Equal("Олимпийская система", table.SystemName);

            Assert.Equal(3, table.OlympicRounds.Count);

            Assert.Equal(4, table.OlympicRounds[0].Matches.Count);

            Assert.True(state.OlympicFirstBracketGenerated);

        }



        [Fact]

        public void RoundRobin_SecondGeneration_DoesNotRepeatPairs()

        {

            var wrestlers = CreateWrestlers(4);

            var service = new TournamentService(new Random(1));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            var first = service.BuildTable(wrestlers, 57, 1, state);

            var second = service.BuildTable(wrestlers, 57, 1, state);



            var firstPairs = ExtractPairs(first.RoundRobinMatches);

            var secondPairs = ExtractPairs(second.RoundRobinMatches);



            Assert.NotEmpty(firstPairs);

            Assert.NotEmpty(secondPairs);

            Assert.All(secondPairs, p => Assert.DoesNotContain(p, firstPairs));

        }



        [Fact]

        public void Olympic_SecondBracket_SeedsByVictoryPoints()

        {

            var wrestlers = CreateWrestlers(8);

            var service = new TournamentService(new Random(99));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };

            state.OlympicFirstBracketGenerated = true;



            state.AddVictoryPoints(1, 5);

            state.AddVictoryPoints(2, 4);

            state.AddVictoryPoints(3, 3);

            state.AddVictoryPoints(4, 2);

            state.AddVictoryPoints(5, 1);

            state.AddVictoryPoints(6, 1);

            state.AddVictoryPoints(7, 0);

            state.AddVictoryPoints(8, 0);



            var participants = wrestlers

                .Select(w => new TournamentParticipant

                {

                    WrestlerId = w.IdWrestler,

                    FullName = w.FullName,

                    VictoryPoints = state.GetVictoryPoints(w.IdWrestler)

                })

                .ToList();



            var (rounds, _) = service.BuildOlympicBracket(participants, state);

            var firstRound = rounds[0].Matches;



            var topSeed = ParticipantNameFormatter.FormatDisplayName("Борец 1");

            var secondSeed = ParticipantNameFormatter.FormatDisplayName("Борец 2");



            Assert.Contains(firstRound, m =>

                m.Participant1 == topSeed || m.Participant2 == topSeed);

            Assert.Contains(firstRound, m =>

                m.Participant1 == secondSeed || m.Participant2 == secondSeed);



            var topMatch = firstRound.First(m =>

                m.Participant1 == topSeed || m.Participant2 == topSeed);

            var opponent = topMatch.Participant1 == topSeed

                ? topMatch.Participant2

                : topMatch.Participant1;

            Assert.Equal(ParticipantNameFormatter.FormatDisplayName("Борец 8"), opponent);
        }

        [Fact]
        public void GetSeedNumbersByBracketSlot_ForEight_MatchesStandardBracket()
        {
            var seeds = TournamentService.GetSeedNumbersByBracketSlot(8);
            Assert.Equal(new[] { 1, 8, 4, 5, 2, 7, 3, 6 }, seeds);

        }



        [Fact]

        public void VictoryPointsCalculator_Fall_ReturnsFive()

        {

            Assert.Equal(5, VictoryPointsCalculator.Calculate(10, 0));

        }



        [Fact]

        public void VictoryPointsCalculator_CloseWin_ReturnsOne()

        {

            Assert.Equal(1, VictoryPointsCalculator.Calculate(3, 2));

        }



        [Fact]

        public void RecordMatchResult_AccumulatesVictoryPoints()

        {

            TournamentStateService.Current.ResetAll();

            TournamentStateService.Current.RecordMatchResult(10, 1, 2, 4);

            TournamentStateService.Current.RecordMatchResult(10, 1, 3, 2);



            var state = TournamentStateService.Current.GetOrCreate(10);

            Assert.Equal(6, state.GetVictoryPoints(1));

            Assert.True(state.IsPairPlayed(1, 2));

            Assert.True(state.IsPairPlayed(1, 3));

        }



        [Fact]

        public void BuildTable_OneParticipant_RoundRobinNoMatches()

        {

            var service = new TournamentService();

            var table = service.BuildTable(CreateWrestlers(1), 57, 1, new TournamentCategoryState { WeightCategoryId = 1 });

            Assert.Equal(TournamentSystemType.RoundRobin, table.SystemType);

            Assert.Empty(table.RoundRobinMatches);

            Assert.Single(table.Participants);

        }



        [Fact]

        public void BuildTable_EmptyCategory_ReturnsEmptyTable()

        {

            var service = new TournamentService();

            var table = service.BuildTable(new List<Models.Wrestler>(), 31, 1, new TournamentCategoryState { WeightCategoryId = 1 });

            Assert.Empty(table.Participants);

            Assert.Equal("—", table.SystemName);

        }



        [Fact]

        public void GetSeedOrder_ForEight_ReturnsStandardPairs()

        {

            var order = TournamentService.GetSeedOrder(8);

            Assert.Equal(new[] { 0, 7, 3, 4, 1, 6, 2, 5 }, order);

        }



        [Fact]

        public void RoundRobin_DisplayOrder_MatchesCurrentTourPairs()

        {

            var wrestlers = CreateWrestlers(4);

            var service = new TournamentService(new Random(1));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            var table = service.BuildTable(wrestlers, 57, 1, state);



            Assert.Equal(2, table.RoundRobinMatches.Count);

            Assert.Equal(4, table.Participants.Count);

            Assert.Equal(

                table.RoundRobinMatches[0].Participant1,

                ParticipantNameFormatter.CombineNameLines(

                    table.Participants[0].NameLine1, table.Participants[0].NameLine2));

            Assert.Equal(

                table.RoundRobinMatches[0].Participant2,

                ParticipantNameFormatter.CombineNameLines(

                    table.Participants[1].NameLine1, table.Participants[1].NameLine2));

        }



        [Fact]

        public void RoundRobin_FiveParticipants_ShowsOneByeBelowPairs()

        {

            var wrestlers = CreateWrestlers(5);

            var service = new TournamentService(new Random(1));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            var table = service.BuildTable(wrestlers, 57, 1, state);



            Assert.Equal(4, table.Participants.Count);

            Assert.Single(table.RoundRobinByeParticipants);

            var display = TournamentService.BuildDisplayLines(table);

            Assert.Equal(5, display.Count);

            Assert.Equal(5, display[4].Participant.Number);
            Assert.True(display[3].GapAfter);
            Assert.False(display[4].GapAfter);

        }



        [Fact]

        public void Olympic_SixParticipants_DisplayMatchesRoundRobinStyle()

        {

            var wrestlers = CreateWrestlers(6);

            var service = new TournamentService(new Random(42));

            var table = service.BuildTable(wrestlers, 65, 1, new TournamentCategoryState { WeightCategoryId = 1 });



            Assert.Equal(TournamentSystemType.Olympic, table.SystemType);

            var display = TournamentService.BuildDisplayLines(table);



            Assert.Equal(6, display.Count);

            Assert.All(display, line => Assert.NotEqual("—", line.Participant.NameLine1));

            Assert.Equal(6, display[^1].Participant.Number);

            Assert.Contains(display, l => l.GapAfter);

            Assert.False(display[^1].GapAfter);

        }



        [Fact]

        public void Olympic_SecondBuild_OrdersParticipantsByBracketSlots()

        {

            var wrestlers = CreateWrestlers(8);

            var service = new TournamentService(new Random(999));

            var state = new TournamentCategoryState { WeightCategoryId = 1 };



            service.BuildTable(wrestlers, 65, 1, state);



            state.AddVictoryPoints(1, 10);

            state.AddVictoryPoints(2, 5);

            var table = service.BuildTable(wrestlers, 65, 1, state);



            var top = ParticipantNameFormatter.FormatDisplayName("Борец 1");

            var weakest = ParticipantNameFormatter.FormatDisplayName("Борец 8");



            Assert.Equal(top, ParticipantNameFormatter.CombineNameLines(

                table.Participants[0].NameLine1, table.Participants[0].NameLine2));

            Assert.Equal(weakest, ParticipantNameFormatter.CombineNameLines(

                table.Participants[1].NameLine1, table.Participants[1].NameLine2));

            Assert.Equal(top, table.OlympicRounds[0].Matches[0].Participant1);

            Assert.Equal(weakest, table.OlympicRounds[0].Matches[0].Participant2);

        }



        [Fact]

        public void SelectNonOverlappingPairs_PicksDisjointMatches()

        {

            var pairs = new List<(int, int)>

            {

                (1, 2), (1, 3), (4, 5), (2, 3)

            };

            var selected = TournamentService.SelectNonOverlappingPairs(pairs);

            Assert.Equal(2, selected.Count);

            Assert.Equal((1, 2), selected[0]);

            Assert.Equal((4, 5), selected[1]);

        }



        private static HashSet<string> ExtractPairs(IEnumerable<RoundRobinMatch> matches)

        {

            var set = new HashSet<string>(StringComparer.Ordinal);

            foreach (var m in matches)

            {

                var names = new[] { m.Participant1, m.Participant2 }.OrderBy(x => x, StringComparer.Ordinal).ToArray();

                set.Add($"{names[0]}|{names[1]}");

            }

            return set;

        }



        private static List<Models.Wrestler> CreateWrestlers(int count)

        {

            var list = new List<Models.Wrestler>();

            for (int i = 0; i < count; i++)

            {

                list.Add(new Models.Wrestler

                {

                    IdWrestler = i + 1,

                    FullName = $"Борец {i + 1}",

                    IdWeightCategory = 1,

                    IdRegion = 1,

                    BirthDate = new DateTime(2005, 1, 1)

                });

            }

            return list;

        }

    }

}


