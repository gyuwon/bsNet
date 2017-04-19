using Microsoft.Extensions.DependencyInjection;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static void service(IServiceCollection service) {
            service.AddMvc(opt => {

            });
        }
    }
}