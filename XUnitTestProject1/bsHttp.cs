using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace test {
    public class bsHttp : bs.Test{
        public bsHttp() : base(
            "C:\\Users\\user\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2",
            "D:\\dev\\cmpsedu\\WebApplication2\\WebApplication2",
            "C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2"
        ) {}
        [Fact]
        public async Task Test16() {
            var a = await bs.GET<string>("http://www.naver.com");
            Assert.Equal("<!doctype html>", a.Substring(0, "<!doctype html>".Length));
        }
    }
}