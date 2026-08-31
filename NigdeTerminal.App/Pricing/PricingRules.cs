namespace NigdeTerminal.App.Pricing;

public static class PricingRules
{
    public const decimal ShortDistanceAmount = 250m;
    public const decimal NightAmount = 300m;
    public const decimal StandardAmount = 500m;
    public const decimal CenterDepartureAmount = 700m;

    public const string ShortDistanceTariffName = "Yakın Mesafe Tarifesi";
    public const string CenterDepartureTariffName = "Merkez Kalkış Tarifesi";
    public const string NightTariffName = "Gece Tarifesi";
    public const string StandardTariffName = "Standart Tarife";

    public static readonly TimeOnly NightStart = new(1, 50);
    public static readonly TimeOnly NightEnd = new(7, 0);
}
