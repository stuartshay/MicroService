namespace MicroService.Test.Fixture
{
    public abstract class FixtureConfig : IDisposable
    {
        protected FixtureConfig()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine("ENV:" + env);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
