namespace MicroService.Common.Configuration
{
    public class JaegerConfiguration
    {
        public string? AgentHost { get; set; }

        public string? ServiceName { get; set; }

        public int AgentPort { get; set; }

        public bool Enabled { get; set; }

        public bool RemoteAgentEnabled { get; set; }

    }
}
