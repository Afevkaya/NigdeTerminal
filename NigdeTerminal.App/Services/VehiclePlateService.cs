using System.Text.RegularExpressions;

namespace NigdeTerminal.App.Services;

public sealed partial class VehiclePlateService
{
    public bool TryNormalize(string? input, out string normalizedPlate)
    {
        normalizedPlate = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var compactPlate = WhitespacePattern().Replace(input, string.Empty);
        var match = TurkishPlatePattern().Match(compactPlate);

        if (!match.Success
            || !int.TryParse(match.Groups["cityCode"].Value, out var cityCode)
            || cityCode is < 1 or > 81)
        {
            return false;
        }

        normalizedPlate = $"{cityCode:00} "
            + $"{match.Groups["letters"].Value.ToUpperInvariant()} "
            + match.Groups["number"].Value;

        return true;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();

    [GeneratedRegex(@"^(?<cityCode>\d{2})(?<letters>[A-Za-z]{1,3})(?<number>\d{2,4})$")]
    private static partial Regex TurkishPlatePattern();
}
