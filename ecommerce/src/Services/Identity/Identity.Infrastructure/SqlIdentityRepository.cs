using Identity.Application;
using Identity.Domain;
using Microsoft.Data.SqlClient;

namespace Identity.Infrastructure;

public sealed class SqlIdentityRepository(string connectionString) : IIdentityRepository
{
    public async Task<IdentityAccount?> FindByEmailAsync(
        string email,
        AccountType accountType,
        CancellationToken cancellationToken)
    {
        var sql = accountType == AccountType.Customer
            ? """
              SELECT TOP (1) IdCliente, Nombres, Apellidos, Correo, Clave, Restablecer
              FROM CLIENTE
              WHERE LOWER(Correo) = @Email;
              """
            : """
              SELECT TOP (1) IdUsuario, Nombres, Apellidos, Correo, Clave, Restablecer, Activo
              FROM USUARIO
              WHERE LOWER(Correo) = @Email;
              """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new IdentityAccount(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetBoolean(5),
            accountType == AccountType.Customer || reader.GetBoolean(6),
            accountType);
    }

    public async Task<IdentityAccount?> FindByIdAsync(
        int id,
        AccountType accountType,
        CancellationToken cancellationToken)
    {
        var sql = accountType == AccountType.Customer
            ? "SELECT TOP (1) IdCliente, Nombres, Apellidos, Correo, Clave, Restablecer FROM CLIENTE WHERE IdCliente = @Id;"
            : "SELECT TOP (1) IdUsuario, Nombres, Apellidos, Correo, Clave, Restablecer, Activo FROM USUARIO WHERE IdUsuario = @Id;";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadAccount(reader, accountType) : null;
    }

    public async Task<bool> UpdatePasswordHashAsync(
        int id,
        AccountType accountType,
        string expectedCurrentHash,
        string newHash,
        CancellationToken cancellationToken)
    {
        var sql = accountType == AccountType.Customer
            ? "UPDATE CLIENTE SET Clave = @NewHash, Restablecer = 0 WHERE IdCliente = @Id AND Clave = @ExpectedHash;"
            : "UPDATE USUARIO SET Clave = @NewHash, Restablecer = 0 WHERE IdUsuario = @Id AND Clave = @ExpectedHash;";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        command.Parameters.Add("@ExpectedHash", System.Data.SqlDbType.VarChar, 150).Value = expectedCurrentHash;
        command.Parameters.Add("@NewHash", System.Data.SqlDbType.VarChar, 150).Value = newHash;
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return true;
    }

    private static IdentityAccount ReadAccount(SqlDataReader reader, AccountType accountType)
    {
        return new IdentityAccount(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetBoolean(5),
            accountType == AccountType.Customer || reader.GetBoolean(6),
            accountType);
    }
}
