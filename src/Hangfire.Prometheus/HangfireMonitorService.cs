namespace Hangfire.Prometheus
{
    public class HangfireMonitorService(JobStorage hangfireJobStorage) : IHangfireMonitorService
    {
        private const string RetrySetName = "retries";
        
        public HangfireJobStatistics GetJobStatistics()
        {
            var hangfireStats = hangfireJobStorage.GetMonitoringApi().GetStatistics();

            using var storageConnection = hangfireJobStorage.GetConnection();
            var retryJobs = storageConnection.GetAllItemsFromSet(RetrySetName).Count;

            return new HangfireJobStatistics
            {
                Failed = hangfireStats.Failed,
                Enqueued = hangfireStats.Enqueued,
                Scheduled = hangfireStats.Scheduled,
                Processing = hangfireStats.Processing,
                Succeeded = hangfireStats.Succeeded,
                Retry = retryJobs
            };
        }
    }
}