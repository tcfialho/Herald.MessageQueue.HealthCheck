using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.RabbitMq
{

    public static class Configurations
    {
        public static IHealthChecksBuilder AddRabbitMqCheck<T>(this IHealthChecksBuilder builder) where T : MessageBase
        {
            builder.Services.AddSingleton<HealthCheckRabbitMq<T>>();

            return builder.AddCheck<HealthCheckRabbitMq<T>>(typeof(T).Name);
        }
    }
}
