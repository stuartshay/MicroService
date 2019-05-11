using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MicroService.WebApi
{
    /// <summary>
    /// Represents the current application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point to the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Builds a new web host for the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
