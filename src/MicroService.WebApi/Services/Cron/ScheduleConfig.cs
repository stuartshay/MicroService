namespace MicroService.WebApi.Services.Cron
{
    /// <summary>
    /// 
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
    /// 
    /// </summary>
    public class ScheduleConfig : IScheduleConfig
    {
        /// <inheritdoc/>
        public string? CronExpression { get; set; }

        /// <inheritdoc/>
        public TimeZoneInfo? TimeZoneInfo { get; set; }
    }
}
