using Confluent.Kafka;

using Herald.MessageQueue.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.HealthCheck.Kafka.Tests
{
    public class HealthCheckKafkaTests
    {
        private class TestMessage : MessageBase { };

        [Fact]
        public void ShouldAddHealthCheck()
        {
            //Arrange
            var kafkaConsumerMock = new Mock<IConsumer<Ignore, string>>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(x => new MessageQueueOptions())
                             .AddScoped(x => kafkaConsumerMock.Object)
                             .AddHealthChecks()
                             .AddKafkaCheck<TestMessage>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();

            //Act
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Assert
            Assert.IsType<HealthCheckKafka>(healtCheck);
        }

        [Fact]
        public async Task ShouldReturnHealthy()
        {
            //Arrange
            var kafkaConsumerMock = new Mock<IConsumer<Ignore, string>>();
            kafkaConsumerMock
                .Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()));
            var healthCheck = new HealthCheckKafka(kafkaConsumerMock.Object, new MessageQueueOptions());
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = new HealthCheckRegistration(nameof(TestMessage), healthCheck, default, default)
            };

            //Act
            var healthCheckResult = await healthCheck.CheckHealthAsync(healthCheckContext);

            //Assert
            Assert.Equal(HealthStatus.Healthy, healthCheckResult.Status);
        }

        [Fact]
        public async Task ShouldReturnUnhealthy()
        {
            //Arrange
            var kafkaConsumerMock = new Mock<IConsumer<Ignore, string>>();
            kafkaConsumerMock
                .Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Throws<Exception>();
            var healthCheck = new HealthCheckKafka(kafkaConsumerMock.Object, new MessageQueueOptions());
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = new HealthCheckRegistration(nameof(TestMessage), healthCheck, default, default)
            };

            //Act
            var healthCheckResult = await healthCheck.CheckHealthAsync(healthCheckContext);

            //Assert
            Assert.Equal(HealthStatus.Unhealthy, healthCheckResult.Status);
        }
    }
}
