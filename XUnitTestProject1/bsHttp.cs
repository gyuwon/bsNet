using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace test {
    public class bsHttp : bs.Test{
        public bsHttp() : base(
            "C:\\Users\\user\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2",
            "D:\\dev\\cmpsedu\\WebApplication2\\WebApplication2",
            "C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2"
        ) { }
        
        [Fact]
        public async Task http_get0() {
            //var a = await bs.GET<string>("http://www.naver.com");
            //Assert.Equal("<!doctype html>", a.Substring(0, "<!doctype html>".Length));

            var xls = await bs.GET<Stream>("http://localhost:9785/demo.xlsx");
            var img = await bs.GET<byte[]>("http://localhost:9785/200x200.jpg");
        }
        /*
        [Fact]
        public async Task http_post0() {
            var a = await bs.POST<string>("http://localhost:9785/httppost0", "a", "abc", "b", "123");
            Assert.Equal("abc::123", a);

            var t0 = new Dictionary<string, object>() { { "a", "123" }, { "b", "abc" } };
            var b = await bs.POST<string>("http://localhost:9785/httppost0", t0);
            Assert.Equal("123::abc", b);
        }*/
    }
}