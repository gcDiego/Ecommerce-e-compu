namespace Cart.Domain;

public sealed record CartItem(
    int ProductId,
    string ProductName,
    string BrandName,
    decimal UnitPrice,
    int Quantity);

public sealed record CartSnapshot(IReadOnlyList<CartItem> Items)
{
    public int DistinctItemCount => Items.Count;
    public decimal Total => Items.Sum(item => item.UnitPrice * item.Quantity);
}
