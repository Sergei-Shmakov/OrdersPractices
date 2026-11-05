using Orders.Services;

namespace Orders.Tests;

public class DiscountCalculatorTests
{
    private readonly DiscountCalculator _sut = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999.99, 0)]
    [InlineData(1000, 100)]
    [InlineData(2000, 200)]
    public void Calculate_ReturnsExpected(decimal total, decimal expectedDiscount)
    {
        // Arrange — входная сумма и ожидаемая скидка заданы в InlineData.

        // Act
        var actualDiscount = _sut.Calculate(total);

        // Assert
        Assert.Equal(expectedDiscount, actualDiscount);
    }

    [Fact]
    public void Calculate_NegativeTotal_Throws()
    {
        // Arrange
        const decimal total = -1m;

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => _sut.Calculate(total));

        // Assert
        Assert.Equal("total", exception.ParamName);
    }
}
