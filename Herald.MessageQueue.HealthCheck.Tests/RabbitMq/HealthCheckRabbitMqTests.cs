using Herald.MessageQueue.HealthCheck.RabbitMq;
using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using RabbitMQ.Client;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.HealthCheck.Tests.RabbitMq
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
            serviceCollection.AddScoped(x => new MessageQueueOptions())
                             .AddScoped(x => rabbitMqMock.Object)
                             .AddScoped<IQueueInfo, QueueInfo>()
                             .AddScoped<IExchangeInfo, ExchangeInfo>()
                             .AddHealthChecks()
                             .AddRabbitMqCheck<TestMessage>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();

            //Act
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Assert
            Assert.IsType<HealthCheckRabbitMq<TestMessage>>(healtCheck);
        }

        [Fact]
        public async Task ShouldBeHealthy()
        {
            //Arrange
            var rabbitMqMock = new Mock<IModel>();

            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var exchangeInfoMock = new Mock<IExchangeInfo>();
            exchangeInfoMock.Setup(x => x.GetExchangeName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckRabbitMq<TestMessage>(rabbitMqMock.Object, messageQueueInfoMock.Object, exchangeInfoMock.Object);
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = new HealthCheckRegistration(nameof(TestMessage), healthCheck, default, default)
            };

            //Act
            var healthCheckResult = await healthCheck.CheckHealthAsync(healthCheckContext, default);

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

            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var exchangeInfoMock = new Mock<IExchangeInfo>();
            exchangeInfoMock.Setup(x => x.GetExchangeName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckRabbitMq<TestMessage>(rabbitMqMock.Object, messageQueueInfoMock.Object, exchangeInfoMock.Object);
            var healthCheckContext = new HealthCheckContext()
            {
                Registration = new HealthCheckRegistration(nameof(TestMessage), healthCheck, default, default)
            };

            //Act
            var healthCheckResult = await healthCheck.CheckHealthAsync(healthCheckContext, default);

            //Assert
            Assert.Equal(HealthStatus.Unhealthy, healthCheckResult.Status);
        }
    }
}
