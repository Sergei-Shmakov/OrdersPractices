using Orders.Models;
using Orders.Repositories;

namespace Orders.Tests;

public class InMemoryOrderRepositoryTests
{
    [Fact]
    public async Task SaveAsync_ThenFindAsync_ReturnsSameOrder()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        var order = new Order();
        order.Add("Книга", 300m, 2);

        // Act
        await repository.SaveAsync(order);
        var foundOrder = await repository.FindAsync(order.Id);

        // Assert
        Assert.Same(order, foundOrder);
    }

    [Fact]
    public async Task FindAsync_UnknownId_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        var unknownOrderId = Guid.NewGuid();

        // Act
        var foundOrder = await repository.FindAsync(unknownOrderId);

        // Assert
        Assert.Null(foundOrder);
    }

    [Fact]
    public async Task SaveAsync_Null_Throws()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        Order order = null!;

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(order));
    }
}
