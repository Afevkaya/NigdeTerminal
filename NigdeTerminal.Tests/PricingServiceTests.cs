using NigdeTerminal.App.Models;
using NigdeTerminal.App.Pricing;

namespace NigdeTerminal.Tests;

public sealed class PricingServiceTests
{
    private readonly PricingService _pricingService = new();

    public static TheoryData<int, int, decimal, string> TimeBasedPricingCases => new()
    {
        { 1, 49, PricingRules.StandardAmount, PricingRules.StandardTariffName },
        { 1, 50, PricingRules.NightAmount, PricingRules.NightTariffName },
        { 3, 0, PricingRules.NightAmount, PricingRules.NightTariffName },
        { 7, 0, PricingRules.NightAmount, PricingRules.NightTariffName },
        { 7, 1, PricingRules.StandardAmount, PricingRules.StandardTariffName }
    };

    [Theory]
    [MemberData(nameof(TimeBasedPricingCases))]
    public void Normal_company_is_priced_by_departure_time(
        int hour,
        int minute,
        decimal expectedAmount,
        string expectedTariffName)
    {
        var result = _pricingService.Calculate(
            CreateCompany("Normal Şehirlerarası Firma"),
            CreateDeparture(hour, minute),
            departedFromCenter: false);

        AssertPricing(result, expectedAmount, expectedTariffName);
    }

    [Theory]
    [InlineData("Derinkuyu")]
    [InlineData("Aksaray Birlik")]
    [InlineData("Karacaerler")]
    public void Short_distance_company_uses_short_distance_tariff(string companyName)
    {
        var result = _pricingService.Calculate(
            CreateCompany(companyName),
            CreateDeparture(12, 0),
            departedFromCenter: false);

        AssertPricing(
            result,
            PricingRules.ShortDistanceAmount,
            PricingRules.ShortDistanceTariffName);
    }

    [Theory]
    [InlineData("NİĞDE İNAN TURİZM")]
    [InlineData("NİĞDE AYDOĞANLAR SEYAHAT")]
    [InlineData("LÜKS EREĞLİ")]
    [InlineData("NET TURİZM SEYAHAT")]
    public void Company_that_can_depart_from_center_uses_center_tariff_when_selected(
        string companyName)
    {
        var result = _pricingService.Calculate(
            CreateCompany(companyName, canDepartFromCenter: true),
            CreateDeparture(12, 0),
            departedFromCenter: true);

        AssertPricing(
            result,
            PricingRules.CenterDepartureAmount,
            PricingRules.CenterDepartureTariffName);
    }

    [Theory]
    [InlineData("NİĞDE İNAN TURİZM")]
    [InlineData("NİĞDE AYDOĞANLAR SEYAHAT")]
    [InlineData("NET TURİZM SEYAHAT")]
    [InlineData("Normal Şehirlerarası Firma")]
    public void Intercity_company_uses_standard_tariff_during_day_when_center_is_not_selected(
        string companyName)
    {
        var result = _pricingService.Calculate(
            CreateCompany(companyName, canDepartFromCenter: true),
            CreateDeparture(12, 0),
            departedFromCenter: false);

        AssertPricing(
            result,
            PricingRules.StandardAmount,
            PricingRules.StandardTariffName);
    }

    [Fact]
    public void Short_distance_tariff_has_priority_over_other_rules()
    {
        var result = _pricingService.Calculate(
            CreateCompany("Derinkuyu", canDepartFromCenter: true),
            CreateDeparture(3, 0),
            departedFromCenter: true);

        AssertPricing(
            result,
            PricingRules.ShortDistanceAmount,
            PricingRules.ShortDistanceTariffName);
    }

    [Fact]
    public void Valid_center_departure_has_priority_over_night_tariff()
    {
        var result = _pricingService.Calculate(
            CreateCompany("NİĞDE İNAN TURİZM", canDepartFromCenter: true),
            CreateDeparture(3, 0),
            departedFromCenter: true);

        AssertPricing(
            result,
            PricingRules.CenterDepartureAmount,
            PricingRules.CenterDepartureTariffName);
    }

    [Fact]
    public void Company_outside_center_departure_list_cannot_use_center_tariff()
    {
        var result = _pricingService.Calculate(
            CreateCompany("Normal Şehirlerarası Firma", canDepartFromCenter: true),
            CreateDeparture(12, 0),
            departedFromCenter: true);

        AssertPricing(
            result,
            PricingRules.StandardAmount,
            PricingRules.StandardTariffName);
    }

    private static Company CreateCompany(
        string name,
        bool canDepartFromCenter = false)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            CompanyType = CompanyType.Intercity,
            CanDepartFromCenter = canDepartFromCenter,
            IsActive = true
        };
    }

    private static DateTimeOffset CreateDeparture(int hour, int minute)
    {
        return new DateTimeOffset(2026, 8, 31, hour, minute, 0, TimeSpan.FromHours(3));
    }

    private static void AssertPricing(
        PricingResult result,
        decimal expectedAmount,
        string expectedTariffName)
    {
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(expectedTariffName, result.TariffName);
    }
}
