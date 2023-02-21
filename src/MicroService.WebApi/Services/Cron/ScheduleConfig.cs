namespace MicroService.WebApi.Services.Cron
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScheduleConfig<T>
    {
        string? CronExpression { get; set; }

        TimeZoneInfo TimeZoneInfo { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        /// <inheritdoc/>
        public string? CronExpression { get; set; }

        /// <inheritdoc/>
        public TimeZoneInfo? TimeZoneInfo { get; set; }
    }
}
