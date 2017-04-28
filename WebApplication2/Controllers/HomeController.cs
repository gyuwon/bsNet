using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WebApplication2.Controllers {
    public class HomeController:Controller {
        private bs bs;
        public HomeController(bs b) {
            bs = b;
        }
        public string _Index(ActionExecutingContext c) {
            
            var a = JObject.Parse("{}");
            return "test";
        }
        public IActionResult Index() {
            var r = bs.valiResult();
            var rs = bs.dbSelect<List<Object[]>>(out r, "test:a", "title", "1PD시험a");
            return Json(new { data = rs, a = bs.before(this), b = bs.fr<string>(true, "test.html")});
        }
    }
}