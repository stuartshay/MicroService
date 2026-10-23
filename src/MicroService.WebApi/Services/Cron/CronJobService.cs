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
        private ITimer? _timer;
        private readonly TimeProvider _timeProvider;
        private bool _stopped;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CronJobService"/> class.
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <param name="timeZoneInfo"></param>
        /// <param name="timeProvider">Clock and timer provider; defaults to the system clock.</param>
        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, TimeProvider? timeProvider = null)
        {
            _timeProvider = timeProvider ?? TimeProvider.System;
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
            _stopped = true;
            _timer?.Dispose();
            _timer = null;
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
        protected virtual Task ScheduleJob(CancellationToken cancellationToken)
        {
            if (_disposed || _stopped || cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var now = _timeProvider.GetUtcNow();
            var next = _expression.GetNextOccurrence(now, _timeZoneInfo, false);
            if (next.HasValue)
            {
                var delay = next.Value - now;
                _timer = _timeProvider.CreateTimer(async _ => await RunScheduledJob(cancellationToken),
                    null, delay, Timeout.InfiniteTimeSpan);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes a scheduled run and schedules the following occurrence unless stopped.
        /// </summary>
        protected async Task RunScheduledJob(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _timer = null;
            if (!_disposed && !_stopped && !cancellationToken.IsCancellationRequested)
            {
                await DoWork(cancellationToken);
                await ScheduleJob(cancellationToken);
            }
        }
    }
}
