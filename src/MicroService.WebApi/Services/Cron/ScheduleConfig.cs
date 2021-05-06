using System;

namespace MicroService.WebApi.Services.Cron
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }

        TimeZoneInfo TimeZoneInfo { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; }
    }
}
