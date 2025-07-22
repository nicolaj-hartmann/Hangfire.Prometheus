using Prometheus;
using System;
using Xunit;

namespace Hangfire.Prometheus.UnitTests
{
    public class SettingsTests
    {
        [Fact]
        public void SettingsDefaultsTest()
        {
            var settings = new HangfirePrometheusSettings();
            Assert.Same(Metrics.DefaultRegistry, settings.CollectorRegistry);
            Assert.True(settings.FailScrapeOnException);
        }

        [Fact]
        public void SettingCustomRegistryPositive()
        {
            var settings = new HangfirePrometheusSettings();
            var collectorRegistry = Metrics.NewCustomRegistry();
            settings.CollectorRegistry = collectorRegistry;
            Assert.Same(collectorRegistry, settings.CollectorRegistry);
        }

        [Fact]
        public void SettingCustomRegistryToNullThrowsException()
        {
            var settings = new HangfirePrometheusSettings();
            Assert.Throws<ArgumentNullException>(() => settings.CollectorRegistry = null);
        }

        [Fact]
        public void SettingFailScrapeOnExceptionToNonDefault()
        {
            var settings = new HangfirePrometheusSettings
            {
                FailScrapeOnException = false
            };
            Assert.False(settings.FailScrapeOnException);
        }
    }
}
