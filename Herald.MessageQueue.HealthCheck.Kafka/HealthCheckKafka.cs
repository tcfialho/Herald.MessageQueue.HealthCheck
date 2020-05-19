using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Kafka
{
    public class HealthCheckKafka<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;
        private readonly DateTime _healthCheckTime;
        private readonly HealthCheckResult _healthCheckResult;

        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _queueName;

        public HealthCheckKafka(IConsumer<Ignore, string> consumer, ITopicInfo topicOptions, int healthCheckInterval = 1)
        {
            _consumer = consumer;
            _queueName = topicOptions.GetTopicName(typeof(T));
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
                _consumer.QueryWatermarkOffsets(new TopicPartition(_queueName, new Partition(0)), TimeSpan.FromSeconds(5));

                return await Task.FromResult(HealthCheckResult.Healthy());
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