using System.Net;

using System.Text;



namespace DPWrestlingScoreboard.Services.Tournament

{

    /// <summary>

    /// Экспорт листа для печати: весовая категория, этап и таблица участников.

    /// </summary>

    public static class TournamentHtmlExporter

    {

        public static string Export(TournamentTableResult table)

        {

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");

            sb.AppendLine("<html lang=\"ru\">");

            sb.AppendLine("<head>");

            sb.AppendLine("<meta charset=\"utf-8\"/>");

            sb.AppendLine($"<title>Весовая категория {table.WeightCategoryKg} кг</title>");

            sb.AppendLine($"<style>{GetStyles()}</style>");

            sb.AppendLine("</head>");

            sb.AppendLine("<body>");

            sb.AppendLine("<div class=\"page\">");

            sb.AppendLine($"<h1>Весовая категория {table.WeightCategoryKg}кг</h1>");

            if (!string.IsNullOrWhiteSpace(table.CompetitionStage))

                sb.AppendLine($"<p class=\"stage\">{Encode(table.CompetitionStage)}</p>");



            if (table.SystemType == TournamentSystemType.RoundRobin

                && table.RoundRobinMatches.Count == 0)

            {

                sb.AppendLine("<p class=\"hint\">Все пары в этой категории уже сыграны. Нажмите «Сбросить» для нового зачёта.</p>");

                sb.AppendLine("</div>");

                sb.AppendLine("</body>");

                sb.AppendLine("</html>");

                return sb.ToString();

            }



            AppendParticipantsTable(sb, TournamentService.BuildDisplayLines(table));

            sb.AppendLine("</div>");

            sb.AppendLine("</body>");

            sb.AppendLine("</html>");



            return sb.ToString();

        }



        private static void AppendParticipantsTable(StringBuilder sb, IReadOnlyList<TournamentDisplayLine> lines)

        {

            sb.AppendLine("<table class=\"participants-list\">");

            for (int i = 0; i < lines.Count; i++)

            {

                var p = lines[i].Participant;

                var nameCell = ParticipantNameFormatter.CombineNameLines(p.NameLine1, p.NameLine2);

                var regionCell = ParticipantNameFormatter.CombineRegionLines(p.RegionLine1, p.RegionLine2);

                var birth = string.IsNullOrEmpty(p.BirthDatePrint) ? Encode(p.BirthDateText) : Encode(p.BirthDatePrint);

                bool pairEnd = lines[i].GapAfter;

                var rowClass = pairEnd ? " class=\"pair-end\"" : string.Empty;



                sb.Append($"<tr{rowClass}>");

                sb.Append($"<td class=\"num\">{p.Number}</td>");

                sb.Append($"<td class=\"name\">{Encode(nameCell)}</td>");

                sb.Append($"<td class=\"birth\">{birth}</td>");

                sb.Append($"<td class=\"region\">{Encode(regionCell)}</td>");

                sb.AppendLine("</tr>");

                if (pairEnd)

                    sb.AppendLine("<tr class=\"pair-gap\"><td colspan=\"4\"></td></tr>");

            }

            sb.AppendLine("</table>");

        }



        private static string GetStyles() =>

            """

            body { font-family: 'Times New Roman', Times, serif; margin: 0; padding: 16px 24px; background: #fff; color: #000; }

            .page { max-width: 720px; margin: 0 auto; }

            h1 { font-size: 16pt; font-weight: bold; text-align: center; margin: 0 0 12px; }

            .stage { font-size: 12pt; text-align: center; margin: 0 0 18px; font-weight: normal; }

            .hint { font-size: 11pt; color: #444; font-style: italic; text-align: center; }

            table.participants-list { width: 100%; border-collapse: collapse; font-size: 11pt; }

            table.participants-list td { border: 1px solid #000; padding: 6px 8px; vertical-align: middle; word-wrap: break-word; overflow-wrap: anywhere; }

            table.participants-list tr.pair-gap td { border: none; height: 8px; padding: 0; background: #fff; }

            table.participants-list tr.pair-end td { border-bottom: 3px solid #000; }

            table.participants-list tr.pair-end + tr td { border-top: 2px solid #000; }

            table.participants-list td.num { width: 40px; text-align: center; }

            table.participants-list td.name { width: 42%; text-align: left; }

            table.participants-list td.birth { width: 100px; text-align: left; }

            table.participants-list td.region { text-align: left; }

            @media print {

                body { padding: 8mm; }

                table.participants-list tr { page-break-inside: avoid; }

            }

            """;



        private static string Encode(string text) => WebUtility.HtmlEncode(text);

    }

}


