using AutoFixture;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Hangfire.Prometheus.UnitTests
{
    public class HangfireMonitorTests
    {
        private const string RetryKey = "retries";

        private readonly Fixture _fixture = new();
        private readonly StatisticsDto _expectedStats;
        private readonly HashSet<string> _expectedRetrySet;
        private readonly IHangfireMonitorService _hangfireMonitorService;
        private readonly Mock<IStorageConnection> _storageConnection;

        public HangfireMonitorTests()
        {
            _expectedStats = _fixture.Create<StatisticsDto>();
            _expectedRetrySet = new HashSet<string>();
            _expectedRetrySet.AddMany(() => _fixture.Create<string>(), new Random().Next(100));

            _storageConnection = new Mock<IStorageConnection>();
            _storageConnection.Setup(x => x.GetAllItemsFromSet(RetryKey)).Returns(_expectedRetrySet);
            _storageConnection.Setup(x => x.Dispose());

            var mockMonitoringApi = new Mock<IMonitoringApi>();
            mockMonitoringApi.Setup(x => x.GetStatistics()).Returns(_expectedStats);

            var mockStorage = new Mock<JobStorage>();
            mockStorage.Setup(x => x.GetConnection()).Returns(_storageConnection.Object);
            mockStorage.Setup(x => x.GetMonitoringApi()).Returns(mockMonitoringApi.Object);

            _hangfireMonitorService = new HangfireMonitorService(mockStorage.Object);
        }

        [Fact]
        public void ShouldGetNumberOfJobs()
        {
            var actual = _hangfireMonitorService.GetJobStatistics();
            Assert.Equal(_expectedStats.Failed, actual.Failed);
            Assert.Equal(_expectedStats.Enqueued, actual.Enqueued);
            Assert.Equal(_expectedStats.Scheduled, actual.Scheduled);
            Assert.Equal(_expectedStats.Processing, actual.Processing);
            Assert.Equal(_expectedStats.Succeeded, actual.Succeeded);
            Assert.Equal(_expectedRetrySet.Count, actual.Retry);
        }

        [Fact]
        public void ShouldDisposeOfStorageConnection()
        {
            _hangfireMonitorService.GetJobStatistics();
            _storageConnection.Verify(x => x.Dispose(), Times.AtLeastOnce);
        }
    }
}
