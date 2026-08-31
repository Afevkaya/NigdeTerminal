using NigdeTerminal.App.Services;

namespace NigdeTerminal.Tests;

public sealed class VehiclePlateServiceTests
{
    private readonly VehiclePlateService _service = new();

    [Fact]
    public void Lowercase_letters_are_normalized_to_uppercase()
    {
        AssertNormalized("51 abc 123", "51 ABC 123");
    }

    [Fact]
    public void Plate_without_spaces_is_normalized()
    {
        AssertNormalized("51abc123", "51 ABC 123");
    }

    [Theory]
    [InlineData("51 ABC123")]
    [InlineData("51abc 123")]
    [InlineData("  51   ABC   123  ")]
    [InlineData("51\tABC\t123")]
    public void Different_whitespace_uses_are_normalized(string input)
    {
        AssertNormalized(input, "51 ABC 123");
    }

    [Theory]
    [InlineData("01 A 1234", "01 A 1234")]
    [InlineData("06 AB 123", "06 AB 123")]
    [InlineData("34 ABC 12", "34 ABC 12")]
    [InlineData("51 ABC 123", "51 ABC 123")]
    [InlineData("81 Z 9999", "81 Z 9999")]
    public void Valid_turkish_plates_are_accepted(string input, string expected)
    {
        AssertNormalized(input, expected);
    }

    [Theory]
    [InlineData("00 ABC 123")]
    [InlineData("82 ABC 123")]
    public void Invalid_city_codes_are_rejected(string input)
    {
        AssertInvalid(input);
    }

    [Theory]
    [InlineData("51")]
    [InlineData("51 ABC")]
    [InlineData("ABC 123")]
    [InlineData("51 123 ABC")]
    [InlineData("51 ABCD 123")]
    [InlineData("51 ABC 1")]
    [InlineData("51 ABC 12345")]
    [InlineData("51 ÇA 123")]
    [InlineData("51-A-1234")]
    public void Missing_or_malformed_plates_are_rejected(string input)
    {
        AssertInvalid(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Null_or_empty_input_is_rejected(string? input)
    {
        AssertInvalid(input);
    }

    private void AssertNormalized(string input, string expected)
    {
        var isValid = _service.TryNormalize(input, out var normalizedPlate);

        Assert.True(isValid);
        Assert.Equal(expected, normalizedPlate);
    }

    private void AssertInvalid(string? input)
    {
        var isValid = _service.TryNormalize(input, out var normalizedPlate);

        Assert.False(isValid);
        Assert.Equal(string.Empty, normalizedPlate);
    }
}
