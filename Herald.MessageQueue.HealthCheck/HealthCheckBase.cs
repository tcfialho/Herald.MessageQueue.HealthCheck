
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck
{
    public abstract class HealthCheckBase<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;
        private DateTime _healthCheckTime;
        private HealthCheckResult _healthCheckResult;

        public HealthCheckBase(int healthCheckInterval = 1)
        {
            _healthCheckInterval = healthCheckInterval;
            _healthCheckTime = DateTime.MinValue;
            _healthCheckResult = HealthCheckResult.Unhealthy();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!IsExpired())
            {
                return _healthCheckResult;
            }
            _healthCheckTime = DateTime.Now;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

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
