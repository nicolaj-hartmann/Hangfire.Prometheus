using AutoFixture;
using Moq;
using Prometheus;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Hangfire.Prometheus.UnitTests
{
    public class PrometheusExporterTests
    {
        private readonly HangfirePrometheusExporter _classUnderTest;

        private readonly Mock<IHangfireMonitorService> _mockHangfireMonitor;

        private readonly IFixture _autoFixture;

        private readonly CollectorRegistry _collectorRegistry;

        private readonly HangfirePrometheusSettings _settings;

        private const string MetricName = "hangfire_job_count";
        private const string MetricHelp = "Number of Hangfire jobs";
        private const string StateLabelName = "state";

        private const string FailedLabelValue = "failed";
        private const string EnqueuedLabelValue = "enqueued";
        private const string ScheduledLabelValue = "scheduled";
        private const string ProcessingLabelValue = "processing";
        private const string SucceededLabelValue = "succeeded";
        private const string RetryLabelValue = "retry";

        public PrometheusExporterTests()
        {
            _autoFixture = new Fixture();

            _mockHangfireMonitor = new Mock<IHangfireMonitorService>();

            _collectorRegistry = Metrics.NewCustomRegistry();

            _settings = new HangfirePrometheusSettings
            {
                CollectorRegistry = _collectorRegistry
            };

            _classUnderTest = new HangfirePrometheusExporter(_mockHangfireMonitor.Object, _settings);
        }

        [Fact]
        public void ConstructorHangfireMonitorNullCheck() => Assert.Throws<ArgumentNullException>(() => new HangfirePrometheusExporter(null, _settings));

        [Fact]
        public void MetricsWithAllStatesGetCreated()
        {
            var hangfireJobStatistics = _autoFixture.Create<HangfireJobStatistics>();
            PerformMetricsTest(hangfireJobStatistics);
            _mockHangfireMonitor.Verify(x => x.GetJobStatistics(), Times.Once);

        }

        [Fact]
        public void MetricsWithAllStatesGetUpdatedOnSubsequentCalls()
        {
            var count = 10;
            for (var i = 0; i < count; i++)
            {
                var hangfireJobStatistics = _autoFixture.Create<HangfireJobStatistics>();
                PerformMetricsTest(hangfireJobStatistics);
            }

            _mockHangfireMonitor.Verify(x => x.GetJobStatistics(), Times.Exactly(10));
        }

        [Fact]
        public void MetricsShouldNotGetPublishedFirstTimeOnException()
        {
            _mockHangfireMonitor.Setup(x => x.GetJobStatistics()).Throws(new Exception());
            var actual = GetPrometheusContent();

            //The metric description will get published regardless, so we have to test for the labeled metrics.
            Assert.DoesNotContain($"{MetricName}{{{StateLabelName}=\"", actual);
        }

        [Fact]
        public void MetricsShouldNotGetUpdatedOnExceptionWhenSettingDisabled()
        {
            _settings.FailScrapeOnException = false;

            var hangfireJobStatistics = _autoFixture.Create<HangfireJobStatistics>();
            PerformMetricsTest(hangfireJobStatistics);

            _mockHangfireMonitor.Setup(x => x.GetJobStatistics()).Throws(new Exception());
            _classUnderTest.ExportHangfireStatistics();
            VerifyPrometheusMetrics(hangfireJobStatistics);

            hangfireJobStatistics = _autoFixture.Create<HangfireJobStatistics>();
            PerformMetricsTest(hangfireJobStatistics);

            _mockHangfireMonitor.Verify(x => x.GetJobStatistics(), Times.Exactly(3));
        }

        [Fact]
        public void ScrapeShouldFailOnExceptionWhenSettingEnabled()
        {
            var hangfireJobStatistics = _autoFixture.Create<HangfireJobStatistics>();
            PerformMetricsTest(hangfireJobStatistics);

            _settings.FailScrapeOnException = true;
            var exToThrow = new Exception();
            _mockHangfireMonitor.Setup(x => x.GetJobStatistics()).Throws(exToThrow);
            var ex = Assert.Throws<ScrapeFailedException>(() => _classUnderTest.ExportHangfireStatistics());
            Assert.Same(exToThrow, ex.InnerException);
            Assert.Equal("Scrape failed due to exception. See InnerException for details.", ex.Message);
        }

        private void PerformMetricsTest(HangfireJobStatistics hangfireJobStatistics)
        {
            _mockHangfireMonitor.Setup(x => x.GetJobStatistics()).Returns(hangfireJobStatistics);
            _classUnderTest.ExportHangfireStatistics();
            VerifyPrometheusMetrics(hangfireJobStatistics);
        }

        private void VerifyPrometheusMetrics(HangfireJobStatistics hangfireJobStatistics)
        {
            var expectedStrings = CreateExpectedStrings(hangfireJobStatistics);
            var actual = GetPrometheusContent();

            foreach (var expected in expectedStrings)
            {
                Assert.Contains(expected, actual);
            }
        }

        private List<string> CreateExpectedStrings(HangfireJobStatistics hangfireJobStatistics)
        {
            var expectedStrings = new List<string>
            {
                $"# HELP {MetricName} {MetricHelp}\n# TYPE {MetricName} gauge",
                GetMetricString(FailedLabelValue, hangfireJobStatistics.Failed),
                GetMetricString(EnqueuedLabelValue, hangfireJobStatistics.Enqueued),
                GetMetricString(ScheduledLabelValue, hangfireJobStatistics.Scheduled),
                GetMetricString(ProcessingLabelValue, hangfireJobStatistics.Processing),
                GetMetricString(SucceededLabelValue, hangfireJobStatistics.Succeeded),
                GetMetricString(RetryLabelValue, hangfireJobStatistics.Retry)
            };
            return expectedStrings;
        }

        private static string GetMetricString(string labelValue, double metricValue)
        {
            return $"{MetricName}{{{StateLabelName}=\"{labelValue}\"}} {metricValue}";
        }

        private string GetPrometheusContent()
        {
            using var myStream = new MemoryStream();
            _collectorRegistry.CollectAndExportAsTextAsync(myStream).Wait();
            myStream.Seek(0, SeekOrigin.Begin);
            using var sr = new StreamReader(myStream);
            var content = sr.ReadToEnd();

            return content;
        }
    }
}
