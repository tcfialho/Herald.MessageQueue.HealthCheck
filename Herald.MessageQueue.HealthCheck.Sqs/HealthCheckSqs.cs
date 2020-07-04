using Amazon.SQS;

using Herald.MessageQueue.Sqs;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public class HealthCheckSqs<T> : HealthCheckBase<T> where T : MessageBase
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly string _queueUrl;

        public HealthCheckSqs(IAmazonSQS amazonSQS, IMessageQueueInfo info, int healthCheckInterval = 1) : base(healthCheckInterval)
        {
            _amazonSqs = amazonSQS;
            _queueUrl = info.GetQueueName(typeof(T));
        }

        protected override async Task<HealthCheckResult> ProcessHealthCheck(HealthCheckContext context)
        {
            var response = await _amazonSqs.GetQueueUrlAsync(_queueUrl);

            return !string.IsNullOrEmpty(response.QueueUrl) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
    }
}
