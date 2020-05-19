
using Microsoft.Extensions.Diagnostics.HealthChecks;

using RabbitMQ.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.RabbitMq
{
    public class HealthCheckRabbitMq<T> : IHealthCheck where T : MessageBase
    {
        private readonly int _healthCheckInterval;
        private readonly DateTime _healthCheckTime;
        private readonly HealthCheckResult _healthCheckResult;

        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public HealthCheckRabbitMq(IModel channel, IQueueInfo queueInfo, IExchangeInfo exchangeInfo, int healthCheckInterval = 1)
        {
            _channel = channel;
            _queueName = queueInfo.GetQueueName(typeof(T));
            _exchangeName = exchangeInfo.GetExchangeName(typeof(T));
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
                _channel.ExchangeDeclarePassive(_exchangeName);
                _channel.QueueDeclarePassive(_queueName);

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