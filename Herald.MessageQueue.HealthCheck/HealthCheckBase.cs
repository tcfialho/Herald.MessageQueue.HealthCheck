
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck
{
    public abstract class HealthCheckBase<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;

        private static DateTime _healthCheckTime = DateTime.MinValue;
        private static HealthCheckResult _healthCheckResult = HealthCheckResult.Unhealthy();

        public HealthCheckBase(int healthCheckInterval)
        {
            _healthCheckInterval = healthCheckInterval;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!IsExpired())
            {
                return _healthCheckResult;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _healthCheckTime = DateTime.Now;

                _healthCheckResult = await ProcessHealthCheck(context);
            }
            catch (Exception ex)
            {
                _healthCheckResult = new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }

            return await Task.FromResult(_healthCheckResult);
        }

        protected abstract Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context);

        protected bool IsExpired()
        {
            return (_healthCheckInterval == 0 || _healthCheckTime.AddSeconds(_healthCheckInterval) <= DateTime.Now);
        }
    }
}
