using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public static class Configurations
    {
        public static IHealthChecksBuilder AddSqsCheck<T>(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck<HealthCheckSqs>(typeof(T).Name);
        }
    }
}
