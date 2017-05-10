using com.bsidesoft.cs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using Xunit;

namespace test {
    public class bsRsa
    {
        private bs bs;
        private R​SA​Parameters p = new R​SA​Parameters();
        public bsRsa()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("D:\\dev\\cmpsedu\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile("C:\\Users\\hika0\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("C:\\Users\\user\\Documents\\Visual Studio 2017\\Projects\\WebApplication2\\WebApplication2\\appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var bs = new bs(null, null);
            bs.service(builder.Build(), null);
            var n = BigInteger.Parse("19579160939939334264971282204525611731944172893619019759209712156289528980860378672033164235760825723282900348193871051950190013953658941960463089031452404364269503721476236241284015792700835264262839734314564696723261501877759107784604657504350348081273959965406686529089170062268136253938904906635532824296510859016002105655690559115059267476786307037941751235763572931501055146976797606538425089134251611194500570922973015579287289778637105402129208324300035518642730384616767241853993887666288072512402523498267733725021939287517009966986976768028023180137546958580922532786773172365428677544232641888174470601681");
            var e = BigInteger.Parse("65537");
            p.Modulus = n.ToByteArray();
            p.Exponent = e.ToByteArray();
        }
        [Fact]
        public void Test1() {
            var a = bs.rsaEncrypt("test", p, false);
            //Assert.Equal("耿圦덠鼒橦푐ਨ쀅̢쟸ꉌ习ྋ떧뗥⥏氈켲婏苜챕訡駆㦪ᮋॷ긯㓝蓤苰ᄮ퀃噆ᕫ摵뱑驪쒝�資㳽뚤謌䔺ȭ澵낲岒鲨织簻㳧繚ｉ쵡햭ꃢ飇⛮졽译촧뭼�쀳㻛�ℏ䵒푹ꅌ胗ꦥ⠿㲦䲘㗾皀怯㞈퐽謄䫬싼䬡ⲛ츾葜욋ᬠ拺ﲉ\u1fc5᳁蠧㢣졪윤縫팥�秴輐㠘�饌⫢䵡䔯袢帅윶ᱺ䀛\ufaea⚺ꮃ䐠�", a);
            var b = bs.rsaDecrypt(a, p, false);
            Assert.Equal("test", b);
        }
        /*
        [Fact]
        public void Test0() {
            bs.vali("test0", "a", "html", "b", "image");
            var result = bs.valiResult();

            var isOK = bs.vali("test0").check(out result, "a", "<div>div<span>span</span></div><br>", "b", "asdfasdf.jpg");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "<div>div<span>span</span></div><br>");
            Assert.Equal(result["b"].value, "asdfasdf.jpg");
        }
        [Fact]
        public void Test1() {
            bs.vali("test1", "a", "ip", "b", "url", "c", "email");
            var result = bs.valiResult();
            var isOK = bs.vali("test1").check(out result, "a", "112.220.245.82", "b", "http://www.naver.com", "c", "hyej.kim@bsidesoft.com");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "112.220.245.82");
            Assert.Equal(result["b"].value, "http://www.naver.com");
            Assert.Equal(result["c"].value, "hyej.kim@bsidesoft.com");

            var isFAIL = bs.vali("test1").check(out result, "a", "112.220.245", "b", "www.naver.com", "c", "hyej.kim@bsidesoft");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
            Assert.False(bs.isOK(result["c"].value));
        }
        [Fact]
        public void Test2() {
            bs.vali("test2", "a", "korean", "b", "japanese");
            var result = bs.valiResult();
            var isOK = bs.vali("test2").check(out result, "a", "안녕", "b", "ありがとうございます");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "안녕");
            Assert.Equal(result["b"].value, "ありがとうございます");

            var isFAIL = bs.vali("test2").check(out result, "a", "ありがとうございます", "b", "안녕");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test3() {
            bs.vali("test3", "a", "alpha", "b", "ALPHA", "c", "1alpha|alpha", "d", "1ALPHA");
            var result = bs.valiResult();
            var isOK = bs.vali("test3").check(out result, "a", "abcdefghijklmnopqrstuvwxyz", "b", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "c", "a", "d", "Y");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "abcdefghijklmnopqrstuvwxyz");
            Assert.Equal(result["b"].value, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.Equal(result["c"].value, "a");
            Assert.Equal(result["d"].value, "Y");

            var isFAIL = bs.vali("test3").check(out result, "a", "aaaA", "b", "BBBb", "c", "C", "d", "d");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
            Assert.False(bs.isOK(result["c"].value));
            Assert.False(bs.isOK(result["d"].value));
        }
        [Fact]
        public void Test4() {
            bs.vali("test4", "a", "num", "b", "alphanum");
            var result = bs.valiResult();
            var isOK = bs.vali("test4").check(out result, "a", "-1234.567890", "b", "abcdefghijklmnopqrstuvwxyz0123456789");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "-1234.567890");
            Assert.Equal(result["b"].value, "abcdefghijklmnopqrstuvwxyz0123456789");

            var isFAIL = bs.vali("test4").check(out result, "a", "1-234.567.890", "b", "A1BC1");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test10() {
            bs.vali("test10", "a", "int", "b", "float", "c", "int|equalto[a]");
            var result = bs.valiResult();
            var isOK = bs.vali("test10").check(out result, "a", "1242", "b", "33.3452", "c", "1242");
            //Assert.True(false, result["c"].value+"");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, 1242);
            Assert.Equal(result["b"].value, 33.3452F);
            Assert.Equal(result["c"].value, 1242);

            var isFAIL = bs.vali("test10").check(out result, "a", "112.220.245", "b", "120-", "c", "---");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
            Assert.False(bs.isOK(result["c"].value));

            isFAIL = bs.vali("test10").check(out result, "a", "112", "b", "120", "c", "234");
            Assert.Equal(isFAIL, false);
            Assert.Equal(result["a"].value, 112);
            Assert.Equal(result["b"].value, 120F);
            Assert.False(bs.isOK(result["c"].value));
        }
        [Fact]
        public void Test11() {
            bs.vali("test11", "a", "max[5]", "b", "int|max[5]");
            var result = bs.valiResult();
            var isOK = bs.vali("test11").check(out result, "a", "abcd", "b", "3");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "abcd");
            Assert.Equal(result["b"].value, 3);

            var isFAIL = bs.vali("test11").check(out result, "a", "abcded", "b", "8");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test12() {
            bs.vali("test12", "a", "min[2]", "b", "int|min[2]");
            var result = bs.valiResult();
            var isOK = bs.vali("test12").check(out result, "a", "abcd", "b", "3");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "abcd");
            Assert.Equal(result["b"].value, 3);

            var isFAIL = bs.vali("test12").check(out result, "a", "ab", "b", "1");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test13() {
            bs.vali("test13", "a", "length[2]", "b", "int|length[2]");
            var result = bs.valiResult();
            var isOK = bs.vali("test13").check(out result, "a", "ab", "b", "2");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "ab");
            Assert.Equal(result["b"].value, 2);

            var isFAIL = bs.vali("test13").check(out result, "a", "abc", "b", "1");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test14() {
            bs.vali("test14", "a", "range[2,5]", "b", "int|length[2,5]");
            var result = bs.valiResult();
            var isOK = bs.vali("test14").check(out result, "a", "abcd", "b", "2");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "abcd");
            Assert.Equal(result["b"].value, 2);

            var isFAIL = bs.vali("test14").check(out result, "a", "abcded", "b", "1");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        [Fact]
        public void Test15() {
            bs.vali("test15", "a", "in[남자,여자]", "b", "notin[남자,여자]");
            var result = bs.valiResult();
            var isOK = bs.vali("test15").check(out result, "a", "남자", "b", "남성");
            Assert.Equal(isOK, true);
            Assert.Equal(result["a"].value, "남자");
            Assert.Equal(result["b"].value, "남성");

            var isFAIL = bs.vali("test15").check(out result, "a", "남성", "b", "남자");
            Assert.Equal(isFAIL, false);
            Assert.False(bs.isOK(result["a"].value));
            Assert.False(bs.isOK(result["b"].value));
        }
        */

    }
}