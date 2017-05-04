using com.bsidesoft.cs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApplication2 {
    public class Startup {
        private IConfigurationRoot Configuration;
        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public void ConfigureServices(IServiceCollection services) {
            bs.service(Configuration, services);
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            } else {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            var i = 0;
            app.UseMvc(routes => {
                routes.MapRoute(
                    name: (i++) + "",
                    template: "professor/contents/{controller=Contents}/{action=Index}"
                );
                routes.MapRoute(
                    name: (i++) + "",
                    template: "professor/{action=Index}",
                    defaults: new { controller = "Professor" }
                );
                routes.MapRoute(
                    name: (i++) + "",
                    template: "professor/popup/{controller=Student}/{action=Index}"
                );
                routes.MapRoute(
                    name: (i++) + "",
                    template: "{action=Index}",
                    defaults: new { controller = "Home" }
                );
            });
        }
    }
}