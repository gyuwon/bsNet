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
            var a = await bs.GET<string>("http://www.naver.com");
            Assert.Equal("<!doctype html>", a.Substring(0, "<!doctype html>".Length));
        }
        [Fact]
        public async Task http_post0() {
            var a = await bs.POST<string>("http://localhost:9785/testText", "a","abc","b","def");
            Assert.Equal("abc::def", a);
        }
    }
}