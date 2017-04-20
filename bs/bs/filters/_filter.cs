using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static bool bsInited = false;
        public static void service(IConfigurationRoot configuration, IServiceCollection services) {
            if(bsInited) throw new Exception();
            bsInited = true;
            if (services == null) return;
            services.AddScoped<bs>();
            services.AddMvc(opt => {

            });
            dbInit(configuration);
        }
    }
}