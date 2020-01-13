using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using RabbitMQ.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.RabbitMq
{
    public class HealthCheckRabbitMq : IHealthCheck
    {
        private readonly IModel _channel;
        private readonly MessageQueueOptions _messageQueueOptions;

        public HealthCheckRabbitMq(IModel channel, MessageQueueOptions messageQueueOptions)
        {
            _channel = channel;
            _messageQueueOptions = messageQueueOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _channel.ExchangeDeclarePassive(_messageQueueOptions.ExchangeName);
                _channel.QueueDeclarePassive(context.Registration.Name);

                return await Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}