namespace MicroService.Common.Configuration
{
    public class ApplicationOptions
    {
        public HealthCheckConfiguration? HealthCheckConfiguration { get; set; }

        public GrafanaLokiConfiguration? GrafanaLokiConfiguration { get; set; }

        public JaegerConfiguration? JaegerConfiguration { get; set; }
    }
}
