namespace DPWrestlingScoreboard.Services.Tournament
{
    public static class ParticipantNameFormatter
    {
        public static (string Line1, string Line2) FormatName(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                >= 3 => ($"{parts[0].ToUpperInvariant()} {parts[1]}", string.Join(' ', parts.Skip(2))),
                2 => ($"{parts[0].ToUpperInvariant()} {parts[1]}", string.Empty),
                1 => (parts[0].ToUpperInvariant(), string.Empty),
                _ => (fullName, string.Empty)
            };
        }

        public static string FormatBirthDate(DateTime birthDate) => birthDate.ToString("dd.MM.yy");

        public static (string Line1, string Line2) FormatRegion(string? region)
        {
            if (string.IsNullOrWhiteSpace(region))
                return ("—", string.Empty);

            var commaIndex = region.IndexOf(',');
            if (commaIndex >= 0)
            {
                return (
                    region[..commaIndex].Trim(),
                    region[(commaIndex + 1)..].Trim());
            }

            return (region.Trim(), string.Empty);
        }

        public static string FormatDisplayName(string fullName)
        {
            var (line1, line2) = FormatName(fullName);
            return string.IsNullOrEmpty(line2) ? line1 : $"{line1} {line2}";
        }

        public static string CombineNameLines(string line1, string line2) =>
            string.IsNullOrWhiteSpace(line2) ? line1 : $"{line1} {line2}";

        public static string CombineRegionLines(string line1, string line2) =>
            string.IsNullOrWhiteSpace(line2) ? line1 : $"{line1}, {line2}";
    }
}
