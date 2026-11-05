using Moq;
using Orders.Abstractions;
using Orders.Models;
using Orders.Services;

namespace Orders.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IMessagePublisher> _publisher = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_repo.Object, _publisher.Object, new DiscountCalculator());
    }

    private static NewOrder Cmd() =>
        new(new List<OrderItem> { new("Книга", 300m, 2) });

    [Fact]
    public async Task CreateAsync_ValidOrder_SavesOrder()
    {
        // Arrange
        var command = Cmd();

        // Act
        await _sut.CreateAsync(command);

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once());
    }

    [Fact]
    public async Task CreateAsync_ValidOrder_PublishesEventWithSameId()
    {
        // Arrange
        var command = Cmd();

        // Act
        var orderId = await _sut.CreateAsync(command);

        // Assert
        _publisher.Verify(
            p => p.PublishAsync(It.Is<OrderCreated>(message => message.OrderId == orderId)),
            Times.Once());
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Once());
    }

    [Fact]
    public async Task CreateAsync_SaveFails_DoesNotPublish()
    {
        // Arrange
        _repo.Setup(r => r.SaveAsync(It.IsAny<Order>()))
            .ThrowsAsync(new TimeoutException("Storage timed out"));

        // Act
        await Assert.ThrowsAsync<TimeoutException>(() => _sut.CreateAsync(Cmd()));

        // Assert
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<OrderCreated>()),
            Times.Never());
    }

    [Fact]
    public async Task CreateAsync_NullCommand_Throws()
    {
        // Arrange
        NewOrder command = null!;

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(command));

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never());
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never());
    }

    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsAndDoesNotSave()
    {
        // Arrange
        var command = new NewOrder(Array.Empty<OrderItem>());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(command));

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never());
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never());
    }

    [Fact]
    public async Task CreateAsync_PublishFails_ExceptionPropagates()
    {
        // Arrange
        _publisher.Setup(p => p.PublishAsync(It.IsAny<OrderCreated>()))
            .ThrowsAsync(new InvalidOperationException("Publisher unavailable"));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(Cmd()));

        // Assert
        _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once());
        _publisher.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Once());
    }

    [Fact]
    public async Task GetPayableAsync_TotalBelowThreshold_NoDiscount()
    {
        // Arrange
        var order = new Order();
        order.Add("Книга", 500m, 1);
        _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

        // Act
        var payable = await _sut.GetPayableAsync(order.Id);

        // Assert
        Assert.Equal(500m, payable);
    }

    [Fact]
    public async Task GetPayableAsync_TotalAboveThreshold_AppliesDiscount()
    {
        // Arrange
        var order = new Order();
        order.Add("Книга", 1500m, 1);
        _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

        // Act
        var payable = await _sut.GetPayableAsync(order.Id);

        // Assert
        Assert.Equal(1350m, payable);
    }

    [Fact]
    public async Task GetPayableAsync_OrderNotFound_Throws()
    {
        // Arrange
        var unknownOrderId = Guid.NewGuid();
        _repo.Setup(r => r.FindAsync(unknownOrderId)).ReturnsAsync((Order?)null);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetPayableAsync(unknownOrderId));

        // Assert
        Assert.Contains(unknownOrderId.ToString(), exception.Message);
    }
}
