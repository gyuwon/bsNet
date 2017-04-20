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
        public bsVali()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var bs = new bs(null);
            bs.service(builder.Build(), null);
        }
        /*
        [Fact]
        public void Test0() {
            bs.vali("test0", "a", "creditCard", "b", "html", "c", "image");
            var result = bs.valiResult();
            var isOK = bs.vali("test0").check(out result, "a", "5342920001495006", "b", "<div>div<span>span</span></div><br>", "c", "asdfasdf.jpg");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "5342920001495006");
            Assert.Equal(result["b"].value, "<div>div<span>span</span></div><br>");
            Assert.Equal(result["c"].value, "asdfasdf.jpg");
        }
        [Fact]
        public void Test1()
        {
            bs.vali("test1", "a", "ip", "b", "url", "c", "email");
            var result = bs.valiResult();
            var isOK = bs.vali("test1").check(out result, "a", "112.220.245.82", "b", "http://www.naver.com", "c", "hyej.kim@bsidesoft.com");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "112.220.245.82");
            Assert.Equal(result["b"].value, "http://www.naver.com");
            Assert.Equal(result["c"].value, "hyej.kim@bsidesoft.com");
        }
        [Fact]
        public void Test2()
        {
            bs.vali("test2", "a", "korean", "b", "japanese");
            var result = bs.valiResult();
            var isOK = bs.vali("test2").check(out result, "a", "¾È³ç", "b", "ª¢ªêª¬ªÈª¦ª´ª¶ª¤ªÞª¹");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "¾È³ç");
            Assert.Equal(result["b"].value, "ª¢ªêª¬ªÈª¦ª´ª¶ª¤ªÞª¹");
        }
        [Fact]
        public void Test3()
        {
            bs.vali("test3", "a", "alpha", "b", "ALPHA", "c", "alpha", "d", "1ALPHA");//|1alpha
            var result = bs.valiResult();
            var isOK = bs.vali("test3").check(out result, "a", "abcdefghijklmnopqrstuvwxyz", "b", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "c", "a", "d", "Y");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "abcdefghijklmnopqrstuvwxyz");
            Assert.Equal(result["b"].value, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.Equal(result["c"].value, "a");
            Assert.Equal(result["d"].value, "Y");
        }
        [Fact]
        public void Test4()
        {
            bs.vali("test4", "a", "num", "b", "alphanum");
            var result = bs.valiResult();
            var isOK = bs.vali("test4").check(out result, "a", "-1234.567890", "b", "abcdefghijklmnopqrstuvwxyz0123456789");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "-1234.567890");
            Assert.Equal(result["b"].value, "abcdefghijklmnopqrstuvwxyz0123456789");
        }*/
        [Fact]
        public void Test10() {
            bs.vali("test10", "a", "int", "b", "float");
            var result = bs.valiResult();
            var isOK = bs.vali("test10").check(out result, "a", "1242", "b", "33.3452");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, 1242);
            Assert.Equal(result["b"].value, 33.3452F);
        }
    }
}