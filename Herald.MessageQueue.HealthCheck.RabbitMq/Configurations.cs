using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.RabbitMq
{

    public static class Configurations
    {
        public static IHealthChecksBuilder AddRabbitMqCheck<T>(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck<HealthCheckRabbitMq>(typeof(T).Name);
        }
    }
}
