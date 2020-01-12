using Amazon.SQS;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public class HealthCheckSqs : IHealthCheck
    {
        private readonly IAmazonSQS _amazonSqs;

        public HealthCheckSqs(IAmazonSQS amazonSQS)
        {
            _amazonSqs = amazonSQS;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _amazonSqs.GetQueueUrlAsync(context.Registration.Name, cancellationToken);

                return !string.IsNullOrEmpty(response.QueueUrl) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
