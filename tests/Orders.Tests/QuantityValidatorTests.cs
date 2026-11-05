using Orders.Validation;

namespace Orders.Tests;

public class QuantityValidatorTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void IsValidQty_ChecksBounds(int quantity, bool expected)
    {
        // Arrange — значения и ожидаемый результат заданы в InlineData.

        // Act
        var actual = QuantityValidator.IsValidQty(quantity);

        // Assert
        Assert.Equal(expected, actual);
    }
}
