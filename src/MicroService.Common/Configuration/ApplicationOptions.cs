namespace MicroService.Common.Configuration
{
    public class ApplicationOptions
    {
        public GrafanaLokiConfiguration GrafanaLokiConfiguration { get; set; }

        public JaegerConfiguration JaegerConfiguration { get; set; }
    }
}
