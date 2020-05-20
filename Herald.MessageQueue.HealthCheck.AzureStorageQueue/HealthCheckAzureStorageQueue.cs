using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueue<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly string _queueName;

        public HealthCheckAzureStorageQueue(CloudQueueClient cloudQueueClient, IQueueInfo messageQueueInfo, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _cloudQueueClient = cloudQueueClient;
            _queueName = messageQueueInfo.GetQueueName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            var queue = _cloudQueueClient.GetQueueReference(_queueName);

            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}