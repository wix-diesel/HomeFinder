using HomeFinder.Application.Helper;

namespace ContractTests;

public class JanValidatorContractTests
{
    [Theory]
    [InlineData("4901234567890")]
    [InlineData("12345678")]
    public void IsValid_ValidJan_ReturnsTrue(string jan)
    {
        Assert.True(JanValidator.IsValid(jan));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    [InlineData("12345678901234")]
    [InlineData("12AB5678")]
    public void IsValid_InvalidJan_ReturnsFalse(string jan)
    {
        Assert.False(JanValidator.IsValid(jan));
    }
}
