﻿using Amazon.SQS;
using Amazon.SQS.Model;

using Herald.MessageQueue.HealthCheck.Sqs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Moq;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.RabbitMq.Tests
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
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var healthCheckServiceOptions = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var healthCheckRegistration = healthCheckServiceOptions.Value.Registrations.First();
            var healtCheck = healthCheckRegistration.Factory(serviceProvider);

            //Act
            serviceCollection
                .AddScoped(x => amazonSqsMock.Object)
                .AddHealthChecks()
                .AddSqsCheck<TestMessage>();

            //Assert
            Assert.IsType<HealthCheckSqs>(healtCheck);
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
            var healthCheck = new HealthCheckSqs(amazonSqsMock.Object);
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
            var queueUrl = string.Empty;
            var amazonSqsMock = new Mock<IAmazonSQS>();
            amazonSqsMock
                .Setup(x => x.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetQueueUrlResponse() { QueueUrl = queueUrl });
            var healthCheck = new HealthCheckSqs(amazonSqsMock.Object);
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
