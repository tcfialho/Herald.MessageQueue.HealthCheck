
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

        public HealthCheckRabbitMq(IModel channel, IQueueInfo queueInfo, IExchangeInfo exchangeInfo, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _channel = channel;
            _queueName = queueInfo.GetQueueName(typeof(T));
            _exchangeName = exchangeInfo.GetExchangeName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            _channel.ExchangeDeclarePassive(_exchangeName);
            _channel.QueueDeclarePassive(_queueName);

            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}