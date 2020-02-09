﻿using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.HealthCheck.AzureStorageQueue
{

    public static class Configurations
    {
        public static IHealthChecksBuilder AddAzureStorageQueueCheck<T>(this IHealthChecksBuilder builder) where T : MessageBase
        {
            return builder.AddCheck<HealthCheckAzureStorageQueue<T>>(typeof(T).Name);
        }
    }
}
