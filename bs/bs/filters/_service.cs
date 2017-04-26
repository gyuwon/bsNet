using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static bool bsInited = false;
        public static void service(IConfigurationRoot configuration, IServiceCollection services) {
            if (bsInited) return;
            bsInited = true;

            assemblyInit();
            dbInit(configuration);

            if (services == null) return;
            services.AddScoped<bs>();
            services.AddMvc(opt => {
                opt.Filters.Add(new FilterAction());
            });            
        }

        private static IHostingEnvironment environment;
        private static ILogger logger;
        private static void config(ILogger l, IHostingEnvironment e) {
            if(logger == null) logger = l;
            if(environment == null) environment = e;
        }
    }
}