using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace WebApplication2.Controllers {
    public class RsaController:Controller {
        private bs bs;
        public RsaController(bs b) {
            bs = b;
        }
        [HttpPost]
        public IActionResult encdec(string text) {
            var rsa = bs.rsaGenerate();
            var cipherText = rsa.encrypt(text);
            var plainText = rsa.decrypt(cipherText);
            return bs.apiOk(new { text = plainText });
        }
        [HttpGet]
        public IActionResult publickey() {
            var rsa = bs.rsaGenerate();
            bs.S<bs.RSAKeyPair>("rsa", rsa);
            return Json(rsa.getPublicInfo());
        }
        [HttpPost]
        public IActionResult login(string email, string pw) {
            var rsa = bs.S<bs.RSAKeyPair>("rsa");
            return bs.apiOk(new { email = rsa.decrypt(email), pw = rsa.decrypt(pw)});
        }

    }
}
