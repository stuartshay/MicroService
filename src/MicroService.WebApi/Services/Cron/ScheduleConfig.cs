namespace MicroService.WebApi.Services.Cron
{
    /// <summary>
    /// Configuration for a scheduled cron job's expression and time zone.
    /// </summary>
    public interface IScheduleConfig
    {
        /// <summary>
        /// 
        /// </summary>
        string? CronExpression { get; set; }

        /// <summary>
        /// 
        /// </summary>
        TimeZoneInfo? TimeZoneInfo { get; set; }
    }

    /// <summary>
    /// Default implementation of <see cref="IScheduleConfig"/>.
    /// </summary>
    public class ScheduleConfig : IScheduleConfig
    {
        /// <inheritdoc/>
        public string? CronExpression { get; set; }

        /// <inheritdoc/>
        public TimeZoneInfo? TimeZoneInfo { get; set; }
    }
}
