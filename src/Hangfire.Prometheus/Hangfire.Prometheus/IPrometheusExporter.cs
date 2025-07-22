namespace Hangfire.Prometheus
{
    public interface IPrometheusExporter
    {
        /// <summary>
        /// Exports current Hangfire job statistics into Prometheus metrics.
        /// </summary>
        void ExportHangfireStatistics();
    }
}
