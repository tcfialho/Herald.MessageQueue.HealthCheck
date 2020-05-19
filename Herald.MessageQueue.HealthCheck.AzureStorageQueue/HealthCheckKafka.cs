using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueue<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;
        private readonly DateTime _healthCheckTime;
        private HealthCheckResult _healthCheckResult;

        private readonly CloudQueueClient _cloudQueueClient;
        private readonly string _queueName;

        public HealthCheckAzureStorageQueue(CloudQueueClient cloudQueueClient, IQueueInfo messageQueueInfo, int healthCheckInterval = 1)
        {
            _cloudQueueClient = cloudQueueClient;
            _queueName = messageQueueInfo.GetQueueName(typeof(T));
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
                var queue = _cloudQueueClient.GetQueueReference(_queueName);

                _healthCheckResult = HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                _healthCheckResult = new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }

            return await Task.FromResult(_healthCheckResult);
        }

        private bool IsExpired()
        {
            return (_healthCheckInterval == 0 || _healthCheckTime.AddSeconds(_healthCheckInterval) <= DateTime.Now);
        }
    }
}