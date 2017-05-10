using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace test {
    public class bsHttp {
        private bs bs;
        public bsHttp() {
            string path = null;
            foreach(var k in new string[] {
                "C:\\Users\\user\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2",
                "D:\\dev\\cmpsedu\\WebApplication2\\WebApplication2",
                "C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2"
            }) {
                if(Directory.Exists(k)) {
                    path = k + "\\appsettings.json";
                    break;
                }
            };
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var bs = new bs(null, null);
            bs.service(builder.Build(), null);
        }
        [Fact]
        public async Task Test16() {
            var a = await bs.GET<string>("http://www.naver.com");
            Assert.Equal("<!doctype html>", a.Substring(0, "<!doctype html>".Length));
        }
    }
}