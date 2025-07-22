using Prometheus;
using System;

namespace Hangfire.Prometheus
{
    public class HangfirePrometheusExporter : IPrometheusExporter
    {
        private readonly IHangfireMonitorService _hangfireMonitorService;
        private readonly HangfirePrometheusSettings _settings;
        private readonly Gauge _hangfireGauge;

        private const string MetricName = "hangfire_job_count";
        private const string MetricHelp = "Number of Hangfire jobs";
        private const string StateLabelName = "state";

        private const string FailedLabelValue = "failed";
        private const string EnqueuedLabelValue = "enqueued";
        private const string ScheduledLabelValue = "scheduled";
        private const string ProcessingLabelValue = "processing";
        private const string SucceededLabelValue = "succeeded";
        private const string RetryLabelValue = "retry";

        public HangfirePrometheusExporter(IHangfireMonitorService hangfireMonitorService, HangfirePrometheusSettings settings)
        {
            _hangfireMonitorService = hangfireMonitorService ?? throw new ArgumentNullException(nameof(hangfireMonitorService));
            _settings = settings;
            var collectorRegistry = settings.CollectorRegistry;
            _hangfireGauge = Metrics.WithCustomRegistry(collectorRegistry).CreateGauge(MetricName, MetricHelp, StateLabelName);
        }

        public void ExportHangfireStatistics()
        {
            try
            {
                var hangfireJobStatistics = _hangfireMonitorService.GetJobStatistics();
                _hangfireGauge.WithLabels(FailedLabelValue).Set(hangfireJobStatistics.Failed);
                _hangfireGauge.WithLabels(ScheduledLabelValue).Set(hangfireJobStatistics.Scheduled);
                _hangfireGauge.WithLabels(ProcessingLabelValue).Set(hangfireJobStatistics.Processing);
                _hangfireGauge.WithLabels(EnqueuedLabelValue).Set(hangfireJobStatistics.Enqueued);
                _hangfireGauge.WithLabels(SucceededLabelValue).Set(hangfireJobStatistics.Succeeded);
                _hangfireGauge.WithLabels(RetryLabelValue).Set(hangfireJobStatistics.Retry);
            }
            catch (Exception ex)
            {
                if (_settings.FailScrapeOnException)
                {
                    throw new ScrapeFailedException("Scrape failed due to exception. See InnerException for details.", ex);
                }
            }
        }
    }
}
