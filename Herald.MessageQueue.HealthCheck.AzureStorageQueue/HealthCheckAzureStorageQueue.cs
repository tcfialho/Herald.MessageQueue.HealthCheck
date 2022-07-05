using Herald.MessageQueue.AzureStorageQueue;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueue<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly string _queueName;
        private readonly IQueueClientFactory _queueClientFactory;
        private readonly MessageQueueOptions _options;

        public HealthCheckAzureStorageQueue(IQueueClientFactory queueClientFactory, MessageQueueOptions options, IMessageQueueInfo info, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _queueName = info.GetQueueName(typeof(T));
            _options = options;
            _queueClientFactory = queueClientFactory;            
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            var queueClient = _queueClientFactory.Create(_options.ConnectionString, _queueName);

            var queueExists = await queueClient.ExistsAsync();

            if (queueExists)            
                return await Task.FromResult(HealthCheckResult.Healthy());

            return await Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}