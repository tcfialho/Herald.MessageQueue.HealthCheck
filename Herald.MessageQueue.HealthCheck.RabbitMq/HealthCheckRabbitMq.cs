
using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using RabbitMQ.Client;

using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.RabbitMq
{
    public class HealthCheckRabbitMq<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public HealthCheckRabbitMq(IModel channel, IMessageQueueInfo info, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _channel = channel;
            _queueName = info.GetQueueName(typeof(T));
            _exchangeName = info.GetExchangeName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            _channel.ExchangeDeclarePassive(_exchangeName);
            _channel.QueueDeclarePassive(_queueName);

            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}