using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using RabbitMQ.Client;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.RabbitMq.Tests
{
    public class HealthCheckRabbitMqTests
    {
        private class TestMessage : MessageBase { };

        [Fact]
        public void ShouldAddHealthCheck()
        {
            //Arrange
            var rabbitMqMock = new Mock<IModel>();
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Act
            serviceCollection
                .AddScoped(x => new MessageQueueOptions())
                .AddScoped(x => rabbitMqMock.Object)
                .AddHealthChecks()
                .AddRabbitMqCheck<TestMessage>();

            //Assert
            Assert.IsType<HealthCheckRabbitMq>(healtCheck);
        }

        [Fact]
        public async Task ShouldBeHealthy()
        {
            //Arrange
            var rabbitMqMock = new Mock<IModel>();
            var healthCheck = new HealthCheckRabbitMq(rabbitMqMock.Object, new MessageQueueOptions());
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
        public async Task ShouldBeUnhealthy()
        {
            //Arrange
            var rabbitMqMock = new Mock<IModel>();
            rabbitMqMock
                .Setup(x => x.QueueDeclarePassive(It.IsAny<string>()))
                .Throws<Exception>();
            var healthCheck = new HealthCheckRabbitMq(rabbitMqMock.Object, new MessageQueueOptions());
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
