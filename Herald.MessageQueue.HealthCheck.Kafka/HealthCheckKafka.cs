using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.Kafka
{
    public class HealthCheckKafka : IHealthCheck
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly MessageQueueOptions _messageQueueOptions;

        public HealthCheckKafka(IConsumer<Ignore, string> consumer, MessageQueueOptions messageQueueOptions)
        {
            _consumer = consumer;
            _messageQueueOptions = messageQueueOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _consumer.QueryWatermarkOffsets(new TopicPartition(context.Registration.Name.ToLower(), new Partition(0)), TimeSpan.FromSeconds(5));

                return await Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}