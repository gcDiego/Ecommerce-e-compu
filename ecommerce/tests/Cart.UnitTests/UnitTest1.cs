using Cart.Application;
using Cart.Domain;

namespace Cart.UnitTests;

public sealed class CartServiceTests
{
    [Fact]
    public async Task GetAsync_ComputesTotalFromRepositoryPrices()
    {
        var repository = new StubRepository
        {
            Items = new[] { new CartItem(8, "Producto", "Marca", 125.50m, 2) }
        };

        var result = await new CartService(repository).GetAsync(7, CancellationToken.None);

        Assert.Equal(251m, result.Total);
        Assert.Equal(7, repository.LastCustomerId);
    }

    [Fact]
    public async Task AddAsync_WhenProductAlreadyExists_DoesNotMutateCart()
    {
        var repository = new StubRepository { Contains = true };

        var result = await new CartService(repository).AddAsync(7, 8, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, repository.ChangeCalls);
    }

    private sealed class StubRepository : ICartRepository
    {
        public IReadOnlyList<CartItem> Items { get; init; } = new List<CartItem>();
        public bool Contains { get; init; }
        public int LastCustomerId { get; private set; }
        public int ChangeCalls { get; private set; }

        public Task<IReadOnlyList<CartItem>> GetItemsAsync(int customerId, CancellationToken cancellationToken)
        {
            LastCustomerId = customerId;
            return Task.FromResult(Items);
        }

        public Task<bool> ContainsAsync(int customerId, int productId, CancellationToken cancellationToken) => Task.FromResult(Contains);

        public Task<CartOperationResult> ChangeQuantityAsync(int customerId, int productId, bool increase, CancellationToken cancellationToken)
        {
            ChangeCalls++;
            return Task.FromResult(new CartOperationResult(true, string.Empty));
        }

        public Task<bool> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}