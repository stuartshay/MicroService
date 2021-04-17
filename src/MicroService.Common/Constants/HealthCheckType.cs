namespace MicroService.Common.Constants
{
    public enum HealthCheckType
    {
        Infrastructure,

        Database,

        Logging,

        Monitoring,

        Metrics,

        System,

        ReadinessCheck,
    }
}
