using Catalog.Application;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.Infrastructure;

public sealed class CatalogDatabaseHealthCheck(ICatalogRepository repository) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("No fue posible conectar con SQL Server.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("No fue posible conectar con SQL Server.", exception);
        }
    }
}
