using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication2.Controllers {
    public class HomeController:Controller {
        private bs bs;
        public HomeController(bs b) {
            bs = b;
        }
        public string _Index(ActionExecutingContext c) {
            //bs.log(bs.to<string>("aaaa"));
            bs.log((bs.to<int>("1234") is int)+ "");
            //bs.log(bs.to<float>(333.1F) + "");
            //bs.log(bs.to<double>(333.3) + "");
            //bs.log(bs.to<bool>(false) + "");
            
            return "test";
        }
        public IActionResult Index() {
            var r = bs.valiResult();
            var rs = bs.dbSelect(out r, "remote:a", "title", "1PD시험a");

            return Json(new { data = rs, a = bs.before(this)});
        }
    }
}