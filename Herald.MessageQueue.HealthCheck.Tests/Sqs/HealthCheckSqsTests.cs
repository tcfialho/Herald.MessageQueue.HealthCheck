using Amazon.SQS;
using Amazon.SQS.Model;

using Herald.MessageQueue.HealthCheck.Sqs;
using Herald.MessageQueue.Sqs;

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

namespace Herald.MessageQueue.HealthCheck.Tests.Sqs
{
    public class HealthCheckSqsTests
    {
        private class TestMessage : MessageBase { };

        [Fact]
        public void ShouldAddHealthCheck()
        {
            //Arrange
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();

            serviceCollection.AddScoped<IConfiguration>(x => config)
                             .AddScoped(x => new MessageQueueOptions())
                             .AddScoped(x => amazonSqsMock.Object)
                             .AddScoped<IQueueInfo, QueueInfo>()
                             .AddHealthChecks()
                             .AddSqsCheck<TestMessage>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();

            //Act
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Assert
            Assert.IsType<HealthCheckSqs<TestMessage>>(healtCheck);
        }

        [Fact]
        public async Task ShouldBeHealthy()
        {
            //Arrange
            var queueUrl = $"http://localhost:4576/event/{nameof(TestMessage)}.fifo";
            var amazonSqsMock = new Mock<IAmazonSQS>();
            amazonSqsMock
                .Setup(x => x.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetQueueUrlResponse() { QueueUrl = queueUrl });
            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);
            var healthCheck = new HealthCheckSqs<TestMessage>(amazonSqsMock.Object, messageQueueInfoMock.Object, 0);
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
            var queueUrl = string.Empty;
            var amazonSqsMock = new Mock<IAmazonSQS>();
            amazonSqsMock
                .Setup(x => x.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetQueueUrlResponse() { QueueUrl = queueUrl });
            var messageQueueInfoMock = new Mock<IQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>())).Returns(typeof(TestMessage).Name);
            var healthCheck = new HealthCheckSqs<TestMessage>(amazonSqsMock.Object, messageQueueInfoMock.Object, 0);
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
