﻿using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Kafka
{
    public class HealthCheckKafka<T> : IHealthCheck where T : MessageBase
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly string _queueName;

        public HealthCheckKafka(IConsumer<Ignore, string> consumer, IMessageQueueInfo messageQueueOptions)
        {
            _consumer = consumer;
            _queueName = messageQueueOptions.GetQueueName(typeof(T));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
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
    }
}