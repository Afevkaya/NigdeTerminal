using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Pricing;

public sealed class PricingService
{
    private static readonly HashSet<string> ShortDistanceCompanies = new(
        ["DERİNKUYU", "AKSARAY BİRLİK", "KARACAERLER"],
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> CenterDepartureCompanies = new(
        [
            "NİĞDE İNAN TURİZM",
            "NİĞDE AYDOĞANLAR SEYAHAT",
            "LÜKS EREĞLİ",
            "NET TURİZM SEYAHAT"
        ],
        StringComparer.OrdinalIgnoreCase);

    public PricingResult Calculate(
        Company company,
        DateTimeOffset departureDateTime,
        bool departedFromCenter)
    {
        ArgumentNullException.ThrowIfNull(company);

        if (ShortDistanceCompanies.Contains(company.Name))
        {
            return new PricingResult(
                PricingRules.ShortDistanceTariffName,
                PricingRules.ShortDistanceAmount);
        }

        if (departedFromCenter && CenterDepartureCompanies.Contains(company.Name))
        {
            return new PricingResult(
                PricingRules.CenterDepartureTariffName,
                PricingRules.CenterDepartureAmount);
        }

        var departureTime = TimeOnly.FromDateTime(departureDateTime.DateTime);

        if (departureTime >= PricingRules.NightStart && departureTime <= PricingRules.NightEnd)
        {
            return new PricingResult(
                PricingRules.NightTariffName,
                PricingRules.NightAmount);
        }

        return new PricingResult(
            PricingRules.StandardTariffName,
            PricingRules.StandardAmount);
    }
}
