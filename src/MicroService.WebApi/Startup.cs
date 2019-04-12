using MicroService.Service.Configuration;
using MicroService.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroService.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Hosting Environment.
        /// </summary>
        private IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ApplicationOptions>(Configuration);
            services.AddSingleton(Configuration);

            services.AddCorsConfiguration(Configuration);
            services.AddSwaggerConfiguration(Configuration);

            var config = Configuration.Get<ApplicationOptions>();
            services.DisplayConfiguration(Configuration, HostingEnvironment);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            ConfigureSwagger(app);
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void ConfigureSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string swaggerEndpoint = $"/swagger/v1/swagger.json";
                c.SwaggerEndpoint(swaggerEndpoint, "MicroService.WebApi");
            });
        }
    }
}
