using Catalog.Application;
using Catalog.Domain;
using Microsoft.Data.SqlClient;

namespace Catalog.Infrastructure;

public sealed class SqlCatalogRepository(string connectionString) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(bool? isActive, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT IdCategoria, Descripcion, Activo
            FROM CATEGORIA
            WHERE (@IsActive IS NULL OR Activo = @IsActive)
            ORDER BY Descripcion;
            """;

        var categories = new List<Category>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IsActive", isActive is null ? DBNull.Value : isActive.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new Category(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return categories;
    }

    public async Task<IReadOnlyList<Brand>> GetBrandsAsync(int? categoryId, bool? isActive, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT m.IdMarca, m.Descripcion, m.Activo
            FROM MARCA m
            WHERE (@IsActive IS NULL OR m.Activo = @IsActive)
              AND (@CategoryId IS NULL OR EXISTS (
                  SELECT 1
                  FROM PRODUCTO p
                  WHERE p.IdMarca = m.IdMarca AND p.IdCategoria = @CategoryId
              ))
            ORDER BY m.Descripcion;
            """;

        var brands = new List<Brand>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CategoryId", categoryId is null ? DBNull.Value : categoryId.Value);
        command.Parameters.AddWithValue("@IsActive", isActive is null ? DBNull.Value : isActive.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            brands.Add(new Brand(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return brands;
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(int? categoryId, int? brandId, bool? isActive, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.IdProducto, p.Nombre, p.Descripcion,
                   m.IdMarca, m.Descripcion, m.Activo,
                   c.IdCategoria, c.Descripcion, c.Activo,
                   p.Precio, p.Stock, p.RutaImagen, p.NombreImagen, p.Activo
            FROM PRODUCTO p
            INNER JOIN MARCA m ON m.IdMarca = p.IdMarca
            INNER JOIN CATEGORIA c ON c.IdCategoria = p.IdCategoria
            WHERE (@CategoryId IS NULL OR p.IdCategoria = @CategoryId)
              AND (@BrandId IS NULL OR p.IdMarca = @BrandId)
              AND (@IsActive IS NULL OR p.Activo = @IsActive)
            ORDER BY p.Nombre;
            """;

        var products = new List<Product>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CategoryId", categoryId is null ? DBNull.Value : categoryId.Value);
        command.Parameters.AddWithValue("@BrandId", brandId is null ? DBNull.Value : brandId.Value);
        command.Parameters.AddWithValue("@IsActive", isActive is null ? DBNull.Value : isActive.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.IdProducto, p.Nombre, p.Descripcion,
                   m.IdMarca, m.Descripcion, m.Activo,
                   c.IdCategoria, c.Descripcion, c.Activo,
                   p.Precio, p.Stock, p.RutaImagen, p.NombreImagen, p.Activo
            FROM PRODUCTO p
            INNER JOIN MARCA m ON m.IdMarca = p.IdMarca
            INNER JOIN CATEGORIA c ON c.IdCategoria = p.IdCategoria
            WHERE p.IdProducto = @Id;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapProduct(reader) : null;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return true;
    }

    private static Product MapProduct(SqlDataReader reader) =>
        new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            new Brand(reader.GetInt32(3), reader.GetString(4), reader.GetBoolean(5)),
            new Category(reader.GetInt32(6), reader.GetString(7), reader.GetBoolean(8)),
            reader.GetDecimal(9),
            reader.GetInt32(10),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            reader.IsDBNull(12) ? null : reader.GetString(12),
            reader.GetBoolean(13));
}
