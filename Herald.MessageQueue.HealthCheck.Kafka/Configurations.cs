
using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.Kafka
{

    public static class Configurations
    {
        public static IHealthChecksBuilder AddKafkaCheck<T>(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck<HealthCheckKafka>(typeof(T).Name);
        }
    }
}
