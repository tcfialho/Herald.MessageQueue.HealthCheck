using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Kafka
{
    public class HealthCheckKafka<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _queueName;

        public HealthCheckKafka(IConsumer<Ignore, string> consumer, ITopicInfo topicOptions, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _consumer = consumer;
            _queueName = topicOptions.GetTopicName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            _consumer.QueryWatermarkOffsets(new TopicPartition(_queueName, new Partition(0)), TimeSpan.FromSeconds(5));

            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}