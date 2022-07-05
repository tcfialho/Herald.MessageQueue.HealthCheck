using Azure.Storage.Queues;
using Azure;
using Herald.MessageQueue.AzureStorageQueue;
using Herald.MessageQueue.HealthCheck.AzureStorageQueue;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Newtonsoft.Json;

namespace Herald.MessageQueue.HealthCheck.Tests.AzureStorageQueue
{
    public class HealthCheckAzureStorageQueueTests
    {
        private class TestMessage : MessageBase { };

        [Fact]
        public void ShouldAddHealthCheck()
        {
            //Arrange
            var mockMessageReponse = new Mock<Response<bool>>();
            mockMessageReponse
                .SetupGet(m => m.Value).Returns(true)
                .Verifiable();

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockMessageReponse.Object)
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();

            serviceCollection.AddScoped<IConfiguration>(x => config)
                             .AddScoped(x => new MessageQueueOptions())
                             .AddScoped(x => queueClientFactoryMock.Object)
                             .AddScoped<IMessageQueueInfo, MessageQueueInfo>()
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
            var mockMessageReponse = new Mock<Response<bool>>();
            mockMessageReponse
                .SetupGet(m => m.Value).Returns(true)
                .Verifiable();

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockMessageReponse.Object)
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();

            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckAzureStorageQueue<TestMessage>(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object, 0);
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
            var mockMessageReponse = new Mock<Response<bool>>();
            mockMessageReponse
                .SetupGet(m => m.Value).Returns(false)
                .Verifiable();

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockMessageReponse.Object)
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();

            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);

            var healthCheck = new HealthCheckAzureStorageQueue<TestMessage>(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object, 0);
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
