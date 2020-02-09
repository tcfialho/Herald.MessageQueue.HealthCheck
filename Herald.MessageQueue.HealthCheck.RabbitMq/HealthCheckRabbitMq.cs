using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.RabbitMq;
using Herald.MessageQueue.RabbitMq.Attributes;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using RabbitMQ.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.RabbitMq
{
    public class HealthCheckRabbitMq<T> : IHealthCheck where T : MessageBase
    {
        private readonly IModel _channel;
        private readonly MessageQueueOptions _options;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public HealthCheckRabbitMq(IModel channel, MessageQueueOptions options, IMessageQueueInfo messageQueueInfo)
        {
            _channel = channel;
            _options = options;
            _queueName = messageQueueInfo.GetQueueName(typeof(T));
            _exchangeName = GetExchangeName(typeof(T));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _channel.ExchangeDeclarePassive(_exchangeName);
                _channel.QueueDeclarePassive(_queueName);

                return await Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private string GetExchangeName(Type type)
        {
            return type.GetAttribute<ExchangeNameAttribute>()?.ExchangeName ?? string.Concat(type.Name, _options.ExchangeNameSufix);
        }
    }
}