using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace InvestmentApp.Infrastructure.HealthChecks;

public class SimpleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        HealthCheckResult result;
        /* Start Health Check Code */
        Random rand = new();
        var test = rand.Next(1, 4);

        IReadOnlyDictionary<string, object> data = new Dictionary<string, object>
            {
                { "Value", test }
            };

        if (test == 1)
        {
            Exception ex = new("Value 1 Exception");
            result = new HealthCheckResult(HealthStatus.Unhealthy, "Value was 1", ex, data);
        }
        else if (test == 2)
        {
            result = new HealthCheckResult(HealthStatus.Degraded, "Value was 2", null, data);
        }
        else
        {
            result = new HealthCheckResult(HealthStatus.Healthy, "Value was 3", null, data);
        }
        /* End Health Check Code */

        stopwatch.Stop();

        return Task.FromResult(result);
    }
}
