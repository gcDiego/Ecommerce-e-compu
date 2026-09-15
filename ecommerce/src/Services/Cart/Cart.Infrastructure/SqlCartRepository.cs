namespace Cart.Infrastructure;

using System.Data;
using Cart.Application;
using Cart.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class Class1
{

}

public sealed class SqlCartRepository(string connectionString) : ICartRepository
{
    public async Task<IReadOnlyList<CartItem>> GetItemsAsync(int customerId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT IdProducto, Nombre, Precio, DesMarca, Cantidad FROM fn_obtenerCarritoCliente(@CustomerId);";
        var items = new List<CartItem>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CartItem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(3),
                reader.GetDecimal(2),
                reader.GetInt32(4)));
        }

        return items;
    }

    public async Task<bool> ContainsAsync(int customerId, int productId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("sp_ExisteCarrito", connection) { CommandType = CommandType.StoredProcedure };
        AddIds(command, customerId, productId);
        var result = command.Parameters.Add("@Resultado", SqlDbType.Bit);
        result.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return result.Value is true;
    }

    public async Task<CartOperationResult> ChangeQuantityAsync(
        int customerId,
        int productId,
        bool increase,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("sp_OperacionCarrito", connection) { CommandType = CommandType.StoredProcedure };
        AddIds(command, customerId, productId);
        command.Parameters.Add("@Sumar", SqlDbType.Bit).Value = increase;
        var result = command.Parameters.Add("@Resultado", SqlDbType.Bit);
        result.Direction = ParameterDirection.Output;
        var message = command.Parameters.Add("@Mensaje", SqlDbType.VarChar, 500);
        message.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return new CartOperationResult(result.Value is true, Convert.ToString(message.Value) ?? string.Empty);
    }

    public async Task<bool> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("sp_EliminarCarrito", connection) { CommandType = CommandType.StoredProcedure };
        AddIds(command, customerId, productId);
        var result = command.Parameters.Add("@Resultado", SqlDbType.Bit);
        result.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return result.Value is true;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return true;
    }

    private static void AddIds(SqlCommand command, int customerId, int productId)
    {
        command.Parameters.Add("@IdCliente", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@IdProducto", SqlDbType.Int).Value = productId;
    }
}

public sealed class CartDatabaseHealthCheck(ICartRepository repository) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("No fue posible conectar con la base del carrito.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("No fue posible conectar con la base del carrito.", exception);
        }
    }
}
