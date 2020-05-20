using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.Kafka
{

    public static class Configurations
    {
        public static IHealthChecksBuilder AddKafkaCheck<T>(this IHealthChecksBuilder builder) where T : MessageBase
        {
            builder.Services.AddSingleton<HealthCheckKafka<T>>();

            return builder.AddCheck<HealthCheckKafka<T>>(typeof(T).Name);
        }
    }
}
