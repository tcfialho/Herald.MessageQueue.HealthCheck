using Herald.MessageQueue.AzureStorageQueue;
using Herald.MessageQueue.HealthCheck.AzureStorageQueue;

using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.HealthCheck.Tests.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueueTests
    {
        private class TestMessage : MessageBase { };

        [Fact]
        public void ShouldAddHealthCheck()
        {
            //Arrange
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), null, null);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(x => new MessageQueueOptions())
                             .AddScoped(x => clouldQueueClientMock.Object)
                             .AddScoped<IQueueInfo, QueueInfo>()
                             .AddHealthChecks()
                             .AddAzureStorageQueueCheck<TestMessage>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();

            //Act
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Assert
            Assert.IsType<HealthCheckAzureStorageQueue<TestMessage>>(healtCheck);
        }

        [Fact]
        public async Task ShouldBeHealthy()
        {
            //Arrange
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), null, null);

            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckAzureStorageQueue<TestMessage>(clouldQueueClientMock.Object, messageQueueInfoMock.Object, 0);
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
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), null, null);
            clouldQueueClientMock
                .Setup(x => x.GetQueueReference(It.IsAny<string>()))
                .Throws<Exception>();

            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckAzureStorageQueue<TestMessage>(clouldQueueClientMock.Object, messageQueueInfoMock.Object, 0);
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
