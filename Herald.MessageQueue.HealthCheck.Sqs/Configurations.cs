using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.Sqs
{
    public static class Configurations
    {
        public static IHealthChecksBuilder AddSqsCheck<T>(this IHealthChecksBuilder builder) where T : MessageBase
        {
            builder.Services.AddSingleton<HealthCheckSqs<T>>();

            return builder.AddCheck<HealthCheckSqs<T>>(typeof(T).Name);
        }
    }
}
