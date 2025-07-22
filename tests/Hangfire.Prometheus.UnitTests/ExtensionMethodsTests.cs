using Microsoft.AspNetCore.Builder;
using Moq;
using System;
using Xunit;

namespace Hangfire.Prometheus.UnitTests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void UninitializedJobStorage_ThrowsException()
        {
            var appBuilderMock = new Mock<IApplicationBuilder>();
            appBuilderMock.Setup(x => x.ApplicationServices.GetService(typeof(JobStorage)))
                          .Returns(null);

            var ex = Assert.Throws<Exception>(() => appBuilderMock.Object.UsePrometheusHangfireExporter(new HangfirePrometheusSettings()));
            Assert.Equal("Cannot find Hangfire JobStorage class.", ex.Message);
        }

        [Fact]
        public void InitializedJobStorage_DoesNotThrow()
        {
            var appBuilderMock = new Mock<IApplicationBuilder>();
            appBuilderMock.Setup(x => x.ApplicationServices.GetService(typeof(JobStorage)))
                          .Returns(new Mock<JobStorage>().Object);

            appBuilderMock.Object.UsePrometheusHangfireExporter(new HangfirePrometheusSettings());
        }
        
        [Fact]
        public void UsePrometheusHangfireExporter_UsesDefaultSettings()
        {
            var appBuilderMock = new Mock<IApplicationBuilder>();
            appBuilderMock.Setup(x => x.ApplicationServices.GetService(typeof(JobStorage)))
                .Returns(new Mock<JobStorage>().Object);

            appBuilderMock.Object.UsePrometheusHangfireExporter();
        }
    }
}
