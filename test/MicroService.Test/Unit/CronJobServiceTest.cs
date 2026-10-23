using MicroService.WebApi.Services.Cron;
using Xunit;

namespace MicroService.Test.Unit
{
    public class CronJobServiceTest
    {
        [Fact]
        public async Task Start_SchedulesNextOccurrence_AndRunReschedules()
        {
            var clock = new RecordingTimeProvider();
            using var job = new TestJob(clock);
            await job.StartAsync(CancellationToken.None);

            Assert.Equal(TimeSpan.FromSeconds(30), clock.Delay);
            Assert.Equal(Timeout.InfiniteTimeSpan, clock.Period);
            Assert.Equal(1, clock.TimerCount);

            clock.LastTimer!.Fire();

            Assert.Equal(1, job.RunCount);
            Assert.Equal(2, clock.TimerCount);
        }

        [Fact]
        public async Task CancelledRun_DoesNotExecuteOrReschedule()
        {
            var clock = new RecordingTimeProvider();
            using var job = new TestJob(clock);
            using var cancellation = new CancellationTokenSource();
            await job.StartAsync(cancellation.Token);
            cancellation.Cancel();

            await job.Run(cancellation.Token);

            Assert.Equal(0, job.RunCount);
            Assert.Equal(1, clock.TimerCount);
            Assert.True(clock.LastTimer!.Disposed);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task StopOrDispose_PreventsFurtherWork(bool stop)
        {
            var clock = new RecordingTimeProvider();
            using var job = new TestJob(clock);
            await job.StartAsync(CancellationToken.None);
            if (stop)
                await job.StopAsync(CancellationToken.None);
            else
                job.Dispose();

            await job.Run(CancellationToken.None);
            await job.StartAsync(CancellationToken.None);
            job.Dispose();

            Assert.Equal(0, job.RunCount);
            Assert.Equal(1, clock.TimerCount);
            Assert.True(clock.LastTimer!.Disposed);
        }

        [Fact]
        public async Task CancelledStart_DoesNotCreateTimer()
        {
            var clock = new RecordingTimeProvider();
            using var job = new TestJob(clock);
            await job.StartAsync(new CancellationToken(true));
            Assert.Equal(0, clock.TimerCount);
        }

        [Fact]
        public async Task BaseWork_ObservesCancellation()
        {
            using var job = new DefaultJob();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => job.DoWork(new CancellationToken(true)));
        }

        private sealed class DefaultJob() : CronJobService("* * * * *", TimeZoneInfo.Utc);

        private sealed class TestJob(TimeProvider clock) : CronJobService("* * * * *", TimeZoneInfo.Utc, clock)
        {
            public int RunCount { get; private set; }
            public Task Run(CancellationToken token) => RunScheduledJob(token);
            public override Task DoWork(CancellationToken cancellationToken)
            {
                RunCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingTimeProvider : TimeProvider
        {
            public TimeSpan Delay { get; private set; }
            public TimeSpan Period { get; private set; }
            public int TimerCount { get; private set; }
            public RecordingTimer? LastTimer { get; private set; }
            public override DateTimeOffset GetUtcNow() => new(2026, 1, 1, 0, 0, 30, TimeSpan.Zero);
            public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            {
                Delay = dueTime;
                Period = period;
                TimerCount++;
                LastTimer = new RecordingTimer(callback, state);
                return LastTimer;
            }
        }

        private sealed class RecordingTimer(TimerCallback callback, object? state) : ITimer
        {
            public void Fire() => callback(state);
            public bool Disposed { get; private set; }
            public bool Change(TimeSpan dueTime, TimeSpan period) => !Disposed;
            public void Dispose() => Disposed = true;
            public ValueTask DisposeAsync()
            {
                Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}
