using Amazon.SQS;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public class HealthCheckSqs<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly string _queueName;

        public HealthCheckSqs(IAmazonSQS amazonSQS, IQueueInfo queueOptions, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _amazonSqs = amazonSQS;
            _queueName = queueOptions.GetQueueName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            var response = await _amazonSqs.GetQueueUrlAsync(_queueName);

            return !string.IsNullOrEmpty(response.QueueUrl) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
    }
}
