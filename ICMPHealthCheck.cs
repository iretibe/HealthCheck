using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    public class ICMPHealthCheck : IHealthCheck
    {
        private readonly string Host;
        private readonly int HealthyRoundtripTime;

        public ICMPHealthCheck(string host, int healthyRoundtripTime)
        {
            Host = host;
            HealthyRoundtripTime = healthyRoundtripTime;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var ping = new Ping();

                var reply = await ping.SendPingAsync(Host);

                switch (reply.Status)
                {
                    case IPStatus.Success:
                        var msg = $"ICMP to {Host} took {reply.RoundtripTime} ms.";

                        return (reply.RoundtripTime > HealthyRoundtripTime)
                            ? HealthCheckResult.Degraded(msg)
                            : HealthCheckResult.Healthy(msg);
                    default:
                        var err = $"ICMP to {Host} failed: {reply.Status}";

                        return HealthCheckResult.Unhealthy(err);
                }
            }
            catch (Exception e)
            {
                var err = $"ICMP to {Host} failed: {e.Message}";

                return HealthCheckResult.Unhealthy(err);
            }
        }
    }
}

/*
- Healthy, if the PING request gets a successful reply with a round-trip time of 300 ms or less
- Degraded, if the PING request gets a successful reply with a round-trip time greater than 300 ms
- Unhealthy, if the PING request fails or an Exception is thrown
*/