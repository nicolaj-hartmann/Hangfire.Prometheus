using Prometheus;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Prometheus
{
    public static class Extensions
    {
        
        /// <summary>
        /// Initializes Prometheus Hangfire Exporter using current Hangfire job storage and default metrics registry.
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <returns>Provided instance of IApplicationBuilder</returns>
        public static IApplicationBuilder UsePrometheusHangfireExporter(this IApplicationBuilder app)
            => UsePrometheusHangfireExporter(app, new HangfirePrometheusSettings());

        /// <summary>
        /// Initializes Prometheus Hangfire Exporter using current Hangfire job storage and default metrics registry.
        /// </summary>
        /// <param name="app">IApplicationBuilder instance</param>
        /// <param name="settings">Settings instance</param>
        /// <returns>Provided instance of IApplicationBuilder</returns>
        public static IApplicationBuilder UsePrometheusHangfireExporter(this IApplicationBuilder app,
            HangfirePrometheusSettings settings)
        {
            var js = app.ApplicationServices.GetService<JobStorage>();
            if (js == null)
            {
                throw new Exception("Cannot find Hangfire JobStorage class.");
            }

            IHangfireMonitorService hangfireMonitor = new HangfireMonitorService(js);
            IPrometheusExporter exporter = new HangfirePrometheusExporter(hangfireMonitor, settings);
            Metrics.DefaultRegistry.AddBeforeCollectCallback(() => exporter.ExportHangfireStatistics());
            return app;
        }
    }
}