# Hangfire.Prometheus
[![NuGet Package](https://img.shields.io/nuget/v/Hangfire.Prometheus.Exporter.svg)](https://www.nuget.org/packages/Hangfire.Prometheus.Exporter)
[![codecov](https://codecov.io/gh/nicolaj-hartmann/Hangfire.Prometheus/graph/badge.svg?token=M8W3f2PlZu)](https://codecov.io/gh/nicolaj-hartmann/Hangfire.Prometheus)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnicolaj-hartmann%2FHangfire.Prometheus.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnicolaj-hartmann%2FHangfire.Prometheus?ref=badge_shield)

Simple plugin for .NET Core applications to export Hangfire stats to Prometheus.

# Initial Kudos

This plugin was extended based on [Hangfire.Prometheus](https://github.com/fiftyonefifty/Hangfire.Prometheus/tree/master).
All credits go to [fiftyonefifty](https://github.com/fiftyonefifty) for the original work.

# Description
The plugin uses the Hangfire JobStorage class to export metric "hangfire_job_count" using "state" label to indicate jobs in various states. The metrics are updated before every scrape. The states exported are:

* Failed
* Enqueued
* Scheduled
* Processing
* Succeeded
* Retry

# Usage
Hangfire.Prometheus plugin is initialized in Configure() using UseHangfirePrometheusExporter() method. Hangfire job storage must already be initialized.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddHangfire(...);
}

public void Configure(IApplicationBuilder app)
{
    app.UsePrometheusHangfireExporter();
    app.UseMetricServer();
    app.UseHangfireDashboard();
    app.UseHangfireServer();
}
```

## Settings

The following settings are available for this plugin using Hangfire.Prometheus.HangfirePrometheusSettings class:

|     Setting Name      |             Type             |    Default Setting     |                                             Description                                             |
| --------------------- | ---------------------------- | ---------------------- | ----------------------------------------------------------------------------------------------------|
| CollectorRegistry     | Prometheus.CollectorRegistry |Metrics.DefaultRegistry | Prometheus CollectorRegistry to use.                                                                |
| FailScrapeOnException | Boolean                      | true                   | Controls whether to fail the scrape if there is an exception during Hangifre statistics collection. |

An instance of HangfirePrometheusSettings class can be passed to UsePrometheusHangfireExporter() to use settings other than defaults:

```
public void Configure(IApplicationBuilder app)
{
    CollectionRegistry myRegistry = Metrics.NewCustomRegistry();
    app.UsePrometheusHangfireExporter(new HangfirePrometheusSettings { CollectorRegistry = myRegistry });
    app.UseMetricServer(...);
}
```

## Simultaneous Scrapes
Simultaneous scrapes proceed at the same time. Care should be taken when setting the scrape interval period to minimize simultaneous scrapes.

# Multiple Servers
This plugin uses Hangfire job storage to retrieve job statistics. If multiple Hangfire servers are using the same job storage only a single instance should be exporting Hangfire metrics or only a single instance must be scraped. 


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fnicolaj-hartmann%2FHangfire.Prometheus.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fnicolaj-hartmann%2FHangfire.Prometheus?ref=badge_large)
