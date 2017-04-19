using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
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
                .AddJsonFile("C:\\Users\\user\\documents\\visual studio 2017\\Projects\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var bs = new bs(builder.Build());
        }
        [Fact]
        public void Test1()
        {
            bs.Vali.add("test1", "a", "int", "b", "float");
            var result = bs.valiResult();
            var isOK = bs.Vali.get("test1").check(out result, "a", "1242", "b", "33.3452");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, 1242);
            Assert.Equal(result["b"].value, 33.3452F);
        }
    }
}
