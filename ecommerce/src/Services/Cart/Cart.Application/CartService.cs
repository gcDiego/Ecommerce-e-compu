using Cart.Domain;

namespace Cart.Application;

public interface ICartRepository
{
    Task<IReadOnlyList<CartItem>> GetItemsAsync(int customerId, CancellationToken cancellationToken);
    Task<bool> ContainsAsync(int customerId, int productId, CancellationToken cancellationToken);
    Task<CartOperationResult> ChangeQuantityAsync(int customerId, int productId, bool increase, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken);
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}

public sealed record CartOperationResult(bool Success, string Message);

public sealed class CartService(ICartRepository repository)
{
    public async Task<CartSnapshot> GetAsync(int customerId, CancellationToken cancellationToken)
    {
        EnsureCustomer(customerId);
        return new CartSnapshot(await repository.GetItemsAsync(customerId, cancellationToken));
    }

    public async Task<CartOperationResult> AddAsync(int customerId, int productId, CancellationToken cancellationToken)
    {
        EnsureIds(customerId, productId);
        if (await repository.ContainsAsync(customerId, productId, cancellationToken))
        {
            return new CartOperationResult(false, "El producto ya existe en el carrito.");
        }

        return await repository.ChangeQuantityAsync(customerId, productId, true, cancellationToken);
    }

    public Task<CartOperationResult> ChangeQuantityAsync(
        int customerId,
        int productId,
        bool increase,
        CancellationToken cancellationToken)
    {
        EnsureIds(customerId, productId);
        return repository.ChangeQuantityAsync(customerId, productId, increase, cancellationToken);
    }

    public Task<bool> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken)
    {
        EnsureIds(customerId, productId);
        return repository.RemoveAsync(customerId, productId, cancellationToken);
    }

    private static void EnsureIds(int customerId, int productId)
    {
        EnsureCustomer(customerId);
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
    }

    private static void EnsureCustomer(int customerId)
    {
        if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));
    }
}
