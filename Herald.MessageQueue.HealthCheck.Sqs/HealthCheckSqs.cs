using Amazon.SQS;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public class HealthCheckSqs<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;
        private readonly DateTime _healthCheckTime;
        private readonly HealthCheckResult _healthCheckResult;

        private readonly IAmazonSQS _amazonSqs;
        private readonly string _queueName;

        public HealthCheckSqs(IAmazonSQS amazonSQS, IQueueInfo queueOptions)
        {
            _amazonSqs = amazonSQS;
            _queueName = queueOptions.GetQueueName(typeof(T));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!IsExpired())
            {
                return _healthCheckResult;
            }

            try
            {
                var response = await _amazonSqs.GetQueueUrlAsync(_queueName, cancellationToken);

                return !string.IsNullOrEmpty(response.QueueUrl) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private bool IsExpired()
        {
            return (_healthCheckInterval == 0 || _healthCheckTime.AddSeconds(_healthCheckInterval) <= DateTime.Now);
        }
    }
}
