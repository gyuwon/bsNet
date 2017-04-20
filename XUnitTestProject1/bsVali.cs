using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using Xunit;

namespace test
{
    public class bsVali
    {
        private bs bs;
        public bsVali() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var bs = new bs(null);
            bs.service(builder.Build(), null);
        }
        [Fact]
        public void Test1()
        {
            bs.vali("test1", "a", "int", "b", "float");
            var result = bs.valiResult();
            var isOK = bs.vali("test1").check(out result, "a", "1242", "b", "33.3452");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, 1242);
            Assert.Equal(result["b"].value, 33.3452F);
        }
        [Fact]
        public void Test2()
        {
            bs.vali("test2", "a", "ip", "b", "url", "c", "url");
            var result = bs.valiResult();
            var isOK = bs.vali("test1").check(out result, "a", "1242", "b", "33.3452");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, 1242);
            Assert.Equal(result["b"].value, 33.3452F);
        }
    }
}
