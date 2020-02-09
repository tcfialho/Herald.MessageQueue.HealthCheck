using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueue<T> : IHealthCheck where T : MessageBase
    {
        private readonly CloudQueueClient _cloudQueueClient;
        private readonly string _queueName;

        public HealthCheckAzureStorageQueue(CloudQueueClient cloudQueueClient, IMessageQueueInfo messageQueueInfo)
        {
            _cloudQueueClient = cloudQueueClient;
            _queueName = messageQueueInfo.GetQueueName(typeof(T));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var queue = _cloudQueueClient.GetQueueReference(_queueName);

                return await Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}