using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication2.Controllers {
    public class RsaController:Controller {
        private bs bs;
        public RsaController(bs b) {
            bs = b;
        }
        [HttpGet]
        public IActionResult publickey() {
            /*
            var cipherText = bs.rsaEncrypt("안녕하세요!");
            var plainText = bs.rsaDecrypt(cipherText);
            return Json(new { text = plainText });
            */
            return Json(bs.rsaGetPublic());
        }
        [HttpPost]
        public IActionResult test(string email, string pw) {
            return Json(new { email = bs.rsaDecrypt(email), pw = bs.rsaDecrypt(pw) });
        }
    }
}
