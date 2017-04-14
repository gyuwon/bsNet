using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers {
    public class HomeController : Controller
    {
        private bs bs;
        public HomeController(bs b)
        {
            bs = b;
        }
        public IActionResult Index(string a, int b)
        {
            bs.dbQuery("local", "a", "select * from test where title=@title@");
            var rs = bs.dbSelect("local:a", "hika");
            return Json(new {data = rs, a = 3});
        }
    }
}