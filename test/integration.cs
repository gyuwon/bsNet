using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Xunit;
using System.Threading.Tasks;
using WebApplication2;

namespace WebApplication2 {
    class Integ0
    {
        private TestServer server;
        private HttpClient client;

        public Integ0() {
            server = new TestServer(new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot("C:\\Users\\user\\documents\\visual studio 2017\\Projects\\WebApplication2\\WebApplication2")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights());
            client = server.CreateClient();
        }
        
        public async Task check1() {
            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            Assert.Equal("a", str);
            Assert.True("a" == "b", "bssdsdsdsd");
        }
    }
}
