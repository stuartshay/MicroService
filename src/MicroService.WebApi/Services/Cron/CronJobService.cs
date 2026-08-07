using Cronos;

namespace MicroService.WebApi.Services.Cron
{
    /// <summary>
    /// Cron Job Service
    /// </summary>
    public abstract class CronJobService : IHostedService, IDisposable
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private System.Timers.Timer? _timer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CronJobService"/> class.
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <param name="timeZoneInfo"></param>
        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZoneInfo = timeZoneInfo;
        }

        /// <inheritdoc/>
        public virtual async Task StartAsync(CancellationToken cancellationToken) => await ScheduleJob(cancellationToken);

        /// <summary>
        /// Performs the work for a single scheduled run. Override in a derived class.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task DoWork(CancellationToken cancellationToken) => await Task.Delay(5000, cancellationToken);

        /// <summary>
        /// Stops the scheduled timer.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Releases the resources used by this <see cref="CronJobService"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the scheduled timer when disposing.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Schedules the next run based on the configured cron expression, then recurses after each run.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo, true);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)
                {
                    await ScheduleJob(cancellationToken);
                }

                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null!;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await DoWork(cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleJob(cancellationToken);
                    }
                };
                _timer.Start();
            }

            await Task.CompletedTask;
        }
    }
}
